import 'package:flutter/material.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';

Future<T?> showCenteredDialog<T>({
  required BuildContext context,
  required Widget Function(BuildContext dialogContext) builder,
  bool barrierDismissible = true,
}) {
  return showDialog<T>(
    context: context,
    barrierDismissible: barrierDismissible,
    builder: (dialogContext) {
      final viewInsets = MediaQuery.viewInsetsOf(dialogContext);
      return Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 24, vertical: 24),
        child: AnimatedPadding(
          duration: const Duration(milliseconds: 180),
          curve: Curves.easeOut,
          padding: EdgeInsets.only(bottom: viewInsets.bottom > 0 ? 8 : 0),
          child: ConstrainedBox(
            constraints: BoxConstraints(
              maxHeight: MediaQuery.sizeOf(dialogContext).height * 0.78,
            ),
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: builder(dialogContext),
            ),
          ),
        ),
      );
    },
  );
}

Future<void> showAppMessage(
  BuildContext context, {
  required String title,
  required String message,
  IconData icon = Icons.info_outline,
  Color color = AppColors.primary,
}) {
  return showCenteredDialog<void>(
    context: context,
    builder: (dialogContext) => Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, color: color, size: 40),
        const SizedBox(height: 12),
        Text(title, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
        const SizedBox(height: 8),
        Text(message, textAlign: TextAlign.center, style: const TextStyle(color: AppColors.textSecondary)),
        const SizedBox(height: 16),
        SizedBox(
          width: double.infinity,
          child: ElevatedButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: const Text('حسناً'),
          ),
        ),
      ],
    ),
  );
}

Future<bool> showConfirmDialog(
  BuildContext context, {
  required String title,
  required String message,
}) async {
  final result = await showCenteredDialog<bool>(
    context: context,
    builder: (dialogContext) => Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(title, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
        const SizedBox(height: 8),
        Text(message, textAlign: TextAlign.center),
        const SizedBox(height: 16),
        Row(
          children: [
            Expanded(
              child: OutlinedButton(
                onPressed: () => Navigator.of(dialogContext).pop(false),
                child: const Text('إلغاء'),
              ),
            ),
            const SizedBox(width: 10),
            Expanded(
              child: ElevatedButton(
                onPressed: () => Navigator.of(dialogContext).pop(true),
                child: const Text('تأكيد'),
              ),
            ),
          ],
        ),
      ],
    ),
  );
  return result == true;
}

Future<String?> showCenteredTextPrompt(
  BuildContext context, {
  required String title,
  String hint = '',
  int maxLines = 4,
}) {
  return showDialog<String>(
    context: context,
    builder: (dialogContext) => _TextPromptDialog(
      title: title,
      hint: hint,
      maxLines: maxLines,
    ),
  );
}

class _TextPromptDialog extends StatefulWidget {
  const _TextPromptDialog({
    required this.title,
    required this.hint,
    required this.maxLines,
  });

  final String title;
  final String hint;
  final int maxLines;

  @override
  State<_TextPromptDialog> createState() => _TextPromptDialogState();
}

class _TextPromptDialogState extends State<_TextPromptDialog> {
  late final TextEditingController _ctrl;

  @override
  void initState() {
    super.initState();
    _ctrl = TextEditingController();
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final viewInsets = MediaQuery.viewInsetsOf(context);
    return Dialog(
      insetPadding: const EdgeInsets.symmetric(horizontal: 24, vertical: 24),
      child: AnimatedPadding(
        duration: const Duration(milliseconds: 180),
        curve: Curves.easeOut,
        padding: EdgeInsets.only(bottom: viewInsets.bottom > 0 ? 8 : 0),
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(widget.title, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
              const SizedBox(height: 10),
              TextField(
                controller: _ctrl,
                maxLines: widget.maxLines,
                autofocus: true,
                decoration: InputDecoration(hintText: widget.hint),
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton(
                      onPressed: () => Navigator.of(context).pop(),
                      child: const Text('إلغاء'),
                    ),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: ElevatedButton(
                      onPressed: () => Navigator.of(context).pop(_ctrl.text.trim()),
                      child: const Text('إرسال'),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
