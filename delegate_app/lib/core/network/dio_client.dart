import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

import '../config/app_env.dart';
import '../security/secure_session_store.dart';
import 'api_exception.dart';

class DioClient {
  DioClient(this._session) {
    _dio = Dio(
      BaseOptions(
        baseUrl: AppEnv.apiBaseUrl,
        connectTimeout: const Duration(seconds: 25),
        receiveTimeout: const Duration(seconds: 60),
        sendTimeout: const Duration(seconds: 45),
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
        },
      ),
    );
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final uri = options.uri;
          if (AppEnv.requireHttps && uri.scheme != 'https') {
            return handler.reject(
              DioException(
                requestOptions: options,
                error: ApiException('الاتصال غير آمن — HTTPS فقط'),
              ),
            );
          }
          options.extra['req_started_ms'] =
              DateTime.now().millisecondsSinceEpoch;
          // Never attach expired-session Bearer on the refresh call itself.
          final isRefresh = options.path.contains('Auth/refresh-token');
          if (!isRefresh) {
            final token = await _session.accessToken;
            if (token != null && token.isNotEmpty) {
              options.headers['Authorization'] = 'Bearer $token';
            }
          }
          handler.next(options);
        },
        onResponse: (response, handler) {
          _logTiming(response.requestOptions, ok: true);
          handler.next(response);
        },
        onError: (e, handler) async {
          _logTiming(e.requestOptions, ok: false);

          if (e.response?.statusCode == 401 &&
              !e.requestOptions.path.contains('Auth/refresh-token') &&
              !e.requestOptions.path.contains('Auth/login') &&
              e.requestOptions.extra['auth_retried'] != true) {
            final refreshed = await _tryRefreshToken();
            if (refreshed) {
              try {
                final req = e.requestOptions;
                req.extra['auth_retried'] = true;
                final token = await _session.accessToken;
                if (token != null && token.isNotEmpty) {
                  req.headers['Authorization'] = 'Bearer $token';
                }
                final clone = await _dio.fetch(req);
                return handler.resolve(clone);
              } catch (_) {
                // Fall through to mapped error.
              }
            }
          }

          final msg = _mapError(e);
          handler.reject(
            DioException(
              requestOptions: e.requestOptions,
              response: e.response,
              type: e.type,
              error: ApiException(msg, statusCode: e.response?.statusCode),
            ),
          );
        },
      ),
    );
  }

  final SecureSessionStore _session;
  late final Dio _dio;
  Dio get dio => _dio;

  bool _refreshing = false;
  Future<bool>? _refreshFuture;

  final List<int> recentDurationsMs = <int>[];

  Future<bool> _tryRefreshToken() async {
    if (_refreshing && _refreshFuture != null) {
      return _refreshFuture!;
    }
    _refreshing = true;
    _refreshFuture = () async {
      try {
        final refresh = await _session.refreshToken;
        if (refresh == null || refresh.isEmpty) return false;
        // Use a bare Dio call to avoid interceptor recursion.
        final bare = Dio(
          BaseOptions(
            baseUrl: AppEnv.apiBaseUrl,
            connectTimeout: const Duration(seconds: 20),
            receiveTimeout: const Duration(seconds: 20),
            headers: {
              'Accept': 'application/json',
              'Content-Type': 'application/json',
            },
          ),
        );
        final res = await bare.post(
          'api/v1/app/Auth/refresh-token',
          data: {'refreshToken': refresh},
        );
        final data = res.data;
        if (data is! Map) return false;
        final access = '${data['accessToken'] ?? data['AccessToken'] ?? ''}';
        final nextRefresh =
            '${data['refreshToken'] ?? data['RefreshToken'] ?? ''}';
        if (access.isEmpty || nextRefresh.isEmpty) return false;
        await _session.saveTokens(
          accessToken: access,
          refreshToken: nextRefresh,
        );
        return true;
      } catch (_) {
        return false;
      } finally {
        _refreshing = false;
        _refreshFuture = null;
      }
    }();
    return _refreshFuture!;
  }

  void _logTiming(RequestOptions options, {required bool ok}) {
    final started = options.extra['req_started_ms'];
    if (started is! int) return;
    final ms = DateTime.now().millisecondsSinceEpoch - started;
    recentDurationsMs.add(ms);
    if (recentDurationsMs.length > 40) {
      recentDurationsMs.removeAt(0);
    }
    debugPrint(
      '[HTTP ${ok ? 'OK' : 'ERR'}] ${options.method} ${options.uri.path} ${ms}ms',
    );
  }

  String _mapError(DioException e) {
    if (e.error is ApiException) {
      return (e.error as ApiException).message;
    }
    final fromBody = _messageFromBody(e.response?.data);
    if (fromBody != null) return fromBody;

    if (e.type == DioExceptionType.connectionError ||
        e.type == DioExceptionType.connectionTimeout) {
      return 'لا يوجد اتصال بالإنترنت';
    }
    final code = e.response?.statusCode;
    if (code == 401) return 'انتهت الجلسة — سجّل الدخول مجدداً';
    if (code == 403) return 'ليس لديك صلاحية لتنفيذ العملية';
    if (code == 400) return 'طلب غير صالح — تحقق من البيانات';
    if (code != null && code >= 500) return 'خطأ في الخادم — حاول لاحقاً';
    return 'حدث خطأ في الاتصال';
  }

  String? _messageFromBody(dynamic data) {
    if (data is! Map) {
      if (data is String && data.trim().isNotEmpty) return data.trim();
      return null;
    }
    final direct = data['message'] ?? data['Message'] ?? data['title'];
    if (direct != null && '$direct'.trim().isNotEmpty) {
      final t = '$direct'.trim();
      // ASP.NET generic validation title — prefer nested errors.
      if (t != 'One or more validation errors occurred.') return t;
    }

    final errors = data['errors'] ?? data['Errors'];
    if (errors is List && errors.isNotEmpty) {
      final first = errors.first;
      if (first is Map) {
        final m = first['message'] ?? first['Message'];
        if (m != null && '$m'.trim().isNotEmpty) return '$m'.trim();
      }
      if (first is String && first.trim().isNotEmpty) return first.trim();
    }
    if (errors is Map && errors.isNotEmpty) {
      final first = errors.values.first;
      if (first is List && first.isNotEmpty) return '${first.first}';
      if (first is String && first.trim().isNotEmpty) return first.trim();
    }
    return null;
  }
}
