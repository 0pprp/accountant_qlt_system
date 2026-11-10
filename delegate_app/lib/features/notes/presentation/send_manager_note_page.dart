import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/app_dialog.dart';
import '../../../injection.dart';
import '../data/app_notes_remote.dart';

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
      await AppDialog.message(
        context,
        title: 'تنبيه',
        body: 'اكتب ملاحظة بطول 10 أحرف على الأقل.',
      );
      return;
    }
    setState(() => _sending = true);
    try {
      await AppNotesRemote(Injection.dioClient).createManagerNote(text);
      if (!mounted) return;
      await AppDialog.message(
        context,
        title: 'تم',
        body: 'أُرسلت الملاحظة إلى الإدارة.',
      );
      if (!mounted) return;
      Navigator.pop(context);
    } on DioException catch (e) {
      if (!mounted) return;
      final msg = e.response?.data is Map
          ? '${(e.response!.data as Map)['message'] ?? (e.response!.data as Map)['title'] ?? 'تعذر الإرسال'}'
          : 'تعذر الإرسال';
      await AppDialog.message(context, title: 'فشل الإرسال', body: msg);
    } catch (_) {
      if (!mounted) return;
      await AppDialog.message(
        context,
        title: 'فشل الإرسال',
        body: 'تعذر إرسال الملاحظة.',
      );
    } finally {
      if (mounted) setState(() => _sending = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      appBar: AppBar(title: const Text('ملاحظة إلى الإدارة')),
      body: SafeArea(
        child: Padding(
          padding: EdgeInsets.fromLTRB(16, 16, 16, 16 + bottom),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                'ملاحظة تشغيلية تُرسل إلى صندوق ملاحظات التطبيقات في لوحة الإدارة.',
                style: AppTextStyles.muted,
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
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 12),
              FilledButton(
                onPressed: _sending ? null : _send,
                child: Text(_sending ? 'جارٍ الإرسال…' : 'إرسال'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
