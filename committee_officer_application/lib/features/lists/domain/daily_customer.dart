enum PaymentFilter { all, belowInstallment, fullInstallment }

class DailyCustomer {
  const DailyCustomer({
    required this.id,
    required this.name,
    required this.phone,
    required this.paid,
    required this.installment,
    this.hasNote = false,
    this.latestFieldFollowupNote,
    this.hasLocation = false,
    this.activeOrderId,
  });
  final int id;
  final String name;
  final String phone;
  final double paid;
  final double installment;
  final bool hasNote;
  final String? latestFieldFollowupNote;
  final bool hasLocation;
  final int? activeOrderId;
  bool get hasPayment => paid > 0;
  bool get belowInstallment => hasPayment && paid < installment;

  factory DailyCustomer.fromJson(Map<String, dynamic> json) {
    Object? read(String name) =>
        json[name] ?? json['${name[0].toUpperCase()}${name.substring(1)}'];
    double number(String name) => double.tryParse('${read(name)}') ?? 0;
    final noteFlag = read('hasNote');
    final locFlag = read('hasLocation');
    final latest = '${read('latestFieldFollowupNote') ?? ''}'.trim();
    final orderId = int.tryParse('${read('activeOrderId') ?? ''}');
    return DailyCustomer(
      id: number('customerId').toInt(),
      name: '${read('fullName') ?? ''}',
      phone: '${read('phone') ?? ''}',
      paid: number('paidToday'),
      installment: number('dailyInstallment'),
      hasNote: noteFlag == true || noteFlag == 'true',
      latestFieldFollowupNote: latest.isEmpty ? null : latest,
      hasLocation: locFlag == true || locFlag == 'true',
      activeOrderId: orderId is int && orderId > 0 ? orderId : null,
    );
  }

  DailyCustomer copyWith({bool? hasLocation}) => DailyCustomer(
        id: id,
        name: name,
        phone: phone,
        paid: paid,
        installment: installment,
        hasNote: hasNote,
        latestFieldFollowupNote: latestFieldFollowupNote,
        hasLocation: hasLocation ?? this.hasLocation,
        activeOrderId: activeOrderId,
      );

  bool matches(String query) {
    final value = query.trim().toLowerCase();
    return name.toLowerCase().contains(value) || phone.contains(value);
  }

  bool matchesPayment(PaymentFilter filter) => switch (filter) {
    PaymentFilter.all => hasPayment,
    PaymentFilter.belowInstallment => belowInstallment,
    PaymentFilter.fullInstallment => hasPayment && !belowInstallment,
  };
}
