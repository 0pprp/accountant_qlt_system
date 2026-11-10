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
  });
  final int id;
  final String name;
  final String phone;
  final double paid;
  final double installment;
  final bool hasNote;
  final String? latestFieldFollowupNote;
  bool get hasPayment => paid > 0;
  bool get belowInstallment => hasPayment && paid < installment;

  factory DailyCustomer.fromJson(Map<String, dynamic> json) {
    Object? read(String name) =>
        json[name] ?? json['${name[0].toUpperCase()}${name.substring(1)}'];
    double number(String name) => double.tryParse('${read(name)}') ?? 0;
    final noteFlag = read('hasNote');
    final latest = '${read('latestFieldFollowupNote') ?? ''}'.trim();
    return DailyCustomer(
      id: number('customerId').toInt(),
      name: '${read('fullName') ?? ''}',
      phone: '${read('phone') ?? ''}',
      paid: number('paidToday'),
      installment: number('dailyInstallment'),
      hasNote: noteFlag == true || noteFlag == 'true',
      latestFieldFollowupNote: latest.isEmpty ? null : latest,
    );
  }

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
