import 'dart:io';

import 'package:dio/dio.dart';

import 'api_exception.dart';
import 'dio_client.dart';

abstract final class ApiErrors {
  static const sessionExpired = 'انتهت الجلسة. أعد تسجيل الدخول.';
  static const forbidden = 'الحساب غير مخوّل لتطبيق المروّج الميداني';
  static const serverError = 'خطأ في الخادم — حاول لاحقاً';
  static const generic = 'تعذر إكمال العملية. حاول مرة أخرى.';

  static ApiException from(Object error) {
    if (error is ApiException) return error;
    if (error is DioException) {
      final inner = error.error;
      if (inner is ApiException) return inner;
      final status = error.response?.statusCode;
      final body = _fromBody(error.response?.data, status);
      if (body != null) return body;
      switch (error.type) {
        case DioExceptionType.connectionError:
        case DioExceptionType.connectionTimeout:
        case DioExceptionType.sendTimeout:
        case DioExceptionType.receiveTimeout:
          return ApiException(DioClient.networkMessage, isNetwork: true);
        case DioExceptionType.unknown:
          if (inner is SocketException || inner is HttpException) {
            return ApiException(DioClient.networkMessage, isNetwork: true);
          }
          break;
        default:
          break;
      }
      if (status == 401) {
        return ApiException(sessionExpired, statusCode: status);
      }
      if (status == 403) return ApiException(forbidden, statusCode: status);
      if (status != null && status >= 500) {
        return ApiException(serverError, statusCode: status);
      }
      return ApiException(generic, statusCode: status);
    }
    if (error is SocketException) {
      return ApiException(DioClient.networkMessage, isNetwork: true);
    }
    return ApiException(generic);
  }

  static String message(Object error) => from(error).message;

  static ApiException? _fromBody(Object? data, int? status) {
    if (data is! Map) return null;
    final errors = data['errors'] ?? data['Errors'];
    if (errors is List && errors.isNotEmpty) {
      final first = errors.first;
      if (first is Map) {
        final m = first['message'] ?? first['Message'];
        final c = first['code'] ?? first['Code'];
        if (m != null && '$m'.trim().isNotEmpty) {
          return ApiException(
            '$m'.trim(),
            code: c == null ? null : '$c',
            statusCode: status,
          );
        }
      }
      if (first is String && first.trim().isNotEmpty) {
        return ApiException(first.trim(), statusCode: status);
      }
    }
    if (errors is Map && errors.isNotEmpty) {
      final first = errors.values.first;
      if (first is List && first.isNotEmpty) {
        return ApiException('${first.first}', statusCode: status);
      }
      if (first is String && first.trim().isNotEmpty) {
        return ApiException(first.trim(), statusCode: status);
      }
    }
    final direct = data['message'] ?? data['Message'] ?? data['detail'];
    if (direct != null && '$direct'.trim().isNotEmpty) {
      return ApiException('$direct'.trim(), statusCode: status);
    }
    return null;
  }
}
