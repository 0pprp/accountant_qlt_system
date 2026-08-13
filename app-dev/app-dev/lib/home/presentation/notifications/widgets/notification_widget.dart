import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_cached_network_image.dart';

class NotificationWidget extends StatelessWidget {
  final bool isNew;
  const NotificationWidget({super.key, this.isNew = false});

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Container(
          padding: EdgeInsets.all(16),
          margin: EdgeInsets.only(top: 16),
          decoration: BoxDecoration(
            color: AppColor.surface2,
            border: Border.all(color: AppColor.stroke, width: 2),
            borderRadius: BorderRadius.circular(16),
          ),
          child: Column(
            children: [
              Padding(
                padding: const EdgeInsets.only(top: 8, bottom: 16),
                child: Text(
                  'وسطوعه مع ضمان وضوح العناصر. إليك بعض الخيارات المناسبة لأنماط مختلفة',
                  style: AppTextStyle.bodyMedium,
                ),
              ),
              Row(
                children: [
                  ClipRRect(
                    borderRadius: BorderRadius.circular(24),
                    child: DefaultCachedNetworkImage(
                      imageUrl: 'https://www.dorbineto.ir/images/friends2.webp',
                      height: 24,
                      width: 24,
                    ),
                  ),
                  // const CircleAvatar(
                  //   radius: 12,
                  //   backgroundColor: Color(0xFF0A6E61),
                  //   child: Icon(Icons.person, color: Colors.white, size: 17),
                  // ),
                  SizedBox(width: 6),
                  Text('هدى محمد', style: AppTextStyle.labelMedium),
                  Spacer(),
                  Text(DateFormat('y/M/d').format(DateTime.now()), style: AppTextStyle.numberSmall),
                ],
              ),
            ],
          ),
        ),
        if (isNew)
          Padding(
            padding: const EdgeInsets.only(top: 16),
            child: Container(
              height: 10,
              width: 10,
              decoration: ShapeDecoration(
                color: AppColor.error,
                shape: CircleBorder(side: BorderSide(color: AppColor.stroke, width: 2)),
              ),
            ),
          ),
      ],
    );
  }
}
