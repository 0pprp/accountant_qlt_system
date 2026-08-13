import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';

class CustomFadeTransitionBuilder extends PageTransitionsBuilder {
  const CustomFadeTransitionBuilder();

  @override
  Widget buildTransitions<T>(
    PageRoute<T> route,
    BuildContext context,
    Animation<double> animation,
    Animation<double> secondaryAnimation,
    Widget child,
  ) {
    return FadeTransition(opacity: animation, child: child);
  }
}

class AppTheme {
  static final ThemeData lightTheme = ThemeData(
    fontFamily: 'ElMessiri',
    primaryColor: AppColor.primary,
    scaffoldBackgroundColor: AppColor.surface1,
    buttonTheme: const ButtonThemeData(height: 40),
    pageTransitionsTheme: PageTransitionsTheme(
      builders: Map<TargetPlatform, PageTransitionsBuilder>.fromIterable(
        TargetPlatform.values,
        value: (_) => CustomFadeTransitionBuilder(),
      ),
    ),
    appBarTheme: AppBarTheme(systemOverlayStyle: SystemUiOverlayStyle.dark),
    snackBarTheme: SnackBarThemeData(
      backgroundColor: AppColor.surface3,
      contentTextStyle: TextStyle(color: AppColor.text1),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: AppColor.primary,
        foregroundColor: AppColor.white,
        elevation: 4,
        shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.3),
        disabledBackgroundColor: AppColor.text3,
        disabledForegroundColor: AppColor.text2,
        disabledIconColor: AppColor.text2,
        textStyle: AppTextStyle.headlineMedium.withColor(Colors.white),
        fixedSize: const Size.fromHeight(50),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      ),
    ),
    iconButtonTheme: IconButtonThemeData(
      style: IconButton.styleFrom(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        backgroundColor: AppColor.surface2,
        elevation: 4,

        shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.3),
      ),
    ),
    textButtonTheme: TextButtonThemeData(
      style: FilledButton.styleFrom(
        foregroundColor: AppColor.primary,
        disabledForegroundColor: AppColor.text2,
        disabledIconColor: AppColor.text2,
        textStyle: AppTextStyle.headlineMedium,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        fixedSize: const Size.fromHeight(40),
        foregroundColor: AppColor.text2,
        textStyle: AppTextStyle.headlineMedium,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
        side: BorderSide(color: AppColor.text2, width: 1),
      ),
    ),
    unselectedWidgetColor: AppColor.white,
    colorScheme: const ColorScheme.light(primary: AppColor.primary),
  );
}
