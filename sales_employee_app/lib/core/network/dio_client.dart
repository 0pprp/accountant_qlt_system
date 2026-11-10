import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:sales_employee_app/core/config/app_env.dart';
import 'package:sales_employee_app/core/network/api_exception.dart';
import 'package:sales_employee_app/core/network/dio_adapter_stub.dart'
    if (dart.library.html) 'package:sales_employee_app/core/network/dio_adapter_web.dart';
import 'package:sales_employee_app/core/network/dio_failure_probe.dart';
import 'package:sales_employee_app/core/session/session_store.dart';

typedef TokenRefresher = Future<bool> Function();

class DioClient {
  DioClient({
    required SessionStore sessionStore,
    required TokenRefresher onRefresh,
  })  : _session = sessionStore,
        _onRefresh = onRefresh {
    _dio = Dio(
      BaseOptions(
        baseUrl: AppEnv.apiBaseUrl,
        connectTimeout: const Duration(seconds: 25),
        receiveTimeout: const Duration(seconds: 45),
        // لا تضع Content-Type عامًا — GET/HEAD يجب ألا يحملا application/json (preflight زائد على Web).
        headers: {'Accept': 'application/json'},
        contentType: Headers.jsonContentType,
        responseType: ResponseType.json,
        followRedirects: true,
        validateStatus: (s) => s != null && s >= 200 && s < 400,
      ),
    );
    configureDioAdapter(_dio);
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          options.extra['probe_phase'] = DioFailureProbe.phaseBeforeSend;
          DioFailureProbe.sanitizeMethodHeaders(options);

          if (AppEnv.requireHttps &&
              options.uri.scheme != 'https' &&
              !AppEnv.isLoopbackHost) {
            return handler.reject(
              DioException(
                requestOptions: options,
                error: ApiException('HTTPS required'),
                type: DioExceptionType.badResponse,
                message: 'HTTPS required',
              ),
            );
          }

          try {
            final token = await _session.getAccessToken();
            if (token != null && token.isNotEmpty) {
              options.headers['Authorization'] = 'Bearer $token';
            }
          } catch (e, st) {
            // فشل قراءة الجلسة قبل الإرسال — ليس CORS/شبكة؛ كان يظهر كـ DioExceptionType.unknown.
            if (kDebugMode) {
              debugPrint(
                'Dio session_read_failed uri=${options.uri} errorType=${e.runtimeType}',
              );
              debugPrint('$st');
            }
            return handler.reject(
              DioException(
                requestOptions: options,
                error: e,
                type: DioExceptionType.unknown,
                message: 'session_read_failed:${e.runtimeType}',
              ),
            );
          }

          options.extra['probe_phase'] = 'sending';
          handler.next(options);
        },
        onResponse: (response, handler) {
          response.requestOptions.extra['probe_phase'] = DioFailureProbe.phaseAfterResponse;
          handler.next(response);
        },
        onError: (err, handler) async {
          if (kDebugMode) {
            final probe = DioFailureProbe.describe(err);
            debugPrint('DioFailureProbe $probe');
            if (err.stackTrace != null) {
              debugPrint('${err.stackTrace}');
            }
          }

          if (err.response?.statusCode == 401 && !_refreshing) {
            _refreshing = true;
            try {
              final ok = await _onRefresh();
              if (ok) {
                final token = await _session.getAccessToken();
                final req = err.requestOptions;
                req.headers['Authorization'] = 'Bearer $token';
                DioFailureProbe.sanitizeMethodHeaders(req);
                final clone = await _dio.fetch(req);
                _refreshing = false;
                return handler.resolve(clone);
              }
            } catch (_) {
              // fall through
            } finally {
              _refreshing = false;
            }
          }
          handler.next(err);
        },
      ),
    );
  }

  late final Dio _dio;
  final SessionStore _session;
  final TokenRefresher _onRefresh;
  bool _refreshing = false;

  Dio get dio => _dio;

  static String mapError(Object error) {
    if (error is ApiException) return error.message;
    if (error is DioException) {
      final msg = error.message ?? '';
      if (msg.startsWith('session_read_failed')) {
        return 'تعذر قراءة الجلسة المحلية. أعد تسجيل الدخول.';
      }
      final code = error.response?.statusCode;
      final data = error.response?.data;
      if (data is Map) {
        final m = data['message'] ?? data['title'];
        if (m != null && '$m'.trim().isNotEmpty) return '$m';
        final errors = data['errors'];
        if (errors is List && errors.isNotEmpty) {
          final first = errors.first;
          if (first is Map && first['message'] != null) return '${first['message']}';
          if (first is String) return first;
        }
        if (errors is Map && errors.isNotEmpty) {
          final firstVal = errors.values.first;
          if (firstVal is List && firstVal.isNotEmpty) return '${firstVal.first}';
          return '$firstVal';
        }
      }
      if (code == 403) {
        return 'لا تملك صلاحية لهذا الإجراء (403). تأكد من صلاحيات حساب موظف المبيعات على الخادم.';
      }
      if (code == 401) {
        return 'انتهت الجلسة. أعد تسجيل الدخول.';
      }
      if (code == 400) {
        return 'اسم المستخدم أو كلمة المرور غير صحيحة، أو الطلب غير صالح (400).';
      }
      if (code == 404) {
        return 'الخدمة غير متوفرة على الخادم بعد (404).';
      }
      switch (error.type) {
        case DioExceptionType.connectionTimeout:
        case DioExceptionType.receiveTimeout:
        case DioExceptionType.sendTimeout:
          return 'انتهت مهلة الاتصال بالخادم';
        case DioExceptionType.connectionError:
          return 'تعذر الاتصال بالخادم';
        default:
          return 'حدث خطأ في الشبكة (${code ?? '-'})';
      }
    }
    return 'حدث خطأ غير متوقع';
  }
}
