class Customer {
  const Customer({
    required this.customerId,
    required this.name,
    required this.delegateId,
    this.phone,
    this.address,
    this.remaining,
    this.dailyInstallment,
    this.salePrice,
    this.lastPaymentDate,
    this.dateSaleDevice,
    this.receiptTotal,
    this.countReceipt,
    this.shopName,
    this.activeOrderId,
  });

  final int customerId;
  final String name;
  final int delegateId;
  final String? phone;
  final String? address;
  final double? remaining;
  final double? dailyInstallment;
  final double? salePrice;
  final String? lastPaymentDate;
  final String? dateSaleDevice;
  final double? receiptTotal;
  final int? countReceipt;
  final String? shopName;

  /// Active order used for installment collection (central API).
  final int? activeOrderId;
}
