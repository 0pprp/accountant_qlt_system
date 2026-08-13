import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/utils/helpers/text_util.dart';
import 'package:team/customer/domain/customer_financial_overview.dart';
import 'package:team/home/presentation/home/widgets/report_row.dart';
import 'package:vector_graphics/vector_graphics.dart';

class CustomerFinancialOverviewWidget extends StatelessWidget {
  final CustomerFinancialOverview? customerFinancialOverview;
  const CustomerFinancialOverviewWidget({super.key, required this.customerFinancialOverview});

  @override
  Widget build(BuildContext context) {
    return Animate(
      effects: [
        if (customerFinancialOverview == null)
          ShimmerEffect(
            colors: [Color(0xFFEBEBF4), Color(0xFFF4F4F4), Color(0xFFEBEBF4)],
            stops: [0.1, 0.3, 0.4],
            blendMode: BlendMode.srcATop,
            duration: 2000.ms,
          ),
      ],
      onComplete: (controller) => customerFinancialOverview != null ? null : controller.repeat(),
      child: Container(
        margin: EdgeInsets.only(bottom: 16),
        decoration: BoxDecoration(
          color: Colors.white,
          border: Border.all(color: customerFinancialOverview == null ? Colors.white : AppColor.stroke, width: 2),
          borderRadius: BorderRadius.circular(24),
        ),
        child: Visibility(
          maintainSize: true,
          maintainState: true,
          maintainAnimation: true,
          visible: customerFinancialOverview != null,
          child: Column(
            children: [
              Stack(
                children: [
                  Positioned.fill(
                    child: Container(
                      decoration: BoxDecoration(
                        color: AppColor.surface3,
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: Image.asset('assets/images/card_background.png', fit: BoxFit.fitHeight),
                    ),
                  ),
                  Positioned(
                    height: MediaQuery.sizeOf(context).height,
                    child: VectorGraphic(
                      loader: AssetBytesLoader('assets/svg/lines_bottom_right.svg'),
                      fit: BoxFit.fill,
                      colorFilter: ColorFilter.mode(
                        Color(0xffCCF5F2).withValues(alpha: 0.75),
                        BlendMode.srcIn,
                      ),
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Padding(
                          padding: const EdgeInsets.only(right: 8),
                          child: Text(
                            "التقرير المالي للزبون",
                            style: AppTextStyle.headlineMedium.withColor(AppColor.text2),
                          ),
                        ),
                        const SizedBox(height: 16),
                        ReportRow(
                          "المبيعات",
                          "مجموع سعر المبيعات",
                          TextUtil.addComma((customerFinancialOverview?.totalSellAmount ?? '').toString()),
                        ),
                        const SizedBox(height: 8),
                        ReportRow(
                          "الأقساط اليومية",
                          "مجموع الأقساط اليومية",
                          TextUtil.addComma(
                            (customerFinancialOverview?.totalDailyInstallmentAmount ?? '').toString(),
                          ),
                        ),
                        const SizedBox(height: 8),
                        ReportRow(
                          "المتأخرات",
                          "مجموع الأقساط المتأخرة",
                          TextUtil.addComma((customerFinancialOverview?.totalOverdueAmount ?? '').toString()),
                        ),
                        const SizedBox(height: 8),
                        ReportRow(
                          "التسديد",
                          "مجموع الأموال المستلمة",
                          TextUtil.addComma((customerFinancialOverview?.totalPaidAmount ?? '').toString()),
                        ),
                        const SizedBox(height: 8),
                        ReportRow(
                          "الباقي",
                          "مجموع الأموال المتبقية",
                          TextUtil.addComma((customerFinancialOverview?.totalRemainingAmount ?? '').toString()),
                        ),
                        const SizedBox(height: 8),
                        ReportRow(
                          "الطلبات النشطة",
                          "عدد الطلبات النشطة",
                          (customerFinancialOverview?.activeOrdersCount ?? '').toString(),
                        ),
                        const SizedBox(height: 8),
                        ReportRow(
                          "الطلبات المكتملة",
                          "عدد الطلبات المكتملة",
                          (customerFinancialOverview?.completedOrdersCount ?? '').toString(),
                        ),
                      ],
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
