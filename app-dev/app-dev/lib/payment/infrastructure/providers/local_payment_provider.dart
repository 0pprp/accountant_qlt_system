import 'dart:convert';

import 'package:hive_ce/hive.dart';
import 'package:team/order/domain/order_list/order_list.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';

class LocalPaymentProvider {
  late final Box _box;

  final _paymentBoxKey = 'paymentBoxKey';
  final _recentPaymentSearchesKey = 'recentPaymentSearchesKey';
  final _ordersListKey = 'ordersListKey';
  final _ordersListTimestampKey = 'ordersListTimestampKey';
  final _installmentPaymentsKey = 'installmentPaymentsKey';
  final _installmentPaymentsTimestampKey = 'installmentPaymentsTimestampKey';
  final _installmentPaymentsByOrderPrefix = 'installmentPaymentsByOrder_';

  Future<void> ensureInitialized() async {
    _box = await Hive.openBox(_paymentBoxKey);
  }

  Future<void> saveRecentSearches(List<InstallmentPayment> recentSearches) {
    final jsonList = recentSearches.map((payment) => payment.toJson()).toList();
    return _box.put(_recentPaymentSearchesKey, jsonEncode(jsonList));
  }

  List<InstallmentPayment> getRecentSearches() {
    final data = _box.get(_recentPaymentSearchesKey);

    if (data == null) return [];

    final List<dynamic> jsonList = jsonDecode(data);
    return jsonList.map((json) => InstallmentPayment.fromJson(json)).toList();
  }

  List<OrderList>? getCachedOrdersList() {
    final data = _box.get(_ordersListKey);
    if (data == null) return null;

    final List<dynamic> jsonList = jsonDecode(data);
    return jsonList.map((json) => OrderList.fromJson(json)).toList();
  }

  // New methods for caching installmentPayments with timestamp
  Future<void> saveOrdersList(List<OrderList> orders) async {
    final jsonList = orders.map((order) => order.toJson()).toList();
    await _box.put(_ordersListKey, jsonEncode(jsonList));
    await _box.put(_ordersListTimestampKey, DateTime.now().millisecondsSinceEpoch);
  }

  bool isOrdersListCacheValid() {
    final timestamp = _box.get(_ordersListTimestampKey);
    if (timestamp == null) return false;

    final cacheDate = DateTime.fromMillisecondsSinceEpoch(timestamp);
    final today = DateTime.now();

    // Check if cache is from today (same day)
    return cacheDate.year == today.year && cacheDate.month == today.month && cacheDate.day == today.day;
  }

  // New methods for caching installmentPayments orders with timestamp
  Future<void> saveInstallmentPayments(List<InstallmentPayment> payments) async {
    final jsonList = payments.map((payment) => payment.toJson()).toList();
    await _box.put(_installmentPaymentsKey, jsonEncode(jsonList));
    await _box.put(_installmentPaymentsTimestampKey, DateTime.now().millisecondsSinceEpoch);
  }

  List<InstallmentPayment> getCachedInstallmentPayments() {
    final data = _box.get(_installmentPaymentsKey);
    if (data == null) return [];

    final List<dynamic> jsonList = jsonDecode(data);
    return jsonList.map((json) => InstallmentPayment.fromJson(json)).toList();
  }

  bool isInstallmentPaymentsCacheValid() {
    final timestamp = _box.get(_installmentPaymentsTimestampKey);
    if (timestamp == null) return false;

    final cacheDate = DateTime.fromMillisecondsSinceEpoch(timestamp);
    final today = DateTime.now();

    // Check if cache is from today (same day)
    return cacheDate.year == today.year && cacheDate.month == today.month && cacheDate.day == today.day;
  }

  // ============================================================================
  // ORDER-SPECIFIC INSTALLMENT PAYMENT CACHING (LAST 7 DAYS)
  // ============================================================================

  /// Save installment payments for a specific orderId
  /// Automatically cleans up payments older than 7 days
  Future<void> saveInstallmentPaymentsByOrderId(
    int orderId,
    List<InstallmentPayment> payments,
  ) async {
    final key = '$_installmentPaymentsByOrderPrefix$orderId';

    // Get existing payments from storage
    final existingData = _box.get(key);
    List<InstallmentPayment> existingPayments = [];

    if (existingData != null) {
      final List<dynamic> jsonList = jsonDecode(existingData);
      existingPayments = jsonList.map((json) => InstallmentPayment.fromJson(json)).toList();
    }

    // Combine existing and new payments
    final allPayments = [...existingPayments, ...payments];

    // Remove duplicates based on payment ID (if available) or other unique criteria
    final Map<int, InstallmentPayment> uniquePaymentsMap = {};
    for (final payment in allPayments) {
      uniquePaymentsMap[payment.id] = payment;
    }
    final uniquePayments = uniquePaymentsMap.values.toList();

    // Filter out payments older than 7 days
    final sevenDaysAgo = DateTime.now().subtract(const Duration(days: 7));
    final recentPayments =
        uniquePayments.where((payment) {
          return payment.date.isAfter(sevenDaysAgo) || payment.date.isAtSameMomentAs(sevenDaysAgo);
        }).toList();

    // Sort by date descending (most recent first)
    recentPayments.sort((a, b) => b.date.compareTo(a.date));

    if (recentPayments.isEmpty) {
      // Delete the key entirely if no recent data
      await _box.delete(key);
    } else {
      final jsonList = recentPayments.map((payment) => payment.toJson()).toList();
      await _box.put(key, jsonEncode(jsonList));
    }
  }

  /// Get installment payments for a specific orderId from the last 7 days
  /// Returns an empty list if no data exists
  List<InstallmentPayment> getInstallmentPaymentsByOrderId(int orderId) {
    final key = '$_installmentPaymentsByOrderPrefix$orderId';
    final data = _box.get(key);

    if (data == null) return [];

    final List<dynamic> jsonList = jsonDecode(data);
    final payments = jsonList.map((json) => InstallmentPayment.fromJson(json)).toList();

    return payments;
  }

  /// Clear installment payments for a specific orderId
  Future<void> clearInstallmentPaymentsByOrderId(int orderId) async {
    final key = '$_installmentPaymentsByOrderPrefix$orderId';
    await _box.delete(key);
  }

  /// Clean up all order-specific caches, removing data older than 7 days
  Future<void> cleanupOldInstallmentPayments() async {
    final sevenDaysAgo = DateTime.now().subtract(const Duration(days: 7));

    // Get all keys that match the order-specific pattern
    final keysToClean = _box.keys.where((key) => key.toString().startsWith(_installmentPaymentsByOrderPrefix)).toList();

    for (final key in keysToClean) {
      final data = _box.get(key);
      if (data == null) continue;

      final List<dynamic> jsonList = jsonDecode(data);
      final allPayments = jsonList.map((json) => InstallmentPayment.fromJson(json)).toList();

      // Filter to only recent payments
      final recentPayments =
          allPayments.where((payment) {
            return payment.date.isAfter(sevenDaysAgo) || payment.date.isAtSameMomentAs(sevenDaysAgo);
          }).toList();

      if (recentPayments.isEmpty) {
        // Delete the key entirely if no recent data
        await _box.delete(key);
      } else if (recentPayments.length < allPayments.length) {
        // Update with cleaned data
        final jsonList = recentPayments.map((payment) => payment.toJson()).toList();
        await _box.put(key, jsonEncode(jsonList));
      }
    }
  }

  Future clear() => _box.clear();
}
