// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'installment_payments_summary.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$InstallmentPaymentsSummaryImpl _$$InstallmentPaymentsSummaryImplFromJson(
  Map<String, dynamic> json,
) => _$InstallmentPaymentsSummaryImpl(
  totalCount: (json['totalCount'] as num).toInt(),
  totalAmount: (json['totalAmount'] as num).toDouble(),
);

Map<String, dynamic> _$$InstallmentPaymentsSummaryImplToJson(
  _$InstallmentPaymentsSummaryImpl instance,
) => <String, dynamic>{
  'totalCount': instance.totalCount,
  'totalAmount': instance.totalAmount,
};
