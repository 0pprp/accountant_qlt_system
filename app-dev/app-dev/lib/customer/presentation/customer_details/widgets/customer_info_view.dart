import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/show_animated_dialog.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:vector_graphics/vector_graphics.dart';

class CustomerInfoView extends StatelessWidget {
  final User? user;
  const CustomerInfoView({super.key, required this.user});

  void _showCallOptionsDialog(BuildContext context) {
    showAnimatedDialog(
      context: context,
      child: StatefulBuilder(
        builder: (context, setState) {
          return Dialog(
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24.0)),
            insetPadding: EdgeInsets.symmetric(horizontal: 24),
            backgroundColor: Colors.white,
            surfaceTintColor: Colors.white,
            child: ClipRRect(
              borderRadius: BorderRadius.circular(24),
              child: Stack(
                children: [
                  VectorGraphic(
                    loader: AssetBytesLoader('assets/svg/dialog_background.svg'),
                    fit: BoxFit.fill,
                    colorFilter: ColorFilter.mode(AppColor.primary.withValues(alpha: 0.15), BlendMode.srcIn),
                  ),
                  Container(
                    padding: EdgeInsets.all(32),
                    decoration: BoxDecoration(borderRadius: BorderRadius.circular(24)),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text('الهاتف', style: AppTextStyle.headlineMedium.withColor(AppColor.text2)),
                            VectorGraphic(loader: AssetBytesLoader('assets/svg/call_icon.svg'), width: 20, height: 21),
                          ],
                        ),
                        Padding(
                          padding: const EdgeInsets.only(top: 24),
                          child: Row(
                            children: [
                              Expanded(
                                child: FilledButton(
                                  onPressed: () async {
                                    Navigator.pop(context);
                                    if (user?.phoneNumber != null) {
                                      final uri = Uri(scheme: 'tel', path: user?.phoneNumber);
                                      if (await canLaunchUrl(uri)) {
                                        await launchUrl(uri);
                                      } else {
                                        ScaffoldMessenger.of(
                                          context,
                                        ).showSnackBar(SnackBar(content: Text('لا يمكن إجراء المكالمة')));
                                      }
                                    }
                                  },
                                  style: FilledButton.styleFrom(
                                    backgroundColor: Colors.white,
                                    foregroundColor: AppColor.primary,
                                    elevation: 4,
                                    side: BorderSide(color: AppColor.stroke, width: 2),
                                    shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.15),
                                  ),
                                  child: Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      Transform.flip(
                                        flipX: true,
                                        child: VectorGraphic(
                                          loader: AssetBytesLoader('assets/svg/call_icon.svg'),
                                          width: 20,
                                          height: 21,
                                        ),
                                      ),
                                      SizedBox(width: 8),
                                      Text('رقم الهاتف', style: AppTextStyle.bodyMedium),
                                      SizedBox(width: 12),
                                    ],
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.only(top: 24),
                          child: Row(
                            children: [
                              Expanded(
                                child: FilledButton(
                                  onPressed: () async {
                                    Navigator.pop(context);
                                    final whatsAppPhoneNumber = user?.whatsAppPhoneNumber ?? user?.phoneNumber;
                                    if (whatsAppPhoneNumber != null) {
                                      final url = Uri.parse('https://wa.me/${whatsAppPhoneNumber.replaceAll('+', '')}');
                                      if (await canLaunchUrl(url)) {
                                        await launchUrl(url, mode: LaunchMode.externalApplication);
                                      } else {
                                        ScaffoldMessenger.of(
                                          context,
                                        ).showSnackBar(SnackBar(content: Text('تعذر فتح واتساب')));
                                      }
                                    }
                                  },
                                  style: FilledButton.styleFrom(
                                    backgroundColor: Colors.white,
                                    foregroundColor: AppColor.primary,
                                    elevation: 4,
                                    side: BorderSide(color: AppColor.stroke, width: 2),
                                    shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.15),
                                  ),
                                  child: Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      VectorGraphic(
                                        loader: AssetBytesLoader('assets/svg/whatsapp_icon.svg'),
                                        width: 18,
                                        height: 17,
                                      ),
                                      SizedBox(width: 8),
                                      Text('الوتساب', style: AppTextStyle.bodyMedium),
                                      SizedBox(width: 32),
                                    ],
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.only(top: 24),
                          child: Row(
                            children: [
                              Expanded(
                                child: FilledButton(
                                  onPressed: () {
                                    Navigator.pop(context);
                                    if (user?.phoneNumber != null) {
                                      Clipboard.setData(ClipboardData(text: user!.phoneNumber));
                                      ScaffoldMessenger.of(
                                        context,
                                      ).showSnackBar(SnackBar(content: Text('تم نسخ الرقم')));
                                    }
                                  },
                                  style: FilledButton.styleFrom(
                                    backgroundColor: Colors.white,
                                    foregroundColor: AppColor.primary,
                                    elevation: 4,
                                    side: BorderSide(color: AppColor.stroke, width: 2),
                                    shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.15),
                                  ),
                                  child: Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      SizedBox(width: 16),
                                      VectorGraphic(
                                        loader: AssetBytesLoader('assets/svg/copy_icon.svg'),
                                        width: 16,
                                        height: 16,
                                      ),
                                      SizedBox(width: 8),
                                      Text('نسخ رقم الهاتف', style: AppTextStyle.bodyMedium),
                                    ],
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  void _showLocationOptionsDialog(BuildContext context) {
    final address = user?.business.address ?? '';

    Future<void> launch(String url, BuildContext ctx) async {
      Navigator.pop(ctx);
      if (address.isEmpty) {
        ScaffoldMessenger.of(ctx).showSnackBar(
          SnackBar(content: Text('لا يوجد عنوان متاح')),
        );
        return;
      }
      final uri = Uri.parse(url);
      // if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
      // } else {
      //   ScaffoldMessenger.of(ctx).showSnackBar(
      //     SnackBar(content: Text('تعذر فتح الخريطة')),
      //   );
      // }
    }

    final encoded = Uri.encodeComponent(address);

    showAnimatedDialog(
      context: context,
      child: StatefulBuilder(
        builder: (context, setState) {
          return Dialog(
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24.0)),
            insetPadding: EdgeInsets.symmetric(horizontal: 24),
            backgroundColor: Colors.white,
            surfaceTintColor: Colors.white,
            child: ClipRRect(
              borderRadius: BorderRadius.circular(24),
              child: Stack(
                children: [
                  VectorGraphic(
                    loader: AssetBytesLoader('assets/svg/dialog_background.svg'),
                    fit: BoxFit.fill,
                    colorFilter: ColorFilter.mode(AppColor.primary.withValues(alpha: 0.15), BlendMode.srcIn),
                  ),
                  Container(
                    padding: EdgeInsets.all(32),
                    decoration: BoxDecoration(borderRadius: BorderRadius.circular(24)),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text('الموقع', style: AppTextStyle.headlineMedium.withColor(AppColor.text2)),
                            VectorGraphic(
                              loader: AssetBytesLoader('assets/svg/location_icon.svg'),
                              width: 20,
                              height: 21,
                            ),
                          ],
                        ),
                        Padding(
                          padding: const EdgeInsets.only(top: 24),
                          child: Row(
                            children: [
                              Expanded(
                                child: FilledButton(
                                  onPressed:
                                      () => launch(
                                        'https://maps.apple.com/?q=$encoded',
                                        context,
                                      ),
                                  style: FilledButton.styleFrom(
                                    backgroundColor: Colors.white,
                                    foregroundColor: AppColor.primary,
                                    elevation: 4,
                                    side: BorderSide(color: AppColor.stroke, width: 2),
                                    shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.15),
                                  ),
                                  child: Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      Transform.flip(
                                        flipX: true,
                                        child: Image.asset('assets/images/apple_map_icon.png', width: 20, height: 20),
                                      ),
                                      SizedBox(width: 8),
                                      Text('الخرائط', style: AppTextStyle.bodyMedium),
                                      SizedBox(width: 38),
                                    ],
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.only(top: 24),
                          child: Row(
                            children: [
                              Expanded(
                                child: FilledButton(
                                  onPressed:
                                      () => launch(
                                        'https://www.google.com/maps/search/?api=1&query=$encoded',
                                        context,
                                      ),
                                  style: FilledButton.styleFrom(
                                    backgroundColor: Colors.white,
                                    foregroundColor: AppColor.primary,
                                    elevation: 4,
                                    side: BorderSide(color: AppColor.stroke, width: 2),
                                    shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.15),
                                  ),
                                  child: Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      Image.asset('assets/images/google_map_icon.png', width: 20, height: 20),
                                      SizedBox(width: 8),
                                      Text('خرائط Google', style: AppTextStyle.bodyMedium),
                                    ],
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.only(top: 24),
                          child: Row(
                            children: [
                              Expanded(
                                child: FilledButton(
                                  onPressed:
                                      () => launch(
                                        'https://waze.com/ul?q=$encoded&navigate=yes',
                                        context,
                                      ),
                                  style: FilledButton.styleFrom(
                                    backgroundColor: Colors.white,
                                    foregroundColor: AppColor.primary,
                                    elevation: 4,
                                    side: BorderSide(color: AppColor.stroke, width: 2),
                                    shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.15),
                                  ),
                                  child: Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      SizedBox(width: 16),
                                      Image.asset('assets/images/waze_icon.png', width: 20, height: 20),
                                      SizedBox(width: 8),
                                      Text('خرائط Waze', style: AppTextStyle.bodyMedium),
                                      SizedBox(width: 28),
                                    ],
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

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
        padding: EdgeInsets.only(right: 24, top: 24, bottom: 24),
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
                  Text('الزبون', style: AppTextStyle.labelLarge.withColor(AppColor.text2)),
                  Padding(
                    padding: const EdgeInsets.only(left: 24),
                    child: VectorGraphic(loader: AssetBytesLoader('assets/svg/id_card_icon.svg')),
                  ),
                ],
              ),
              SizedBox(height: 10),
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Padding(
                          padding: const EdgeInsets.only(top: 32),
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
                                  user?.fullName ?? '',
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
                                  user?.phoneNumber ?? '',
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
                            children: [
                              VectorGraphic(
                                loader: AssetBytesLoader('assets/svg/id_card_icon.svg'),
                                width: 17,
                                colorFilter: ColorFilter.mode(AppColor.text2, BlendMode.srcIn),
                                // height: 17,
                              ),
                              Expanded(
                                child: Text(
                                  user?.nationalCode ?? '',
                                  textDirection: TextDirection.rtl,
                                  style: AppTextStyle.numberSmall.withColor(AppColor.text1),
                                ),
                              ),
                            ],
                          ),
                        ),
                        // Padding(
                        //   padding: const EdgeInsets.only(top: 8),
                        //   child: Row(
                        //     spacing: 8,
                        //     children: [
                        //       VectorGraphic(loader: AssetBytesLoader('assets/svg/location_icon.svg')),
                        //       Expanded(
                        //         child: Text(
                        //           user?.business.address ?? '',
                        //           overflow: TextOverflow.ellipsis,
                        //           style: AppTextStyle.bodyMedium,
                        //         ),
                        //       ),
                        //     ],
                        //   ),
                        // ),
                      ],
                    ),
                  ),
                  VectorGraphic(loader: AssetBytesLoader('assets/svg/user_info_graphic.svg')),
                ],
              ),
              Padding(
                padding: const EdgeInsets.only(left: 24, top: 20, bottom: 0),
                child: Row(
                  children: [
                    Expanded(
                      child: FilledButton(
                        onPressed: () {
                          _showCallOptionsDialog(context);
                        },
                        style: FilledButton.styleFrom(
                          backgroundColor: Colors.white,
                          foregroundColor: AppColor.primary,
                          elevation: 4,
                          side: BorderSide(color: AppColor.stroke, width: 2),
                          shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.15),
                        ),
                        child: VectorGraphic(
                          loader: AssetBytesLoader('assets/svg/call_icon.svg'),
                          width: 20,
                          height: 21,
                          colorFilter: ColorFilter.mode(AppColor.primary, BlendMode.srcIn),
                        ),
                      ),
                    ),
                    SizedBox(width: 16),
                    FilledButton(
                      onPressed: () {
                        _showLocationOptionsDialog(context);
                      },
                      style: FilledButton.styleFrom(
                        backgroundColor: Colors.white,
                        foregroundColor: AppColor.primary,
                        elevation: 4,
                        shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.15),
                        side: BorderSide(color: AppColor.stroke, width: 2),
                      ),
                      child: VectorGraphic(
                        loader: AssetBytesLoader('assets/svg/location_icon.svg'),
                        width: 20,
                        height: 21,
                        colorFilter: ColorFilter.mode(AppColor.primary, BlendMode.srcIn),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
