import 'package:freezed_annotation/freezed_annotation.dart';

part 'order_item_details.freezed.dart';
part 'order_item_details.g.dart';

@freezed
class OrderItemDetails with _$OrderItemDetails {
  const factory OrderItemDetails({
    int? id,
    int? productId,
    String? productName,
    required int productType,
    required int quantity,
    required double buyAmount,
    required double sellAmount,
    required double prepaymentAmount,
    required double dailyInstallmentAmount,
  }) = _OrderItemDetails;

  factory OrderItemDetails.fromJson(Map<String, dynamic> json) => _$OrderItemDetailsFromJson(json);
}
