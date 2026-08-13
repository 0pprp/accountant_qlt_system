import 'package:flutter/material.dart';
import 'package:fluttertoast/fluttertoast.dart';
import 'package:team/common/ui/theme/app_color.dart';

enum ToastType { success, error, warning, info }

showToast(String message, {ToastType type = ToastType.info, Toast toast = Toast.LENGTH_LONG}) {
  Future.delayed(Duration.zero, () {
    Color snackBarColor;
    if (type == ToastType.success) {
      snackBarColor = AppColor.success;
    } else if (type == ToastType.warning) {
      snackBarColor = AppColor.white;
    } else if (type == ToastType.error) {
      snackBarColor = AppColor.error;
    } else {
      snackBarColor = const Color(0xff303030);
    }
    Fluttertoast.showToast(
      msg: message,
      toastLength: toast,
      fontAsset: 'assets/fonts/ElMessiri-Regular.ttf',
      gravity: ToastGravity.BOTTOM,
      timeInSecForIosWeb: 2,
      backgroundColor: snackBarColor.withValues(alpha: 0.8),
      textColor: Colors.white,
      fontSize: 12.0,
    );
  });
}
