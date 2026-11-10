import 'package:flutter/material.dart';

import '../../../../core/network/dio_client.dart';
import '../../../../core/widgets/centered_dialog.dart';

class DelegateNoteButton extends StatefulWidget {
  const DelegateNoteButton({
    super.key,
    required this.listName,
    required this.save,
  });
  final String listName;
  final Future<void> Function(String) save;
  @override
  State<DelegateNoteButton> createState() => _DelegateNoteButtonState();
}

class _DelegateNoteButtonState extends State<DelegateNoteButton> {
  bool _busy = false;
  Future<void> _send() async {
    if (_busy) return;
    setState(() => _busy = true);
    try {
      final text = await showCenteredTextPrompt(
        context,
        title: 'ملاحظة على مندوب ${widget.listName}',
        hint: 'نص الملاحظة',
        maxLines: 4,
      );
      if (text == null || text.trim().isEmpty || !mounted) return;
      await widget.save(text.trim());
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم الحفظ',
        message: 'حُفظت ملاحظة مندوب القائمة.',
      );
    } catch (e) {
      if (mounted)
        await showAppMessage(
          context,
          title: 'تعذر الحفظ',
          message: DioClient.mapError(e),
        );
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) => OutlinedButton.icon(
    onPressed: _busy ? null : _send,
    icon: const Icon(Icons.note_alt_outlined),
    label: Text(_busy ? 'جارٍ الحفظ…' : 'ملاحظة المندوب'),
  );
}
