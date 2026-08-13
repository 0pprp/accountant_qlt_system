// daily_report.dart
import 'package:freezed_annotation/freezed_annotation.dart';

part 'order_list.freezed.dart';
part 'order_list.g.dart';

@freezed
class OrderList with _$OrderList {
  const factory OrderList({
    required int id,
    required String name,
    required String mandobFullName,
    String? mandobPhoneNumber,
    required int totalOrderCount,
    required int todayCollectedOrderCount,
    required double totalSellAmount, // Assuming this can be a decimal value
    required double totalDailyInstallmentAmount, // Assuming this can be a decimal value
    required double totalOverdueInstallmentAmount, // Assuming this can be a decimal value
    required double totalCollectedInstallmentAmount, // Assuming this can be a decimal value
    required double totalUnpaidInstallmentAmount, // Assuming this can be a decimal value
  }) = _OrderList;

  factory OrderList.fromJson(Map<String, Object?> json) => _$OrderListFromJson(json);
}
