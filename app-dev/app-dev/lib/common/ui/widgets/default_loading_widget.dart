import 'package:team/common/ui/theme/app_color.dart';
import 'package:flutter/material.dart'
    show BuildContext, Center, CircularProgressIndicator, Color, StatelessWidget, Widget;

class DefaultLoadingWidget extends StatelessWidget {
  const DefaultLoadingWidget({super.key, this.height, this.strokeWidth, this.width, this.color = AppColor.primary});

  final double? height, width, strokeWidth;
  final Color? color;

  @override
  Widget build(BuildContext context) {
    return Center(child: CircularProgressIndicator(color: color, strokeWidth: 2));
  }
}
