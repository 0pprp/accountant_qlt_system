import 'package:delegate_app/core/network/dio_client.dart';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  final request = RequestOptions(path: 'api/v1/app/Auth/refresh-token');

  test('انقطاع الشبكة أثناء التجديد لا يعني انتهاء الجلسة', () {
    final offline = DioException(requestOptions: request, type: DioExceptionType.connectionError);
    final timeout = DioException(requestOptions: request, type: DioExceptionType.receiveTimeout);
    expect(DioClient.classifyRefreshFailure(offline), RefreshOutcome.unreachable);
    expect(DioClient.classifyRefreshFailure(timeout), RefreshOutcome.unreachable);
  });

  test('خطأ الخادم 5xx يبقي المندوب داخل التطبيق', () {
    final serverDown = DioException(
      requestOptions: request,
      response: Response(requestOptions: request, statusCode: 502),
    );
    expect(DioClient.classifyRefreshFailure(serverDown), RefreshOutcome.unreachable);
  });

  test('رفض الخادم للمفتاح هو وحده انتهاء الجلسة', () {
    final rejected = DioException(
      requestOptions: request,
      response: Response(requestOptions: request, statusCode: 400),
    );
    expect(DioClient.classifyRefreshFailure(rejected), RefreshOutcome.rejected);
  });
}
