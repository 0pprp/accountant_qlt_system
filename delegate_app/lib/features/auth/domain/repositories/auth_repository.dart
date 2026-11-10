import '../entities/delegate_session.dart';

abstract class AuthRepository {
  Future<DelegateSession> login({
    required String username,
    required String password,
  });

  Future<DelegateSession?> currentSession();

  Future<void> logout({required bool force});

  Future<int> pendingPaymentsCount();
}
