import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/time/baghdad_time.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/catalog/domain/entities/product_item.dart';
import 'package:sales_employee_app/features/customers/domain/entities/customer_lite.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';
import 'package:sales_employee_app/features/sales/presentation/sale_success_dialog.dart';
import 'package:sales_employee_app/injection.dart';

/// Legacy free-form wizard — blocked without a linked sales request (golden rule).
class SaleWizardPage extends StatefulWidget {
  const SaleWizardPage({super.key, this.linkedOrderId});

  final int? linkedOrderId;

  @override
  State<SaleWizardPage> createState() => _SaleWizardPageState();
}

class _SaleWizardPageState extends State<SaleWizardPage> {
  int _step = 0;
  CustomerLite? _customer;
  final List<ProductItem> _products = [];
  final Map<int, int> _qty = {};
  final List<XFile> _docs = [];
  final _addressCtrl = TextEditingController();
  bool _busy = false;
  bool _blocked = false;

  List<ProductItem> get _selected =>
      _products.where((p) => (_qty[p.id] ?? 0) > 0).toList();

  @override
  void initState() {
    super.initState();
    if (!SalesWorkflowRules.hasLinkedOrder(widget.linkedOrderId)) {
      _blocked = true;
      WidgetsBinding.instance.addPostFrameCallback((_) async {
        if (!mounted) return;
        await showAppMessage(
          context,
          title: 'مرفوض',
          message: 'لا بيع بدون طلب مبيع مرتبط بالموظف. افتح الطلب من طلبات البيع أو المعلقة.',
        );
        if (mounted) Navigator.of(context).maybePop();
      });
    }
  }

  @override
  void dispose() {
    _addressCtrl.dispose();
    super.dispose();
  }

