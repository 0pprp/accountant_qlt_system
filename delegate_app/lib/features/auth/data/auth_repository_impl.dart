import '../../../core/config/app_env.dart';
import '../../../core/network/api_exception.dart';
import '../../../core/security/secure_session_store.dart';
import '../../payments/data/payments_local_datasource.dart';
import '../domain/entities/delegate_session.dart';
import '../domain/repositories/auth_repository.dart';
import 'auth_remote_datasource.dart';

class AuthRepositoryImpl implements AuthRepository {
  AuthRepositoryImpl({
    required AuthRemoteDataSource remote,
    required SecureSessionStore session,
    required PaymentsLocalDataSource paymentsLocal,
  })  : _remote = remote,
        _session = session,
        _paymentsLocal = paymentsLocal;

  final AuthRemoteDataSource _remote;
  final SecureSessionStore _session;
  final PaymentsLocalDataSource _paymentsLocal;

  @override
  Future<DelegateSession> login({
    required String username,
    required String password,
  }) async {
    final result = await _remote.login(
      username: username.trim(),
      password: password.trim(),
    );
    final access = result.session.accessToken;
    if (access == null || access.isEmpty) {
      throw ApiException('فشل تسجيل الدخول');
    }
    await _session.save(
      accessToken: access,
      refreshToken: result.refreshToken,
      userId: result.session.delegateId,
      username: result.session.username,
      delegateName: result.session.delegateName,
      cityName: result.session.cityName,
      orderListId: result.session.orderListId?.toString(),
      followerCollectionEnabled: result.session.followerCollectionEnabled,
    );
    // Reclaim queued payments left under a legacy owner key after JWT login.
    final delegateId = int.tryParse(result.session.delegateId) ?? 0;
    if (delegateId > 0) {
      await _paymentsLocal.reclaimForOwner(
        ownerKey: result.session.delegateId,
        delegateId: delegateId,
      );
    }
    return DelegateSession(
      delegateId: result.session.delegateId,
      asyncId: result.session.asyncId,
      apiBase: AppEnv.apiBaseUrl,
      delegateName: result.session.delegateName,
      cityName: result.session.cityName,
      orderListId: result.session.orderListId,
      followerCollectionEnabled: result.session.followerCollectionEnabled,
      username: result.session.username,
    );
  }

  @override
  Future<DelegateSession?> currentSession() async {
    if (!await _session.hasSession) return null;
    final userId = await _session.userId;
    final owner = await _session.ownerKey;
    if (userId == null || owner == null) return null;
    final listRaw = await _session.orderListId;
    return DelegateSession(
      delegateId: userId,
      asyncId: owner,
      apiBase: AppEnv.apiBaseUrl,
      delegateName: await _session.delegateName,
      cityName: await _session.cityName,
      orderListId: int.tryParse(listRaw ?? ''),
      followerCollectionEnabled: await _session.followerCollectionEnabled,
      username: await _session.username,
    );
  }

  @override
  Future<void> logout({required bool force}) async {
    if (!force) {
      final pending = await _paymentsLocal.countPending();
      if (pending > 0) {
        throw ApiException('يوجد تسديدات معلّقة');
      }
    }
    final refresh = await _session.refreshToken;
    if (refresh != null && refresh.isNotEmpty) {
      await _remote.logoutRemote(refresh);
    }
    await _session.clear();
  }

  @override
  Future<int> pendingPaymentsCount() => _paymentsLocal.countPending();
}
