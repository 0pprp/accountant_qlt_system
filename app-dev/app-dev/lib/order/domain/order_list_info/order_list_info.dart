// daily_report.dart
import 'package:freezed_annotation/freezed_annotation.dart';

part 'order_list_info.freezed.dart';
part 'order_list_info.g.dart';

@freezed
class OrderListInfo with _$OrderListInfo {
  const factory OrderListInfo({
    required int id,
    required String name,
    final String? mandobFullName,
  }) = _OrderListInfo;

  factory OrderListInfo.fromJson(Map<String, Object?> json) => _$OrderListInfoFromJson(json);
}