  Future<void> _pickCustomer() async {
    final q = TextEditingController();
    List<CustomerLite> results = [];
    await showCenteredDialog(
      context: context,
      child: StatefulBuilder(
        builder: (ctx, setLocal) {
          return SizedBox(
            width: 360,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Text('اختيار زبون', style: TextStyle(fontWeight: FontWeight.w700, fontSize: 17)),
                const SizedBox(height: 10),
                TextField(
                  controller: q,
                  decoration: const InputDecoration(
                    hintText: 'بحث بالاسم أو الهاتف',
                    prefixIcon: Icon(Icons.search),
                  ),
                  onSubmitted: (_) async {
                    results = await Injection.customersRemote.search(query: q.text);
                    setLocal(() {});
                  },
                ),
                const SizedBox(height: 8),
                SizedBox(
                  height: 220,
                  child: results.isEmpty
                      ? const Center(child: Text('لا نتائج', style: TextStyle(color: AppColors.textSecondary)))
                      : ListView.builder(
                          itemCount: results.length,
                          itemBuilder: (_, i) {
                            final c = results[i];
                            return ListTile(
                              title: Text(c.fullName),
                              subtitle: Text(c.phone ?? ''),
                              onTap: () {
                                setState(() => _customer = c);
                                Navigator.pop(ctx);
                              },
                            );
                          },
                        ),
                ),
                TextButton(
                  onPressed: () async {
                    results = await Injection.customersRemote.search(query: q.text);
                    setLocal(() {});
                  },
                  child: const Text('بحث'),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Future<void> _loadProducts() async {
    final list = await Injection.catalogRemote.getProducts();
    setState(() {
      _products
        ..clear()
        ..addAll(list);
    });
  }

  Future<void> _pickDoc() async {
    final picker = ImagePicker();
    final file = await picker.pickImage(source: ImageSource.gallery, imageQuality: 85);
    if (file != null) setState(() => _docs.add(file));
  }

  Future<void> _submit() async {
    if (_blocked || !SalesWorkflowRules.hasLinkedOrder(widget.linkedOrderId)) {
      await showAppMessage(context, title: 'مرفوض', message: 'لا بيع بدون طلب مبيع مرتبط');
      return;
    }
    if (_customer == null || _selected.isEmpty) return;
    setState(() => _busy = true);
    try {
      // Must use existing linked request — never create a free-floating sale order.
      final orderId = widget.linkedOrderId!;
      final now = baghdadNow();
      final date =
          '${now.year.toString().padLeft(4, '0')}-${now.month.toString().padLeft(2, '0')}-${now.day.toString().padLeft(2, '0')}';
      final time =
          '${now.hour.toString().padLeft(2, '0')}:${now.minute.toString().padLeft(2, '0')}:00';
      final point = await Injection.locationService.current();
      await Injection.salesRemote.setSellerInfo(
        orderId: orderId,
        creationAddress: _addressCtrl.text.trim().isEmpty ? '—' : _addressCtrl.text.trim(),
        lat: point?.lat,
        lng: point?.lng,
        saleDate: date,
        saleTime: time,
      );
      for (final doc in _docs) {
        final bytes = await doc.readAsBytes();
        await Injection.salesRemote.uploadAttachment(
          orderId: orderId,
          fileName: doc.name.isEmpty ? 'doc.jpg' : doc.name,
          bytes: bytes,
          attachmentType: '0',
        );
      }
      if (_docs.isNotEmpty) {
        await Injection.salesRemote.completeAttachments(orderId);
      }
      await Injection.salesRemote.transfer(id: orderId, targetStatus: SalesWorkflowRules.sold);
      if (!mounted) return;
      await showSaleSuccessDialog(context, orderId: orderId);
      if (!mounted) return;
      Navigator.of(context).pop(true);
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'فشل إتمام البيع',
        message: DioClient.mapError(e),
        icon: Icons.error_outline,
        color: AppColors.danger,
      );
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_blocked) {
      return Scaffold(
        appBar: AppBar(title: const Text('معالج البيع')),
        body: const Center(child: Text('لا بيع بدون طلب مبيع مرتبط')),
      );
    }
    return Scaffold(
      appBar: AppBar(title: Text('معالج البيع — خطوة ${_step + 1}/4')),
      body: Column(
        children: [
          LinearProgressIndicator(value: (_step + 1) / 4, color: AppColors.primary),
          Expanded(child: _buildStep()),
          Padding(
            padding: EdgeInsets.fromLTRB(16, 8, 16, 16 + MediaQuery.paddingOf(context).bottom),
            child: Row(
              children: [
                if (_step > 0)
                  Expanded(
                    child: OutlinedButton(
                      onPressed: _busy ? null : () => setState(() => _step--),
                      child: const Text('السابق'),
                    ),
                  ),
                if (_step > 0) const SizedBox(width: 10),
                Expanded(
                  child: ElevatedButton(
                    onPressed: _busy
                        ? null
                        : () async {
                            if (_step == 0 && _customer == null) {
                              await showAppMessage(context, title: 'تنبيه', message: 'اختر زبوناً أولاً');
                              return;
                            }
                            if (_step == 1) {
                              if (_products.isEmpty) await _loadProducts();
                              if (_selected.isEmpty && _step == 1) {
                                // allow staying to select
                              }
                            }
                            if (_step == 1 && _selected.isEmpty) {
                              await showAppMessage(context, title: 'تنبيه', message: 'اختر منتجاً واحداً على الأقل');
                              return;
                            }
                            if (_step < 3) {
                              if (_step == 0) await _loadProducts();
                              setState(() => _step++);
                            } else {
                              await _submit();
                            }
                          },
                    child: _busy
                        ? const SizedBox(
                            width: 22,
                            height: 22,
                            child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                          )
                        : Text(_step == 3 ? 'إتمام البيع' : 'التالي'),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStep() {
    switch (_step) {
      case 0:
        return ListView(
          padding: const EdgeInsets.all(20),
          children: [
            ListTile(
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
                side: const BorderSide(color: AppColors.border),
              ),
              title: Text(_customer?.fullName ?? 'لم يتم اختيار زبون'),
              subtitle: Text(_customer?.phone ?? 'اضغط للاختيار'),
              trailing: const Icon(Icons.search),
              onTap: _pickCustomer,
            ),
          ],
        );
      case 1:
        return ListView.builder(
          padding: const EdgeInsets.all(12),
          itemCount: _products.length,
          itemBuilder: (_, i) {
            final p = _products[i];
            final q = _qty[p.id] ?? 0;
            return Card(
              child: ListTile(
                title: Text(p.name),
                subtitle: Text(
                  'السعر: ${p.price?.toStringAsFixed(0) ?? '-'} | قسط يومي: ${p.dailyInstallment?.toStringAsFixed(0) ?? '-'}',
                ),
                trailing: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    IconButton(
                      onPressed: q <= 0
                          ? null
                          : () => setState(() => _qty[p.id] = q - 1),
                      icon: const Icon(Icons.remove_circle_outline),
                    ),
                    Text('$q'),
                    IconButton(
                      onPressed: () => setState(() => _qty[p.id] = q + 1),
                      icon: const Icon(Icons.add_circle_outline),
                    ),
                  ],
                ),
              ),
            );
          },
        );
      case 2:
        return ListView(
          padding: const EdgeInsets.all(20),
          children: [
            TextField(
              controller: _addressCtrl,
              decoration: const InputDecoration(
                labelText: 'عنوان المحل / الموقع',
                prefixIcon: Icon(Icons.place_outlined),
              ),
            ),
            const SizedBox(height: 14),
            OutlinedButton.icon(
              onPressed: _pickDoc,
              icon: const Icon(Icons.attach_file),
              label: const Text('رفع مستندات / صورة المحل'),
            ),
            const SizedBox(height: 8),
            const Text(
              'أرفق الهوية والمستندات وصورة المحل ثم راجع في الخطوة التالية.',
              style: TextStyle(color: AppColors.textSecondary, fontSize: 12.5),
            ),
            const SizedBox(height: 8),
            ..._docs.map(
              (d) => ListTile(
                dense: true,
                title: Text(d.name),
                leading: const Icon(Icons.image_outlined),
              ),
            ),
          ],
        );
      default:
        return ListView(
          padding: const EdgeInsets.all(20),
          children: [
            Text('الزبون: ${_customer?.fullName ?? '-'}', style: const TextStyle(fontWeight: FontWeight.w700)),
            const SizedBox(height: 8),
            Text('المنتجات: ${_selected.length}'),
            Text('المستندات: ${_docs.length}'),
            Text('العنوان: ${_addressCtrl.text.trim().isEmpty ? '—' : _addressCtrl.text.trim()}'),
            const SizedBox(height: 12),
            const Text(
              'راجع البيانات ثم اضغط إتمام البيع',
              style: TextStyle(color: AppColors.textSecondary),
            ),
          ],
        );
    }
  }
}
