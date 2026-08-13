import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:team/order/domain/order_details/order_details.dart';

part 'order.freezed.dart';
part 'order.g.dart';

@freezed
class Order with _$Order {
  const Order._();

  const factory Order({
    required final int id,
    required final int sellAmount,
    required final int paidAmount,
    required final int dailyInstallmentAmount,
    required final DateTime createdAt,
    final String? creationAddress,
    final int? orderListId,
    required final Customer customer,
    // final InstallmentPaymentInfo? installmentPayment,
    final int? installmentPaymentId,
    required final List<OrderItem> orderItems,
    final ExecutionStatus? executionStatus,
    final ApprovalStatus? approvalStatus,
    final OrderStep? step,
    @JsonKey(includeFromJson: false, includeToJson: false, defaultValue: false) final bool? isCollectedOffline,
  }) = _Order;

  factory Order.fromJson(Map<String, dynamic> json) => _$OrderFromJson(json);

  /// Draft = create-flow not finished (`notStarted` / `inProgress`).
  bool get isDraft => step != OrderStep.completed;

  bool get isRejected => approvalStatus == ApprovalStatus.rejected;

  bool get isApproved => approvalStatus == ApprovalStatus.approved;

  /// Draft or rejected → edit/delete. Missing statuses keep the old payment UI.
  bool get showEditDeleteActions {
    if (executionStatus == null && approvalStatus == null) return false;
    return isDraft || isRejected;
  }
}

@freezed
class OrderItem with _$OrderItem {
  const factory OrderItem({
    required final int id,
    required final String productName,
    required final int quantity,
    required final int sellAmount,
  }) = _OrderItem;

  factory OrderItem.fromJson(Map<String, dynamic> json) => _$OrderItemFromJson(json);
}

@freezed
class Customer with _$Customer {
  const factory Customer({required int id, required String fullName, required String phoneNumber}) = _Customer;

  factory Customer.fromJson(Map<String, dynamic> json) => _$CustomerFromJson(json);
}
