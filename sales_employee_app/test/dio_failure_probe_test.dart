import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/core/network/dio_failure_probe.dart';

void main() {
  group('DioFailureProbe.sanitizeMethodHeaders', () {
    test('strips Content-Type from GET (Web preflight regression)', () {
      final o = RequestOptions(
        path: 'api/v1/app/SalesRequests/counts',
        method: 'GET',
        baseUrl: 'http://127.0.0.1:4200/',
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
          'Authorization': 'Bearer secret-token-must-not-leak',
        },
      );
      o.contentType = Headers.jsonContentType;

      DioFailureProbe.sanitizeMethodHeaders(o);

      expect(o.headers.containsKey('Content-Type'), isFalse);
      expect(o.headers.containsKey('content-type'), isFalse);
      expect(o.contentType, isNull);
      // Authorization untouched by sanitize (probe.describe redacts it).
      expect(o.headers['Authorization'], 'Bearer secret-token-must-not-leak');
    });

    test('keeps Content-Type on POST', () {
      final o = RequestOptions(
        path: 'api/v1/app/Auth/login',
        method: 'POST',
        baseUrl: 'http://127.0.0.1:4200/',
        headers: {'Content-Type': 'application/json'},
      );
      o.contentType = Headers.jsonContentType;
      DioFailureProbe.sanitizeMethodHeaders(o);
      expect(o.headers['Content-Type'], 'application/json');
    });
  });

  group('DioFailureProbe.describe', () {
    test('redacts Bearer token and reports phase before_send', () {
      final req = RequestOptions(
        path: 'api/v1/app/SalesRequests/counts',
        method: 'GET',
        baseUrl: 'http://127.0.0.1:4200/',
        headers: {'Authorization': 'Bearer eyJhbGciOi.secret'},
      );
      req.extra['probe_phase'] = DioFailureProbe.phaseBeforeSend;
      final err = DioException(
        requestOptions: req,
        type: DioExceptionType.unknown,
        error: StateError('crypto'),
        message: 'session_read_failed:StateError Bearer eyJhbGciOi.secret',
      );

      final d = DioFailureProbe.describe(err);
      expect(d['uri'], 'http://127.0.0.1:4200/api/v1/app/SalesRequests/counts');
      expect(d['phase'], DioFailureProbe.phaseBeforeSend);
      expect(d['hasAuthHeader'], 'true');
      expect(d['requestHeaders']!.contains('Bearer'), isFalse);
      expect(d['message']!.contains('eyJhbGciOi.secret'), isFalse);
      expect(d['message'], contains('Bearer ***'));
    });

    test('phase after_response when status present', () {
      final req = RequestOptions(path: '/x', method: 'GET', baseUrl: 'http://127.0.0.1:4200/');
      final err = DioException(
        requestOptions: req,
        type: DioExceptionType.badResponse,
        response: Response(requestOptions: req, statusCode: 403),
      );
      expect(DioFailureProbe.phaseOf(err), DioFailureProbe.phaseAfterResponse);
    });
  });
}
