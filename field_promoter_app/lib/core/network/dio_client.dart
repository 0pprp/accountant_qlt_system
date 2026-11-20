import 'package:dio/dio.dart';

import '../../config/app_env.dart';
import '../session/session_store.dart';
import 'api_exception.dart';

enum RefreshOutcome { ok, rejected, unreachable }

typedef TokenRefresher = Future<RefreshOutcome> Function();

class DioClient {
  DioClient({
    required SessionStore sessionStore,
    required this._onRefresh,
    this._onSessionExpired,
  }) : _session = sessionStore {
    _dio = Dio(
      BaseOptions(
        baseUrl: AppEnv.apiBaseUrl,
        connectTimeout: const Duration(seconds: 25),
        receiveTimeout: const Duration(seconds: 45),
        sendTimeout: const Duration(seconds: 60),
        followRedirects: false,
        contentType: Headers.jsonContentType,
        headers: {'Accept': 'application/json'},
      ),
    );
    _dio.interceptors.add(
      InterceptorsWrapper(onRequest: _onRequest, onError: _onError),
    );
  }

  static const networkMessage = 'لا يوجد اتصال بالإنترنت';

  late final Dio _dio;
  final SessionStore _session;
  final TokenRefresher _onRefresh;
  void Function()? _onSessionExpired;
  Future<RefreshOutcome>? _refresh;

  Dio get dio => _dio;

  set onSessionExpired(void Function()? callback) =>
      _onSessionExpired = callback;

  static bool isAuthPath(String path) {
    final p = path.toLowerCase();
    return p.endsWith('/auth/login') ||
        p.endsWith('/auth/refresh-token') ||
        p.endsWith('/auth/logout');
  }

  static bool isTrustedDestination(
    Uri uri,
    Uri base, {
    required bool requireHttps,
  }) {
    if (uri.origin != base.origin) return false;
    if (requireHttps && uri.scheme != 'https') return false;
    return true;
  }

  static bool isAnonymousPath(String path) {
    final p = path.toLowerCase();
    return p.endsWith('/auth/login') || p.endsWith('/auth/refresh-token');
  }

  static RefreshOutcome classifyRefreshFailure(Object error) {
    if (error is DioException) {
      final code = error.response?.statusCode;
      if (code == 400 || code == 401 || code == 403) {
        return RefreshOutcome.rejected;
      }
      return RefreshOutcome.unreachable;
    }
    return RefreshOutcome.rejected;
  }

  Future<void> _onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final base = Uri.parse(AppEnv.apiBaseUrl);
    if (!isTrustedDestination(
      options.uri,
      base,
      requireHttps: AppEnv.requireHttps,
    )) {
      return handler.reject(
        DioException(
          requestOptions: options,
          type: DioExceptionType.cancel,
          error: ApiException('الاتصال غير آمن — HTTPS فقط'),
        ),
      );
    }
    options.headers.remove('Authorization');
    if (!isAnonymousPath(options.uri.path)) {
      final token = await _session.getAccessToken();
      if (token != null && token.isNotEmpty) {
        options.headers['Authorization'] = 'Bearer $token';
      }
    }
    handler.next(options);
  }

  Future<void> _onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    final request = err.requestOptions;
    if (err.response?.statusCode != 401 ||
        isAuthPath(request.uri.path) ||
        request.extra['sessionRetried'] == true) {
      return handler.next(err);
    }
    final current = await _session.getAccessToken();
    final alreadyRefreshed =
        current != null &&
        current.isNotEmpty &&
        request.headers['Authorization'] != 'Bearer $current';
    RefreshOutcome outcome;
    if (alreadyRefreshed) {
      outcome = RefreshOutcome.ok;
    } else {
      final pending = _refresh ??= _onRefresh();
      try {
        outcome = await pending;
      } catch (e) {
        outcome = classifyRefreshFailure(e);
      } finally {
        if (identical(_refresh, pending)) _refresh = null;
      }
    }
    switch (outcome) {
      case RefreshOutcome.ok:
        try {
          request.extra['sessionRetried'] = true;
          final body = request.data;
          if (body is FormData) request.data = body.clone();
          return handler.resolve(await _dio.fetch(request));
        } on DioException catch (retryError) {
          return handler.next(retryError);
        }
      case RefreshOutcome.unreachable:
        return handler.reject(
          DioException(
            requestOptions: request,
            type: DioExceptionType.connectionError,
            error: ApiException(networkMessage, isNetwork: true),
          ),
        );
      case RefreshOutcome.rejected:
        _onSessionExpired?.call();
        return handler.next(err);
    }
  }
}
