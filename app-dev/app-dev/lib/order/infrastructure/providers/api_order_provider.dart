import 'package:dio/dio.dart';
import 'package:intl/intl.dart';
import 'package:share_plus/share_plus.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/common/services/request/request.dart';
import 'package:team/order/domain/collect_installment/collect_installment.dart';
import 'package:team/order/domain/order/order.dart';
import 'package:team/order/domain/order_details/order_details.dart';
import 'package:team/order/domain/order_item_details/order_item_details.dart';
import 'package:team/order/domain/order_list/order_list.dart';
import 'package:team/order/domain/order_status.dart';
import 'package:team/order/domain/product/product.dart';
import 'package:team/order/domain/product_category/product_category.dart';

class ApiOrderProvider {
  final Request request;

  ApiOrderProvider({required this.request});

  Future<List<Order>> getMandoobOrders({
    final int? orderListId,
    final DateTime? startDate,
    final DateTime? endDate,
    final String? searchText,
    final List<ExecutionStatus>? executionStatuses,
  }) async {
    final statuses =
        executionStatuses ??
        const [ExecutionStatus.notStarted, ExecutionStatus.inProgress];

    final response = await request.get(
      'api/v1/app/Orders',
      queryParameters: {
        'orderListId': orderListId,
        'startDate': startDate?.toIso8601String(),
        'endDate': endDate?.toIso8601String(),
        'fullName': searchText,
        'executionStatuses': statuses.map((status) => status.index).toList(),
      },
    );

    return (response.data as List).map((e) => Order.fromJson(e)).toList();
  }

  Future<List<OrderDetails>> getUserOrders(final int id) async {
    final response = await request.get('api/v1/app/customers/$id/orders');

    return (response.data as List).map((e) => OrderDetails.fromJson(e)).toList();
  }

  Future<OrderDetails> getOrderDetails(final int id) async {
    final response = await request.get('api/v1/app/Orders/$id');

    return OrderDetails.fromJson(response.data);
  }

  Future<void> collectInstallment(CollectInstallment collectInstallment) async {
    await request.post(
      'api/v1/app/orders/${collectInstallment.orderId}/installment-payments',
      data: collectInstallment.toJson(),
    );
  }

  Future<void> completedAttachments(int orderId) async {
    await request.post('api/v1/app/orders/$orderId/complete-attachments');
  }

  Future<void> collectOfflineInstallments(List<CollectInstallment> collects) async {
    await request.post(
      'api/v1/app/InstallmentPayments/sync',
      data: {'installmentPayments': collects.map((e) => e.toJson()).toList()},
    );
  }

  Future<void> updateOrderWithSellerInfo({
    required int orderId,
    required String creationAddress,
    required DateTime saleDateTime,
    required int? orderListId,
  }) async {
    await request.put(
      'api/v1/app/Orders/$orderId/seller-info',
      data: {
        'creationAddress': creationAddress,
        'saleDate': DateFormat('yyyy-MM-dd').format(saleDateTime),
        'saleTime': DateFormat('HH:mm').format(saleDateTime),
        'orderListId': orderListId,
      },
    );
  }

  Future<int> createOrder({required List<OrderItemDetails> orderItems, required int customerId}) async {
    final response = await request.post(
      'api/v1/app/Orders',
      data: {'orderItems': orderItems, 'customerId': customerId},
    );
    return response.data['id'];
  }

  Future<List<OrderList>> getOrdersList() async {
    final response = await request.get('api/v1/app/OrderLists');

    return (response.data as List).map((e) => OrderList.fromJson(e)).toList();
  }

  Future<void> editOrderItems({
    required int orderId,
    required List<OrderItemDetails> orderItems,
  }) async {
    await request.put(
      'api/v1/app/Orders/$orderId',
      data: {'orderItems': orderItems},
    );
  }

  Future<void> editOrder({required OrderDetails order, int? orderListId}) async {
    await request.put(
      'api/v1/app/Orders/${order.id}',
      data: order.toJson(),
    );
  }

  Future<void> deleteOrder(int orderId) async {
    await request.delete('api/v1/app/Orders/$orderId');
  }

  Future<AttachmentInformation> uploadAttachment({
    required XFile file,
    final int? userId,
    required final AttachmentType attachmentType,
    final int? orderId,
  }) async {
    final response = await request.post(
      'api/v1/app/Attachments',
      data: FormData.fromMap({
        'file': await MultipartFile.fromFile(file.path, filename: file.name),
        'type': attachmentType.index,
        'customerId': userId,
        'orderId': orderId,
      }),
    );

    return AttachmentInformation.fromJson(
      (response.data as Map<String, dynamic>)..putIfAbsent('type', () => attachmentType.index),
    );
  }

  Future<String> editAttachment({
    required XFile file,
    final int? userId,
    final int? orderId,
    required final AttachmentType attachmentType,
    required final int attachmentId,
  }) async {
    final response = await request.put(
      'api/v1/app/Attachments/$attachmentId',
      data: FormData.fromMap({
        'file': await MultipartFile.fromFile(file.path, filename: file.name),
        'type': attachmentType.index,
        'customerId': userId,
        'orderId': orderId,
      }),
    );

    return response.data['relativePath'];
  }

  Future<List<ProductCategory>> getProductCategories({final String? searchText, final DateTime? lastDateTime}) async {
    final response = await request.get(
      'api/v1/app/ProductCategories',
      queryParameters: {
        'SearchTerm': searchText,
        'lastId': lastDateTime,
      },
    );

    return (response.data as List).map((e) => ProductCategory.fromJson(e)).toList();
  }

  Future<List<Product>> getProducts({
    final String? searchText,
    final DateTime? lastDateTime,
    final int? categoryId,
  }) async {
    final response = await request.get(
      'api/v1/app/Products',
      queryParameters: {
        'SearchTerm': searchText,
        'lastId': lastDateTime,
        'categoryId': categoryId,
      },
    );

    return (response.data as List).map((e) => Product.fromJson(e)).toList();
  }
}
