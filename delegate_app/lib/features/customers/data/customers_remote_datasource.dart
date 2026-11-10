import '../../../core/network/dio_client.dart';
import '../../../core/network/json_map.dart';
import '../../../core/security/secure_session_store.dart';
import '../domain/entities/customer.dart';

class CustomersRemoteDataSource {
  CustomersRemoteDataSource(this._dio, this._session);

  final DioClient _dio;
  final SecureSessionStore _session;

  Future<List<Customer>> fetchAllForDelegate() async {
    final userId = int.tryParse((await _session.userId) ?? '') ?? 0;
    final basics = await _fetchAllPages();
    if (basics.isEmpty) return [];

    final enriched = <Customer>[];
    const chunk = 6;
    for (var i = 0; i < basics.length; i += chunk) {
      final slice = basics.sublist(
        i,
        (i + chunk).clamp(0, basics.length),
      );
      final parts = await Future.wait(slice.map((c) => _enrich(c, userId)));
      enriched.addAll(parts);
    }
    return enriched;
  }

  Future<List<Customer>> _fetchAllPages() async {
    final all = <Customer>[];
    String? lastId;
    for (var page = 0; page < 50; page++) {
      final res = await _dio.dio.get(
        'api/v1/app/Customers',
        queryParameters: {
          'size': 50,
          if (lastId != null) 'lastId': lastId,
        },
      );
      final data = res.data;
      if (data is! List || data.isEmpty) break;

      for (final e in data.whereType<Map>()) {
        final id = JsonMap.asInt(e, ['id', 'Id']) ?? 0;
        if (id <= 0) continue;
        final name = JsonMap.str(e, ['fullName', 'FullName', 'name', 'Name']);
        final shop = JsonMap.str(e, [
          'businessName',
          'BusinessName',
        ]);
        final created = JsonMap.str(e, ['createdAt', 'CreatedAt']);
        all.add(
          Customer(
            customerId: id,
            name: name.isEmpty ? 'زبون #$id' : name,
            shopName: shop.isEmpty ? null : shop,
            delegateId: 0,
            dateSaleDevice: created.isEmpty ? null : created,
          ),
        );
        if (created.isNotEmpty) lastId = created;
      }

      if (data.length < 50) break;
    }
    return all;
  }

  Future<Customer> _enrich(Customer base, int userId) async {
    double? remaining;
    double? sale;
    double? paid;
    double? daily;
    int? orderId;
    String? lastPay;
    String? phone;
    String? address;

    try {
      final fin = await _dio.dio.get(
        'api/v1/app/Customers/${base.customerId}/financial-overview',
      );
      if (fin.data is Map) {
        final m = fin.data as Map;
        sale = JsonMap.asDouble(m, [
          'totalSellAmount',
          'TotalSellAmount',
        ]);
        paid = JsonMap.asDouble(m, [
          'totalPaidAmount',
          'TotalPaidAmount',
        ]);
        remaining = JsonMap.asDouble(m, [
          'totalRemainingAmount',
          'TotalRemainingAmount',
        ]);
        if (remaining == null && sale != null && paid != null) {
          remaining = sale - paid;
        }
        daily = JsonMap.asDouble(m, [
          'totalDailyInstallmentAmount',
          'TotalDailyInstallmentAmount',
        ]);
      }
    } catch (_) {}

    try {
      final detail = await _dio.dio.get(
        'api/v1/app/Customers/${base.customerId}',
      );
      if (detail.data is Map) {
        final m = detail.data as Map;
        final p = JsonMap.str(m, [
          'phoneNumber',
          'PhoneNumber',
          'phone',
          'Phone',
        ]);
        if (p.isNotEmpty) phone = p;
        final business = m['business'] ?? m['Business'];
        if (business is Map) {
          final addr = JsonMap.str(business, [
            'address',
            'Address',
            'location',
            'Location',
          ]);
          if (addr.isNotEmpty) address = addr;
        }
      }
    } catch (_) {}

    try {
      final orders = await _dio.dio.get(
        'api/v1/app/Customers/${base.customerId}/orders',
      );
      if (orders.data is List) {
        for (final e in (orders.data as List).whereType<Map>()) {
          final id = JsonMap.asInt(e, ['id', 'Id']) ?? 0;
          final sell = JsonMap.asDouble(e, ['sellAmount', 'SellAmount']) ?? 0;
          final paidAmt =
              JsonMap.asDouble(e, ['paidAmount', 'PaidAmount']) ?? 0;
          if (id > 0 && sell > paidAmt) {
            orderId = id;
            break;
          }
          if (orderId == null && id > 0) orderId = id;
        }
      }
    } catch (_) {}

    try {
      final pays = await _dio.dio.get(
        'api/v1/app/Customers/${base.customerId}/installment-payments',
      );
      if (pays.data is List && (pays.data as List).isNotEmpty) {
        final first = (pays.data as List).first;
        if (first is Map) {
          final d = JsonMap.str(first, ['date', 'Date', 'createdAt', 'CreatedAt']);
          if (d.isNotEmpty) lastPay = d;
        }
      }
    } catch (_) {}

    return Customer(
      customerId: base.customerId,
      name: base.name,
      shopName: base.shopName,
      phone: phone ?? base.phone,
      address: address ?? base.address,
      delegateId: userId,
      salePrice: sale,
      remaining: remaining,
      dailyInstallment: daily,
      receiptTotal: paid,
      dateSaleDevice: base.dateSaleDevice,
      lastPaymentDate: lastPay,
      activeOrderId: orderId,
    );
  }
}
