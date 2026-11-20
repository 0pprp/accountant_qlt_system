import 'dart:async';
import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:image_picker/image_picker.dart';
import 'package:latlong2/latlong.dart';

import '../../core/formatting/iraq_time.dart';
import '../../core/location/gps.dart';
import '../../core/network/api_errors.dart';
import '../../core/theme/app_theme.dart';
import '../../core/validation/validators.dart';
import '../../core/widgets/feedback.dart';
import '../../core/widgets/map_parts.dart';
import '../../core/widgets/neu.dart';
import '../../injection.dart';
import '../promoter/models.dart';
import '../promoter/promoter_api.dart';
import 'order_draft.dart';

class NewOrderPage extends StatefulWidget {
  const NewOrderPage({super.key, required this.onOpenOrders});
  final VoidCallback onOpenOrders;

  @override
  State<NewOrderPage> createState() => _NewOrderPageState();
}

class _NewOrderPageState extends State<NewOrderPage> {
  static const otherProduct = 'اخرى';
  static const duplicateCode = 'FieldPromoter.DuplicateOpenOrder';
  static const maxPhotoBytes = 2 * 1024 * 1024;

  final _form = GlobalKey<FormState>();
  final _name = TextEditingController();
  final _phone = TextEditingController();
  final _address = TextEditingController();
  final _note = TextEditingController();
  final _other = TextEditingController();
  final _mapController = MapController();
  SubmissionKeyKeeper _keeper = SubmissionKeyKeeper();

  String _product = '';
  int? _branchId;
  double? _lat;
  double? _lng;
  String? _photoPath;
  bool _shopOpen = false;
  bool _busy = false;
  bool _locating = false;
  String? _productError;
  String? _branchError;
  Timer? _saveTimer;
  bool _restoring = true;

  @override
  void initState() {
    super.initState();
    for (final c in [_name, _phone, _address, _note, _other]) {
      c.addListener(_changed);
    }
    Injection.store.addListener(_onStore);
    unawaited(_restore());
  }

  @override
  void dispose() {
    _saveTimer?.cancel();
    Injection.store.removeListener(_onStore);
    for (final c in [_name, _phone, _address, _note, _other]) {
      c.dispose();
    }
    _mapController.dispose();
    super.dispose();
  }

  void _onStore() {
    if (!mounted) return;
    setState(_preselectBranch);
  }

  void _preselectBranch() {
    final branches = Injection.store.me?.branches ?? const <Branch>[];
    if (_branchId != null && !branches.any((b) => b.id == _branchId)) {
      _branchId = null;
    }
    if (_branchId == null && branches.length == 1) _branchId = branches.first.id;
  }

  Future<void> _restore() async {
    final d = await Injection.drafts.load();
    if (!mounted) return;
    if (d != null) {
      _name.text = d.customerName;
      _phone.text = d.customerPhone;
      _address.text = d.address;
      _note.text = d.note;
      _other.text = d.otherProduct;
      _product = d.product;
      _branchId = d.branchId;
      _lat = d.latitude;
      _lng = d.longitude;
      _photoPath = d.photoPath != null && File(d.photoPath!).existsSync() ? d.photoPath : null;
      _shopOpen = _lat != null || _photoPath != null;
      if (d.submissionKey != null && d.keyFingerprint != null) {
        _keeper = SubmissionKeyKeeper(key: d.submissionKey, fingerprint: d.keyFingerprint);
      }
    }
    _preselectBranch();
    _restoring = false;
    setState(() {});
  }

  OrderDraft _draft({String? key, String? fingerprint}) => OrderDraft(
    product: _product,
    otherProduct: _other.text,
    customerName: _name.text,
    customerPhone: _phone.text,
    branchId: _branchId,
    address: _address.text,
    note: _note.text,
    latitude: _lat,
    longitude: _lng,
    photoPath: _photoPath,
    submissionKey: key,
    keyFingerprint: fingerprint,
  );

  void _changed() {
    if (_restoring) return;
    _saveTimer?.cancel();
    _saveTimer = Timer(const Duration(milliseconds: 600), _persist);
  }

  void _persist() {
    final d = _draft();
    final fp = d.fingerprint;
    final keyMatches = _keeper.currentFingerprint == fp;
    unawaited(
      Injection.drafts.save(
        _draft(
          key: keyMatches ? _keeper.currentKey : null,
          fingerprint: keyMatches ? fp : null,
        ),
      ),
    );
  }

  void _set(VoidCallback change) {
    setState(change);
    _changed();
  }

  String get _productValue => _product == otherProduct ? _other.text : _product;

