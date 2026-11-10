import 'package:flutter/material.dart';

import '../../theme/app_colors.dart';
import 'status_badge.dart';

/// Presentational shell — no endpoints or permissions.
class EntityCard extends StatelessWidget {
  const EntityCard({
    super.key,
    required this.name,
    this.eyebrow,
    this.statusLabel,
    this.statusTone = StatusBadgeTone.neutral,
    this.selected = false,
    this.identityExtra,
    this.fields,
    this.notes,
    this.actions,
    this.onTap,
  });

  final String name;
  final String? eyebrow;
  final String? statusLabel;
  final StatusBadgeTone statusTone;
  final bool selected;
  final Widget? identityExtra;
  final Widget? fields;
  final Widget? notes;
  final Widget? actions;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: AppColors.card,
      borderRadius: BorderRadius.circular(16),
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: onTap,
        child: Container(
          width: double.infinity,
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: selected ? AppColors.primary : AppColors.border,
              width: selected ? 2 : 1,
            ),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        if (eyebrow != null)
                          Text(
                            eyebrow!,
                            style: const TextStyle(
                              color: AppColors.textSecondary,
                              fontSize: 14,
                            ),
                          ),
                        const SizedBox(height: 6),
                        Text(
                          name,
                          style: const TextStyle(
                            color: AppColors.foreground,
                            fontSize: 20,
                            fontWeight: FontWeight.w700,
                            height: 1.5,
                          ),
                        ),
                        if (identityExtra != null) identityExtra!,
                      ],
                    ),
                  ),
                  if (statusLabel != null) ...[
                    const SizedBox(width: 8),
                    StatusBadge(label: statusLabel!, tone: statusTone),
                  ],
                ],
              ),
              if (fields != null) ...[
                const SizedBox(height: 8),
                fields!,
              ],
              if (notes != null) ...[
                const SizedBox(height: 8),
                notes!,
              ],
              if (actions != null) ...[
                const SizedBox(height: 16),
                actions!,
              ],
            ],
          ),
        ),
      ),
    );
  }
}
