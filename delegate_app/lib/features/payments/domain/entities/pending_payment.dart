class PaymentSyncStatuses {
  static const pending = 'pending';
  static const syncing = 'syncing';
  static const failed = 'failed';
}

class PendingPayment {
  const PendingPayment({
    required this.id,
    required this.customerId,
    required this.delegateId,
    required this.amount,
    required this.location,
    required this.clientPaymentId,
    required this.createdAtUtc,
    required this.syncStatus,
    this.customerName,
    this.ownerAsyncId,
    this.receiptNumber,
    this.orderId,
    this.syncError,
    this.permanentFailure = false,
  });

  final int id;
  final int customerId;
  final String? customerName;
  final int delegateId;
  final int? orderId;
  final double amount;
  final String location;
  final String clientPaymentId;
  final String createdAtUtc;
  final String syncStatus;
  final String? ownerAsyncId;
  final String? receiptNumber;
  final String? syncError;
  final bool permanentFailure;

  PendingPayment copyWith({
    int? orderId,
    String? syncStatus,
    String? syncError,
    bool? permanentFailure,
  }) {
    return PendingPayment(
      id: id,
      customerId: customerId,
      customerName: customerName,
      delegateId: delegateId,
      orderId: orderId ?? this.orderId,
      amount: amount,
      location: location,
      clientPaymentId: clientPaymentId,
      createdAtUtc: createdAtUtc,
      syncStatus: syncStatus ?? this.syncStatus,
      ownerAsyncId: ownerAsyncId,
      receiptNumber: receiptNumber,
      syncError: syncError ?? this.syncError,
      permanentFailure: permanentFailure ?? this.permanentFailure,
    );
  }
}
