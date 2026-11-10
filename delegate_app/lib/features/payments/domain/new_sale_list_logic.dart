import '../../customers/domain/entities/customer.dart';

/// Splits customers into «مبيعات جديدة» awaiting first تسديد vs routine names.
/// Ported from SalesHaider NewSaleListLogic — store down-payment alone must NOT
/// remove them from المبيعات الجديدة.
class NewSaleListLogic {
  static bool isNewSaleAwaitingFirstCollection(
    Customer customer, {
    required Set<int> locallyCollectedCustomerIds,
  }) {
    if (customer.customerId <= 0) return false;
    if ((customer.remaining ?? 0) <= 0) return false;
    if ((customer.dateSaleDevice ?? '').trim().isEmpty) return false;
    if (locallyCollectedCustomerIds.contains(customer.customerId)) return false;
    if (_hasServerCollectionEvidence(customer)) return false;
    return true;
  }

  static bool _hasServerCollectionEvidence(Customer customer) {
    final paymentCount = customer.countReceipt ?? 0;
    if (paymentCount >= 2) return true;

    final sale = _tryParseDay(customer.dateSaleDevice);
    final last = _tryParseDay(customer.lastPaymentDate);
    if (sale != null && last != null && last.isAfter(sale)) {
      return true;
    }
    return false;
  }

  static DateTime? _tryParseDay(String? raw) {
    final t = (raw ?? '').trim();
    if (t.isEmpty) return null;
    try {
      final d = DateTime.parse(t);
      return DateTime(d.year, d.month, d.day);
    } catch (_) {
      return null;
    }
  }
}
