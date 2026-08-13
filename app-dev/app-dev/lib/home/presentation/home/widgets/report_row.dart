import 'package:flutter/material.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';

class ReportRow extends StatelessWidget {
  final String title;
  final String subtitle;
  final String amount;
  const ReportRow(this.title, this.subtitle, this.amount, {super.key});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(vertical: 10, horizontal: 16),
      decoration: BoxDecoration(
        color: AppColor.surface2.withValues(alpha: 0.95),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: AppTextStyle.bodyMedium),
                Text(
                  subtitle,
                  overflow: TextOverflow.ellipsis,
                  maxLines: 1,
                  style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                ),
              ],
            ),
          ),
          Text(
            amount,
            style: TextStyle(color: AppColor.text1, fontSize: 16, fontFamily: 'Rubik', fontWeight: FontWeight.w400),
          ),
          SizedBox(width: 4),
          Text('د.ع', style: AppTextStyle.bodyLarge),
        ],
      ),
    );
  }
}
