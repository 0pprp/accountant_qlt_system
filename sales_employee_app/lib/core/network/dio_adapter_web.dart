import 'package:dio/browser.dart';
import 'package:dio/dio.dart';

/// Web: non-credentialed XHR avoids DioExceptionType.unknown against local CORS.
void configureDioAdapter(Dio dio) {
  dio.httpClientAdapter = BrowserHttpClientAdapter(withCredentials: false);
}
