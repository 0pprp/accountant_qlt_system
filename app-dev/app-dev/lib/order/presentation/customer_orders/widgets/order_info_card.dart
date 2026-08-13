import 'package:flutter/material.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/utils/helpers/number_extentions.dart';
import 'package:team/order/domain/order_details/order_details.dart';
import 'package:team/order/presentation/order_details/widgets/donut_chart_widget.dart';
import 'package:vector_graphics/vector_graphics.dart';

class OrderInfoCard extends StatelessWidget {
  final OrderDetails orderDetails;
  OrderInfoCard({super.key, required this.orderDetails});

  late final double totalAmount = orderDetails.sellAmount;
  late final double receivedAmount = orderDetails.paidAmount;
  late final double accumulatedAmount = orderDetails.overdueAmount;
  late final double receivedPercentage = (receivedAmount / totalAmount) * 100;
  late final double accumulatedPercentage = (accumulatedAmount / totalAmount) * 100;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: Colors.white,
        border: Border.all(color: AppColor.stroke, width: 2),
        borderRadius: BorderRadius.circular(24),
      ),
      child: Column(
        children: [
          Row(
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
                          child: Text('سعر البيع', overflow: TextOverflow.ellipsis, style: AppTextStyle.labelMedium),
                        ),
                        Text(
                          orderDetails.sellAmount.toIraqiDinarString(),
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
                            decoration: BoxDecoration(borderRadius: BorderRadius.circular(10), color: AppColor.primary),
                          ),
                          SizedBox(width: 4),
                          Expanded(
                            child: Text('الواصل', overflow: TextOverflow.ellipsis, style: AppTextStyle.labelMedium),
                          ),
                          Text(
                            orderDetails.paidAmount.toIraqiDinarString(),
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
                            decoration: BoxDecoration(borderRadius: BorderRadius.circular(10), color: AppColor.yellow),
                          ),
                          SizedBox(width: 4),
                          Expanded(
                            child: Text('المتراكمات', overflow: TextOverflow.ellipsis, style: AppTextStyle.labelMedium),
                          ),
                          Text(
                            orderDetails.overdueAmount.toIraqiDinarString(),
                            style: AppTextStyle.labelMedium.withColor(AppColor.primary),
                          ),
                        ],
                      ),
                    ),
                    Padding(
                      padding: const EdgeInsets.only(top: 16, bottom: 16),
                      child: Row(
                        children: [
                          VectorGraphic(loader: AssetBytesLoader('assets/svg/clock_icon.svg')),
                          SizedBox(width: 4),
                          Text(
                            DateFormat('y / M / d').format(orderDetails.createdAt),
                            overflow: TextOverflow.ellipsis,
                            textDirection: TextDirection.ltr,
                            textAlign: TextAlign.left,
                            style: TextStyle(color: AppColor.text2, fontSize: 12, fontFamily: 'Rubik'),
                          ),
                        ],
                      ),
                    ),
                    Row(
                      children: [
                        // VectorGraphic(loader: AssetBytesLoader('assets/svg/mobile_icon.svg')),
                        //Todo:probably chane this later
                        // SizedBox(width: 4),
                        // Expanded(
                        //   child: Text(
                        //     'orderDetails.product.name',
                        //     overflow: TextOverflow.ellipsis,
                        //     textAlign: TextAlign.right,
                        //     style: TextStyle(color: AppColor.text2, fontSize: 12, fontFamily: 'Rubik'),
                        //   ),
                        // ),
                      ],
                    ),
                  ],
                ),
              ),
              SizedBox(width: 24),
              Hero(
                tag: (orderDetails.id).toString(),
                child: DonutChartWidget(
                  mainSegmentPercentage: receivedPercentage,
                  secondarySegmentPercentage: accumulatedPercentage,
                  mainColor: AppColor.primary,
                  secondaryColor: AppColor.yellow,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
