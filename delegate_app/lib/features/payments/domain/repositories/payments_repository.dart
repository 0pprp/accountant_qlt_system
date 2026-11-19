import '../entities/paid_collection.dart';
import '../entities/pending_payment.dart';
import '../entities/today_payment_item.dart';

typedef SyncProgressCallback = void Function(int done, int total);

abstract class PaymentsRepository {
  /// Saves locally and returns whether saved. Never waits on network/GPS long.
  Future<bool> collectOffline({
    required int customerId,
    required String customerName,
    int? saleOrderId,
    required double amount,
  });

  /// Last block/error message after a failed collect.
  String? get lastCollectMessage;

  /// Last error from the most recent sync attempt (if any payment failed).
  String? get lastSyncError;

  /// آخر تسديدة ناجحة محلياً لهذا الزبون في يوم العمل الحالي (UUID + مبلغ).
  Future<PaidCollection?> todayPaidForCustomer(int customerId);

  Future<List<PendingPayment>> listPending();
  Future<List<PendingPayment>> listTodayLocal();

  /// مخزن المسددين + طابور اليوم + تسديدات السيرفر لليوم.
  Future<List<TodayPaymentItem>> listTodayMerged();

  Future<int> syncPending({SyncProgressCallback? onProgress});
  Future<int> countPending();
}
