import 'package:flutter/material.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/utils/helpers/number_extentions.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';
import 'package:vector_graphics/vector_graphics.dart';

class PaymentInfoCard extends StatelessWidget {
  final InstallmentPayment installmentPayment;
  final VoidCallback onTap;
  const PaymentInfoCard({super.key, required this.installmentPayment, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return Material(
      borderRadius: BorderRadius.circular(16),
      elevation: 0,
      color: AppColor.surface2,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(16),
        child: Container(
          padding: EdgeInsets.symmetric(vertical: 10, horizontal: 16),
          decoration: BoxDecoration(
            border: Border.all(color: AppColor.stroke, width: 2),
            borderRadius: BorderRadius.circular(16),
          ),
          child: Column(
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  Expanded(
                    child: Column(
                      children: [
                        Padding(
                          padding: const EdgeInsets.only(bottom: 12, left: 16),
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Expanded(
                                child: Text(installmentPayment.customerFullName, style: AppTextStyle.headlineMedium),
                              ),
                              Text(
                                installmentPayment.amount.toIraqiDinarString(),
                                style: TextStyle(
                                  color: AppColor.text1,
                                  fontWeight: FontWeight.w400,
                                  fontSize: 14,
                                  fontFamily: 'Rubik',
                                ),
                              ),
                            ],
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.only(bottom: 4),
                          child: Column(
                            children: [
                              Padding(
                                padding: const EdgeInsets.only(right: 8),
                                child: Row(
                                  children: [
                                    VectorGraphic(loader: AssetBytesLoader('assets/svg/clock_icon.svg')),
                                    Expanded(
                                      child: Align(
                                        alignment: Alignment.centerRight,
                                        child: Padding(
                                          padding: const EdgeInsets.only(right: 4),
                                          child: Text(
                                            DateFormat('y / M / d').format(installmentPayment.date),
                                            overflow: TextOverflow.ellipsis,
                                            textDirection: TextDirection.ltr,
                                            style: TextStyle(
                                              color: AppColor.text2,
                                              fontWeight: FontWeight.w300,
                                              fontSize: 12,
                                              fontFamily: 'Rubik',
                                            ),
                                          ),
                                        ),
                                      ),
                                    ),
                                    // VectorGraphic(loader: AssetBytesLoader('assets/svg/mobile_icon.svg')),
                                    //Todo:probably change this later
                                    // Flexible(
                                    //   child: Padding(
                                    //     padding: const EdgeInsets.only(right: 4, left: 16),
                                    //     child: Text(
                                    //       installmentPayment.product.name,
                                    //       overflow: TextOverflow.ellipsis,
                                    //       style: TextStyle(
                                    //         color: AppColor.text2,
                                    //         fontWeight: FontWeight.w300,
                                    //         fontSize: 12,
                                    //         fontFamily: 'Rubik',
                                    //       ),
                                    //     ),
                                    //   ),
                                    // ),
                                  ],
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                  VectorGraphic(
                    loader: AssetBytesLoader('assets/svg/back_icon.svg'),
                    colorFilter: ColorFilter.mode(Color(0xff808189), BlendMode.srcIn),
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
