// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'installment_payment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$InstallmentPaymentImpl _$$InstallmentPaymentImplFromJson(
  Map<String, dynamic> json,
) => _$InstallmentPaymentImpl(
  id: (json['id'] as num).toInt(),
  orderId: (json['orderId'] as num).toInt(),
  customerFullName: json['customerFullName'] as String,
  orderListId: (json['orderListId'] as num?)?.toInt(),
  amount: (json['amount'] as num).toDouble(),
  date: DateTime.parse(json['date'] as String),
  lastUpdatedAt: DateTime.parse(json['lastUpdatedAt'] as String),
);

Map<String, dynamic> _$$InstallmentPaymentImplToJson(
  _$InstallmentPaymentImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'orderId': instance.orderId,
  'customerFullName': instance.customerFullName,
  'orderListId': instance.orderListId,
  'amount': instance.amount,
  'date': instance.date.toIso8601String(),
  'lastUpdatedAt': instance.lastUpdatedAt.toIso8601String(),
};
