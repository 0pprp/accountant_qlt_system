import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../ratings/domain/services/customer_rating_calculator.dart';

class HomeWeekRatingCard extends StatelessWidget {
  const HomeWeekRatingCard({
    super.key,
    required this.previous,
    required this.current,
    required this.trend,
    required this.installmentSum,
    required this.yesterdayIncome,
  });

  final double previous;
  final double current;
  final String trend;
  final double installmentSum;
  final double yesterdayIncome;

  IconData get _arrow {
    switch (trend) {
      case 'up':
        return Icons.arrow_upward_rounded;
      case 'down':
        return Icons.arrow_downward_rounded;
      default:
        return Icons.drag_handle_rounded;
    }
  }

  Color get _arrowColor {
    switch (trend) {
      case 'up':
        return AppColors.success;
      case 'down':
        return AppColors.danger;
      default:
        return AppColors.muted;
    }
  }

  @override
  Widget build(BuildContext context) {
    final money = NumberFormat('#,##0.##', 'en');
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.border),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.04),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        children: [
          Row(
            children: [
              Expanded(
                child: _metric(
                  'الأسبوع السابق',
                  CustomerRatingCalculator.formatAverage(previous),
                ),
              ),
              Icon(_arrow, color: _arrowColor, size: 28),
              Expanded(
                child: _metric(
                  'التقييم الحالي',
                  CustomerRatingCalculator.formatAverage(current),
                  emphasize: true,
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          const Divider(height: 1, color: AppColors.border),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: _metric(
                  'مجموع القسط',
                  money.format(installmentSum),
                ),
              ),
              Expanded(
                child: _metric(
                  'وارد أمس',
                  money.format(yesterdayIncome),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _metric(String label, String value, {bool emphasize = false}) {
    return Column(
      children: [
        Text(label, style: AppTextStyles.muted),
        const SizedBox(height: 4),
        Text(
          value,
          style: AppTextStyles.amount.copyWith(
            color: emphasize ? AppColors.primary : AppColors.foreground,
            fontSize: emphasize ? 22 : 18,
          ),
        ),
      ],
    );
  }
}
