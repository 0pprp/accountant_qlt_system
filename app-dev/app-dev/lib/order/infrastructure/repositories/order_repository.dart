import 'package:dio/dio.dart';
import 'package:flutter/cupertino.dart';
import 'package:get_it/get_it.dart';
import 'package:image_picker/image_picker.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/account/mapper.dart';
import 'package:team/home/infrastructure/providers/local_home_provider.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';
import 'package:team/order/domain/collect_installment/collect_installment.dart';
import 'package:team/order/domain/order/order.dart';
import 'package:team/order/domain/order_details/order_details.dart';
import 'package:team/order/domain/order_item_details/order_item_details.dart';
import 'package:team/order/domain/order_list/order_list.dart';
import 'package:team/order/domain/order_status.dart';
import 'package:team/order/domain/product/product.dart';
import 'package:team/order/domain/product_category/product_category.dart';
import 'package:team/order/infrastructure/providers/api_order_provider.dart';
import 'package:team/order/infrastructure/providers/local_order_provider.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';
import 'package:team/payment/infrastructure/providers/local_payment_provider.dart';

class OrderRepository {
  final ApiOrderProvider apiOrderProvider;
  final LocalOrderProvider localOrderProvider;

  OrderRepository({required this.apiOrderProvider, required this.localOrderProvider});

  Future<void> ensureInitialized() => localOrderProvider.ensureInitialized();

  Future<List<Order>> getMandoobOrders({
    final int? orderListId,
    final DateTime? startDate,
    final DateTime? endDate,
    final String? searchText,
    final List<ExecutionStatus>? executionStatuses,
  }) async {
    try {
      // Fetch fresh data from server if cache is invalid or empty
      final orders = await apiOrderProvider.getMandoobOrders(
        orderListId: orderListId,
        startDate: startDate,
        endDate: endDate,
        searchText: searchText,
        executionStatuses: executionStatuses,
      );

      // Cache the fresh data
      if (orders.isNotEmpty &&
          orderListId == null &&
          startDate == null &&
          endDate == null &&
          searchText == null &&
          (executionStatuses == null ||
              (executionStatuses.length == 2 &&
                  executionStatuses.contains(ExecutionStatus.notStarted) &&
                  executionStatuses.contains(ExecutionStatus.inProgress)))) {
        await localOrderProvider.saveMandoobOrders(orders);
      }
      return orders;
    } catch (e) {
      final cachedOrders = localOrderProvider.getCachedMandoobOrders();
      if (!localOrderProvider.isMandoobOrdersCacheValid() || cachedOrders == null) {
        rethrow;
      }

      final statuses =
          executionStatuses ??
          const [ExecutionStatus.notStarted, ExecutionStatus.inProgress];

      // Apply filters to cached data
      final filteredOrders =
          cachedOrders.where((order) {
            // Filter by orderListId
            if (orderListId != null && order.orderListId != orderListId) {
              return false;
            }

            // Filter by execution status
            if (order.executionStatus != null && !statuses.contains(order.executionStatus)) {
              return false;
            }

            // Filter by full name (case-insensitive search)
            if (searchText != null && searchText.isNotEmpty) {
              final fullName = order.customer.fullName.toLowerCase();
              final search = searchText.toLowerCase();
              if (!fullName.contains(search)) {
                return false;
              }
            }

            // Filter by date range
            if (startDate != null || endDate != null) {
              final orderDate = DateTime(order.createdAt.year, order.createdAt.month, order.createdAt.day);

              if (startDate != null) {
                final start = DateTime(startDate.year, startDate.month, startDate.day);
                if (orderDate.isBefore(start)) {
                  return false;
                }
              }

              if (endDate != null) {
                final end = DateTime(endDate.year, endDate.month, endDate.day);
                if (orderDate.isAfter(end)) {
                  return false;
                }
              }
            }

            return true;
          }).toList();

      // Sort the filtered results
      filteredOrders.sort((a, b) {
        final aCollected = a.installmentPaymentId != null;
        final bCollected = b.installmentPaymentId != null;

        // Prioritize non-collected first
        if (aCollected != bCollected) return aCollected ? 1 : -1;

        // Otherwise, sort by date descending
        return b.createdAt.compareTo(a.createdAt);
      });

      return filteredOrders;
    }
  }

