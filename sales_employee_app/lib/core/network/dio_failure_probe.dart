import 'package:dio/dio.dart';

/// تشخيص فشل Dio دون تسجيل tokens أو أجسام Authorization.
abstract final class DioFailureProbe {
  static const phaseBeforeSend = 'before_send';
  static const phaseTransport = 'transport';
  static const phaseAfterResponse = 'after_response';

  /// مرحلة الفشل: قبل الإرسال / أثناء النقل / بعد استجابة HTTP.
  static String phaseOf(DioException e) {
    final marked = e.requestOptions.extra['probe_phase']?.toString();
    if (e.response != null) return phaseAfterResponse;
    if (marked == 'sending' || marked == phaseTransport) return phaseTransport;
    if (e.type == DioExceptionType.connectionTimeout ||
        e.type == DioExceptionType.sendTimeout ||
        e.type == DioExceptionType.receiveTimeout ||
        e.type == DioExceptionType.connectionError) {
      return phaseTransport;
    }
    if (marked == phaseBeforeSend || marked == null) {
      // استثناء في interceptor قبل next، أو فشل بدون استجابة
      if (e.error != null && e.response == null && marked == phaseBeforeSend) {
        return phaseBeforeSend;
      }
      if (e.type == DioExceptionType.unknown && e.response == null) {
        return marked == 'sending' ? phaseTransport : phaseBeforeSend;
      }
    }
    return phaseTransport;
  }

  /// ملخص آمن للتشخيص (لا يتضمن Authorization أو access_token).
  static Map<String, String> describe(DioException e) {
    final headers = Map<String, dynamic>.from(e.requestOptions.headers);
    headers.remove('Authorization');
    headers.remove('authorization');
    return {
      'type': e.type.name,
      'method': e.requestOptions.method,
      'uri': e.requestOptions.uri.toString(),
      'status': '${e.response?.statusCode ?? ''}',
      'phase': phaseOf(e),
      'errorType': e.error?.runtimeType.toString() ?? '',
      'message': (e.message ?? '').replaceAll(RegExp(r'Bearer\s+\S+', caseSensitive: false), 'Bearer ***'),
      'requestHeaders': headers.toString(),
      'hasAuthHeader': e.requestOptions.headers.containsKey('Authorization').toString(),
      'contentType': '${e.requestOptions.contentType ?? e.requestOptions.headers['Content-Type'] ?? ''}',
      'responseType': e.requestOptions.responseType.toString(),
      'followRedirects': e.requestOptions.followRedirects.toString(),
    };
  }

  /// سياسة Web: لا تضع Content-Type على GET/HEAD حتى لا تُفرض preflight زائدة.
  static void sanitizeMethodHeaders(RequestOptions options) {
    final m = options.method.toUpperCase();
    if (m == 'GET' || m == 'HEAD' || m == 'OPTIONS') {
      options.headers.remove('Content-Type');
      options.headers.remove('content-type');
      options.contentType = null;
    }
  }
}
