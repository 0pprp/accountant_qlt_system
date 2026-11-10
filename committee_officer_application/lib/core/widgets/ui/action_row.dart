import 'package:flutter/material.dart';

import '../../theme/app_colors.dart';

class ActionRow extends StatelessWidget {
  const ActionRow({
    super.key,
    this.primaryLabel,
    this.onPrimary,
    this.secondary = const [],
  });

  final String? primaryLabel;
  final VoidCallback? onPrimary;
  final List<(String label, VoidCallback onTap)> secondary;

  @override
  Widget build(BuildContext context) {
    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: [
        if (primaryLabel != null)
          FilledButton(
            style: FilledButton.styleFrom(
              minimumSize: const Size(44, 44),
              backgroundColor: AppColors.primary,
              foregroundColor: AppColors.onPrimary,
            ),
            onPressed: onPrimary,
            child: Text(primaryLabel!),
          ),
        for (final item in secondary)
          OutlinedButton(
            style: OutlinedButton.styleFrom(
              minimumSize: const Size(44, 44),
              foregroundColor: AppColors.primary,
              side: const BorderSide(color: AppColors.border),
            ),
            onPressed: item.$2,
            child: Text(item.$1),
          ),
      ],
    );
  }
}
