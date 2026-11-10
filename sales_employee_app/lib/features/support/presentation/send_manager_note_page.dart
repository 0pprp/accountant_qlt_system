import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/injection.dart';

/// ملاحظة إلى صندوق AppNotes — ليست شكوى.
/// المصدر على السيرفر: SalesEmployee عند تكليف نشط (لا تصنيف مندوب جباية بمجرد Mandob).
class SendManagerNotePage extends StatefulWidget {
  const SendManagerNotePage({super.key});

  @override
  State<SendManagerNotePage> createState() => _SendManagerNotePageState();
}

class _SendManagerNotePageState extends State<SendManagerNotePage> {
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
        message: 'اكتب ملاحظة بطول 10 أحرف على الأقل.',
      );
      return;
    }
    setState(() => _sending = true);
    try {
      await Injection.dioClient.dio.post(
        'api/v1/app/AppNotes',
        data: {'body': text, 'subject': 'ملاحظة موظف مبيعات'},
      );
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم',
        message: 'أُرسلت الملاحظة إلى الإدارة.',
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
      appBar: AppBar(title: const Text('ملاحظة المدير')),
      body: SafeArea(
        child: Padding(
          padding: EdgeInsets.fromLTRB(16, 16, 16, 16 + bottom),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'ملاحظة تشغيلية إلى صندوق ملاحظات التطبيقات. ليست شكوى ولا تُخلط معها.',
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
                    hintText: 'اكتب الملاحظة…',
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
                child: Text(_sending ? 'جاري الإرسال…' : 'إرسال'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
