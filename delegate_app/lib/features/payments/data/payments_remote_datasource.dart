import 'package:dio/dio.dart';

import '../../../core/network/api_exception.dart';
import '../../../core/network/dio_client.dart';
import '../../../core/network/json_map.dart';
import '../../../core/security/secure_session_store.dart';
import '../domain/entities/pending_payment.dart';

class PaymentUploadResult {
  PaymentUploadResult({
    required this.success,
    this.message,
    this.duplicateAck = false,
    this.paymentId,
    this.orderId,
    this.amount,
    this.date,
    this.clientPaymentId,
  });

  final bool success;
  final String? message;
  final bool duplicateAck;
  final int? paymentId;
  final int? orderId;
  final double? amount;
  final String? date;
  final String? clientPaymentId;
}

class PaymentsRemoteDataSource {
  PaymentsRemoteDataSource(this._dio, this._session);

  final DioClient _dio;
  final SecureSessionStore _session;

  /// True when this customer already has an installment payment on [isoDate]
  /// (yyyy-MM-dd) according to the server.
  Future<bool?> customerHasPaymentOnDate({
    required int customerId,
    required String isoDate,
  }) async {
    try {
      final res = await _dio.dio.get(
        'api/v1/app/Customers/$customerId/installment-payments',
      );
      if (res.data is! List) return false;
      for (final e in (res.data as List).whereType<Map>()) {
        final d = JsonMap.str(e, ['date', 'Date']);
        if (d.isEmpty) continue;
        if (d.startsWith(isoDate)) return true;
      }
      return false;
    } catch (_) {
      return null;
    }
  }

  Future<List<Map<String, dynamic>>> fetchTodayPayments({
    required String startDate,
    required String endDate,
  }) async {
    final out = <Map<String, dynamic>>[];
    String? lastId;
    for (var page = 0; page < 30; page++) {
      final res = await _dio.dio.get(
        'api/v1/app/InstallmentPayments',
        queryParameters: {
          'size': 50,
          'startDate': startDate,
          'endDate': endDate,
          if (lastId != null) 'lastId': lastId,
        },
      );
      final data = res.data;
      if (data is! List || data.isEmpty) break;
      for (final e in data.whereType<Map>()) {
        out.add(Map<String, dynamic>.from(e));
        final lu = JsonMap.str(e, ['lastUpdatedAt', 'LastUpdatedAt']);
        if (lu.isNotEmpty) lastId = lu;
      }
      if (data.length < 50) break;
    }
    return out;
  }

  Future<int?> fetchActiveOrderId(int customerId) async {
    try {
      final res = await _dio.dio.get('api/v1/app/Customers/$customerId/orders');
      if (res.data is! List) return null;
      int? fallback;
      for (final e in (res.data as List).whereType<Map>()) {
        final id = JsonMap.asInt(e, ['id', 'Id']) ?? 0;
        if (id <= 0) continue;
        final sell = JsonMap.asDouble(e, ['sellAmount', 'SellAmount']) ?? 0;
        final paid = JsonMap.asDouble(e, ['paidAmount', 'PaidAmount']) ?? 0;
        if (sell > paid) return id;
        fallback ??= id;
      }
      return fallback;
    } catch (_) {
      return null;
    }
  }

  /// تاريخ التحصيل المعتمد من createdAtUtc (بغداد) — ثابت عبر Create و Sync.
  static String collectionDateOf(PendingPayment payment) {
    final created =
        DateTime.tryParse(payment.createdAtUtc)?.toUtc() ??
        DateTime.now().toUtc();
    final bag = created.add(const Duration(hours: 3));
    return '${bag.year.toString().padLeft(4, '0')}-'
        '${bag.month.toString().padLeft(2, '0')}-'
        '${bag.day.toString().padLeft(2, '0')}';
  }

