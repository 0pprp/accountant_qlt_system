import '../domain/entities/sales_request_item.dart';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/time/baghdad_time.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/core/widgets/sale_photo_slot.dart';
import 'package:sales_employee_app/features/catalog/domain/entities/product_item.dart';
import 'package:sales_employee_app/features/sales/data/demo_orders_store.dart';
import 'package:sales_employee_app/features/sales/data/sale_wizard_local_draft_store.dart';
import 'package:sales_employee_app/features/sales/domain/entities/order_list_lite.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';
import 'package:sales_employee_app/features/sales/presentation/sale_success_dialog.dart';
import 'package:sales_employee_app/injection.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// C1 محل → C2 زبون → C3 منتجات → C4 أسعار → C5 مراجعة → تم البيع | تم الكشف
class CompleteSaleWizardPage extends StatefulWidget {
  const CompleteSaleWizardPage({super.key, required this.orderId, this.fromRequest = false, this.request});

  final int orderId;
  final bool fromRequest;
  final SalesRequestItem? request;

  @override
  State<CompleteSaleWizardPage> createState() => _CompleteSaleWizardPageState();
}

class _CompleteSaleWizardPageState extends State<CompleteSaleWizardPage> {
  late int _orderId = widget.orderId;
  int _step = 0;
  bool _busy = false;
  bool _nameLocked = true;

  // C1 محل
  final _shopName = TextEditingController();
  final _shopType = TextEditingController();
  final _goodsValue = TextEditingController();
  final _dailyIncome = TextEditingController();
  final _shopLength = TextEditingController();
  final _shopWidth = TextEditingController();
  final _shopNote = TextEditingController();
  XFile? _shopPhoto;
  double? _lat;
  double? _lng;

  // C2 زبون — القائمة/المندوب مفهوم واحد (منسدلة قوائم المحافظة)
  List<OrderListLite> _orderLists = const [];
  int? _orderListId;
  bool _listsLoading = false;
  String? _listsError;
  final _fullName = TextEditingController();
  final _phone = TextEditingController();
  final _province = TextEditingController();
  final _nationalId = TextEditingController();
  final _address = TextEditingController();
  final _nearest = TextEditingController();
  final _mukhtar = TextEditingController();
  final _rationCenter = TextEditingController();
  final Map<int, XFile> _uploadedSaleDocuments = {};
  final Map<int, XFile> _saleDocuments = {};
  /// مستمسكات اختيارية — نفس خانات التطبيق القديم (كاميرا + معرض).
  final Map<String, XFile> _kycDocs = {};
  static const _kycDefs = <(String key, String label, String? attachmentType)>[
    ('nationalIdFront', 'البطاقة الوطنية - أمامية', '0'),
    ('nationalIdBack', 'البطاقة الوطنية - خلفية', null),
    ('residenceCardFront', 'بطاقة السكن - أمامية', '1'),
    ('residenceCardBack', 'بطاقة السكن - خلفية', null),
    ('residenceCertificate', 'تأييد السكن', '2'),
  ];
  static const _saleDocLabels = ['وصل الشراء', 'وصل الأمانة', 'عقد البيع'];
  bool _restoringDraft = false;
  bool _dirty = false;

  String get _draftKey => SaleWizardLocalDraftStore.keyFor(
        orderId: _orderId,
        requestId: widget.request?.requestId,
      );

  String get _selectedListLabel {
    for (final e in _orderLists) {
      if (e.id == _orderListId) return e.label;
    }
    return '';
  }

  // C3 منتجات
  List<ProductItem> _catalog = const [];
  String _productQuery = '';
  final Map<int, int> _qty = {};

  // C4 أسعار
  final _total = TextEditingController();
  final _dailyInstallment = TextEditingController();
  final _prepayment = TextEditingController();
  double? _maxSell;

  static const _titles = [
    '1) بيانات المحل',
    '2) بيانات الزبون',
    '3) المنتجات',
    '4) الأسعار',
    '5) المراجعة',
  ];

  bool get _isDemo => _orderId < 0;

  List<ProductItem> get _filtered {
    final q = _productQuery.trim();
    if (q.isEmpty) return _catalog;
    return _catalog.where((p) => p.name.contains(q) || '${p.id}'.contains(q)).toList();
  }

  List<ProductItem> get _selected => _catalog.where((p) => (_qty[p.id] ?? 0) > 0).toList();

  @override
  void initState() {
    super.initState();
    _bootstrap();
  }

