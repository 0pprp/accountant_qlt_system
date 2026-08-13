import 'package:flutter/material.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:team/account/domain/user_info/user_info.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:vector_graphics/vector_graphics.dart';

class CustomerInfo extends StatelessWidget {
  final UserInfo customer;
  final VoidCallback onTap;

  const CustomerInfo({super.key, required this.customer, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Material(
        borderRadius: BorderRadius.circular(16),
        elevation: 0,
        color: AppColor.surface2,
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(16),
          child: Container(
            padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 16),
            decoration: BoxDecoration(
              border: Border.all(color: AppColor.stroke, width: 2),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        customer.fullName,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: AppTextStyle.headlineMedium,
                      ),
                      const SizedBox(height: 8),
                      Container(
                        decoration: BoxDecoration(
                          color: AppColor.surface1,
                          borderRadius: BorderRadius.circular(11),
                        ),
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            VectorGraphic(loader: AssetBytesLoader('assets/svg/clock_icon.svg')),
                            const SizedBox(width: 4),
                            Text(
                              DateFormat('y / M / d').format(customer.createdAt),
                              overflow: TextOverflow.ellipsis,
                              textDirection: TextDirection.ltr,
                              style: TextStyle(
                                color: AppColor.text2,
                                fontWeight: FontWeight.w300,
                                fontSize: 12,
                                fontFamily: 'Rubik',
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
                  colorFilter: const ColorFilter.mode(Color(0xff808189), BlendMode.srcIn),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