  Future<List<OrderDetails>> getUserOrders(final int id) => apiOrderProvider.getUserOrders(id);

  Future<void> completedAttachments(int orderId) => apiOrderProvider.completedAttachments(orderId);

  List<Order> getRecentSearches() => localOrderProvider.getRecentSearches();

  List<CollectInstallment> getOfflineCollects() => localOrderProvider.getOfflineCollects();

  Future<List<ProductCategory>> getProductCategories({final String? searchText, final DateTime? lastDateTime}) =>
      apiOrderProvider.getProductCategories(searchText: searchText, lastDateTime: lastDateTime);

  Future<List<OrderList>> getOrdersList() => apiOrderProvider.getOrdersList();

  Future<List<Product>> getProducts({final String? searchText, final DateTime? lastDateTime, final int? categoryId}) =>
      apiOrderProvider.getProducts(searchText: searchText, lastDateTime: lastDateTime, categoryId: categoryId);

  Future<void> saveRecentSearches(final List<Order> recentSearches) =>
      localOrderProvider.saveRecentSearches(recentSearches);

  Future<OrderDetails> getOrderDetails(final int id) => apiOrderProvider.getOrderDetails(id);

  Future<void> saveOfflineCollects(List<CollectInstallment> collects) =>
      localOrderProvider.saveOfflineCollect(collects);

  Future<void> _updateCachedDataBasedOnCollect(
    CollectInstallment collectInstallment,
    Order order, {
    required bool isOffline,
  }) async {
    try {
      //update orders cache
      final cachedOrders = localOrderProvider.getCachedMandoobOrders();
      final updatedOrders =
          cachedOrders?.map((order) {
            if (order.installmentPaymentId == collectInstallment.orderId) {
              final updatedOrder = order.copyWith(
                isCollectedOffline: isOffline,
                paidAmount: order.paidAmount + collectInstallment.amount,
                // installmentPaymentId: collectInstallment.installmentPaymentId,
              );
              return updatedOrder;
            }
            return order;
          }).toList();
      await localOrderProvider.saveMandoobOrders(updatedOrders ?? []);
    } catch (e) {
      debugPrint(e.toString());
    }

    //update payments
    try {
      final localPaymentProvider = GetIt.I.get<LocalPaymentProvider>();
      final cachedPayments = localPaymentProvider.getCachedInstallmentPayments();
      cachedPayments.insert(
        0,
        InstallmentPayment(
          id: cachedPayments.firstOrNull?.id == null ? 0 : cachedPayments.firstOrNull!.id + 1,
          orderId: order.id,
          customerFullName: order.customer.fullName,
          amount: collectInstallment.amount.toDouble(),
          date: DateTime.now(),
          lastUpdatedAt: DateTime.now(),
          // product: order.orderItems,
        ),
      );
      await localPaymentProvider.saveInstallmentPayments(cachedPayments);
    } catch (e) {
      debugPrint(e.toString());
    }

    // NEW: Update order-specific payment cache (last 7 days)
    try {
      final localPaymentProvider = GetIt.I.get<LocalPaymentProvider>();

      // Get existing payments for this order
      final orderPayments = localPaymentProvider.getInstallmentPaymentsByOrderId(order.id);

      // Create new payment
      final newPayment = InstallmentPayment(
        id: orderPayments.firstOrNull?.id == null ? 0 : orderPayments.firstOrNull!.id + 1,
        orderId: order.id,
        customerFullName: order.customer.fullName,
        amount: collectInstallment.amount.toDouble(),
        date: DateTime.now(),
        lastUpdatedAt: DateTime.now(),
      );

      // Add and save (auto-cleanup happens here)
      orderPayments.insert(0, newPayment);

      await localPaymentProvider.saveInstallmentPaymentsByOrderId(
        order.id,
        orderPayments,
      );
    } catch (e) {
      debugPrint('Error updating order-specific cache: ${e.toString()}');
    }

    //update daily report
    try {
      if (!GetIt.I.get<AccountMapper>().isMotaba) {
        final localHomeProvider = GetIt.I.get<LocalHomeProvider>();
        final cachedDailyReport = localHomeProvider.getDailyReports()?.firstOrNull;
        final updatedDailyReport = cachedDailyReport?.copyWith(
          totalCollectedInstallmentAmount:
              cachedDailyReport.totalCollectedInstallmentAmount + collectInstallment.amount.toDouble(),
          todayCollectedOrderCount: cachedDailyReport.todayCollectedOrderCount + 1,
          totalUnpaidInstallmentAmount:
              cachedDailyReport.totalUnpaidInstallmentAmount - collectInstallment.amount.toDouble(),
        );
        if (updatedDailyReport != null) {
          await localHomeProvider.saveDailyReports([updatedDailyReport]);
        }
      }
    } catch (e) {
      debugPrint(e.toString());
    }
  }

