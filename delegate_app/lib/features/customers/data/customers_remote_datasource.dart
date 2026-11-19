import '../../../core/network/dio_client.dart';
import '../../../core/network/json_map.dart';
import '../../../core/security/secure_session_store.dart';
import '../domain/entities/customer.dart';

class CollectionBookSnapshot {
  const CollectionBookSnapshot({
    required this.customers,
    required this.averageScore,
    required this.averageLabel,
    required this.previousWeekScore,
    required this.ratedCustomerCount,
    required this.installmentSum,
    this.listId,
    this.listName,
  });

  final List<Customer> customers;
  final double averageScore;
  final String averageLabel;
  final double previousWeekScore;
  final int ratedCustomerCount;
  final double installmentSum;
  final int? listId;
  final String? listName;
}

class CustomersRemoteDataSource {
  CustomersRemoteDataSource(this._dio, this._session);

  final DioClient _dio;
  final SecureSessionStore _session;

  /// دفتر التحصيل دفعة واحدة (بدل آلاف طلبات الإثراء).
  Future<CollectionBookSnapshot> fetchCollectionBook() async {
    final userId = int.tryParse((await _session.userId) ?? '') ?? 0;
    final res = await _dio.dio.get('api/v1/app/Customers/collection-book');
    final root = res.data;
    if (root is! Map) {
      return CollectionBookSnapshot(
        customers: const [],
        averageScore: 0,
        averageLabel: 'ضعيف',
        previousWeekScore: 0,
        ratedCustomerCount: 0,
        installmentSum: 0,
      );
    }

    final m = Map<String, dynamic>.from(root);
    final rawCustomers = JsonMap.pick(m, ['customers', 'Customers']);
    final customers = <Customer>[];
    if (rawCustomers is List) {
      for (final e in rawCustomers.whereType<Map>()) {
        final row = Map<String, dynamic>.from(e);
        final id = JsonMap.asInt(row, ['customerId', 'CustomerId']) ?? 0;
        if (id <= 0) continue;
        final name = JsonMap.str(row, ['fullName', 'FullName', 'name', 'Name']);
        final shop = JsonMap.str(row, ['businessName', 'BusinessName']);
        final phone = JsonMap.str(row, ['phoneNumber', 'PhoneNumber']);
        final address = JsonMap.str(row, ['address', 'Address']);
        final saleDate = JsonMap.str(row, ['saleDate', 'SaleDate']);
        final lastPay = JsonMap.str(row, ['lastPaymentDate', 'LastPaymentDate']);
        customers.add(
          Customer(
            customerId: id,
            name: name.isEmpty ? 'زبون #$id' : name,
            shopName: shop.isEmpty ? null : shop,
            phone: phone.isEmpty ? null : phone,
            address: address.isEmpty ? null : address,
            delegateId: userId,
            salePrice: JsonMap.asDouble(row, ['amountTotalSales', 'AmountTotalSales']),
            remaining: JsonMap.asDouble(row, ['amountRemaining', 'AmountRemaining']),
            receiptTotal: JsonMap.asDouble(row, ['receiptsTotal', 'ReceiptsTotal']),
            dailyInstallment:
                JsonMap.asDouble(row, ['dailyInstallmentAmount', 'DailyInstallmentAmount']),
            dateSaleDevice: saleDate.isEmpty ? null : saleDate,
            lastPaymentDate: lastPay.isEmpty ? null : lastPay,
            activeOrderId: JsonMap.asInt(row, ['activeOrderId', 'ActiveOrderId']),
            isLegal: JsonMap.asBool(row, ['isLegal', 'IsLegal']),
            weekAmounts: [
              JsonMap.asDouble(row, ['amount1', 'Amount1']) ?? 0,
              JsonMap.asDouble(row, ['amount2', 'Amount2']) ?? 0,
              JsonMap.asDouble(row, ['amount3', 'Amount3']) ?? 0,
              JsonMap.asDouble(row, ['amount4', 'Amount4']) ?? 0,
              JsonMap.asDouble(row, ['amount5', 'Amount5']) ?? 0,
              JsonMap.asDouble(row, ['amount6', 'Amount6']) ?? 0,
              JsonMap.asDouble(row, ['amount7', 'Amount7']) ?? 0,
            ],
          ),
        );
      }
    }

    return CollectionBookSnapshot(
      customers: customers,
      averageScore: JsonMap.asDouble(m, ['averageScore', 'AverageScore']) ?? 0,
      averageLabel: JsonMap.str(m, ['averageLabel', 'AverageLabel'], fallback: 'ضعيف'),
      previousWeekScore:
          JsonMap.asDouble(m, ['previousWeekScore', 'PreviousWeekScore']) ?? 0,
      ratedCustomerCount:
          JsonMap.asInt(m, ['ratedCustomerCount', 'RatedCustomerCount']) ?? 0,
      installmentSum: JsonMap.asDouble(m, ['installmentSum', 'InstallmentSum']) ?? 0,
      listId: JsonMap.asInt(m, ['listId', 'ListId']),
      listName: () {
        final n = JsonMap.str(m, ['listName', 'ListName']);
        return n.isEmpty ? null : n;
      }(),
    );
  }

  Future<List<Customer>> fetchAllForDelegate() async {
    final book = await fetchCollectionBook();
    return book.customers;
  }
}
