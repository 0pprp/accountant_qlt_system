/// Unified row for the تسديدات tab (local queue + server today).
class TodayPaymentItem {
  const TodayPaymentItem({
    required this.keyId,
    required this.customerName,
    required this.amount,
    required this.isLocal,
    required this.statusLabel,
    this.customerId,
    this.orderId,
    this.syncError,
    this.createdAtUtc,
    this.dateLabel,
  });

  final String keyId;
  final int? customerId;
  final String customerName;
  final double amount;
  final bool isLocal;
  final String statusLabel;
  final int? orderId;
  final String? syncError;
  final String? createdAtUtc;
  final String? dateLabel;
}
