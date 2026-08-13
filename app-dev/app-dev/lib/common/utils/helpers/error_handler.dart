import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:fluttertoast/fluttertoast.dart';
import 'package:team/common/ui/widgets/snackbar.dart';

class MessageException extends Error {
  final String message;

  MessageException(this.message);
}

class ErrorHandler {
  const ErrorHandler();

  static String handleError(dynamic error, {StackTrace? stack}) {
    final message = handleErrorMessage(error);

    if (error is FlutterErrorDetails || error is Error) {
      // TODO: Connect to firebase
      // FirebaseCrashlytics.instance.recordFlutterError(error);
    }

    debugPrint(message);
    if (message.isNotEmpty) {
      showToast(message, type: ToastType.error, toast: Toast.LENGTH_LONG);
    }

    return message;
  }

  static String handleErrorMessage([dynamic e]) {
    String message = 'حدث خطأ، يرجى المحاولة مرة أخرى';

    try {
      if (e is MessageException) {
        message = e.message;
      } else if (e is DioException) {
        if (e.type == DioExceptionType.badResponse) {
          message = e.response!.data['errors'].first['message'].toString();
        }
        // else {
        // message = 'مشکل در ارتباط با سرور';
        // }
      }
    } catch (e) {
      // message = 'مشکل در ارتباط با سرور';
    }
    return message;
  }
}
