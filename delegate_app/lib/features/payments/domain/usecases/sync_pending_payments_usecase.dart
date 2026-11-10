import '../repositories/payments_repository.dart';

class SyncPendingPaymentsUseCase {
  SyncPendingPaymentsUseCase(this._repo);

  final PaymentsRepository _repo;

  Future<int> call({SyncProgressCallback? onProgress}) =>
      _repo.syncPending(onProgress: onProgress);
}
