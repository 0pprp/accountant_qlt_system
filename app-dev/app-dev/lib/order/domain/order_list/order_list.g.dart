// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'order_list.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$OrderListImpl _$$OrderListImplFromJson(Map<String, dynamic> json) =>
    _$OrderListImpl(
      id: (json['id'] as num).toInt(),
      name: json['name'] as String,
      mandobFullName: json['mandobFullName'] as String,
      mandobPhoneNumber: json['mandobPhoneNumber'] as String?,
      totalOrderCount: (json['totalOrderCount'] as num).toInt(),
      todayCollectedOrderCount:
          (json['todayCollectedOrderCount'] as num).toInt(),
      totalSellAmount: (json['totalSellAmount'] as num).toDouble(),
      totalDailyInstallmentAmount:
          (json['totalDailyInstallmentAmount'] as num).toDouble(),
      totalOverdueInstallmentAmount:
          (json['totalOverdueInstallmentAmount'] as num).toDouble(),
      totalCollectedInstallmentAmount:
          (json['totalCollectedInstallmentAmount'] as num).toDouble(),
      totalUnpaidInstallmentAmount:
          (json['totalUnpaidInstallmentAmount'] as num).toDouble(),
    );

Map<String, dynamic> _$$OrderListImplToJson(
  _$OrderListImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'name': instance.name,
  'mandobFullName': instance.mandobFullName,
  'mandobPhoneNumber': instance.mandobPhoneNumber,
  'totalOrderCount': instance.totalOrderCount,
  'todayCollectedOrderCount': instance.todayCollectedOrderCount,
  'totalSellAmount': instance.totalSellAmount,
  'totalDailyInstallmentAmount': instance.totalDailyInstallmentAmount,
  'totalOverdueInstallmentAmount': instance.totalOverdueInstallmentAmount,
  'totalCollectedInstallmentAmount': instance.totalCollectedInstallmentAmount,
  'totalUnpaidInstallmentAmount': instance.totalUnpaidInstallmentAmount,
};
