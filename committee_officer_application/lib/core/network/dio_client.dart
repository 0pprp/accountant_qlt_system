import 'package:committee_officer_application/config/app_env.dart';
import 'package:committee_officer_application/core/network/api_exception.dart';
import 'package:committee_officer_application/core/session/session_store.dart';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

typedef TokenRefresher = Future<bool> Function();

class DioClient {
  DioClient({
    required SessionStore sessionStore,
    required TokenRefresher onRefresh,
  }) : _session = sessionStore,
       _onRefresh = onRefresh {
    _dio = Dio(
      BaseOptions(
        baseUrl: AppEnv.apiBaseUrl,
        connectTimeout: const Duration(seconds: 25),
        receiveTimeout: const Duration(seconds: 45),
        followRedirects: false,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
      ),
    );
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final base = Uri.parse(AppEnv.apiBaseUrl);
          final uri = options.uri;
          if (uri.origin != base.origin ||
              ((kReleaseMode || AppEnv.requireHttps) &&
                  uri.scheme != 'https')) {
            return handler.reject(
              DioException(
                requestOptions: options,
                type: DioExceptionType.cancel,
                error: 'Untrusted API destination',
              ),
            );
          }
          options.headers.remove('Authorization');
          if (!_isPublic(options)) {
            final token = await _session.getAccessToken();
            if (token != null && token.isNotEmpty) {
              options.headers['Authorization'] = 'Bearer $token';
            }
          }
          handler.next(options);
        },
        onError: (err, handler) async {
          final request = err.requestOptions;
          if (err.response?.statusCode != 401 ||
              _isPublic(request) ||
              request.extra['sessionRetried'] == true) {
            return handler.next(err);
          }
          try {
            final current = await _session.getAccessToken();
            final alreadyRefreshed =
                current != null &&
                current.isNotEmpty &&
                request.headers['Authorization'] != 'Bearer $current';
            final pending = alreadyRefreshed
                ? Future.value(true)
                : (_refresh ??= _onRefresh());
            bool refreshed;
            try {
              refreshed = alreadyRefreshed || await pending;
            } finally {
              if (identical(_refresh, pending)) _refresh = null;
            }
            if (refreshed) {
              request.extra['sessionRetried'] = true;
              return handler.resolve(await _dio.fetch(request));
            }
          } on DioException catch (retryError) {
            return handler.next(retryError);
          } catch (_) {
            return handler.next(err);
          }
          handler.next(err);
        },
      ),
    );
  }

  late final Dio _dio;
  final SessionStore _session;
  final TokenRefresher _onRefresh;
  Future<bool>? _refresh;

  static bool _isPublic(RequestOptions options) {
    final path = options.uri.path.toLowerCase();
    return path.endsWith('/auth/login') || path.endsWith('/auth/refresh-token');
  }

  Dio get dio => _dio;

  static String mapError(Object error) {
    if (error is ApiException) return error.message;
    if (error is DioException) {
      final data = error.response?.data;
      if (data is Map) {
        final msg = data['message'] ?? data['title'] ?? data['detail'];
        if (msg != null && '$msg'.trim().isNotEmpty) return '$msg';
      }
      final code = error.response?.statusCode;
      if (code == 403) return 'الحساب غير مخوّل لتطبيق مسؤول اللجنة (403)';
      if (code == 401) return 'انتهت الجلسة. أعد تسجيل الدخول.';
      if (code == 400) return 'طلب غير صالح أو بيانات الدخول خاطئة (400)';
      if (error.type == DioExceptionType.connectionError) {
        return 'تعذر الاتصال بالخادم. تحقق من الشبكة وواجهة الـ API.';
      }
    }
    return 'تعذر إكمال العملية. حاول مرة أخرى.';
  }
}