  /// يرفع عبر Create ثم Sync عند انقطاع الشبكة فقط.
  /// الحذف المحلي يتطلب إقرار خادم يثبت نفس ClientPaymentId/OrderId/Amount.
  Future<PaymentUploadResult> postPaymentIdempotent(
    PendingPayment payment,
  ) async {
    if (!await _session.hasSession) {
      throw ApiException('الجلسة غير صالحة');
    }
    final orderId = payment.orderId;
    if (orderId == null || orderId <= 0) {
      throw ApiException(
        'لا يوجد طلب مرتبط بهذا التسديد — حدّث الزبائن دون تغيير الطلب المحفوظ',
      );
    }

    final amountInt = payment.amount.round();
    if (amountInt <= 0) {
      throw ApiException('مبلغ التسديد غير صالح');
    }

    final clientPaymentId = payment.clientPaymentId.trim();
    if (clientPaymentId.isEmpty) {
      throw ApiException('مفتاح التسديد مفقود — لا يمكن إعادة الإرسال بأمان');
    }
    final dateOnly = collectionDateOf(payment);

    try {
      final res = await _dio.dio.post(
        'api/v1/app/Orders/$orderId/installment-payments',
        data: {
          'amount': amountInt,
          'clientPaymentId': clientPaymentId,
          'date': dateOnly,
        },
      );
      return parseServerAck(
        res.data,
        expectedClientPaymentId: clientPaymentId,
        expectedOrderId: orderId,
        expectedAmount: amountInt.toDouble(),
        expectedDate: dateOnly,
        httpOk: (res.statusCode ?? 0) >= 200 && (res.statusCode ?? 0) < 300,
      );
    } on DioException catch (e) {
      final status =
          e.response?.statusCode ??
          (e.error is ApiException
              ? (e.error as ApiException).statusCode
              : null);
      final message = _dioMessage(e);

      // 409 تعارض بصمة — ليس نجاحًا مكررًا.
      if (status == 409) {
        throw ApiException(
          message ?? 'تعارض مفتاح التسديد مع بيانات مختلفة',
          statusCode: 409,
        );
      }
      // 403/400 ليست إقرار تكرار.
      if (status == 403 || status == 400 || status == 422) {
        throw ApiException(
          message ?? 'رفض السيرفر التسديد',
          statusCode: status,
        );
      }

      // انقطاع / مهلة فقط → Sync بنفس التاريخ والمفتاح والطلب.
      final networkish =
          status == null ||
          status >= 500 ||
          e.type == DioExceptionType.connectionTimeout ||
          e.type == DioExceptionType.receiveTimeout ||
          e.type == DioExceptionType.sendTimeout ||
          e.type == DioExceptionType.connectionError;
      if (!networkish) {
        throw ApiException(message ?? 'فشل الرفع', statusCode: status);
      }

      try {
        final res = await _dio.dio.post(
          'api/v1/app/InstallmentPayments/sync',
          data: {
            'installmentPayments': [
              {
                'orderId': orderId,
                'amount': amountInt,
                'date': dateOnly,
                'clientPaymentId': clientPaymentId,
              },
            ],
          },
        );
        final data = res.data;
        // Sync يرجع { items: [...] }
        dynamic first = data;
        if (data is Map) {
          final items = data['items'] ?? data['Items'];
          if (items is List && items.isNotEmpty) first = items.first;
        }
        return parseServerAck(
          first,
          expectedClientPaymentId: clientPaymentId,
          expectedOrderId: orderId,
          expectedAmount: amountInt.toDouble(),
          expectedDate: dateOnly,
          httpOk: (res.statusCode ?? 0) >= 200 && (res.statusCode ?? 0) < 300,
        );
      } on DioException catch (e2) {
        final status2 = e2.response?.statusCode ?? status;
        final msg2 = _dioMessage(e2) ?? message ?? 'فشل الرفع';
        if (status2 == 409) {
          throw ApiException(msg2, statusCode: 409);
        }
        throw ApiException(msg2, statusCode: status2);
      }
    }
  }

  /// إقرار خادم صالح فقط عند تطابق المفتاح+الطلب+المبلغ+التاريخ وPaymentId>0.
  static PaymentUploadResult parseServerAck(
    dynamic data, {
    required String expectedClientPaymentId,
    required int expectedOrderId,
    required double expectedAmount,
    required String expectedDate,
    required bool httpOk,
  }) {
    if (!httpOk) {
      return PaymentUploadResult(
        success: false,
        message: _msgStatic(data) ?? 'فشل',
      );
    }
    if (data is! Map) {
      return PaymentUploadResult(
        success: false,
        message: 'إقرار السيرفر ناقص — أبقِ التسديد في الطابور',
      );
    }
    final map = Map<String, dynamic>.from(data);
    final cpid = JsonMap.str(map, ['clientPaymentId', 'ClientPaymentId']);
    final oid = int.tryParse(JsonMap.str(map, ['orderId', 'OrderId']));
    final amount = JsonMap.asDouble(map, ['amount', 'Amount']);
    final pid = int.tryParse(JsonMap.str(map, ['paymentId', 'PaymentId']));
    final dateRaw = JsonMap.str(map, ['date', 'Date']);
    final already =
        map['alreadyApplied'] == true || map['AlreadyApplied'] == true;

    final sameKey = cpid.toLowerCase() == expectedClientPaymentId.toLowerCase();
    final sameOrder = oid != null && oid == expectedOrderId;
    final sameAmount =
        amount != null && (amount - expectedAmount).abs() < 0.005;
    final sameDate = _datesEqual(dateRaw, expectedDate);
    final validPaymentId = pid != null && pid > 0;

    if (!sameKey || !sameOrder || !sameAmount || !sameDate || !validPaymentId) {
      return PaymentUploadResult(
        success: false,
        message: 'إقرار السيرفر لا يطابق العملية المحلية',
      );
    }

    return PaymentUploadResult(
      success: true,
      duplicateAck: already,
      paymentId: pid,
      orderId: oid,
      amount: amount,
      date: dateRaw,
      clientPaymentId: cpid,
      message: already ? 'مُقرّ سلفًا' : 'تم',
    );
  }

  static bool _datesEqual(String serverDate, String expectedDate) {
    // API contract is DateOnly (yyyy-MM-dd), not a timestamp prefix.
    final dateOnly = RegExp(r'^\d{4}-\d{2}-\d{2}$');
    return dateOnly.hasMatch(serverDate) &&
        dateOnly.hasMatch(expectedDate) &&
        serverDate == expectedDate;
  }

  static String? _msgStatic(dynamic data) {
    if (data is Map) {
      return JsonMap.str(data, ['message', 'Message', 'title', 'Title']);
    }
    return null;
  }

  String? _dioMessage(DioException e) {
    if (e.error is ApiException) return (e.error as ApiException).message;
    return _msg(e.response?.data) ?? e.message;
  }

  String? _msg(dynamic data) {
    if (data is Map) {
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
      final m = JsonMap.str(data, ['message', 'Message']);
      return m.isEmpty ? null : m;
    }
    if (data is String && data.trim().isNotEmpty) return data.trim();
    return null;
  }
}
