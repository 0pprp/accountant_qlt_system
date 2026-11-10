import '../entities/pending_payment.dart';
import '../entities/today_payment_item.dart';

typedef SyncProgressCallback = void Function(int done, int total);

abstract class PaymentsRepository {
  /// Saves locally and returns whether saved. Never waits on network/GPS long.
  Future<bool> collectOffline({
    required int customerId,
    required String customerName,
    required double amount,
  });

  /// Last block/error message after a failed collect.
  String? get lastCollectMessage;

  /// Last error from the most recent sync attempt (if any payment failed).
  String? get lastSyncError;

  Future<List<PendingPayment>> listPending();
  Future<List<PendingPayment>> listTodayLocal();

  /// Local pending + today's payments already on the server.
  Future<List<TodayPaymentItem>> listTodayMerged();

  Future<int> syncPending({SyncProgressCallback? onProgress});
  Future<int> countPending();
}
