import 'package:flutter/material.dart';

import '../theme/app_theme.dart';
import 'neu.dart';

abstract final class AppFeedback {
  static void snack(BuildContext context, String message, {bool error = false}) {
    final messenger = ScaffoldMessenger.maybeOf(context);
    if (messenger == null) return;
    messenger
      ..hideCurrentSnackBar()
      ..showSnackBar(
        SnackBar(
          content: Text(message),
          backgroundColor: error ? AppColors.danger : AppColors.ink,
        ),
      );
  }

  static Future<T?> dialog<T>(
    BuildContext context, {
    required String title,
    String? message,
    Widget? body,
    IconData icon = Icons.info_outline_rounded,
    Color color = AppColors.primary,
    required List<DialogAction<T>> actions,
    bool dismissible = true,
  }) {
    return showDialog<T>(
      context: context,
      barrierDismissible: dismissible,
      builder: (ctx) => Dialog(
        backgroundColor: AppColors.background,
        insetPadding: const EdgeInsets.symmetric(horizontal: 28),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 24, 20, 18),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                width: 64,
                height: 64,
                decoration: BoxDecoration(
                  color: color.withValues(alpha: 0.12),
                  shape: BoxShape.circle,
                ),
                child: Icon(icon, color: color, size: 34),
              ),
              const SizedBox(height: 14),
              Text(
                title,
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
              ),
              if (message != null) ...[
                const SizedBox(height: 8),
                Text(
                  message,
                  textAlign: TextAlign.center,
                  style: const TextStyle(color: AppColors.muted, fontSize: 14),
                ),
              ],
              if (body != null) ...[const SizedBox(height: 12), body],
              const SizedBox(height: 20),
              Row(
                children: [
                  for (var i = 0; i < actions.length; i++) ...[
                    if (i > 0) const SizedBox(width: 12),
                    Expanded(
                      child: NeuButton(
                        label: actions[i].label,
                        primary: actions[i].primary,
                        danger: actions[i].danger,
                        height: 48,
                        onPressed: () => Navigator.of(ctx).pop(actions[i].value),
                      ),
                    ),
                  ],
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  static Future<bool> confirm(
    BuildContext context, {
    required String title,
    String? message,
    String confirmLabel = 'تأكيد',
    bool danger = false,
    IconData icon = Icons.help_outline_rounded,
  }) async {
    final r = await dialog<bool>(
      context,
      title: title,
      message: message,
      icon: icon,
      color: danger ? AppColors.danger : AppColors.primary,
      actions: [
        DialogAction(label: confirmLabel, value: true, danger: danger),
        const DialogAction(label: 'إلغاء', value: false, primary: false),
      ],
    );
    return r == true;
  }

  static Future<void> error(BuildContext context, String message) => dialog<void>(
    context,
    title: 'تعذر إكمال العملية',
    message: message,
    icon: Icons.error_outline_rounded,
    color: AppColors.danger,
    actions: const [DialogAction(label: 'حسناً', value: null)],
  );
}

class DialogAction<T> {
  const DialogAction({
    required this.label,
    required this.value,
    this.primary = true,
    this.danger = false,
  });

  final String label;
  final T? value;
  final bool primary;
  final bool danger;
}

class StateView extends StatelessWidget {
  const StateView({
    super.key,
    required this.icon,
    required this.message,
    this.actionLabel,
    this.onAction,
  });

  final IconData icon;
  final String message;
  final String? actionLabel;
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(28),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 48, color: AppColors.muted),
            const SizedBox(height: 12),
            Text(
              message,
              textAlign: TextAlign.center,
              style: const TextStyle(color: AppColors.muted, fontSize: 15),
            ),
            if (actionLabel != null && onAction != null) ...[
              const SizedBox(height: 16),
              SizedBox(
                width: 180,
                child: NeuButton(
                  label: actionLabel!,
                  onPressed: onAction,
                  primary: false,
                  height: 46,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
