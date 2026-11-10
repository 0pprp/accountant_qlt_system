import 'dart:math';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import '../../../injection.dart';
import '../../../core/widgets/centered_dialog.dart';
import '../data/sales_request_submission.dart';

class NewSalesRequestPage extends StatefulWidget {
  const NewSalesRequestPage({super.key, this.submission});
  final SalesRequestSubmission? submission;
  @override
  State<NewSalesRequestPage> createState() => _NewSalesRequestPageState();
}

class _NewSalesRequestPageState extends State<NewSalesRequestPage> {
  final _form = GlobalKey<FormState>();
  final _name = TextEditingController();
  final _phone = TextEditingController();
  final _address = TextEditingController();
  final _product = TextEditingController();
  final _note = TextEditingController();
  late final _remote = widget.submission ?? SalesRequestSubmission(Injection.dio.dio, 'Follower');
  List<Map<String, dynamic>> _branches = [];
  int? _branch;
  bool _loading = true, _busy = false, _finished = false;
  String? _error;
  Map<String, dynamic>? _attempt;

  @override
  void initState() { super.initState(); _load(); }
  @override
  void dispose() {
    for (final c in [_name, _phone, _address, _product, _note]) { c.dispose(); }
    super.dispose();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    try {
      final branches = await _remote.branches();
      if (!mounted) return;
      setState(() {
        _branches = branches;
        if (branches.length == 1) _branch = (branches.first['id'] as num).toInt();
        if (branches.isEmpty) _error = 'لا يوجد فرع متاح للإرسال. راجع الإدارة.';
      });
    } catch (_) {
      if (mounted) setState(() => _error = 'تعذر تحميل صلاحية الإرسال والفروع. تحقق من الاتصال أو راجع الإدارة.');
    } finally { if (mounted) setState(() => _loading = false); }
  }

  String _key() {
    final r = Random.secure();
    final bytes = List<int>.generate(16, (_) => r.nextInt(256));
    bytes[6] = (bytes[6] & 15) | 64;
    bytes[8] = (bytes[8] & 63) | 128;
    final h = bytes.map((b) => b.toRadixString(16).padLeft(2, '0')).join();
    return '${h.substring(0,8)}-${h.substring(8,12)}-${h.substring(12,16)}-${h.substring(16,20)}-${h.substring(20)}';
  }

  Future<void> _submit() async {
    if (_busy) return;
    if (_attempt == null && !(_form.currentState?.validate() ?? false)) return;
    if (_branch == null) return;
    _attempt ??= {
      'submissionKey': _key(), 'branchId': _branch,
      'customerName': _name.text.trim(), 'customerPhone': SalesRequestSubmission.phone(_phone.text),
      'address': _address.text.trim(), 'requestedProduct': _product.text.trim(),
      'note': _note.text.trim().isEmpty ? null : _note.text.trim(),
    };
    setState(() { _busy = true; _error = null; });
    try {
      final id = await _remote.submit(_attempt!);
      if (!mounted) return;
      setState(() { _finished = true; _busy = false; });
      await _message('تم إرسال الطلب للتدقيق', 'رقم الطلب #$id\nستراجعه إدارة المبيعات قبل الإسناد.');
      if (mounted) Navigator.of(context).pop(true);
    } on DioException catch (e) {
      if (!mounted) return;
      final status = e.response?.statusCode;
      if (status != null && status >= 400 && status < 500 && status != 409) _attempt = null;
      setState(() => _error = status == 403
          ? 'لا تملك صلاحية الإرسال من هذا التطبيق أو الفرع.'
          : status == 400 ? 'تحقق من الحقول ورقم الهاتف ثم أعد الإرسال.'
          : 'لم نتأكد من وصول الطلب. اضغط إعادة المحاولة للتحقق دون تكراره.');
    } catch (_) {
      if (mounted) setState(() => _error = 'تعذر تأكيد الإرسال. أعد المحاولة دون تغيير البيانات.');
    } finally { if (mounted) setState(() => _busy = false); }
  }

  Future<void> _message(String title, String message) async {
    await showAppMessage(context, title: title, message: message);
  }

