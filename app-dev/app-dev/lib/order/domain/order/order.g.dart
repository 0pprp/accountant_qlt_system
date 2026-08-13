// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'order.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$OrderImpl _$$OrderImplFromJson(Map<String, dynamic> json) => _$OrderImpl(
  id: (json['id'] as num).toInt(),
  sellAmount: (json['sellAmount'] as num).toInt(),
  paidAmount: (json['paidAmount'] as num).toInt(),
  dailyInstallmentAmount: (json['dailyInstallmentAmount'] as num).toInt(),
  createdAt: DateTime.parse(json['createdAt'] as String),
  creationAddress: json['creationAddress'] as String?,
  orderListId: (json['orderListId'] as num?)?.toInt(),
  customer: Customer.fromJson(json['customer'] as Map<String, dynamic>),
  installmentPaymentId: (json['installmentPaymentId'] as num?)?.toInt(),
  orderItems:
      (json['orderItems'] as List<dynamic>)
          .map((e) => OrderItem.fromJson(e as Map<String, dynamic>))
          .toList(),
  executionStatus: $enumDecodeNullable(
    _$ExecutionStatusEnumMap,
    json['executionStatus'],
  ),
  approvalStatus: $enumDecodeNullable(
    _$ApprovalStatusEnumMap,
    json['approvalStatus'],
  ),
  step: $enumDecodeNullable(_$OrderStepEnumMap, json['step']),
);

Map<String, dynamic> _$$OrderImplToJson(_$OrderImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'sellAmount': instance.sellAmount,
      'paidAmount': instance.paidAmount,
      'dailyInstallmentAmount': instance.dailyInstallmentAmount,
      'createdAt': instance.createdAt.toIso8601String(),
      'creationAddress': instance.creationAddress,
      'orderListId': instance.orderListId,
      'customer': instance.customer,
      'installmentPaymentId': instance.installmentPaymentId,
      'orderItems': instance.orderItems,
      'executionStatus': _$ExecutionStatusEnumMap[instance.executionStatus],
      'approvalStatus': _$ApprovalStatusEnumMap[instance.approvalStatus],
      'step': _$OrderStepEnumMap[instance.step],
    };

const _$ExecutionStatusEnumMap = {
  ExecutionStatus.notStarted: 0,
  ExecutionStatus.inProgress: 1,
  ExecutionStatus.completed: 2,
};

const _$ApprovalStatusEnumMap = {
  ApprovalStatus.pending: 0,
  ApprovalStatus.approved: 1,
  ApprovalStatus.rejected: 2,
};

const _$OrderStepEnumMap = {
  OrderStep.attachments: 0,
  OrderStep.sellerInfo: 1,
  OrderStep.completed: 2,
  OrderStep.other: -1,
};

_$OrderItemImpl _$$OrderItemImplFromJson(Map<String, dynamic> json) =>
    _$OrderItemImpl(
      id: (json['id'] as num).toInt(),
      productName: json['productName'] as String,
      quantity: (json['quantity'] as num).toInt(),
      sellAmount: (json['sellAmount'] as num).toInt(),
    );

Map<String, dynamic> _$$OrderItemImplToJson(_$OrderItemImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'productName': instance.productName,
      'quantity': instance.quantity,
      'sellAmount': instance.sellAmount,
    };

_$CustomerImpl _$$CustomerImplFromJson(Map<String, dynamic> json) =>
    _$CustomerImpl(
      id: (json['id'] as num).toInt(),
      fullName: json['fullName'] as String,
      phoneNumber: json['phoneNumber'] as String,
    );

Map<String, dynamic> _$$CustomerImplToJson(_$CustomerImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'fullName': instance.fullName,
      'phoneNumber': instance.phoneNumber,
    };
