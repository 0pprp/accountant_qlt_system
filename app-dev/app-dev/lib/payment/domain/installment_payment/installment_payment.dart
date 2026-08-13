import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:team/order/domain/order/order.dart';

part 'installment_payment.freezed.dart';
part 'installment_payment.g.dart';

@freezed
class InstallmentPayment with _$InstallmentPayment {
  const factory InstallmentPayment({
    required final int id,
    required final int orderId,
    required final String customerFullName,
    final int? orderListId,
    required final double amount,
    required final DateTime date,
    required final DateTime lastUpdatedAt,
    // required final Product product,
  }) = _InstallmentPayment;

  factory InstallmentPayment.fromJson(Map<String, dynamic> json) => _$InstallmentPaymentFromJson(json);
}
