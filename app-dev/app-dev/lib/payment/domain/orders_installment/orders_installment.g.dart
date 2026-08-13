// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'orders_installment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$OrdersInstallmentImpl _$$OrdersInstallmentImplFromJson(
  Map<String, dynamic> json,
) => _$OrdersInstallmentImpl(
  id: (json['id'] as num).toInt(),
  customerFullName: json['customerFullName'] as String,
  sellAmount: (json['sellAmount'] as num).toDouble(),
  createdAt: DateTime.parse(json['createdAt'] as String),
  installmentPayments:
      (json['installmentPayments'] as List<dynamic>)
          .map((e) => Installment.fromJson(e as Map<String, dynamic>))
          .toList(),
);

Map<String, dynamic> _$$OrdersInstallmentImplToJson(
  _$OrdersInstallmentImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'customerFullName': instance.customerFullName,
  'sellAmount': instance.sellAmount,
  'createdAt': instance.createdAt.toIso8601String(),
  'installmentPayments': instance.installmentPayments,
};

_$InstallmentImpl _$$InstallmentImplFromJson(Map<String, dynamic> json) =>
    _$InstallmentImpl(
      id: (json['id'] as num?)?.toInt(),
      date: DateTime.parse(json['date'] as String),
      amount: (json['amount'] as num?)?.toDouble(),
      hasPayment: json['hasPayment'] as bool? ?? false,
      description: json['description'] as String?,
    );

Map<String, dynamic> _$$InstallmentImplToJson(_$InstallmentImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'date': instance.date.toIso8601String(),
      'amount': instance.amount,
      'hasPayment': instance.hasPayment,
      'description': instance.description,
    };
