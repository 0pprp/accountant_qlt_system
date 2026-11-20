import 'package:flutter/material.dart';

import '../../core/location/gps.dart';
import '../../core/network/api_errors.dart';
import '../../core/network/api_exception.dart';
import '../../core/theme/app_theme.dart';
import '../../core/validation/validators.dart';
import '../../core/widgets/neu.dart';
import '../../injection.dart';
import '../promoter/models.dart';

Future<ShopPin?> showPinForm(BuildContext context, {ShopPin? existing}) {
  return showModalBottomSheet<ShopPin>(
    context: context,
    isScrollControlled: true,
    useSafeArea: true,
    builder: (_) => _PinFormSheet(existing: existing),
  );
}

class _PinFormSheet extends StatefulWidget {
  const _PinFormSheet({this.existing});
  final ShopPin? existing;

  @override
  State<_PinFormSheet> createState() => _PinFormSheetState();
}

class _PinFormSheetState extends State<_PinFormSheet> {
  final _form = GlobalKey<FormState>();
  late final TextEditingController _name;
  late final TextEditingController _phone;
  late final TextEditingController _note;
  int? _interest;
  bool _busy = false;
  String? _error;

  bool get _editing => widget.existing != null;

  @override
  void initState() {
    super.initState();
    final e = widget.existing;
    _name = TextEditingController(text: e?.name ?? '');
    _phone = TextEditingController(text: e?.phone ?? '');
    _note = TextEditingController(text: e?.note ?? '');
    _interest = e?.interest;
  }

  @override
  void dispose() {
    _name.dispose();
    _phone.dispose();
    _note.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    final interestError = Validators.interest(_interest);
    setState(() => _error = interestError);
    if (!(_form.currentState?.validate() ?? false) || interestError != null) return;
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final phone = _phone.text.trim().isEmpty
          ? null
          : (Validators.normalizePhone(_phone.text) ?? _phone.text.trim());
      ShopPin saved;
      final e = widget.existing;
      if (e != null) {
        saved = await Injection.api.updatePin(
          e.id,
          PinInput(
            lat: e.lat,
            lng: e.lng,
            name: _name.text,
            interest: _interest!,
            phone: phone,
            note: _note.text,
          ),
        );
      } else {
        final p = await Gps.current();
        if (!Validators.insideIraq(p.latitude, p.longitude)) {
          throw ApiException('الموقع خارج حدود العراق');
        }
        saved = await Injection.api.createPin(
          PinInput(
            lat: p.latitude,
            lng: p.longitude,
            name: _name.text,
            interest: _interest!,
            phone: phone,
            note: _note.text,
            agentLat: p.latitude,
            agentLng: p.longitude,
          ),
        );
      }
      if (mounted) Navigator.of(context).pop(saved);
    } catch (e) {
      if (mounted) setState(() => _error = ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final insets = MediaQuery.viewInsetsOf(context).bottom;
    return Padding(
      padding: EdgeInsets.only(bottom: insets),
      child: SingleChildScrollView(
        padding: const EdgeInsets.fromLTRB(20, 0, 20, 24),
        child: Form(
          key: _form,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                _editing ? 'تعديل المحل' : 'تثبيت محل',
                style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w700),
              ),
              if (!_editing)
                const Text(
                  'سيُثبَّت المحل على موقعك الحالي',
                  style: TextStyle(color: AppColors.muted, fontSize: 13),
                ),
              const SizedBox(height: 16),
              NeuTextField(
                controller: _name,
                label: 'اسم المحل',
                prefixIcon: Icons.storefront_outlined,
                maxLength: Validators.maxPinNameLength,
                validator: Validators.pinName,
              ),
              const SizedBox(height: 16),
              const FieldLabel('مدى اهتمام صاحب المحل'),
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: [
                  for (var i = 1; i <= 5; i++)
                    NeuChip(
                      label: Interest.label(i),
                      selected: _interest == i,
                      color: Interest.color(i),
                      leading: CircleAvatar(radius: 5, backgroundColor: Interest.color(i)),
                      onTap: () => setState(() {
                        _interest = i;
                        _error = null;
                      }),
                    ),
                ],
              ),
              const SizedBox(height: 16),
              NeuTextField(
                controller: _phone,
                label: 'رقم الهاتف',
                optional: true,
                hint: '07XXXXXXXXX',
                keyboardType: TextInputType.phone,
                textDirection: TextDirection.ltr,
                maxLength: 20,
                validator: Validators.optionalPhone,
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
              if (_error != null) ...[
                const SizedBox(height: 12),
                Text(
                  _error!,
                  textAlign: TextAlign.center,
                  style: const TextStyle(color: AppColors.danger, fontWeight: FontWeight.w600),
                ),
              ],
              const SizedBox(height: 20),
              NeuButton(
                label: _editing ? 'حفظ التعديل' : 'تثبيت المحل',
                icon: _editing ? Icons.save_outlined : Icons.add_location_alt_outlined,
                busy: _busy,
                onPressed: _save,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
