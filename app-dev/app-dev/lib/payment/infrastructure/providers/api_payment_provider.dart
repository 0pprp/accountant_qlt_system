import 'package:team/common/services/request/request.dart';
import 'package:team/order/domain/order_list/order_list.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';
import 'package:team/payment/domain/installment_payments_summary/installment_payments_summary.dart';
import 'package:team/payment/domain/orders_installment/orders_installment.dart';

class ApiPaymentProvider {
  final Request request;

  ApiPaymentProvider({required this.request});

  Future<List<OrderList>> getOrdersList() async {
    final response = await request.get('api/v1/app/OrderLists');

    return (response.data as List).map((e) => OrderList.fromJson(e)).toList();
  }

  Future<List<InstallmentPayment>> getUserInstallmentPayments(final int id) async {
    final response = await request.get('api/v1/app/customers/$id/installment-payments');

    return (response.data as List).map((e) => InstallmentPayment.fromJson(e)).toList();
  }

  Future<OrdersInstallment> getOrderInstallments(final int id) async {
    final response = await request.get('api/v1/app/Orders/$id/installment-payments');

    return OrdersInstallment.fromJson(response.data);
  }

  Future<InstallmentPaymentsSummary> getInstallmentPaymentsSummary({
    final int? orderListId,
    final DateTime? startDate,
    final DateTime? endDate,
    final String? searchText,
  }) async {
    final response = await request.get(
      'api/v1/app/InstallmentPayments/summary',
      queryParameters: {
        'orderListId': orderListId,
        'startDate': startDate?.toIso8601String(),
        'endDate': endDate?.toIso8601String(),
        'fullName': searchText,
      },
    );

    return InstallmentPaymentsSummary.fromJson(response.data);
  }

  Future<List<InstallmentPayment>> getInstallmentPayments({
    final DateTime? lastDate,
    final int? orderListId,
    final DateTime? startDate,
    final DateTime? endDate,
    final String? searchText,
  }) async {
    final response = await request.get(
      'api/v1/app/InstallmentPayments',
      queryParameters: {
        'lastId': lastDate?.toIso8601String(),
        'orderListId': orderListId,
        'size': 20,
        'startDate': startDate?.toIso8601String(),
        'endDate': endDate?.toIso8601String(),
        'fullName': searchText,
      },
    );

    return (response.data as List).map((e) => InstallmentPayment.fromJson(e)).toList();
  }
}
