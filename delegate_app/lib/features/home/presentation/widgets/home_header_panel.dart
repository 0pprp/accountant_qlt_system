import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../ratings/domain/services/customer_rating_calculator.dart';

class HomeHeaderPanel extends StatelessWidget {
  const HomeHeaderPanel({
    super.key,
    required this.delegateName,
    required this.listName,
    required this.rating,
    required this.onOpenMenu,
    this.avatarUrl,
  });

  final String delegateName;
  final String listName;
  final double rating;
  final VoidCallback onOpenMenu;
  final String? avatarUrl;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.fromLTRB(12, 8, 12, 16),
      decoration: const BoxDecoration(
        color: AppColors.primary,
        borderRadius: BorderRadius.vertical(bottom: Radius.circular(24)),
      ),
      child: SafeArea(
        bottom: false,
        child: Row(
          children: [
            IconButton(
              onPressed: onOpenMenu,
              icon: const Icon(Icons.menu_rounded, color: Colors.white),
              style: IconButton.styleFrom(
                backgroundColor: Colors.white.withValues(alpha: 0.15),
              ),
            ),
            const SizedBox(width: 8),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.16),
                borderRadius: BorderRadius.circular(20),
              ),
              child: Text(
                CustomerRatingCalculator.formatAverage(rating),
                style: AppTextStyles.body.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    delegateName,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: AppTextStyles.title.copyWith(
                      color: Colors.white,
                      fontSize: 18,
                    ),
                  ),
                  Text(
                    listName,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: AppTextStyles.muted.copyWith(
                      color: Colors.white.withValues(alpha: 0.85),
                    ),
                  ),
                ],
              ),
            ),
            CircleAvatar(
              radius: 26,
              backgroundColor: Colors.white.withValues(alpha: 0.2),
              backgroundImage:
                  avatarUrl != null ? NetworkImage(avatarUrl!) : null,
              child: avatarUrl == null
                  ? Text(
                      delegateName.isNotEmpty
                          ? String.fromCharCode(delegateName.runes.first)
                          : 'م',
                      style: AppTextStyles.title.copyWith(color: Colors.white),
                    )
                  : null,
            ),
          ],
        ),
      ),
    );
  }
}
