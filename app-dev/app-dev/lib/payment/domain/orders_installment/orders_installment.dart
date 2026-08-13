import 'package:freezed_annotation/freezed_annotation.dart';

part 'orders_installment.freezed.dart';
part 'orders_installment.g.dart';

@freezed
class OrdersInstallment with _$OrdersInstallment {
  const factory OrdersInstallment({
    required int id,
    required String customerFullName,
    required double sellAmount,
    required DateTime createdAt,
    // required String productName,
    required List<Installment> installmentPayments,
  }) = _OrdersInstallment;

  factory OrdersInstallment.fromJson(Map<String, dynamic> json) => _$OrdersInstallmentFromJson(json);
}

@freezed
class Installment with _$Installment {
  const factory Installment({
    int? id,
    required DateTime date,
    double? amount,
    // required InstallmentStatus status,
    @Default(false) bool hasPayment,
    String? description,
  }) = _Installment;

  factory Installment.fromJson(Map<String, dynamic> json) => _$InstallmentFromJson(json);
}
