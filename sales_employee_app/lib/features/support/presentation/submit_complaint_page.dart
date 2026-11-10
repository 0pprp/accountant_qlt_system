import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/injection.dart';

/// شكوى موظف المبيعات → Complaints (مصدر sales-employee من السيرفر). ليست AppNote.
class SubmitComplaintPage extends StatefulWidget {
  const SubmitComplaintPage({super.key});

  @override
  State<SubmitComplaintPage> createState() => _SubmitComplaintPageState();
}

class _SubmitComplaintPageState extends State<SubmitComplaintPage> {
  final _body = TextEditingController();
  bool _sending = false;

  @override
  void dispose() {
    _body.dispose();
    super.dispose();
  }

  Future<void> _send() async {
    final text = _body.text.trim();
    if (text.length < 10) {
      await showAppMessage(
        context,
        title: 'تنبيه',
        message: 'اكتب الشكوى بطول 10 أحرف على الأقل.',
      );
      return;
    }
    setState(() => _sending = true);
    try {
      await Injection.dioClient.dio.post(
        'api/v1/app/Complaints',
        data: {'body': text, 'subject': 'شكوى موظف مبيعات'},
      );
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم',
        message: 'أُرسلت الشكوى إلى الإدارة.',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
      if (!mounted) return;
      Navigator.pop(context);
    } on DioException catch (e) {
      if (!mounted) return;
      final msg = e.response?.data is Map
          ? '${(e.response!.data as Map)['message'] ?? (e.response!.data as Map)['title'] ?? 'تعذر الإرسال'}'
          : DioClient.mapError(e);
      await showAppMessage(context, title: 'فشل الإرسال', message: msg);
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(context, title: 'فشل الإرسال', message: '$e');
    } finally {
      if (mounted) setState(() => _sending = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('شكوى')),
      body: SafeArea(
        child: Padding(
          padding: EdgeInsets.fromLTRB(16, 16, 16, 16 + bottom),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'شكوى تشغيلية من موظف المبيعات المكلّف. ليست ملاحظة مدير ولا تُخلط مع AppNotes.',
                style: TextStyle(fontSize: 14, color: AppColors.muted, height: 1.5),
              ),
              const SizedBox(height: 12),
              Expanded(
                child: TextField(
                  controller: _body,
                  maxLines: null,
                  expands: true,
                  textAlignVertical: TextAlignVertical.top,
                  decoration: InputDecoration(
                    hintText: 'اكتب الشكوى…',
                    filled: true,
                    fillColor: AppColors.surface,
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                  ),
                ),
              ),
              const SizedBox(height: 12),
              FilledButton(
                style: FilledButton.styleFrom(minimumSize: const Size(44, 48)),
                onPressed: _sending ? null : _send,
                child: Text(_sending ? 'جاري الإرسال…' : 'إرسال الشكوى'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
