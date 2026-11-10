import '../repositories/payments_repository.dart';

class CollectPaymentUseCase {
  CollectPaymentUseCase(this._repo);

  final PaymentsRepository _repo;

  String? get lastMessage => _repo.lastCollectMessage;

  /// Returns `true` when saved locally. Never hangs the UI (no network wait).
  Future<bool> call({
    required int customerId,
    required String customerName,
    required double amount,
  }) {
    return _repo.collectOffline(
      customerId: customerId,
      customerName: customerName,
      amount: amount,
    );
  }
}
