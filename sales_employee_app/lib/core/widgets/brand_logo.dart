import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

enum BrandLogoTone { green, white }

class BrandLogo extends StatelessWidget {
  const BrandLogo({
    super.key,
    this.height = 72,
    this.tone = BrandLogoTone.green,
  });

  final double height;
  final BrandLogoTone tone;

  @override
  Widget build(BuildContext context) {
    final asset = tone == BrandLogoTone.white
        ? 'assets/svg/logo_white.svg'
        : 'assets/svg/logo_green.svg';
    return SvgPicture.asset(
      asset,
      height: height,
      fit: BoxFit.contain,
      semanticsLabel: 'قلعة الضمان',
    );
  }
}
