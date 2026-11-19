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
    this.isLegal = false,
    this.weekAmounts = const [0, 0, 0, 0, 0, 0, 0],
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

  /// قضايا قانونية — لا تُعرض للمندوب.
  final bool isLegal;

  /// تسديدات آخر 7 أيام: [0]=اليوم … [6]=قبل 6 أيام.
  final List<double> weekAmounts;

  bool get isActive => (remaining ?? 0) > 0;
  bool get isZeroed => (remaining ?? 0) <= 0;

  /// متوقف: باقي > 0 وليس قانوني وآخر تسديد قبل سنة أو بلا تسديد.
  bool get isStopped {
    if (isLegal || !isActive) return false;
    final last = _parseLastPayment();
    if (last == null) return true;
    return DateTime.now().difference(last).inDays >= 365;
  }

  /// مستمر: باقي > 0 وليس قانوني وآخر تسديد خلال سنة.
  bool get isContinuous {
    if (isLegal || !isActive) return false;
    final last = _parseLastPayment();
    if (last == null) return false;
    return DateTime.now().difference(last).inDays < 365;
  }

  /// يظهر للمندوب: ليس قانوني وليس مصفّر.
  bool get isVisibleToDelegate => !isLegal && isActive;

  DateTime? _parseLastPayment() {
    final raw = lastPaymentDate?.trim();
    if (raw == null || raw.isEmpty) return null;
    return DateTime.tryParse(raw);
  }
}

enum CustomerListFilter {
  continuous,
  stopped,
  allVisible,
}

extension CustomerListFilterX on CustomerListFilter {
  String get labelAr => switch (this) {
        CustomerListFilter.continuous => 'المستمرين',
        CustomerListFilter.stopped => 'المتوقفين',
        CustomerListFilter.allVisible => 'الكل',
      };

  bool matches(Customer c) {
    if (!c.isVisibleToDelegate) return false;
    return switch (this) {
      CustomerListFilter.continuous => c.isContinuous,
      CustomerListFilter.stopped => c.isStopped,
      CustomerListFilter.allVisible => true,
    };
  }
}
