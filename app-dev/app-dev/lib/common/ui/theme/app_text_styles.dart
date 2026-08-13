import 'package:flutter/material.dart';
import 'package:team/common/ui/theme/app_color.dart';

extension AppTextStyles on TextStyle {
  TextStyle withColor(Color color) {
    return copyWith(color: color);
  }
}

class AppTextStyle {
  static const _regular = FontWeight.w400;
  static const _medium = FontWeight.w500;
  static const _semiBold = FontWeight.w600;
  static const _bold = FontWeight.w700;

  // Display
  static const displayLarge = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 39,
    fontWeight: _bold,
    color: AppColor.text1,
  );

  static const displayMedium = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 30,
    fontWeight: _semiBold,
    color: AppColor.text1,
  );

  // Headline
  static const headlineLarge = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 20,
    fontWeight: _semiBold,
    color: AppColor.text1,
  );

  static const headlineMedium = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 16,
    fontWeight: _medium,
    color: AppColor.text1,
  );

  static const headlineSmall = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 14,
    fontWeight: _bold,
    color: AppColor.text1,
  );

  // Title
  static const titleLarge = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 20,
    fontWeight: _semiBold,
    color: AppColor.text1,
  );

  static const titleMedium = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 20,
    fontWeight: _medium,
    color: AppColor.text1,
  );

  static const titleSmall = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 20,
    fontWeight: _regular,
    color: AppColor.text1,
  );

  // Body
  static const bodyLarge = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 16,
    fontWeight: _regular,
    color: AppColor.text1,
  );

  static const bodyMedium = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 14,
    fontWeight: _regular,
    color: AppColor.text1,
  );

  static const bodySmall = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 12,
    fontWeight: _regular,
    color: AppColor.text1,
  );

  // Label
  static const labelLarge = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 11,
    fontWeight: _semiBold,
    color: AppColor.text1,
  );

  static const labelMedium = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 10,
    fontWeight: _medium,
    color: AppColor.text1,
  );

  static const labelSmall = TextStyle(
    fontFamily: 'ElMessiri',
    fontSize: 8,
    fontWeight: _semiBold,
    color: AppColor.text1,
  );

  static const numberSmall = TextStyle(color: AppColor.text1, fontSize: 12, fontWeight: _regular, fontFamily: 'Rubik');

  static const numberMedium = TextStyle(color: AppColor.text1, fontSize: 14, fontWeight: _regular, fontFamily: 'Rubik');

  static const numberLarge = TextStyle(color: AppColor.text1, fontSize: 16, fontWeight: _regular, fontFamily: 'Rubik');
}
