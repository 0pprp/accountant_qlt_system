import '../entities/delegate_session.dart';

abstract class AuthRepository {
  Future<DelegateSession> login({
    required String username,
    required String password,
  });

  Future<DelegateSession?> currentSession();

  /// Re-read جباية المتابع from API and update local session cache.
  Future<bool> refreshFollowerCollectionEnabled();

  Future<void> logout({required bool force});

  Future<int> pendingPaymentsCount();
}