  Future<void> _submit() async {
    if (_busy) return;
    final me = Injection.store.me;
    setState(() {
      _productError = _product.isEmpty
          ? 'اختر المنتج'
          : (_product == otherProduct ? Validators.product(_other.text) : null);
      _branchError = _branchId == null ? 'اختر الفرع' : null;
    });
    final formOk = _form.currentState?.validate() ?? false;
    if (!formOk || _productError != null || _branchError != null || me == null) {
      AppFeedback.snack(context, 'راجع الحقول المطلوبة', error: true);
      return;
    }
    if ((_lat == null) != (_lng == null) ||
        (_lat != null && !Validators.insideIraq(_lat!, _lng!))) {
      AppFeedback.snack(context, 'الموقع خارج حدود العراق', error: true);
      return;
    }
    if (_photoPath != null) {
      final f = File(_photoPath!);
      if (!f.existsSync()) {
        setState(() => _photoPath = null);
      } else if (f.lengthSync() > maxPhotoBytes) {
        await AppFeedback.error(context, 'الصورة يجب أن تكون JPG أو PNG بحجم لا يتجاوز 2 ميغابايت');
        return;
      }
    }
    FocusScope.of(context).unfocus();
    final fingerprint = _draft().fingerprint;
    final key = _keeper.keyFor(fingerprint);
    await Injection.drafts.save(_draft(key: key, fingerprint: fingerprint));
    setState(() => _busy = true);
    try {
      final receipt = await Injection.api.submitOrder(
        OrderSubmission(
          submissionKey: key,
          branchId: _branchId!,
          customerName: Validators.normalizeTripleName(_name.text) ?? _name.text.trim(),
          customerPhone: Validators.normalizePhone(_phone.text) ?? _phone.text.trim(),
          product: Validators.normalizeProduct(_productValue) ?? _productValue.trim(),
          address: _address.text.trim(),
          note: _note.text.trim().isEmpty ? null : _note.text.trim(),
          latitude: _lat,
          longitude: _lng,
          photoPath: _photoPath,
        ),
      );
      _resetForm();
      unawaited(Injection.store.afterActivity());
      if (!mounted) return;
      final open = await _showSuccess(receipt);
      if (open == true) widget.onOpenOrders();
    } catch (e) {
      final err = ApiErrors.from(e);
      if (!mounted) return;
      if (err.code == duplicateCode) {
        final open = await AppFeedback.dialog<bool>(
          context,
          title: 'الطلب مرسل مسبقاً',
          message: err.message,
          icon: Icons.content_copy_rounded,
          color: AppColors.warning,
          actions: const [
            DialogAction(label: 'عرض الطلبات', value: true),
            DialogAction(label: 'حسناً', value: false, primary: false),
          ],
        );
        if (open == true) widget.onOpenOrders();
      } else {
        await AppFeedback.error(context, err.message);
      }
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  void _resetForm() {
    _restoring = true;
    _keeper.renew();
    _saveTimer?.cancel();
    _name.clear();
    _phone.clear();
    _address.clear();
    _note.clear();
    _other.clear();
    setState(() {
      _product = '';
      _lat = null;
      _lng = null;
      _photoPath = null;
      _shopOpen = false;
      _productError = null;
      _branchError = null;
      _preselectBranch();
    });
    _form.currentState?.reset();
    _restoring = false;
    unawaited(Injection.drafts.clear());
  }

  Future<bool?> _showSuccess(OrderReceipt receipt) => AppFeedback.dialog<bool>(
    context,
    title: receipt.alreadySubmitted ? 'الطلب مسجّل مسبقاً' : 'تم إرسال الطلب',
    message: 'رقم الطلب ${receipt.id}\nوصل الطلب إلى قسم المبيعات',
    icon: Icons.check_circle_outline_rounded,
    color: AppColors.success,
    actions: const [
      DialogAction(label: 'طلب جديد', value: false),
      DialogAction(label: 'عرض الطلبات', value: true, primary: false),
    ],
  );

  Future<void> _pickPhoto(ImageSource source) async {
    final picker = ImagePicker();
    XFile? file;
    try {
      file = await picker.pickImage(
        source: source,
        maxWidth: 1600,
        imageQuality: 72,
        requestFullMetadata: false,
      );
    } catch (_) {
      if (source == ImageSource.camera) {
        try {
          file = await picker.pickImage(
            source: ImageSource.gallery,
            maxWidth: 1600,
            imageQuality: 72,
            requestFullMetadata: false,
          );
        } catch (_) {
          if (mounted) AppFeedback.snack(context, 'تعذر فتح الكاميرا أو المعرض', error: true);
        }
      } else if (mounted) {
        AppFeedback.snack(context, 'تعذر فتح المعرض', error: true);
      }
    }
    if (file == null || !mounted) return;
    _set(() => _photoPath = file!.path);
  }

  Future<void> _useMyLocation() async {
    if (_locating) return;
    setState(() => _locating = true);
    try {
      final p = await Gps.current();
      if (!Validators.insideIraq(p.latitude, p.longitude)) {
        if (mounted) AppFeedback.snack(context, 'الموقع خارج حدود العراق', error: true);
        return;
      }
      _set(() {
        _lat = p.latitude;
        _lng = p.longitude;
      });
      _mapController.move(LatLng(p.latitude, p.longitude), 17);
    } catch (e) {
      if (mounted) AppFeedback.snack(context, ApiErrors.message(e), error: true);
    } finally {
      if (mounted) setState(() => _locating = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final me = Injection.store.me;
    final products = me?.products ?? const <String>[];
    final branches = me?.branches ?? const <Branch>[];
    return Form(
      key: _form,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(18, 16, 18, 28),
        children: [
          PageHeader(
            title: 'طلب بيع جديد',
            subtitle: 'يصل مباشرة إلى قسم المبيعات',
            trailing: me != null && me.isWorking
                ? StatusPill(
                    'في الدوام منذ ${IraqTime.hm(me.shiftStartedAt)}',
                    color: AppColors.success,
                    icon: Icons.radio_button_checked_rounded,
                  )
                : null,
          ),
          const SizedBox(height: 18),
          const FieldLabel('المنتج'),
          Wrap(
            spacing: 10,
            runSpacing: 10,
            children: [
              for (final p in [...products, otherProduct])
                NeuChip(
                  label: p,
                  selected: _product == p,
                  onTap: () => _set(() {
                    _product = p;
                    _productError = null;
                  }),
                ),
            ],
          ),
          if (_product == otherProduct) ...[
            const SizedBox(height: 12),
            NeuTextField(
              controller: _other,
              hint: 'اكتب اسم المنتج',
              maxLength: Validators.maxProductLength,
            ),
          ],
          if (_productError != null) _ErrorLine(_productError!),
          const SizedBox(height: 16),
          NeuTextField(
            controller: _name,
            label: 'اسم الزبون الثلاثي',
            hint: 'مثال: علي حسين عبد الله',
            prefixIcon: Icons.person_outline_rounded,
            textInputAction: TextInputAction.next,
            maxLength: Validators.maxNameLength,
            validator: Validators.customerName,
          ),
          const SizedBox(height: 14),
          NeuTextField(
            controller: _phone,
            label: 'رقم الهاتف',
            hint: '07XXXXXXXXX',
            prefixIcon: Icons.phone_outlined,
            keyboardType: TextInputType.phone,
            textDirection: TextDirection.ltr,
            textInputAction: TextInputAction.next,
            maxLength: 20,
            validator: Validators.customerPhone,
          ),
          const SizedBox(height: 14),
          const FieldLabel('الفرع'),
          NeuInset(
            padding: const EdgeInsets.symmetric(horizontal: 12),
            child: DropdownButtonHideUnderline(
              child: DropdownButton<int>(
                isExpanded: true,
                value: branches.any((b) => b.id == _branchId) ? _branchId : null,
                hint: const Text('اختر الفرع'),
                dropdownColor: AppColors.surface,
                borderRadius: BorderRadius.circular(16),
                items: [
                  for (final b in branches)
                    DropdownMenuItem(value: b.id, child: Text(b.name)),
                ],
                onChanged: (v) => _set(() {
                  _branchId = v;
                  _branchError = null;
                }),
              ),
            ),
          ),
          if (_branchError != null) _ErrorLine(_branchError!),
          const SizedBox(height: 14),
          NeuTextField(
            controller: _address,
            label: 'العنوان',
            hint: 'المنطقة، أقرب نقطة دالة',
            prefixIcon: Icons.place_outlined,
            maxLines: 2,
            maxLength: Validators.maxAddressLength,
            validator: Validators.address,
          ),
          const SizedBox(height: 14),
          NeuTextField(
            controller: _note,
            label: 'ملاحظة',
            optional: true,
            maxLines: 3,
            maxLength: Validators.maxNoteLength,
            validator: Validators.note,
          ),
          const SizedBox(height: 18),
          _shopSection(),
          const SizedBox(height: 22),
          NeuButton(
            label: 'إرسال الطلب',
            icon: Icons.send_rounded,
            busy: _busy,
            onPressed: me == null ? null : _submit,
          ),
        ],
      ),
    );
  }

  Widget _shopSection() {
    final point = _lat != null && _lng != null ? LatLng(_lat!, _lng!) : null;
    return NeuCard(
      padding: EdgeInsets.zero,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          InkWell(
            borderRadius: BorderRadius.circular(20),
            onTap: () => setState(() => _shopOpen = !_shopOpen),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  const Icon(Icons.storefront_outlined, color: AppColors.primary),
                  const SizedBox(width: 10),
                  const Expanded(
                    child: Text(
                      'صورة المحل وموقعه — اختياري',
                      style: TextStyle(fontWeight: FontWeight.w600),
                    ),
                  ),
                  if (_photoPath != null || point != null)
                    const Padding(
                      padding: EdgeInsetsDirectional.only(end: 6),
                      child: Icon(Icons.check_circle, color: AppColors.success, size: 18),
                    ),
                  Icon(
                    _shopOpen ? Icons.expand_less_rounded : Icons.expand_more_rounded,
                    color: AppColors.muted,
                  ),
                ],
              ),
            ),
          ),
          if (_shopOpen)
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  if (_photoPath != null)
                    ClipRRect(
                      borderRadius: BorderRadius.circular(14),
                      child: Stack(
                        children: [
                          Image.file(
                            File(_photoPath!),
                            height: 170,
                            width: double.infinity,
                            fit: BoxFit.cover,
                            cacheWidth: 800,
                          ),
                          PositionedDirectional(
                            top: 8,
                            end: 8,
                            child: NeuIconButton(
                              icon: Icons.close_rounded,
                              color: AppColors.danger,
                              size: 36,
                              tooltip: 'حذف الصورة',
                              onPressed: () => _set(() => _photoPath = null),
                            ),
                          ),
                        ],
                      ),
                    ),
                  if (_photoPath != null) const SizedBox(height: 10),
                  Row(
                    children: [
                      Expanded(
                        child: NeuButton(
                          label: 'التقاط صورة',
                          icon: Icons.photo_camera_outlined,
                          primary: false,
                          height: 46,
                          onPressed: () => _pickPhoto(ImageSource.camera),
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: NeuButton(
                          label: 'من المعرض',
                          icon: Icons.photo_library_outlined,
                          primary: false,
                          height: 46,
                          onPressed: () => _pickPhoto(ImageSource.gallery),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 14),
                  ClipRRect(
                    borderRadius: BorderRadius.circular(14),
                    child: SizedBox(
                      height: 200,
                      child: Stack(
                        children: [
                          FlutterMap(
                            mapController: _mapController,
                            options: MapOptions(
                              initialCenter: point ?? MapParts.baghdad,
                              initialZoom: point == null ? 11 : 17,
                              onTap: (_, p) {
                                if (!Validators.insideIraq(p.latitude, p.longitude)) {
                                  AppFeedback.snack(context, 'الموقع خارج حدود العراق', error: true);
                                  return;
                                }
                                _set(() {
                                  _lat = p.latitude;
                                  _lng = p.longitude;
                                });
                              },
                            ),
                            children: [
                              MapParts.tiles(),
                              if (point != null)
                                MarkerLayer(
                                  markers: [
                                    Marker(
                                      point: point,
                                      width: 36,
                                      height: 36,
                                      alignment: Alignment.topCenter,
                                      child: const Icon(
                                        Icons.location_on,
                                        color: AppColors.primary,
                                        size: 36,
                                      ),
                                    ),
                                  ],
                                ),
                              MapParts.attribution(),
                            ],
                          ),
                          if (point != null)
                            PositionedDirectional(
                              top: 8,
                              end: 8,
                              child: NeuIconButton(
                                icon: Icons.close_rounded,
                                color: AppColors.danger,
                                size: 36,
                                tooltip: 'إزالة الموقع',
                                onPressed: () => _set(() {
                                  _lat = null;
                                  _lng = null;
                                }),
                              ),
                            ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'اضغط على الخريطة لتحديد موقع المحل',
                    style: TextStyle(color: AppColors.muted, fontSize: 12),
                  ),
                  const SizedBox(height: 10),
                  NeuButton(
                    label: 'موقعي الحالي',
                    icon: Icons.my_location_rounded,
                    primary: false,
                    height: 46,
                    busy: _locating,
                    onPressed: _useMyLocation,
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }
}

class _ErrorLine extends StatelessWidget {
  const _ErrorLine(this.text);
  final String text;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsetsDirectional.only(start: 8, top: 6),
    child: Text(text, style: const TextStyle(color: AppColors.danger, fontSize: 12)),
  );
}
