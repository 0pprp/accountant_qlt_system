import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

abstract final class AppColors {
  static const background = Color(0xFFEEF3F2);
  static const surface = Color(0xFFF6F9F8);
  static const shadowDark = Color(0xFFA7BBB8);
  static const shadowLight = Color(0xFFFFFFFF);
  static const primary = Color(0xFF006A64);
  static const primarySoft = Color(0xFFD7ECE9);
  static const ink = Color(0xFF00201E);
  static const muted = Color(0xFF5A6B6A);
  static const danger = Color(0xFFC62828);
  static const success = Color(0xFF2E7D32);
  static const warning = Color(0xFFEF6C00);
  static const insetTop = Color(0xFFDDE6E4);
  static const insetBottom = Color(0xFFF7FAF9);
  static const line = Color(0xFFDCE6E4);
}

abstract final class AppTheme {
  static const fontFamily = 'ElMessiri';

  static ThemeData light() {
    final scheme = ColorScheme.fromSeed(
      seedColor: AppColors.primary,
      primary: AppColors.primary,
      surface: AppColors.surface,
      error: AppColors.danger,
      brightness: Brightness.light,
    );
    final base = ThemeData(
      useMaterial3: true,
      colorScheme: scheme,
      fontFamily: fontFamily,
      scaffoldBackgroundColor: AppColors.background,
    );
    return base.copyWith(
      textTheme: base.textTheme.apply(
        bodyColor: AppColors.ink,
        displayColor: AppColors.ink,
        fontFamily: fontFamily,
      ),
      appBarTheme: const AppBarTheme(
        backgroundColor: AppColors.background,
        foregroundColor: AppColors.ink,
        elevation: 0,
        scrolledUnderElevation: 0,
        centerTitle: true,
        systemOverlayStyle: SystemUiOverlayStyle(
          statusBarColor: Colors.transparent,
          statusBarIconBrightness: Brightness.dark,
        ),
        titleTextStyle: TextStyle(
          fontFamily: fontFamily,
          fontWeight: FontWeight.w700,
          fontSize: 18,
          color: AppColors.ink,
        ),
      ),
      dialogTheme: const DialogThemeData(
        backgroundColor: AppColors.surface,
        surfaceTintColor: Colors.transparent,
      ),
      bottomSheetTheme: const BottomSheetThemeData(
        backgroundColor: AppColors.background,
        surfaceTintColor: Colors.transparent,
        showDragHandle: true,
      ),
      snackBarTheme: const SnackBarThemeData(
        behavior: SnackBarBehavior.floating,
        contentTextStyle: TextStyle(fontFamily: fontFamily, fontSize: 14),
      ),
      progressIndicatorTheme: const ProgressIndicatorThemeData(
        color: AppColors.primary,
      ),
      inputDecorationTheme: const InputDecorationTheme(
        border: InputBorder.none,
        hintStyle: TextStyle(color: AppColors.muted, fontSize: 14),
        errorStyle: TextStyle(color: AppColors.danger, fontSize: 12),
      ),
    );
  }
}
