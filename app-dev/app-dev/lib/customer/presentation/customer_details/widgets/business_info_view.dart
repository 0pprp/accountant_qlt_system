import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:vector_graphics/vector_graphics.dart';

class BusinessInfoView extends StatelessWidget {
  final User? user;
  const BusinessInfoView({super.key, required this.user});

  @override
  Widget build(BuildContext context) {
    return Animate(
      effects: [
        if (user == null)
          ShimmerEffect(
            colors: [Color(0xFFEBEBF4), Color(0xFFF4F4F4), Color(0xFFEBEBF4)],
            stops: [0.1, 0.3, 0.4],
            blendMode: BlendMode.srcATop,
            duration: 2000.ms,
          ),
      ],
      onComplete: (controller) => user != null ? null : controller.repeat(),
      child: Container(
        margin: EdgeInsets.only(bottom: 16),
        padding: EdgeInsets.only(right: 24, top: 24, bottom: 0),
        decoration: BoxDecoration(
          color: Colors.white,
          border: Border.all(color: user == null ? Colors.white : AppColor.stroke, width: 2),
          borderRadius: BorderRadius.circular(24),
        ),
        child: Visibility(
          maintainSize: true,
          maintainState: true,
          maintainAnimation: true,
          visible: user != null,
          child: Column(
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('تفاصيل العميل', style: AppTextStyle.labelLarge.withColor(AppColor.text2)),
                  Padding(
                    padding: const EdgeInsets.only(left: 24),
                    child: VectorGraphic(
                      loader: AssetBytesLoader('assets/svg/market_icon.svg'),
                      width: 20,
                      height: 20,
                    ),
                  ),
                ],
              ),
              SizedBox(height: 10),
              Row(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Padding(
                          padding: const EdgeInsets.only(top: 14),
                          child: Row(
                            spacing: 8,
                            children: [
                              VectorGraphic(
                                loader: AssetBytesLoader('assets/svg/user_icon.svg'),
                                width: 16,
                                height: 16,
                              ),
                              Expanded(
                                child: Text(
                                  user?.business.name ?? '',
                                  overflow: TextOverflow.ellipsis,
                                  style: AppTextStyle.bodyMedium,
                                ),
                              ),
                            ],
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.only(top: 8),
                          child: Row(
                            spacing: 8,
                            children: [
                              VectorGraphic(loader: AssetBytesLoader('assets/svg/call_icon.svg')),
                              Expanded(
                                child: Text(
                                  user?.whatsAppPhoneNumber ?? '',
                                  textDirection: TextDirection.rtl,
                                  style: AppTextStyle.numberSmall.withColor(AppColor.text1),
                                ),
                              ),
                            ],
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.only(top: 8),
                          child: Row(
                            spacing: 8,
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              VectorGraphic(loader: AssetBytesLoader('assets/svg/location_icon.svg')),
                              Expanded(
                                child: Text(
                                  user?.business.address ?? '',
                                  style: AppTextStyle.bodyMedium,
                                ),
                              ),
                            ],
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.only(top: 8),
                          child: Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            spacing: 8,
                            children: [
                              VectorGraphic(loader: AssetBytesLoader('assets/svg/signpost_icon.svg')),
                              Expanded(
                                child: Text(
                                  user?.business.nearestKnownLocation ?? '',
                                  style: AppTextStyle.bodyMedium,
                                ),
                              ),
                            ],
                          ),
                        ),
                        SizedBox(height: 24),
                      ],
                    ),
                  ),
                  Align(
                    alignment: AlignmentGeometry.bottomLeft,
                    child: ClipRRect(
                      borderRadius: BorderRadius.only(bottomLeft: Radius.circular(22)),
                      child: VectorGraphic(loader: AssetBytesLoader('assets/svg/business_graphic.svg')),
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
