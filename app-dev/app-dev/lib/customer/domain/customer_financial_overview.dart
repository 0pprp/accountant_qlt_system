import 'package:freezed_annotation/freezed_annotation.dart';

part 'customer_financial_overview.freezed.dart';
part 'customer_financial_overview.g.dart';

@freezed
class CustomerFinancialOverview with _$CustomerFinancialOverview {
  const factory CustomerFinancialOverview({
    required int customerId,
    required double totalPaidAmount,
    required double totalOverdueAmount,
    required double totalSellAmount,
    required double totalRemainingAmount,
    required double totalDailyInstallmentAmount,
    required int activeOrdersCount,
    required int completedOrdersCount,
  }) = _CustomerFinancialOverview;

  factory CustomerFinancialOverview.fromJson(Map<String, dynamic> json) => _$CustomerFinancialOverviewFromJson(json);
}
