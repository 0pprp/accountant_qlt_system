import 'package:sales_employee_app/core/network/json_map.dart';

class SaleOrder {
  const SaleOrder({
    required this.id,
    required this.customerName,
    this.status,
    this.total,
    this.createdAt,
    this.customerId,
    this.customerUuid,
    this.customerPhone,
    this.customerAddress,
    this.businessName,
    this.nearestKnownLocation,
    this.branchId,
    this.branchName,
    this.creationAddress,
    this.orderListName,
    this.sellerName,
    this.productNames = const [],
    this.workflowReason,
  });

  final int id;
  final String customerName;
  final String? status;
  final double? total;
  final String? createdAt;
  final int? customerId;
  /// Stable customer key — one UUID may appear in only one bin/card.
  final String? customerUuid;
  final String? customerPhone;
  final String? customerAddress;
  final String? businessName;
  final String? nearestKnownLocation;
  final int? branchId;
  final String? branchName;
  final String? creationAddress;
  final String? orderListName;
  final String? sellerName;
  final List<String> productNames;
  final String? workflowReason;

  factory SaleOrder.fromJson(Map<String, dynamic> json) {
    final customer = JsonMap.pick(json, ['customer', 'Customer']);
    final branch = JsonMap.pick(json, ['branch', 'Branch']);
    final seller = JsonMap.pick(json, ['seller', 'Seller']);
    final orderList = JsonMap.pick(json, ['orderList', 'OrderList']);
    final items = JsonMap.pick(json, ['orderItems', 'OrderItems', 'items', 'Items']);

    String name = JsonMap.str(json, ['customerName', 'CustomerName', 'fullName', 'FullName']);
    String? phone;
    String? address;
    int? customerId = JsonMap.asInt(json, ['customerId', 'CustomerId']);
    if (customer is Map) {
      if (name.isEmpty) {
        name = JsonMap.str(customer, ['fullName', 'FullName', 'name', 'Name']);
      }
      phone = JsonMap.str(customer, ['phoneNumber', 'PhoneNumber']);
      address = JsonMap.str(customer, ['businessAddress', 'BusinessAddress', 'address', 'Address']);
      customerId ??= JsonMap.asInt(customer, ['id', 'Id']);
    }
    final businessName = customer is Map
        ? JsonMap.str(customer, ['businessName', 'BusinessName'])
        : '';
    final nearest = customer is Map
        ? JsonMap.str(customer, ['nearestKnownLocation', 'NearestKnownLocation'])
        : '';

    String? branchName;
    int? branchId;
    if (branch is Map) {
      branchName = JsonMap.str(branch, ['name', 'Name']);
      branchId = JsonMap.asInt(branch, ['id', 'Id']);
    }

    String? sellerName;
    if (seller is Map) {
      sellerName = JsonMap.str(seller, ['name', 'Name', 'fullName', 'FullName']);
    }

    String? listName;
    if (orderList is Map) {
      listName = JsonMap.str(orderList, ['name', 'Name']);
    }

    final productNames = <String>[];
    if (items is List) {
      for (final raw in items) {
        if (raw is! Map) continue;
        final n = JsonMap.str(raw, ['productName', 'ProductName', 'name', 'Name']);
        if (n.isNotEmpty) productNames.add(n);
      }
    }

    final workflow = JsonMap.str(json, ['workflowStatus', 'WorkflowStatus']);
    final statusRaw = JsonMap.str(json, ['status', 'Status']);
    String? status = statusRaw.isNotEmpty ? statusRaw : null;
    if (status == null && workflow.isNotEmpty) {
      status = switch (workflow) {
        '0' || 'Pending' || 'pending' => 'pending',
        '1' || 'Ready' || 'ready' => 'ready',
        '2' || 'Disclosure' || 'disclosure' => 'disclosure',
        '3' || 'OnHold' || 'on_hold' => 'on_hold',
        '4' || 'Rejected' || 'rejected' => 'rejected',
        '5' || 'Sold' || 'sold' => 'sold',
        _ => workflow.toLowerCase(),
      };
    }

    return SaleOrder(
      id: JsonMap.asInt(json, ['id', 'Id', 'orderId', 'OrderId']) ?? 0,
      customerName: name.isEmpty ? '—' : name,
      status: status,
      total: JsonMap.asDouble(json, ['total', 'Total', 'sellAmount', 'SellAmount', 'amount', 'Amount']),
      createdAt: JsonMap.str(json, ['createdAt', 'CreatedAt', 'saleDate', 'SaleDate']).isEmpty
          ? null
          : JsonMap.str(json, ['createdAt', 'CreatedAt', 'saleDate', 'SaleDate']),
      customerId: customerId,
      customerUuid: () {
        final u = JsonMap.str(json, ['customerUuid', 'CustomerUuid', 'uuid', 'Uuid']);
        return u.isEmpty ? null : u;
      }(),
      customerPhone: (phone == null || phone.isEmpty) ? null : phone,
      customerAddress: (address == null || address.isEmpty) ? null : address,
      businessName: businessName.isEmpty ? null : businessName,
      nearestKnownLocation: nearest.isEmpty ? null : nearest,
      branchId: branchId,
      branchName: (branchName == null || branchName.isEmpty) ? null : branchName,
      creationAddress: JsonMap.str(json, ['creationAddress', 'CreationAddress']).isEmpty
          ? null
          : JsonMap.str(json, ['creationAddress', 'CreationAddress']),
      orderListName: (listName == null || listName.isEmpty) ? null : listName,
      sellerName: (sellerName == null || sellerName.isEmpty) ? null : sellerName,
      productNames: productNames,
      workflowReason: JsonMap.str(json, ['workflowChangeReason', 'WorkflowChangeReason', 'reason', 'Reason']).isEmpty
          ? null
          : JsonMap.str(json, ['workflowChangeReason', 'WorkflowChangeReason', 'reason', 'Reason']),
    );
  }
}

class SalesBinSummary {
  const SalesBinSummary({
    required this.key,
    required this.label,
    required this.count,
  });

  final String key;
  final String label;
  final int count;
}
