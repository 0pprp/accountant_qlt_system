/// تسديدة محفوظة ليوم العمل الحالي (مخزن المسددين).
class PaidCollection {
  const PaidCollection({
    required this.customerId,
    required this.delegateId,
    required this.businessDay,
    required this.amount,
    required this.clientPaymentId,
    required this.createdAtUtc,
    this.customerName,
  });

  final int customerId;
  final int delegateId;
  final String businessDay;
  final double amount;
  final String clientPaymentId;
  final String createdAtUtc;
  final String? customerName;

  bool get hasValidUuid => clientPaymentId.trim().isNotEmpty;
}
