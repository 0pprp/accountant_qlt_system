// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'customer_financial_overview.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CustomerFinancialOverviewImpl _$$CustomerFinancialOverviewImplFromJson(
  Map<String, dynamic> json,
) => _$CustomerFinancialOverviewImpl(
  customerId: (json['customerId'] as num).toInt(),
  totalPaidAmount: (json['totalPaidAmount'] as num).toDouble(),
  totalOverdueAmount: (json['totalOverdueAmount'] as num).toDouble(),
  totalSellAmount: (json['totalSellAmount'] as num).toDouble(),
  totalRemainingAmount: (json['totalRemainingAmount'] as num).toDouble(),
  totalDailyInstallmentAmount:
      (json['totalDailyInstallmentAmount'] as num).toDouble(),
  activeOrdersCount: (json['activeOrdersCount'] as num).toInt(),
  completedOrdersCount: (json['completedOrdersCount'] as num).toInt(),
);

Map<String, dynamic> _$$CustomerFinancialOverviewImplToJson(
  _$CustomerFinancialOverviewImpl instance,
) => <String, dynamic>{
  'customerId': instance.customerId,
  'totalPaidAmount': instance.totalPaidAmount,
  'totalOverdueAmount': instance.totalOverdueAmount,
  'totalSellAmount': instance.totalSellAmount,
  'totalRemainingAmount': instance.totalRemainingAmount,
  'totalDailyInstallmentAmount': instance.totalDailyInstallmentAmount,
  'activeOrdersCount': instance.activeOrdersCount,
  'completedOrdersCount': instance.completedOrdersCount,
};