  Future<void> _leave() async {
    if (_busy) return;
    if (!_finished && [_name, _phone, _address, _product, _note].any((c) => c.text.isNotEmpty)) {
      final leave = await showCenteredDialog<bool>(context: context, builder: (d) => Column(mainAxisSize: MainAxisSize.min, children: [const Text('مغادرة النموذج؟ لن تُحفظ المدخلات. إذا كان الإرسال غير مؤكد فتحقق منه قبل طلب جديد.'), TextButton(onPressed: () => Navigator.pop(d, false), child: const Text('متابعة')), TextButton(onPressed: () => Navigator.pop(d, true), child: const Text('مغادرة'))]));
      if (leave != true || !mounted) return;
    }
    if (mounted) { setState(() => _finished = true); Navigator.of(context).pop(); }
  }

  Widget _field(String label, TextEditingController controller, int max, {int lines = 1, bool phone = false, bool optional = false}) {
    return Padding(padding: const EdgeInsets.only(bottom: 20), child: TextFormField(
      controller: controller, enabled: !_busy && _attempt == null,
      maxLength: max, minLines: lines, maxLines: lines == 1 ? 1 : 5,
      style: const TextStyle(fontSize: 17, height: 1.5),
      keyboardType: phone ? TextInputType.phone : lines > 1 ? TextInputType.multiline : TextInputType.text,
      textDirection: phone ? TextDirection.ltr : TextDirection.rtl,
      decoration: InputDecoration(labelText: label, alignLabelWithHint: true,
        helperText: phone ? '11 رقمًا تبدأ بـ07' : null,
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(14))),
      validator: (v) {
        if (!optional && (v == null || v.trim().isEmpty)) return 'هذا الحقل مطلوب';
        if (phone && !SalesRequestSubmission.validPhone(v ?? '')) return 'أدخل 11 رقمًا تبدأ بـ07';
        return null;
      },
    ));
  }

  @override
  Widget build(BuildContext context) => PopScope(
    canPop: _finished || (!_busy && [_name, _phone, _address, _product, _note].every((c) => c.text.isEmpty)),
    onPopInvokedWithResult: (didPop, _) { if (!didPop) _leave(); },
    child: Directionality(textDirection: TextDirection.rtl, child: Scaffold(
      appBar: AppBar(title: const Text('طلب بيع جديد'), leading: IconButton(icon: const Icon(Icons.arrow_back), onPressed: _leave)),
      body: SafeArea(child: _loading ? const Center(child: CircularProgressIndicator()) : SingleChildScrollView(
        padding: const EdgeInsets.all(22), child: Center(child: ConstrainedBox(constraints: const BoxConstraints(maxWidth: 640),
        child: Form(key: _form, child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          const Text('إرسال إلى إدارة المبيعات', style: TextStyle(fontSize: 24, fontWeight: FontWeight.w700)),
          const SizedBox(height: 8),
          const Text('يدخل الطلب غير مسند لمطابقة بيانات الزبون والتدقيق قبل اختيار موظف المبيعات.', style: TextStyle(fontSize: 16, height: 1.7)),
          const SizedBox(height: 26),
          if (_branches.length > 1) Padding(padding: const EdgeInsets.only(bottom: 20), child: DropdownButtonFormField<int>(
            initialValue: _branch, isExpanded: true, decoration: const InputDecoration(labelText: 'الفرع'),
            items: _branches.map((b) => DropdownMenuItem(value: (b['id'] as num).toInt(), child: Text('${b['name']}'))).toList(),
            onChanged: _busy || _attempt != null ? null : (v) => setState(() => _branch = v),
            validator: (v) => v == null ? 'اختر الفرع' : null,
          )),
          _field('اسم الزبون', _name, 120),
          _field('رقم الهاتف', _phone, 11, phone: true),
          _field('العنوان', _address, 500, lines: 2),
          _field('المنتج المطلوب', _product, 200),
          _field('ملاحظة (اختياري)', _note, 1000, lines: 3, optional: true),
          if (_error != null) Padding(padding: const EdgeInsets.only(bottom: 16), child: Text(_error!, style: const TextStyle(color: Colors.red, fontSize: 16, height: 1.6))),
          if (_branches.isEmpty) OutlinedButton(onPressed: _load, child: const Text('إعادة تحميل الفروع')),
          FilledButton(onPressed: _busy || _branches.isEmpty ? null : _submit,
            style: FilledButton.styleFrom(minimumSize: const Size.fromHeight(54), backgroundColor: const Color(0xFF006A64)),
            child: _busy ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
              : Text(_attempt == null ? 'إرسال الطلب' : 'إعادة المحاولة', style: const TextStyle(fontSize: 18))),
          const SizedBox(height: 20),
        ]))))),
      ),
    )),
  );
}
