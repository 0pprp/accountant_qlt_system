import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../theme/app_theme.dart';
import 'neu.dart';

class LogoTile extends StatelessWidget {
  const LogoTile({super.key, this.size = 88});
  final double size;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      padding: EdgeInsets.all(size * 0.18),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(size * 0.28),
        boxShadow: neuShadows(depth: 1.1),
      ),
      child: SvgPicture.asset('assets/svg/logo_green.svg', fit: BoxFit.contain),
    );
  }
}
