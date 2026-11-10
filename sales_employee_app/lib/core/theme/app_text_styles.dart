import 'package:flutter/material.dart';

import 'app_colors.dart';

abstract final class AppTextStyles {
  static const _family = 'ElMessiri';

  static TextStyle get display => const TextStyle(
        fontFamily: _family,
        fontSize: 28,
        fontWeight: FontWeight.w700,
        color: AppColors.foreground,
      );

  static TextStyle get title => const TextStyle(
        fontFamily: _family,
        fontSize: 20,
        fontWeight: FontWeight.w700,
        color: AppColors.foreground,
      );

  static TextStyle get body => const TextStyle(
        fontFamily: _family,
        fontSize: 15,
        fontWeight: FontWeight.w400,
        color: AppColors.foreground,
      );

  static TextStyle get muted => const TextStyle(
        fontFamily: _family,
        fontSize: 13,
        fontWeight: FontWeight.w400,
        color: AppColors.muted,
      );

  static TextStyle get button => const TextStyle(
        fontFamily: _family,
        fontSize: 16,
        fontWeight: FontWeight.w600,
        color: AppColors.onPrimary,
      );

  static TextStyle get amount => const TextStyle(
        fontFamily: _family,
        fontSize: 18,
        fontWeight: FontWeight.w700,
        color: AppColors.primary,
      );
}
