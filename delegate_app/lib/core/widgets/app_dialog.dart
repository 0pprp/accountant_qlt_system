import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_text_styles.dart';

/// Centered system-style dialogs used across the app.
abstract final class AppDialog {
  static Future<T?> show<T>({
    required BuildContext context,
    required String title,
    required Widget child,
    List<Widget>? actions,
    bool barrierDismissible = true,
  }) {
    return showDialog<T>(
      context: context,
      barrierDismissible: barrierDismissible,
      builder: (ctx) => Dialog(
        backgroundColor: AppColors.surface,
        insetPadding: const EdgeInsets.symmetric(horizontal: 24, vertical: 24),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 20, 20, 12),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(title, style: AppTextStyles.title, textAlign: TextAlign.center),
              const SizedBox(height: 14),
              ConstrainedBox(
                constraints: const BoxConstraints(maxHeight: 420),
                child: SingleChildScrollView(child: child),
              ),
              if (actions != null && actions.isNotEmpty) ...[
                const SizedBox(height: 8),
                ...actions,
              ],
            ],
          ),
        ),
      ),
    );
  }

  static Future<void> message(
    BuildContext context, {
    required String title,
    required String body,
    String okLabel = 'حسناً',
  }) {
    return show<void>(
      context: context,
      title: title,
      child: Text(body, style: AppTextStyles.body, textAlign: TextAlign.center),
      actions: [
        FilledButton(
          onPressed: () => Navigator.pop(context),
          child: Text(okLabel),
        ),
      ],
    );
  }

  /// Blocking progress dialog. Caller must pop when done.
  static Future<void> showProgress(
    BuildContext context, {
    required ValueNotifier<SyncProgress> progress,
    String title = 'إرسال التسديدات',
  }) {
    return showDialog<void>(
      context: context,
      barrierDismissible: false,
      builder: (ctx) {
        return PopScope(
          canPop: false,
          child: Dialog(
            backgroundColor: AppColors.surface,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(20),
            ),
            child: Padding(
              padding: const EdgeInsets.fromLTRB(22, 24, 22, 22),
              child: ValueListenableBuilder<SyncProgress>(
                valueListenable: progress,
                builder: (_, p, _) {
                  final total = p.total <= 0 ? 1 : p.total;
                  final ratio = (p.done / total).clamp(0.0, 1.0);
                  return Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(title, style: AppTextStyles.title),
                      const SizedBox(height: 16),
                      const CircularProgressIndicator(color: AppColors.primary),
                      const SizedBox(height: 16),
                      Text(
                        p.total <= 0
                            ? 'جارٍ التحضير…'
                            : 'وصل ${p.done} من ${p.total} تسديد',
                        style: AppTextStyles.body.copyWith(
                          fontWeight: FontWeight.w700,
                          color: AppColors.primary,
                        ),
                      ),
                      const SizedBox(height: 12),
                      ClipRRect(
                        borderRadius: BorderRadius.circular(8),
                        child: LinearProgressIndicator(
                          value: p.total <= 0 ? null : ratio,
                          minHeight: 8,
                          backgroundColor: AppColors.surfaceMuted,
                          color: AppColors.primary,
                        ),
                      ),
                      if (p.message != null && p.message!.isNotEmpty) ...[
                        const SizedBox(height: 10),
                        Text(
                          p.message!,
                          style: AppTextStyles.muted,
                          textAlign: TextAlign.center,
                        ),
                      ],
                    ],
                  );
                },
              ),
            ),
          ),
        );
      },
    );
  }
}

class SyncProgress {
  const SyncProgress({
    required this.done,
    required this.total,
    this.message,
  });

  final int done;
  final int total;
  final String? message;
}
