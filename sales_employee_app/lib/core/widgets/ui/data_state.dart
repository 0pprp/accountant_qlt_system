import 'package:flutter/material.dart';

import '../../theme/app_colors.dart';

enum DataStateKind { loading, empty, error, forbidden }

class DataStateView extends StatelessWidget {
  const DataStateView({
    super.key,
    required this.kind,
    required this.title,
    this.message,
    this.onRetry,
  });

  final DataStateKind kind;
  final String title;
  final String? message;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) {
    final isBad = kind == DataStateKind.error || kind == DataStateKind.forbidden;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: isBad ? const Color(0xFFFCEBEA) : AppColors.surfaceSoft,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        children: [
          if (kind == DataStateKind.loading)
            const Padding(
              padding: EdgeInsets.only(bottom: 12),
              child: CircularProgressIndicator(color: AppColors.primary),
            ),
          Text(
            title,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w700,
              color: isBad ? AppColors.danger : AppColors.foreground,
            ),
          ),
          if (message != null) ...[
            const SizedBox(height: 8),
            Text(
              message!,
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 16,
                height: 1.6,
                color: isBad ? AppColors.danger : AppColors.muted,
              ),
            ),
          ],
          if (onRetry != null) ...[
            const SizedBox(height: 16),
            FilledButton(
              style: FilledButton.styleFrom(minimumSize: const Size(44, 44)),
              onPressed: onRetry,
              child: const Text('إعادة المحاولة'),
            ),
          ],
        ],
      ),
    );
  }
}
