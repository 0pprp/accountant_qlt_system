import 'package:freezed_annotation/freezed_annotation.dart';

part 'payment_installment_info.freezed.dart';
part 'payment_installment_info.g.dart';

@freezed
class InstallmentPaymentInfo with _$InstallmentPaymentInfo {
  const factory InstallmentPaymentInfo({
    required int id,
    // required InstallmentStatus status,
    @Default(false) hasPayment,
  }) = _InstallmentPaymentInfo;

  factory InstallmentPaymentInfo.fromJson(Map<String, dynamic> json) => _$InstallmentPaymentInfoFromJson(json);
}

// enum InstallmentStatus {
//   @JsonValue(0)
//   pending,
//   @JsonValue(1)
//   collected,
//   @JsonValue(2)
//   overdue,
//   @JsonValue(3)
//   collectedWithDelay,
// }
//
// extension InstallmentStatusExtension on InstallmentStatus {
//   Color? get color {
//     switch (this) {
//       case InstallmentStatus.pending:
//         return null;
//       case InstallmentStatus.collected:
//         return AppColor.surface3;
//       case InstallmentStatus.overdue:
//         return AppColor.surface1;
//       case InstallmentStatus.collectedWithDelay:
//         return AppColor.surface4;
//     }
//   }
// }
