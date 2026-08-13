import 'package:freezed_annotation/freezed_annotation.dart';

part 'collect_installment.freezed.dart';
part 'collect_installment.g.dart';

@freezed
class CollectInstallment with _$CollectInstallment {
  const factory CollectInstallment({
    required int orderId,
    required int amount,
    @DateOnlyConverter() DateTime? date,
  }) = _CollectInstallment;

  factory CollectInstallment.fromJson(Map<String, dynamic> json) => _$CollectInstallmentFromJson(json);
}

class DateOnlyConverter implements JsonConverter<DateTime?, String?> {
  const DateOnlyConverter();

  @override
  DateTime? fromJson(String? json) {
    if (json == null) return null;
    return DateTime.parse(json);
  }

  @override
  String? toJson(DateTime? date) {
    if (date == null) return null;
    // Only date part
    return "${date.year.toString().padLeft(4, '0')}-"
        "${date.month.toString().padLeft(2, '0')}-"
        "${date.day.toString().padLeft(2, '0')}";
  }
}
