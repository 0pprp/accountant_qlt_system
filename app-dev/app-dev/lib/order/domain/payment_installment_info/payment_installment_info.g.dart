// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'payment_installment_info.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$InstallmentPaymentInfoImpl _$$InstallmentPaymentInfoImplFromJson(
  Map<String, dynamic> json,
) => _$InstallmentPaymentInfoImpl(
  id: (json['id'] as num).toInt(),
  hasPayment: json['hasPayment'] ?? false,
);

Map<String, dynamic> _$$InstallmentPaymentInfoImplToJson(
  _$InstallmentPaymentInfoImpl instance,
) => <String, dynamic>{'id': instance.id, 'hasPayment': instance.hasPayment};