  Future<void> collectInstallment(CollectInstallment collectInstallment, Order order) async {
    try {
      await apiOrderProvider.collectInstallment(collectInstallment);
      await _updateCachedDataBasedOnCollect(collectInstallment, order, isOffline: false);
      GetIt.I.get<MainBloc>().add(OrderPaymentEvent());
    } catch (e) {
      if (e is DioException && e.response?.statusCode == 400) {
        GetIt.I.get<MainBloc>().add(OrderPaymentEvent());
        rethrow;
      }
      final offlineInstallments = localOrderProvider.getOfflineCollects();
      offlineInstallments.add(collectInstallment);
      await localOrderProvider.saveOfflineCollect(offlineInstallments);
      await _updateCachedDataBasedOnCollect(collectInstallment, order, isOffline: true);
      GetIt.I.get<MainBloc>().add(OrderPaymentEvent());
      rethrow;
    }
  }

  Future<void> collectOfflineInstallments(List<CollectInstallment> collects) async {
    await apiOrderProvider.collectOfflineInstallments(collects);
    GetIt.I.get<MainBloc>().add(OrderPaymentEvent());
  }

  Future<void> updateOrderWithSellerInfo({
    required int orderId,
    required String creationAddress,
    required DateTime saleDateTime,
    required int? orderListId,
  }) => apiOrderProvider.updateOrderWithSellerInfo(
    orderId: orderId,
    creationAddress: creationAddress,
    saleDateTime: saleDateTime,
    orderListId: orderListId,
  );

  Future<int> createOrder({required List<OrderItemDetails> orderItems, required int customerId}) =>
      apiOrderProvider.createOrder(orderItems: orderItems, customerId: customerId);

  Future<void> editOrder({required OrderDetails orderDetails, int? orderListId}) =>
      apiOrderProvider.editOrder(order: orderDetails, orderListId: orderListId);

  Future<void> editOrderItems({required int orderId, required List<OrderItemDetails> orderItems}) =>
      apiOrderProvider.editOrderItems(orderId: orderId, orderItems: orderItems);

  Future<void> deleteOrder(int orderId) => apiOrderProvider.deleteOrder(orderId);

  Future<AttachmentInformation> uploadAttachment({
    required XFile file,
    final int? userId,
    required final AttachmentType attachmentType,
    final int? orderId,
  }) => apiOrderProvider.uploadAttachment(
    file: file,
    userId: userId,
    attachmentType: attachmentType,
    orderId: orderId,
  );

  Future<String> editAttachment({
    required XFile file,
    final int? userId,
    required final AttachmentType attachmentType,
    required final int attachmentId,
    final int? orderId,
  }) => apiOrderProvider.editAttachment(
    file: file,
    userId: userId,
    attachmentType: attachmentType,
    attachmentId: attachmentId,
    orderId: orderId,
  );
}
