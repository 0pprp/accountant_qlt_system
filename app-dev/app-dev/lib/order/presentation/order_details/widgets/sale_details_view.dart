import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/utils/helpers/number_extentions.dart';
import 'package:team/common/utils/helpers/text_util.dart';
import 'package:team/order/domain/order_details/order_details.dart';
import 'package:team/order/presentation/order_details/widgets/donut_chart_widget.dart';
import 'package:vector_graphics/vector_graphics.dart';

String _getArabicDayName(DateTime date) {
  const arabicDays = [
    'الاثنين', // Monday
    'الثلاثاء', // Tuesday
    'الاربعاء', // Wednesday
    'الخميس', // Thursday
    'الجمعة', // Friday
    'السبت', // Saturday
    'الاحد', // Sunday
  ];
  // DateTime.weekday returns 1 for Monday, 7 for Sunday
  return arabicDays[date.weekday - 1];
}

class SalesDetailsView extends StatelessWidget {
  final OrderDetails? orderDetails;
  const SalesDetailsView({super.key, required this.orderDetails});

  @override
  Widget build(BuildContext context) {
    return Animate(
      effects: [
        if (orderDetails == null)
          ShimmerEffect(
            colors: [Color(0xFFEBEBF4), Color(0xFFF4F4F4), Color(0xFFEBEBF4)],
            stops: [0.1, 0.3, 0.4],
            blendMode: BlendMode.srcATop,
            duration: 2000.ms,
          ),
      ],
      onComplete: (controller) => orderDetails != null ? null : controller.repeat(),
      child: Container(
        padding: EdgeInsets.all(24),
        decoration: BoxDecoration(
          color: Colors.white,
          border: orderDetails != null ? Border.all(color: AppColor.stroke, width: 2) : null,
          borderRadius: BorderRadius.circular(24),
        ),
        child: Visibility(
          maintainSize: true,
          maintainState: true,
          maintainAnimation: true,
          visible: orderDetails != null,
          child: Column(
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('تفاصيل البيع', style: AppTextStyle.labelLarge.withColor(AppColor.text2)),
                  Padding(
                    padding: const EdgeInsets.only(left: 24),
                    child: VectorGraphic(loader: AssetBytesLoader('assets/svg/clipboard_icon.svg')),
                  ),
                ],
              ),
              if (orderDetails?.executionStatus != null || orderDetails?.approvalStatus != null)
                Padding(
                  padding: const EdgeInsets.only(top: 16),
                  child: Builder(
                    builder: (context) {
                      final isDraft = orderDetails?.executionStatus?.isDraft ?? false;
                      final showApproval =
                          orderDetails?.approvalStatus != null &&
                          !(isDraft && orderDetails?.approvalStatus == ApprovalStatus.pending);

                      return Row(
                        children: [
                          if (isDraft) ...[
                            Text('الحالة', style: AppTextStyle.labelMedium.withColor(AppColor.text2)),
                            const SizedBox(width: 8),
                            Text(
                              'لم يكتمل',
                              style: AppTextStyle.labelMedium.withColor(AppColor.primary),
                            ),
                          ] else if (orderDetails?.executionStatus != null) ...[
                            Text('حالة التنفيذ', style: AppTextStyle.labelMedium.withColor(AppColor.text2)),
                            const SizedBox(width: 8),
                            Text(
                              orderDetails!.executionStatus!.arabicLabel,
                              style: AppTextStyle.labelMedium.withColor(AppColor.primary),
                            ),
                          ],
                          if ((isDraft || orderDetails?.executionStatus != null) && showApproval)
                            const SizedBox(width: 16),
                          if (showApproval) ...[
                            Text('حالة الموافقة', style: AppTextStyle.labelMedium.withColor(AppColor.text2)),
                            const SizedBox(width: 8),
                            Text(
                              orderDetails!.approvalStatus!.arabicLabel,
                              style: AppTextStyle.labelMedium.withColor(AppColor.primary),
                            ),
                          ],
                        ],
                      );
                    },
                  ),
                ),
              Padding(
                padding: const EdgeInsets.only(top: 32, bottom: 24),
                child: Builder(
                  builder: (context) {
                    final double totalAmount = orderDetails?.sellAmount ?? 0;
                    final double paidAmount = orderDetails?.paidAmount ?? 0;
                    final double accumulatedAmount = orderDetails?.overdueAmount ?? 0;
                    final double dailyInstallment = orderDetails?.dailyInstallmentAmount ?? 0;
                    final double remainingAmount = orderDetails?.unpaidAmount ?? 0;
                    // final double buyAmount = orderDetails?.buyAmount ?? 0;
                    final double receivedPercentage = (paidAmount / totalAmount) * 100;
                    final double accumulatedPercentage = (accumulatedAmount / totalAmount) * 100;
                    return Row(
                      children: [
                        Expanded(
                          child: Column(
                            children: [
                              Row(
                                children: [
                                  Container(
                                    width: 10,
                                    height: 10,
                                    decoration: BoxDecoration(
                                      borderRadius: BorderRadius.circular(10),
                                      color: AppColor.surface4,
                                      border: Border.all(color: AppColor.stroke, width: 2),
                                    ),
                                  ),
                                  SizedBox(width: 4),
                                  Expanded(
                                    child: Text(
                                      'سعر البيع',
                                      overflow: TextOverflow.ellipsis,
                                      style: AppTextStyle.labelMedium,
                                    ),
                                  ),
                                  Text(
                                    totalAmount.toIraqiDinarString(),
                                    style: AppTextStyle.labelMedium.withColor(AppColor.primary),
                                  ),
                                ],
                              ),
                              Padding(
                                padding: const EdgeInsets.only(top: 8),
                                child: Row(
                                  children: [
                                    Container(
                                      width: 10,
                                      height: 10,
                                      decoration: BoxDecoration(
                                        borderRadius: BorderRadius.circular(10),
                                        color: AppColor.primary,
                                      ),
                                    ),
                                    SizedBox(width: 4),
                                    Expanded(
                                      child: Text(
                                        'الواصل',
                                        overflow: TextOverflow.ellipsis,
                                        style: AppTextStyle.labelMedium,
                                      ),
                                    ),
                                    Text(
                                      paidAmount.toIraqiDinarString(),
                                      style: AppTextStyle.labelMedium.withColor(AppColor.primary),
                                    ),
                                  ],
                                ),
                              ),
                              Padding(
                                padding: const EdgeInsets.only(top: 8),
                                child: Row(
                                  children: [
                                    Container(
                                      width: 10,
                                      height: 10,
                                      decoration: BoxDecoration(
                                        borderRadius: BorderRadius.circular(10),
                                        color: AppColor.yellow,
                                      ),
                                    ),
                                    SizedBox(width: 4),
                                    Expanded(
                                      child: Text(
                                        'المتراكمات',
                                        overflow: TextOverflow.ellipsis,
                                        style: AppTextStyle.labelMedium,
                                      ),
                                    ),
                                    Text(
                                      accumulatedAmount.toIraqiDinarString(),
                                      style: AppTextStyle.labelMedium.withColor(AppColor.primary),
                                    ),
                                  ],
                                ),
                              ),
                              Padding(
                                padding: const EdgeInsets.only(top: 16),
                                child: Row(
                                  children: [
                                    Expanded(
                                      child: Text(
                                        'القسط اليومي',
                                        overflow: TextOverflow.ellipsis,
                                        style: AppTextStyle.labelMedium,
                                      ),
                                    ),
                                    Text(
                                      dailyInstallment.toIraqiDinarString(),
                                      style: AppTextStyle.labelMedium.withColor(AppColor.text2),
                                    ),
                                  ],
                                ),
                              ),
                              Padding(
                                padding: const EdgeInsets.only(top: 8),
                                child: Row(
                                  children: [
                                    Expanded(
                                      child: Text(
                                        'المتبقي',
                                        overflow: TextOverflow.ellipsis,
                                        style: AppTextStyle.labelMedium,
                                      ),
                                    ),
                                    Text(
                                      remainingAmount.toIraqiDinarString(),
                                      style: AppTextStyle.labelMedium.withColor(AppColor.text2),
                                    ),
                                  ],
                                ),
                              ),
                              // Padding(
                              //   padding: const EdgeInsets.only(top: 8),
                              //   child: Row(
                              //     children: [
                              //       Expanded(
                              //         child: Text(
                              //           'سعر الشراء',
                              //           overflow: TextOverflow.ellipsis,
                              //           style: AppTextStyle.labelMedium,
                              //         ),
                              //       ),
                              //       Text(
                              //         buyAmount.toIraqiDinarString(),
                              //         style: AppTextStyle.labelMedium.withColor(AppColor.text2),
                              //       ),
                              //     ],
                              //   ),
                              // ),
                            ],
                          ),
                        ),
                        SizedBox(width: 24),
                        Hero(
                          tag: (orderDetails?.id).toString(),
                          child: DonutChartWidget(
                            mainSegmentPercentage: receivedPercentage,
                            secondarySegmentPercentage: accumulatedPercentage,
                            mainColor: AppColor.primary,
                            secondaryColor: AppColor.yellow,
                          ),
                        ),
                      ],
                    );
                  },
                ),
              ),
              if (orderDetails != null)
                Padding(
                  padding: const EdgeInsets.only(bottom: 6),
                  child: Row(
                    children: [
                      Expanded(
                        child: Align(
                          alignment: Alignment.centerRight,
                          child: Container(
                            decoration: BoxDecoration(
                              color: AppColor.surface1,
                              borderRadius: BorderRadius.circular(11),
                            ),
                            padding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                            child: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Text(
                                  (orderDetails!.orderItems?.length ?? 0).toString(),
                                  style: AppTextStyle.numberSmall,
                                ),
                                Text(
                                  ' عنصر',
                                  style: AppTextStyle.bodySmall,
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),

                      Row(
                        children: [
                          Text(
                            '${_getArabicDayName(orderDetails!.createdAt)} - ',
                            overflow: TextOverflow.ellipsis,
                            style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                          ),
                          Text(
                            DateFormat('y/M/d').format(orderDetails!.createdAt),
                            overflow: TextOverflow.ellipsis,
                            textDirection: TextDirection.ltr,
                            style: AppTextStyle.numberSmall.withColor(AppColor.text2),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              Container(height: 1, color: AppColor.stroke),
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 16),
                child: Column(
                  children: [
                    // Header Row
                    Padding(
                      padding: const EdgeInsets.only(bottom: 12),
                      child: Row(
                        children: [
                          Expanded(
                            flex: 2,
                            child: Text(
                              'العنصر',
                              style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                              textAlign: TextAlign.right,
                            ),
                          ),
                          Expanded(
                            child: Text(
                              'الكمية',
                              style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                              textAlign: TextAlign.center,
                            ),
                          ),
                          Expanded(
                            child: Text(
                              'السعر',
                              style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                              textAlign: TextAlign.center,
                            ),
                          ),
                        ],
                      ),
                    ),
                    // Items List
                    ...(orderDetails?.orderItems ?? []).map((item) {
                      return Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: Row(
                          children: [
                            Expanded(
                              flex: 2,
                              child: Text(
                                item.productName ?? '', // Adjust based on your actual property name
                                style: AppTextStyle.bodyMedium,
                                textAlign: TextAlign.right,
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                              ),
                            ),
                            Expanded(
                              child: Text(
                                item.quantity.toString(),
                                style: AppTextStyle.numberSmall,
                                textAlign: TextAlign.center,
                              ),
                            ),
                            Expanded(
                              child: Text(
                                '${TextUtil.addComma(item.sellAmount.toString())} د.ع',
                                style: AppTextStyle.numberSmall,
                                textAlign: TextAlign.left,
                                textDirection: TextDirection.rtl,
                              ),
                            ),
                          ],
                        ),
                      );
                    }),
                  ],
                ),
              ),
              // Row(
              //   mainAxisSize: MainAxisSize.min,
              //   children: [
              //     VectorGraphic(loader: AssetBytesLoader('assets/svg/clock_icon.svg')),
              //     SizedBox(width: 4),
              //     Text(
              //       DateFormat('y / M / d').format(orderDetails?.createdAt ?? DateTime.now()),
              //       overflow: TextOverflow.ellipsis,
              //       textDirection: TextDirection.ltr,
              //       style: TextStyle(color: AppColor.text2, fontSize: 12, fontFamily: 'Rubik'),
              //     ),
              //     SizedBox(width: 34),
              //     VectorGraphic(loader: AssetBytesLoader('assets/svg/mobile_icon.svg')),
              //     //Todo:probably chane this later
              //     // SizedBox(width: 4),
              //     // Flexible(
              //     //   child: Text(
              //     //     orderDetails?.product.name ?? '1',
              //     //     overflow: TextOverflow.ellipsis,
              //     //     style: TextStyle(color: AppColor.text2, fontSize: 12, fontFamily: 'Rubik'),
              //     //   ),
              //     // ),
              //   ],
              // ),
            ],
          ),
        ),
      ),
    );
  }
}
