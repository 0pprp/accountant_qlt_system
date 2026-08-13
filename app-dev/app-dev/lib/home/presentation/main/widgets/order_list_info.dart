import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/order/domain/order_list/order_list.dart';

class OrderListInfo extends StatelessWidget {
  final OrderList orderList;
  final VoidCallback? onTap;
  const OrderListInfo({super.key, required this.orderList, this.onTap});

  double get _progress {
    if (orderList.totalOrderCount == 0) return 0;
    return orderList.todayCollectedOrderCount / orderList.totalOrderCount;
  }

  String _formatAmount(double value) => NumberFormat('#,###', 'en').format(value);

  @override
  Widget build(BuildContext context) {
    return Material(
      color: AppColor.surface2,
      borderRadius: BorderRadius.circular(24),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(24),
        child: Container(
          decoration: BoxDecoration(
            border: Border.all(color: AppColor.stroke, width: 2),
            borderRadius: BorderRadius.circular(24),
          ),
          padding: const EdgeInsets.symmetric(horizontal: 24),
          child: Center(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // Title
                Text(
                  orderList.name,
                  textAlign: TextAlign.center,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: AppTextStyle.headlineMedium,
                ),
                const SizedBox(height: 24),

                // Donut chart (collected / total)
                Center(
                  child: SizedBox(
                    width: 100,
                    height: 100,
                    child: CircularProgressIndicator(
                      value: _progress,
                      color: AppColor.primary,
                      backgroundColor: AppColor.surface4,
                      strokeWidth: 16,
                      strokeCap: StrokeCap.round,
                    ),
                  ),
                ),
                const SizedBox(height: 24),

                // Legend
                _LegendItem(
                  dotColor: AppColor.surface4,
                  dotBorderColor: AppColor.text4,
                  label: 'العملاء',
                  value: _CountValue(orderList.totalOrderCount),
                ),
                const SizedBox(height: 8),
                _LegendItem(
                  dotColor: AppColor.primary,
                  dotBorderColor: AppColor.surface1,
                  label: 'مسددين',
                  value: _CountValue(orderList.todayCollectedOrderCount),
                ),
                const SizedBox(height: 8),
                _LegendItem(
                  label: 'المبيعات',
                  value: Text(
                    '${_formatAmount(orderList.totalSellAmount)} د.ع',
                    style: AppTextStyle.numberSmall.withColor(AppColor.primary),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _LegendItem extends StatelessWidget {
  final Color? dotColor;
  final Color? dotBorderColor;
  final String label;
  final Widget value;

  const _LegendItem({
    this.dotColor,
    this.dotBorderColor,
    required this.label,
    required this.value,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Flexible(
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              if (dotColor != null) ...[
                _Dot(color: dotColor!, borderColor: dotBorderColor ?? AppColor.text4),
                const SizedBox(width: 4),
              ],
              Flexible(
                child: Text(
                  label,
                  overflow: TextOverflow.ellipsis,
                  style: AppTextStyle.labelMedium.withColor(AppColor.text2),
                ),
              ),
            ],
          ),
        ),
        const SizedBox(width: 8),
        value,
      ],
    );
  }
}

/// Number + unit ("40 عميل") with the count emphasized and the unit muted.
class _CountValue extends StatelessWidget {
  final int count;
  const _CountValue(this.count);

  @override
  Widget build(BuildContext context) {
    return Text.rich(
      TextSpan(
        children: [
          TextSpan(
            text: '$count ',
            style: AppTextStyle.numberSmall,
          ),
          TextSpan(
            text: 'عميل',
            style: AppTextStyle.labelLarge.withColor(AppColor.text2),
          ),
        ],
      ),
    );
  }
}

class _Dot extends StatelessWidget {
  final Color color;
  final Color borderColor;
  const _Dot({required this.color, required this.borderColor});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 10,
      height: 10,
      decoration: ShapeDecoration(
        color: color,
        shape: OvalBorder(
          side: BorderSide(
            width: 0.30,
            strokeAlign: BorderSide.strokeAlignOutside,
            color: borderColor,
          ),
        ),
      ),
    );
  }
}