  Future<void> _bootstrap() async {
    if (widget.request == null && !SalesWorkflowRules.hasLinkedOrder(_orderId)) {
      if (mounted) {
        await showAppMessage(context, title: 'مرفوض', message: 'لا بيع بدون طلب مبيع مرتبط');
        if (mounted) Navigator.of(context).pop(false);
      }
      return;
    }

    String? status;
    final source = widget.request;
    if (source != null) {
      status = SalesWorkflowRules.pending;
      _fullName.text = source.customerName;
      _phone.text = source.customerPhone ?? '';
      _address.text = source.address ?? '';
      _province.text = source.branchName ?? '';
      try {
        await _loadOrderLists();
      } catch (e) {
        if (mounted) await showAppMessage(context, title: 'قوائم المحافظة', message: DioClient.mapError(e));
      }
    } else if (_isDemo) {
      final o = await Injection.demoOrders.byId(_orderId);
      if (o == null) {
        if (mounted) {
          await showAppMessage(context, title: 'غير موجود', message: 'الطلب غير مرتبط أو نُقل لزميل');
          if (mounted) Navigator.of(context).pop(false);
        }
        return;
      }
      status = o.status;
      _fullName.text = o.customerName;
      _nameLocked = true;
      if (o.customerPhone != null) _phone.text = o.customerPhone!;
      if (o.branchName != null) _province.text = o.branchName!;
      final draft = await Injection.demoOrders.draftOf(_orderId);
      if (draft != null) _hydrateDraft(draft);
      _maxSell = await Injection.demoOrders.maxSellOf(_orderId) ?? 2000000;
      await _loadOrderLists(preferName: draft?['orderListName']?.toString());
    } else {
      try {
        final o = await Injection.salesRemote.getOrder(_orderId);
        if (o == null) {
          if (mounted) {
            await showAppMessage(context, title: 'غير موجود', message: 'لا يوجد طلب مرتبط');
            if (mounted) Navigator.of(context).pop(false);
          }
          return;
        }
        status = o.status;
        _fullName.text = o.customerName;
        _nameLocked = o.customerName.isNotEmpty && o.customerName != '—';
        if (o.customerPhone != null) _phone.text = o.customerPhone!;
        if (o.branchName != null) _province.text = o.branchName!;
        if (o.businessName != null) _shopName.text = o.businessName!;
        if (o.creationAddress != null) _address.text = o.creationAddress!;
        await _loadOrderLists(preferName: o.orderListName);
      } catch (_) {
        if (mounted) {
          await showAppMessage(context, title: 'خطأ', message: 'تعذر تحميل الطلب المرتبط');
          if (mounted) Navigator.of(context).pop(false);
        }
        return;
      }
    }

    if (!SalesWorkflowRules.canStartSale(status)) {
      if (mounted) {
        await showAppMessage(
          context,
          title: 'غير مسموح',
          message: status == SalesWorkflowRules.rejected
              ? 'لا إتمام بيع من طلب مرفوض'
              : 'لا يمكن فتح إتمام البيع من هذه الحالة',
        );
        if (mounted) Navigator.of(context).pop(false);
      }
      return;
    }

    try {
      final list = await Injection.catalogRemote.getInventory(requestId: source?.requestId, orderId: source == null && widget.fromRequest ? _orderId : null);
      if (list.isEmpty) {
        if (!mounted) return;
        await showAppMessage(
          context,
          title: 'المخزن',
          message: 'لا منتجات متاحة من الخادم. لا يُستخدم منتج تجريبي في المسار الحي.',
        );
        _catalog = const [];
      } else {
        _catalog = list;
      }
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'المخزن',
        message: DioClient.mapError(e),
      );
      _catalog = const [];
    }
    if (!widget.fromRequest && _selected.isEmpty && _catalog.isNotEmpty) {
      _qty[_catalog.first.id] = 1;
      _applyDefaultPrices();
    }
    await _restoreLocalDraft();
    if (mounted) setState(() {});
  }

  void _hydrateDraft(Map<String, dynamic> d) {
    _shopName.text = '${d['shopName'] ?? ''}';
    _shopType.text = '${d['shopType'] ?? ''}';
    _goodsValue.text = '${d['goodsValue'] ?? ''}';
    _dailyIncome.text = '${d['dailyIncome'] ?? ''}';
    _shopLength.text = '${d['shopLength'] ?? ''}';
    _shopWidth.text = '${d['shopWidth'] ?? ''}';
    _shopNote.text = '${d['shopNote'] ?? ''}';
    final listId = d['orderListId'];
    if (listId is num) _orderListId = listId.toInt();
    _phone.text = '${d['phone'] ?? _phone.text}';
    _province.text = '${d['province'] ?? _province.text}';
    _nationalId.text = '${d['nationalId'] ?? ''}';
    _address.text = '${d['address'] ?? ''}';
    _nearest.text = '${d['nearest'] ?? ''}';
    _mukhtar.text = '${d['mukhtar'] ?? ''}';
    _rationCenter.text = '${d['rationCenter'] ?? _rationCenter.text}';
    _total.text = '${d['total'] ?? ''}';
    _dailyInstallment.text = '${d['dailyInstallment'] ?? ''}';
    _prepayment.text = '${d['prepayment'] ?? ''}';
    _lat = (d['lat'] as num?)?.toDouble();
    _lng = (d['lng'] as num?)?.toDouble();
    final products = d['products'];
    if (products is List) {
      _qty.clear();
      for (final row in products) {
        if (row is! Map) continue;
        final id = int.tryParse('${row['id'] ?? ''}');
        final q = int.tryParse('${row['qty'] ?? ''}');
        if (id != null && q != null && q > 0) _qty[id] = q;
      }
    }
  }

  Future<void> _restoreLocalDraft() async {
    _restoringDraft = true;
    try {
      final raw = await Injection.saleWizardDrafts.load(_draftKey);
      if (raw == null) return;
      final fields = raw['fields'];
      if (fields is Map) _hydrateDraft(Map<String, dynamic>.from(fields));
      final step = raw['step'];
      if (step is num) _step = step.toInt().clamp(0, 4);
      final photos = raw['photos'];
      if (photos is Map) {
        final map = Map<String, dynamic>.from(photos);
        final shop = map['shopPhoto']?.toString();
        if (shop != null && shop.isNotEmpty) _shopPhoto = XFile(shop);
        for (var i = 0; i < _saleDocLabels.length; i++) {
          final path = map['saleDoc_$i']?.toString();
          if (path != null && path.isNotEmpty) _saleDocuments[i] = XFile(path);
        }
        for (final def in _kycDefs) {
          final path = map[def.$1]?.toString();
          if (path != null && path.isNotEmpty) _kycDocs[def.$1] = XFile(path);
        }
      }
      _dirty = false;
    } finally {
      _restoringDraft = false;
    }
  }

  Map<String, String?> _photoPathsForDraft() => {
        'shopPhoto': _shopPhoto?.path,
        for (var i = 0; i < _saleDocLabels.length; i++)
          'saleDoc_$i': _saleDocuments[i]?.path,
        for (final def in _kycDefs) def.$1: _kycDocs[def.$1]?.path,
      };

  Future<void> _persistLocalDraft() async {
    if (_restoringDraft) return;
    try {
      await Injection.saleWizardDrafts.save(
        draftKey: _draftKey,
        step: _step,
        fields: _payload(),
        photoPaths: _photoPathsForDraft(),
      );
      _dirty = false;
    } catch (_) {}
  }

  void _markDirty() {
    if (_restoringDraft) return;
    _dirty = true;
  }

  Future<void> _loadOrderLists({String? preferName}) async {
    setState(() {
      _listsLoading = true;
      _listsError = null;
    });
    List<OrderListLite> rows = const [];
    try {
      rows = widget.fromRequest ? await Injection.salesRemote.checkoutLists(_orderId, requestId: widget.request?.requestId) : await Injection.salesRemote.getOrderLists(forceRefresh: true);
    } catch (e) {
      if (widget.fromRequest) {
        if (mounted) setState(() {
          _listsLoading = false;
          _listsError = DioClient.mapError(e);
        });
        return;
      }
      rows = const [];
    }
    // قوائم سالبة للتجريبي فقط — الطلب الحي يعتمد قائمة المندوب من السيرفر (Mandob).
    if (rows.isEmpty && _isDemo) {
      final province = _province.text.trim().isEmpty ? DemoOrdersStore.demoProvince : _province.text.trim();
      rows = [
        OrderListLite(id: -101, name: 'قائمة $province — أ'),
        OrderListLite(id: -102, name: 'قائمة $province — ب'),
        OrderListLite(id: -103, name: 'قائمة $province — ج'),
      ];
    }
    if (preferName != null && preferName.trim().isNotEmpty) {
      for (final e in rows) {
        if (e.label == preferName.trim()) {
          _orderListId = e.id;
          break;
        }
      }
    }
    if (_orderListId != null && !rows.any((e) => e.id == _orderListId)) {
      _orderListId = null;
    }
    if (_orderListId == null && rows.length == 1) {
      _orderListId = rows.first.id;
    }
    if (mounted) {
      setState(() {
        _orderLists = rows;
        _listsLoading = false;
      });
    }
  }

  void _applyDefaultPrices() {
    final sum = _selected.fold<double>(0, (s, p) => s + (p.price ?? 0) * (_qty[p.id] ?? 1));
    final installment = _selected.fold<double>(0, (s, p) => s + (p.dailyInstallment ?? 0) * (_qty[p.id] ?? 1));
    if (_total.text.trim().isEmpty) _total.text = sum > 0 ? sum.toStringAsFixed(0) : '';
    if (_dailyInstallment.text.trim().isEmpty) {
      _dailyInstallment.text = installment > 0 ? installment.toStringAsFixed(0) : '';
    }
    if (_prepayment.text.trim().isEmpty && sum > 0) {
      _prepayment.text = (sum * 0.05).toStringAsFixed(0);
    }
  }

  @override
  void dispose() {
    for (final c in [
      _shopName, _shopType, _goodsValue, _dailyIncome, _shopLength, _shopWidth, _shopNote,
      _fullName, _phone, _province, _nationalId, _address, _nearest, _mukhtar, _rationCenter,
      _total, _dailyInstallment, _prepayment,
    ]) {
      c.dispose();
    }
    super.dispose();
  }

  Map<String, dynamic> _payload() {
    _applyDefaultPrices();
    final total = double.tryParse(_total.text.trim()) ?? 0;
    final listName = _selectedListLabel;
    return {
      'shopName': _shopName.text.trim(),
      'shopType': _shopType.text.trim(),
      'goodsValue': _goodsValue.text.trim(),
      'dailyIncome': _dailyIncome.text.trim(),
      'shopLength': _shopLength.text.trim(),
      'shopWidth': _shopWidth.text.trim(),
      'shopNote': _shopNote.text.trim(),
      'shopPhoto': _shopPhoto?.name,
      'lat': _lat,
      'lng': _lng,
      'orderListId': _orderListId,
      'orderListName': listName,
      'mandobName': listName,
      'fullName': _fullName.text.trim(),
      'phone': _phone.text.trim(),
      'province': _province.text.trim(),
      'nationalId': _nationalId.text.trim(),
      'address': _address.text.trim(),
      'nearest': _nearest.text.trim(),
      'mukhtar': _mukhtar.text.trim(),
      'rationCenter': _rationCenter.text.trim(),
      'kycKeys': _kycDocs.keys.toList(),
      'products': _selected
          .map((p) => {
                'id': p.id,
                'name': p.name,
                'qty': _qty[p.id] ?? 1,
                'sellAmount': p.price,
                'dailyInstallment': p.dailyInstallment,
              })
          .toList(),
      'total': total,
      'dailyInstallment': double.tryParse(_dailyInstallment.text.trim()) ?? 0,
      'prepayment': double.tryParse(_prepayment.text.trim()) ?? 0,
      'maxSellAmount': _maxSell,
    };
  }

  Future<bool> _validateStep() async {
    switch (_step) {
      case 0:
        if ([_shopName, _shopType, _goodsValue, _dailyIncome, _shopLength, _shopWidth]
            .any((c) => c.text.trim().isEmpty)) {
          await showAppMessage(context, title: 'بيانات المحل', message: 'أكمل كل الحقول الإلزامية للمحل');
          return false;
        }
        if (_shopPhoto == null) {
          await showAppMessage(context, title: 'صورة المحل', message: 'صورة المحل إلزامية');
          return false;
        }
        if (_lat == null || _lng == null) {
          await showAppMessage(context, title: 'الموقع', message: 'تحديد موقع المحل إلزامي');
          return false;
        }
        return true;
      case 1:
        // حي + Mandob: القائمة تُحل على السيرفر؛ لا تفرض اختيارًا تجريبيًا سالبًا.
        if (_isDemo && (_orderListId == null || _orderListId == 0)) {
          await showAppMessage(context, title: 'القائمة/المندوب', message: 'اختر القائمة/المندوب من قوائم المحافظة');
          return false;
        }
        if (!_isDemo && (widget.fromRequest || _orderLists.isNotEmpty) && (_orderListId == null || _orderListId! <= 0)) {
          await showAppMessage(context, title: 'القائمة/المندوب', message: 'اختر القائمة/المندوب من قوائم المحافظة');
          return false;
        }
        if ([_phone, _province, _nationalId, _address, _nearest, _mukhtar].any((c) => c.text.trim().isEmpty)) {
          await showAppMessage(context, title: 'بيانات الزبون', message: 'أكمل حقول الزبون الإلزامية');
          return false;
        }
        return true;
      case 2:
        if (_selected.isEmpty) {
          await showAppMessage(context, title: 'المنتجات', message: 'اختر مادة واحدة على الأقل');
          return false;
        }
        return true;
      case 3:
        _applyDefaultPrices();
        final total = double.tryParse(_total.text.trim()) ?? 0;
        final installment = double.tryParse(_dailyInstallment.text.trim()) ?? 0;
        if (total <= 0) {
          await showAppMessage(context, title: 'الأسعار', message: 'إجمالي البيع يجب أن يكون أكبر من صفر');
          return false;
        }
        if (_maxSell != null && total > _maxSell!) {
          await showAppMessage(context, title: 'سقف السعر', message: 'تجاوزت الحد الأقصى المسموح من الطلب ($_maxSell)');
          return false;
        }
        if (installment <= 0) {
          await showAppMessage(context, title: 'الأسعار', message: 'القسط اليومي إلزامي ويجب أن يكون أكبر من صفر');
          return false;
        }
        return true;
      default:
        return true;
    }
  }

  Future<void> _captureGps() async {
    final p = await Injection.locationService.current();
    if (!mounted) return;
    if (p == null) {
      await showAppMessage(context, title: 'GPS', message: 'تعذر قراءة الموقع');
      return;
    }
    setState(() {
      _lat = p.lat;
      _lng = p.lng;
    });
  }

  Future<void> _finishSold() async {
    if (!await _validateAllRequired()) return;
    setState(() => _busy = true);
    try {
      final payload = _payload();
      if (_isDemo) {
        await Injection.demoOrders.completeSale(negativeId: _orderId, payload: payload);
      } else {
        // ترتيب عقود السيرفر: Attachments(0) → complete-attachments → SellerInfo(1) → setSellerInfo(=Sold)
        // ثم SalesWorkflow/complete لمزامنة SalesRequest المرتبط.
        final evidence = _shopPhoto;
        if (evidence == null) {
          await showAppMessage(context, title: 'مرفقات', message: 'صورة المحل إلزامية كمرفق بيع');
          return;
        }
        // PurchaseReceipt=4, TrustReceipt=5, SaleContract=6
        const requiredTypes = ['4', '5', '6'];
        if (_saleDocuments.length != 3) {
          await showAppMessage(context, title: 'مستندات البيع', message: 'أرفق وصل الشراء ووصل الأمانة وعقد البيع');
          return;
        }
        for (var i = 0; i < requiredTypes.length; i++) {
          final src = _saleDocuments[i]!;
          if (identical(_uploadedSaleDocuments[i], src)) continue;
          final bytes = await src.readAsBytes();
          await Injection.salesRemote.uploadAttachment(
            orderId: _orderId,
            fileName: src.name.isEmpty ? 'attach_$i.jpg' : src.name,
            bytes: bytes,
            attachmentType: requiredTypes[i],
          );
          _uploadedSaleDocuments[i] = src;
        }
        if (widget.fromRequest) {
          await Injection.salesRemote.checkout(_orderId, payload);
        } else {
        await Injection.salesRemote.completeAttachments(_orderId);
        // موظفو التطبيق هنا Mandob: إرسال orderListId يُرفض بـ MandobCanNotSetOrderListId.
        // السيرفر يحل قائمة المندوب تلقائياً — لا تمرّر حتى لو اختار المستخدم قائمة في الواجهة.
        await Injection.salesRemote.setSellerInfo(
          orderId: _orderId,
          creationAddress: _address.text.trim().isEmpty ? '—' : _address.text.trim(),
          lat: _lat,
          lng: _lng,
          saleDate: formatBaghdadDateOnly(baghdadNow()).replaceAll('/', '-'),
          saleTime:
              '${baghdadNow().hour.toString().padLeft(2, '0')}:${baghdadNow().minute.toString().padLeft(2, '0')}:00',
          orderListId: null,
        );
        await Injection.salesRemote.complete(_orderId);
        }
        await _uploadOptionalKycBestEffort();
      }
      await Injection.saleWizardDrafts.clear(_draftKey);
      if (!mounted) return;
      await showSaleSuccessDialog(context, orderId: _orderId.abs());
      if (!mounted) return;
      if (!widget.fromRequest) await _mandateListRatingGate();
      if (!mounted) return;
      Navigator.of(context).pop(true);
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(context, title: 'فشل تم البيع', message: DioClient.mapError(e), color: AppColors.danger);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _uploadOptionalKycBestEffort() async {
    for (final def in _kycDefs) {
      final type = def.$3;
      final file = _kycDocs[def.$1];
      if (type == null || file == null) continue;
      try {
        final bytes = await file.readAsBytes();
        await Injection.salesRemote.uploadAttachment(
          orderId: _orderId,
          fileName: file.name.isEmpty ? '${def.$1}.jpg' : file.name,
          bytes: bytes,
          attachmentType: type,
        );
      } catch (_) {
        // المستمسكات اختيارية — لا تُفشل البيع إن رُفض الرفع بعد الإتمام.
      }
    }
  }

  Future<void> _finishDisclosure() async {
    // Partial save allowed if at least shop basics OR any progress — require shop name + gps minimum
    if (_shopName.text.trim().isEmpty || _lat == null) {
      await showAppMessage(context, title: 'تم الكشف', message: 'لحفظ الكشف لزم اسم المحل وتحديد الموقع على الأقل');
      return;
    }
    setState(() => _busy = true);
    try {
      await _persistLocalDraft();
      final payload = _payload();
      if (_isDemo) {
        await Injection.demoOrders.saveDraft(
          negativeId: _orderId,
          draft: payload,
          status: SalesWorkflowRules.disclosure,
        );
      } else {
        await Injection.salesRemote.transfer(
          id: _orderId,
          targetStatus: SalesWorkflowRules.disclosure,
          reason: 'تم الكشف — حفظ تقدّم الزيارة',
        );
      }
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم الكشف',
        message: 'حُفظ التقدّم محلياً ونُقل الطلب إلى «تم الكشف» دون إتمام البيع.',
        icon: Icons.fact_check_rounded,
        color: AppColors.info,
      );
      if (!mounted) return;
      Navigator.of(context).pop(true);
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(context, title: 'فشل الكشف', message: DioClient.mapError(e));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<bool> _validateAllRequired() async {
    for (var i = 0; i <= 3; i++) {
      _step = i;
      if (!await _validateStep()) {
        setState(() {});
        return false;
      }
    }
    _step = 4;
    setState(() {});
    return true;
  }

  Future<void> _mandateListRatingGate() async {
    final prefs = await SharedPreferences.getInstance();
    final listKey = _selectedListLabel.isEmpty ? '${_orderListId ?? 0}' : _selectedListLabel;
    final key = 'list_rating_${listKey}_${baghdadNow().toIso8601String().substring(0, 10)}';
    if (prefs.getBool(key) == true) return;
    if (!mounted) return;
    final rating = await showCenteredDialog<int>(
      context: context,
      barrierDismissible: false,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Text('تقييم مندوب القائمة (إلزامي)', style: TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
          const SizedBox(height: 8),
          Text('القائمة/المندوب: $listKey', style: const TextStyle(color: AppColors.textSecondary)),
          const SizedBox(height: 12),
          Wrap(
            spacing: 8,
            children: List.generate(5, (i) {
              final v = i + 1;
              return ElevatedButton(onPressed: () => Navigator.pop(context, v), child: Text('$v'));
            }),
          ),
        ],
      ),
    );
    if (rating != null) await prefs.setBool(key, true);
  }

  Future<void> _onWillPop() async {
    await _persistLocalDraft();
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return PopScope(
      canPop: true,
      onPopInvokedWithResult: (didPop, _) async {
        if (didPop || _dirty || _shopName.text.trim().isNotEmpty || _shopPhoto != null || _kycDocs.isNotEmpty) {
          await _onWillPop();
        }
      },
      child: Scaffold(
      appBar: AppBar(title: Text('إتمام البيع — ${_titles[_step]}')),
      body: Column(
        children: [
          LinearProgressIndicator(value: (_step + 1) / 5, color: AppColors.primary),
          Expanded(child: _buildStep()),
          Padding(
            padding: EdgeInsets.fromLTRB(12, 8, 12, 10 + bottom),
            child: Column(
              children: [
                if (!widget.fromRequest && _shopName.text.trim().isNotEmpty && _lat != null)
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton.icon(
                      onPressed: _busy ? null : _finishDisclosure,
                      icon: const Icon(Icons.fact_check_outlined),
                      label: const Text('تم الكشف (حفظ بلا إغلاق)'),
                    ),
                  ),
                if (!widget.fromRequest && _shopName.text.trim().isNotEmpty && _lat != null) const SizedBox(height: 8),
                Row(
                  children: [
                    if (_step > 0)
                      Expanded(
                        child: OutlinedButton(
                          onPressed: _busy ? null : () => setState(() => _step--),
                          child: const Text('السابق'),
                        ),
                      ),
                    if (_step > 0) const SizedBox(width: 8),
                    Expanded(
                      child: ElevatedButton(
                        onPressed: _busy
                            ? null
                            : () async {
                                  if (_step < 4) {
                                  if (!await _validateStep()) return;
                                  if (_step == 2) _applyDefaultPrices();
                                  if (_step == 3 && widget.request != null && _orderId <= 0) {
                                    setState(() => _busy = true);
                                    try {
                                      final draft = await Injection.salesRemote.ensureDraft(widget.request!.requestId);
                                      _orderId = draft?.linkedOrderId ?? 0;
                                      if (_orderId <= 0) throw StateError('تعذر حفظ المسودة');
                                    } catch (e) {
                                      if (mounted) await showAppMessage(context, title: 'مسودة البيع', message: DioClient.mapError(e));
                                      return;
                                    } finally {
                                      if (mounted) setState(() => _busy = false);
                                    }
                                  }
                                  if (mounted) {
                                    setState(() => _step++);
                                    await _persistLocalDraft();
                                  }
                                } else {
                                  await _finishSold();
                                }
                              },
                        child: _busy
                            ? const SizedBox(
                                width: 22,
                                height: 22,
                                child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                              )
                            : Text(_step == 4 ? 'تم البيع' : 'التالي'),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    ),
    );
  }

  Widget _buildStep() {
    switch (_step) {
      case 0:
        return ListView(
          padding: const EdgeInsets.all(16),
          children: [
            _field(_shopName, 'اسم المحل *'),
            _field(_shopType, 'طبيعة العمل *'),
            _field(_goodsValue, 'تقدير قيمة البضاعة *', number: true),
            _field(_dailyIncome, 'تقدير الوارد اليومي *', number: true),
            _field(_shopLength, 'طول المحل *', number: true),
            _field(_shopWidth, 'عرض المحل *', number: true),
            _field(_shopNote, 'ملاحظة الموظف (اختياري)'),
            const SizedBox(height: 8),
            SalePhotoSlot(
              label: 'صورة المحل',
              required: true,
              preferCamera: true,
              file: _shopPhoto,
              onChanged: (f) {
                setState(() => _shopPhoto = f);
                _markDirty();
                _persistLocalDraft();
              },
            ),
            const SizedBox(height: 8),
            OutlinedButton.icon(
              onPressed: () async {
                await _captureGps();
                _markDirty();
                await _persistLocalDraft();
              },
              icon: const Icon(Icons.my_location),
              label: Text(_lat == null ? 'تحديد موقع المحل *' : 'تم تحديد الموقع'),
            ),
          ],
        );
      case 1:
        return ListView(
          padding: const EdgeInsets.all(16),
          children: [
            const Text('القائمة/المندوب *', style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            InputDecorator(
              decoration: const InputDecoration(
                contentPadding: EdgeInsets.fromLTRB(12, 4, 12, 4),
                border: OutlineInputBorder(),
              ),
              child: _listsLoading
                  ? const Padding(
                      padding: EdgeInsets.symmetric(vertical: 12),
                      child: Center(child: SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2))),
                    )
                  : DropdownButtonHideUnderline(
                      child: DropdownButton<int>(
                        isExpanded: true,
                        value: _orderLists.any((e) => e.id == _orderListId) ? _orderListId : null,
                        hint: const Text('اختر القائمة/المندوب من قوائم المحافظة'),
                        items: [
                          for (final list in _orderLists)
                            DropdownMenuItem(value: list.id, child: Text(list.mandobFullName?.isNotEmpty == true ? '${list.label} · ${list.mandobFullName}' : list.label)),
                        ],
                        onChanged: (value) {
                          setState(() => _orderListId = value);
                          _markDirty();
                          _persistLocalDraft();
                        },
                      ),
                    ),
            ),
            if (!_listsLoading && _listsError != null)
              TextButton.icon(onPressed: () => _loadOrderLists(), icon: const Icon(Icons.refresh), label: Text('تعذّر تحميل قوائم المحافظة: $_listsError · إعادة المحاولة')),
            if (!_listsLoading && _listsError == null && _orderLists.isEmpty)
              const Padding(
                padding: EdgeInsets.only(top: 6, bottom: 4),
                child: Text('لا توجد قوائم معرفة في المحافظة', style: TextStyle(color: AppColors.muted, fontSize: 12)),
              ),
            if (!_listsLoading && _orderLists.any((e) => e.id < 0))
              const Padding(
                padding: EdgeInsets.only(top: 6, bottom: 4),
                child: Text('عرض قوائم تجريبية للمحافظة (الـ API لم يُرجع قوائم)', style: TextStyle(color: AppColors.muted, fontSize: 12)),
              ),
            const SizedBox(height: 12),
            TextField(
              controller: _fullName,
              readOnly: _nameLocked,
              decoration: InputDecoration(
                labelText: 'اسم الزبون${_nameLocked ? ' (مقفول من الطلب)' : ' *'}',
                filled: _nameLocked,
              ),
            ),
            const SizedBox(height: 8),
            _field(_phone, 'الهاتف *', number: true),
            _field(_province, 'المحافظة *'),
            _field(_nationalId, 'البطاقة الوطنية *'),
            _field(_address, 'العنوان *'),
            _field(_nearest, 'أقرب نقطة دالة *'),
            _field(_mukhtar, 'اسم المختار *'),
            _field(_rationCenter, 'رقم مركز التموين (اختياري)'),
            const SizedBox(height: 8),
            const Text('مستندات البيع *', style: TextStyle(fontWeight: FontWeight.w700)),
            const SizedBox(height: 8),
            for (final entry in _saleDocLabels.asMap().entries)
              SalePhotoSlot(
                label: entry.value,
                required: true,
                preferCamera: true,
                file: _saleDocuments[entry.key],
                onChanged: (f) {
                  setState(() {
                    if (f == null) {
                      _saleDocuments.remove(entry.key);
                    } else {
                      _saleDocuments[entry.key] = f;
                    }
                  });
                  _markDirty();
                  _persistLocalDraft();
                },
              ),
            const SizedBox(height: 8),
            const Text('مستمسكات الزبون (اختياري)', style: TextStyle(fontWeight: FontWeight.w700)),
            const SizedBox(height: 4),
            const Text(
              'كاميرا أو معرض — لا تمنع إتمام البيع إن تُركت فارغة',
              style: TextStyle(color: AppColors.textSecondary, fontSize: 12.5),
            ),
            const SizedBox(height: 8),
            for (final def in _kycDefs)
              SalePhotoSlot(
                label: def.$2,
                required: false,
                preferCamera: true,
                file: _kycDocs[def.$1],
                onChanged: (f) {
                  setState(() {
                    if (f == null) {
                      _kycDocs.remove(def.$1);
                    } else {
                      _kycDocs[def.$1] = f;
                    }
                  });
                  _markDirty();
                  _persistLocalDraft();
                },
              ),
          ],
        );
      case 2:
        return Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(12),
              child: TextField(
                decoration: const InputDecoration(
                  hintText: 'بحث بالاسم أو الكود',
                  prefixIcon: Icon(Icons.search),
                ),
                onChanged: (v) => setState(() => _productQuery = v),
              ),
            ),
            Expanded(
              child: ListView.builder(
                itemCount: _filtered.length,
                itemBuilder: (_, i) {
                  final p = _filtered[i];
                  final q = _qty[p.id] ?? 0;
                  return Card(
                    child: ListTile(
                      title: Text(p.name),
                      subtitle: Text('بيع: ${p.price?.toStringAsFixed(0) ?? '-'} | قسط: ${p.dailyInstallment?.toStringAsFixed(0) ?? '-'}'),
                      trailing: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          IconButton(
                            onPressed: q <= 0 ? null : () => setState(() {
                              _qty[p.id] = q - 1;
                              _total.clear();
                              _dailyInstallment.clear();
                              _applyDefaultPrices();
                            }),
                            icon: const Icon(Icons.remove_circle_outline),
                          ),
                          Text('$q'),
                          IconButton(
                            onPressed: () => setState(() {
                              _qty[p.id] = q + 1;
                              _total.clear();
                              _dailyInstallment.clear();
                              _applyDefaultPrices();
                            }),
                            icon: const Icon(Icons.add_circle_outline),
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
            ),
          ],
        );
      case 3:
        return ListView(
          padding: const EdgeInsets.all(16),
          children: [
            if (_maxSell != null)
              Text('الحد الأقصى من الطلب: ${_maxSell!.toStringAsFixed(0)}', style: const TextStyle(color: AppColors.warning)),
            const SizedBox(height: 8),
            _field(_total, 'إجمالي البيع *', number: true),
            _field(_dailyInstallment, 'القسط اليومي *', number: true),
            _field(_prepayment, 'المقدمة (افتراضي إن فُرغ)', number: true),
            TextButton(
              onPressed: () => setState(_applyDefaultPrices),
              child: const Text('اعتماد الافتراضي من المنتجات'),
            ),
          ],
        );
      default:
        final p = _payload();
        return ListView(
          padding: const EdgeInsets.all(16),
          children: [
            const Text('مراجعة قبل الإقرار', style: TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
            const SizedBox(height: 10),
            Text('المحل: ${p['shopName']} — ${p['shopType']}'),
            Text('المساحة: ${p['shopLength']} × ${p['shopWidth']} | GPS: ${p['lat']}, ${p['lng']}'),
            Text('الزبون: ${p['fullName']} | ${p['phone']} | ${p['province']}'),
            Text('القائمة/المندوب: ${p['orderListName']}'),
            Text('المنتجات: ${_selected.length}'),
            Text('الإجمالي: ${p['total']} | القسط: ${p['dailyInstallment']} | المقدمة: ${p['prepayment']}'),
            Text(
              'صورة محل: ${_shopPhoto != null ? 'نعم' : 'لا'} | '
              'مستندات بيع: ${_saleDocuments.length}/3 | '
              'مستمسكات: ${_kycDocs.length}',
            ),
            const SizedBox(height: 12),
            const Text(
              '«تم البيع» يغلق الطلب في صندوق تم البيع. الخروج من أي خطوة يحفظ التقدّم محلياً. «تم الكشف» يحفظ دون إغلاق.',
              style: TextStyle(color: AppColors.textSecondary, fontSize: 12.5),
            ),
          ],
        );
    }
  }

  Widget _field(TextEditingController c, String label, {bool number = false}) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: TextField(
        controller: c,
        keyboardType: number ? TextInputType.number : TextInputType.text,
        decoration: InputDecoration(labelText: label),
        onChanged: (_) {
          _markDirty();
        },
      ),
    );
  }
}
