import 'package:flutter/foundation.dart';
import 'package:team/order/domain/order_list/order_list.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';
import 'package:team/payment/domain/installment_payments_summary/installment_payments_summary.dart';
import 'package:team/payment/domain/orders_installment/orders_installment.dart';
import 'package:team/payment/infrastructure/providers/api_payment_provider.dart';
import 'package:team/payment/infrastructure/providers/local_payment_provider.dart';

class PaymentRepository {
  final ApiPaymentProvider apiPaymentProvider;
  final LocalPaymentProvider localPaymentProvider;

  PaymentRepository({required this.apiPaymentProvider, required this.localPaymentProvider});

  Future<void> ensureInitialized() => localPaymentProvider.ensureInitialized();

  Future<List<InstallmentPayment>> getUserInstallmentPayments(final int userId) =>
      apiPaymentProvider.getUserInstallmentPayments(userId);

  List<InstallmentPayment> getRecentSearches() => localPaymentProvider.getRecentSearches();

  Future<void> saveRecentSearches(final List<InstallmentPayment> recentSearches) =>
      localPaymentProvider.saveRecentSearches(recentSearches);

  Future<OrdersInstallment> getOrderInstallments(final int id) => apiPaymentProvider.getOrderInstallments(id);

  Future<InstallmentPaymentsSummary> getInstallmentPaymentsSummary({
    final int? orderListId,
    final DateTime? startDate,
    final DateTime? endDate,
    final String? searchText,
  }) => apiPaymentProvider.getInstallmentPaymentsSummary(
    searchText: searchText,
    endDate: endDate,
    startDate: startDate,
    orderListId: orderListId,
  );

  Future<List<InstallmentPayment>> getInstallmentPayments({
    final DateTime? lastId,
    final int? orderListId,
    final DateTime? startDate,
    final DateTime? endDate,
    final String? searchText,
  }) async {
    try {
      // Fetch fresh data from server
      final payments = await apiPaymentProvider.getInstallmentPayments(
        lastDate: lastId,
        orderListId: orderListId,
        startDate: startDate,
        endDate: endDate,
        searchText: searchText,
      );

      // Cache the fresh data
      await localPaymentProvider.saveInstallmentPayments(payments);
      return payments;
    } catch (e) {
      final cachedPayments = localPaymentProvider.getCachedInstallmentPayments();
      if (!localPaymentProvider.isInstallmentPaymentsCacheValid() || cachedPayments.isEmpty) {
        rethrow;
      }

      // Apply filters to cached data
      final filteredPayments =
          cachedPayments.where((payment) {
            // Filter by orderListId
            if (orderListId != null && payment.orderListId != orderListId) {
              return false;
            }

            // Filter by full name (case-insensitive search)
            if (searchText != null && searchText.isNotEmpty) {
              final fullName = payment.customerFullName.toLowerCase();
              final search = searchText.toLowerCase();
              if (!fullName.contains(search)) {
                return false;
              }
            }

            // Filter by date range
            if (startDate != null || endDate != null) {
              final paymentDate = DateTime(
                payment.lastUpdatedAt.year,
                payment.lastUpdatedAt.month,
                payment.lastUpdatedAt.day,
              );

              if (startDate != null) {
                final start = DateTime(startDate.year, startDate.month, startDate.day);
                if (paymentDate.isBefore(start)) {
                  return false;
                }
              }

              if (endDate != null) {
                final end = DateTime(endDate.year, endDate.month, endDate.day);
                if (paymentDate.isAfter(end)) {
                  return false;
                }
              }
            }

            return true;
          }).toList();

      // Sort the filtered results by date descending
      filteredPayments.sort((a, b) => b.lastUpdatedAt.compareTo(a.lastUpdatedAt));

      return filteredPayments;
    }
  }

  Future<List<OrderList>> getOrdersList() async {
    // Check if we have valid cached data from today
    final cachedOrdersList = localPaymentProvider.getCachedOrdersList();
    if (cachedOrdersList != null) {
      try {
        apiPaymentProvider.getOrdersList().then((serverUser) async {
          await localPaymentProvider.saveOrdersList(cachedOrdersList);
        });
      } catch (e) {
        debugPrint(e.toString());
      }
      return cachedOrdersList;
    }

    // Fetch fresh data from server if cache is invalid or empty
    final serverOrdersList = await apiPaymentProvider.getOrdersList();
    // Cache the fresh data
    await localPaymentProvider.saveOrdersList(serverOrdersList);

    return serverOrdersList;
  }
}
