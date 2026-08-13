import 'package:freezed_annotation/freezed_annotation.dart';

part 'installment_payments_summary.freezed.dart';
part 'installment_payments_summary.g.dart';

@freezed
class InstallmentPaymentsSummary with _$InstallmentPaymentsSummary {
  const factory InstallmentPaymentsSummary({required int totalCount, required double totalAmount}) =
      _InstallmentPaymentsSummary;

  factory InstallmentPaymentsSummary.fromJson(Map<String, dynamic> json) => _$InstallmentPaymentsSummaryFromJson(json);
}
