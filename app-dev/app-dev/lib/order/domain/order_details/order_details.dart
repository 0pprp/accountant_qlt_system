import 'package:flutter/material.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:intl/intl.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/order/domain/order_item_details/order_item_details.dart';
import 'package:team/order/domain/order_status.dart';

export 'package:team/order/domain/order_status.dart';

part 'order_details.freezed.dart';
part 'order_details.g.dart';

@freezed
class OrderDetails with _$OrderDetails {
  const factory OrderDetails({
    required int id,
    required double sellAmount,
    required double paidAmount,
    required double overdueAmount,
    double? dailyInstallmentAmount,
    String? creationAddress,
    double? unpaidAmount,
    double? buyAmount,
    required DateTime createdAt,
    @SaleDateConverter() DateTime? saleDate,
    @SaleTimeConverter() TimeOfDay? saleTime,
    OrderStep? step,
    @JsonKey(includeToJson: false) ExecutionStatus? executionStatus,
    @JsonKey(includeToJson: false) ApprovalStatus? approvalStatus,
    final List<OrderItemDetails>? orderItems,
    final List<AttachmentInformation>? attachments,
  }) = _OrderDetails;

  factory OrderDetails.fromJson(Map<String, dynamic> json) => _$OrderDetailsFromJson(json);
}

enum OrderStep {
  @JsonValue(0)
  attachments,
  @JsonValue(1)
  sellerInfo,
  @JsonValue(2)
  completed,
  @JsonValue(-1)
  other,
}

// --- Custom Converters (Placed at the bottom) ---

class SaleDateConverter implements JsonConverter<DateTime?, String?> {
  const SaleDateConverter();

  @override
  DateTime? fromJson(String? json) {
    if (json == null || json.isEmpty) return null;
    try {
      return DateFormat('yyyy-MM-dd').parse(json);
    } catch (e) {
      return null;
    }
  }

  @override
  String? toJson(DateTime? object) {
    if (object == null) return null;
    return DateFormat('yyyy-MM-dd').format(object);
  }
}

class SaleTimeConverter implements JsonConverter<TimeOfDay?, String?> {
  const SaleTimeConverter();

  @override
  TimeOfDay? fromJson(String? json) {
    if (json == null || json.isEmpty) return null;
    try {
      // Split by ':' to get [HH, mm, ss]
      final parts = json.split(':');
      if (parts.length < 2) return null;

      return TimeOfDay(
        hour: int.parse(parts[0]),
        minute: int.parse(parts[1]),
      );
    } catch (e) {
      return null;
    }
  }

  @override
  String? toJson(TimeOfDay? object) {
    if (object == null) return null;
    // Format as HH:mm:ss to match the expected input format
    final hour = object.hour.toString().padLeft(2, '0');
    final minute = object.minute.toString().padLeft(2, '0');
    return '$hour:$minute:00'; // Seconds are always 00 for TimeOfDay
  }
}
