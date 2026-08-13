// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'collect_installment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CollectInstallmentImpl _$$CollectInstallmentImplFromJson(
  Map<String, dynamic> json,
) => _$CollectInstallmentImpl(
  orderId: (json['orderId'] as num).toInt(),
  amount: (json['amount'] as num).toInt(),
  date: const DateOnlyConverter().fromJson(json['date'] as String?),
);

Map<String, dynamic> _$$CollectInstallmentImplToJson(
  _$CollectInstallmentImpl instance,
) => <String, dynamic>{
  'orderId': instance.orderId,
  'amount': instance.amount,
  'date': const DateOnlyConverter().toJson(instance.date),
};
