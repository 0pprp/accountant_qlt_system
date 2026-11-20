import 'package:dio/dio.dart';
import 'package:field_promoter_app/core/network/api_errors.dart';
import 'package:field_promoter_app/core/network/api_exception.dart';
import 'package:field_promoter_app/core/network/dio_client.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  final request = RequestOptions(path: 'api/v1/app/Auth/refresh-token');

  DioException withStatus(int code) => DioException(
    requestOptions: request,
    response: Response(requestOptions: request, statusCode: code),
  );

  test('انقطاع الشبكة أثناء التجديد لا يعني انتهاء الجلسة', () {
    for (final type in [
      DioExceptionType.connectionError,
      DioExceptionType.connectionTimeout,
      DioExceptionType.receiveTimeout,
      DioExceptionType.sendTimeout,
      DioExceptionType.unknown,
    ]) {
      expect(
        DioClient.classifyRefreshFailure(DioException(requestOptions: request, type: type)),
        RefreshOutcome.unreachable,
      );
    }
  });

  test('خطأ الخادم 5xx يبقي المروّج داخل التطبيق', () {
    expect(DioClient.classifyRefreshFailure(withStatus(500)), RefreshOutcome.unreachable);
    expect(DioClient.classifyRefreshFailure(withStatus(502)), RefreshOutcome.unreachable);
  });

  test('رفض الخادم 400/401 وحده انتهاء الجلسة', () {
    expect(DioClient.classifyRefreshFailure(withStatus(400)), RefreshOutcome.rejected);
    expect(DioClient.classifyRefreshFailure(withStatus(401)), RefreshOutcome.rejected);
  });

  test('رسالة فشل التجديد بسبب الشبكة هي «لا يوجد اتصال بالإنترنت»', () {
    final err = DioException(
      requestOptions: request,
      type: DioExceptionType.connectionError,
      error: ApiException(DioClient.networkMessage, isNetwork: true),
    );
    expect(ApiErrors.message(err), 'لا يوجد اتصال بالإنترنت');
    expect(
      ApiErrors.message(DioException(requestOptions: request, type: DioExceptionType.connectionTimeout)),
      'لا يوجد اتصال بالإنترنت',
    );
  });

  test('رسالة الخادم العربية تُعرض كما هي مع الرمز', () {
    final err = DioException(
      requestOptions: request,
      response: Response(
        requestOptions: request,
        statusCode: 400,
        data: {
          'errors': [
            {'message': 'يوجد طلب مفتوح لنفس الزبون ونفس المنتج', 'code': 'FieldPromoter.DuplicateOpenOrder'},
          ],
        },
      ),
    );
    final api = ApiErrors.from(err);
    expect(api.message, 'يوجد طلب مفتوح لنفس الزبون ونفس المنتج');
    expect(api.code, 'FieldPromoter.DuplicateOpenOrder');
    expect(api.isRetryable, isFalse);
  });

  test('مسارات الدخول لا تُرفق رمز الجلسة ولا تُجدّد', () {
    expect(DioClient.isAnonymousPath('/api/v1/app/Auth/login'), isTrue);
    expect(DioClient.isAnonymousPath('/api/v1/app/Auth/refresh-token'), isTrue);
    expect(DioClient.isAuthPath('/api/v1/app/Auth/logout'), isTrue);
    expect(DioClient.isAuthPath('/api/v1/app/field-promoter/me'), isFalse);
  });

  test('HTTPS إلزامي ووجهة الخادم فقط', () {
    final base = Uri.parse('https://api.qalataldhaman.com/');
    expect(
      DioClient.isTrustedDestination(
        Uri.parse('https://api.qalataldhaman.com/api/v1/app/field-promoter/me'),
        base,
        requireHttps: true,
      ),
      isTrue,
    );
    expect(
      DioClient.isTrustedDestination(Uri.parse('https://evil.example/x'), base, requireHttps: true),
      isFalse,
    );
    final httpBase = Uri.parse('http://10.0.2.2:15999/');
    expect(
      DioClient.isTrustedDestination(Uri.parse('http://10.0.2.2:15999/api'), httpBase, requireHttps: true),
      isFalse,
    );
    expect(
      DioClient.isTrustedDestination(Uri.parse('http://10.0.2.2:15999/api'), httpBase, requireHttps: false),
      isTrue,
    );
  });
}
