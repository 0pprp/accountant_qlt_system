import 'dart:convert';

import 'package:dio/dio.dart';

import '../../../core/network/api_exception.dart';
import '../../../core/network/dio_client.dart';
import '../../../core/network/json_map.dart';
import '../../../core/security/secure_session_store.dart';
import '../../../core/time/iraq_date.dart';
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
      if (res.data is! List) return null;
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

  /// تاريخ التحصيل = يوم العمل بغداد (حدود 03:00) من createdAtUtc.
  static String collectionDateOf(PendingPayment payment) {
    final created =
        DateTime.tryParse(payment.createdAtUtc)?.toUtc() ??
        (throw ApiException('تاريخ وصل القبض غير صالح'));
    return IraqDate.isoDate(created);
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
        'api/v1/app/Orders/$orderId/installment-payments/verified',
        data: {
          'amount': amountInt,
          'clientPaymentId': clientPaymentId,
          'date': dateOnly,
          'checkLegacyPayments': payment.legacyRetry,
        },
      );
      final ack = parseServerAck(
        res.data,
        expectedClientPaymentId: clientPaymentId,
        expectedOrderId: orderId,
        expectedAmount: amountInt.toDouble(),
        expectedDate: dateOnly,
        httpOk: (res.statusCode ?? 0) >= 200 && (res.statusCode ?? 0) < 300,
      );
      if (ack.success || _unwrapAckMap(res.data) != null) return ack;
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
    }
    try {
      final res = await _dio.dio.post(
        'api/v1/app/InstallmentPayments/sync/verified',
        data: {
          'installmentPayments': [
            {
              'orderId': orderId,
              'amount': amountInt,
              'date': dateOnly,
              'checkLegacyPayments': payment.legacyRetry,
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
      final status2 = e2.response?.statusCode;
      final msg2 = _dioMessage(e2) ?? 'فشل الرفع';
      if (status2 == 409) {
        throw ApiException(msg2, statusCode: 409);
      }
      throw ApiException(msg2, statusCode: status2);
    }
  }

  /// إقرار خادم صالح عند تطابق المفتاح+الطلب+المبلغ(+التاريخ إن وُجد) وPaymentId>0.
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

    final map = _unwrapAckMap(data);
    if (map == null) {
      return PaymentUploadResult(
        success: false,
        message: 'إقرار السيرفر ناقص — أبقِ التسديد في الطابور',
      );
    }

    final cpid = JsonMap.str(map, ['clientPaymentId', 'ClientPaymentId']);
    final oid = int.tryParse('${map['orderId'] ?? map['OrderId']}');
    final amount = JsonMap.asDouble(map, ['amount', 'Amount']);
    final pid = int.tryParse(
      '${map['paymentId'] ?? map['PaymentId'] ?? map['id'] ?? map['Id']}',
    );
    final dateRaw = JsonMap.str(map, ['date', 'Date']);
    final already =
        map['alreadyApplied'] == true || map['AlreadyApplied'] == true;

    final sameKey =
        cpid.isNotEmpty &&
        cpid.toLowerCase() == expectedClientPaymentId.toLowerCase();
    final sameOrder = oid != null && oid == expectedOrderId;
    final sameAmount =
        amount != null && (amount - expectedAmount).abs() < 0.005;
    final validPaymentId = pid != null && pid > 0;
    if (!sameKey ||
        !sameOrder ||
        !sameAmount ||
        !validPaymentId ||
        dateRaw != expectedDate) {
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
      date: dateRaw.isEmpty ? expectedDate : dateRaw,
      clientPaymentId: cpid.isEmpty ? expectedClientPaymentId : cpid,
      message: already ? 'مُقرّ سلفًا' : 'تم',
    );
  }

  static Map<String, dynamic>? _unwrapAckMap(dynamic data) {
    dynamic cur = data;
    for (var i = 0; i < 4; i++) {
      if (cur == null) return null;
      if (cur is String) {
        final t = cur.trim();
        if (t.isEmpty) return null;
        try {
          cur = jsonDecode(t);
        } catch (_) {
          return null;
        }
        continue;
      }
      if (cur is List) {
        if (cur.isEmpty) return null;
        cur = cur.first;
        continue;
      }
      if (cur is Map) {
        final m = Map<String, dynamic>.from(cur);
        final items = m['items'] ?? m['Items'];
        if (items is List && items.isNotEmpty) {
          cur = items.first;
          continue;
        }
        final nested =
            m['data'] ??
            m['Data'] ??
            m['value'] ??
            m['Value'] ??
            m['result'] ??
            m['Result'];
        if (nested != null &&
            nested is! num &&
            nested is! bool &&
            (nested is Map || nested is List || nested is String)) {
          // لا نفكّ إن كانت الخريطة نفسها تحمل paymentId.
          final hasPayment =
              m.containsKey('paymentId') ||
              m.containsKey('PaymentId') ||
              m.containsKey('clientPaymentId') ||
              m.containsKey('ClientPaymentId');
          if (!hasPayment) {
            cur = nested;
            continue;
          }
        }
        return m;
      }
      return null;
    }
    if (cur is Map) return Map<String, dynamic>.from(cur);
    return null;
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
