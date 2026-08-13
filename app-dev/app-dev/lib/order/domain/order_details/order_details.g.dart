// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'order_details.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$OrderDetailsImpl _$$OrderDetailsImplFromJson(
  Map<String, dynamic> json,
) => _$OrderDetailsImpl(
  id: (json['id'] as num).toInt(),
  sellAmount: (json['sellAmount'] as num).toDouble(),
  paidAmount: (json['paidAmount'] as num).toDouble(),
  overdueAmount: (json['overdueAmount'] as num).toDouble(),
  dailyInstallmentAmount: (json['dailyInstallmentAmount'] as num?)?.toDouble(),
  creationAddress: json['creationAddress'] as String?,
  unpaidAmount: (json['unpaidAmount'] as num?)?.toDouble(),
  buyAmount: (json['buyAmount'] as num?)?.toDouble(),
  createdAt: DateTime.parse(json['createdAt'] as String),
  saleDate: const SaleDateConverter().fromJson(json['saleDate'] as String?),
  saleTime: const SaleTimeConverter().fromJson(json['saleTime'] as String?),
  step: $enumDecodeNullable(_$OrderStepEnumMap, json['step']),
  executionStatus: $enumDecodeNullable(
    _$ExecutionStatusEnumMap,
    json['executionStatus'],
  ),
  approvalStatus: $enumDecodeNullable(
    _$ApprovalStatusEnumMap,
    json['approvalStatus'],
  ),
  orderItems:
      (json['orderItems'] as List<dynamic>?)
          ?.map((e) => OrderItemDetails.fromJson(e as Map<String, dynamic>))
          .toList(),
  attachments:
      (json['attachments'] as List<dynamic>?)
          ?.map(
            (e) => AttachmentInformation.fromJson(e as Map<String, dynamic>),
          )
          .toList(),
);

Map<String, dynamic> _$$OrderDetailsImplToJson(_$OrderDetailsImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'sellAmount': instance.sellAmount,
      'paidAmount': instance.paidAmount,
      'overdueAmount': instance.overdueAmount,
      'dailyInstallmentAmount': instance.dailyInstallmentAmount,
      'creationAddress': instance.creationAddress,
      'unpaidAmount': instance.unpaidAmount,
      'buyAmount': instance.buyAmount,
      'createdAt': instance.createdAt.toIso8601String(),
      'saleDate': const SaleDateConverter().toJson(instance.saleDate),
      'saleTime': const SaleTimeConverter().toJson(instance.saleTime),
      'step': _$OrderStepEnumMap[instance.step],
      'orderItems': instance.orderItems,
      'attachments': instance.attachments,
    };

const _$OrderStepEnumMap = {
  OrderStep.attachments: 0,
  OrderStep.sellerInfo: 1,
  OrderStep.completed: 2,
  OrderStep.other: -1,
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
