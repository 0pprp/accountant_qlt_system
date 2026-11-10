import 'package:sales_employee_app/core/session/session_store.dart';
import 'package:sales_employee_app/features/auth/data/auth_remote_datasource.dart';
import 'package:sales_employee_app/features/auth/domain/entities/sales_session.dart';
import 'package:sales_employee_app/features/auth/domain/repositories/auth_repository.dart';

class AuthRepositoryImpl implements AuthRepository {
  AuthRepositoryImpl({
    required AuthRemoteDataSource remote,
    required SessionStore session,
  })  : _remote = remote,
        _session = session;

  final AuthRemoteDataSource _remote;
  final SessionStore _session;

  @override
  Future<SalesSession> login({
    required String username,
    required String password,
  }) async {
    final s = await _remote.login(username: username, password: password);
    await _session.saveSession(
      accessToken: s.accessToken,
      refreshToken: s.refreshToken,
      userId: s.userId,
      userName: s.userName,
      fullName: s.fullName,
    );
    return s;
  }

  @override
  Future<bool> refresh() async {
    final refresh = await _session.getRefreshToken();
    if (refresh == null || refresh.isEmpty) return false;
    final s = await _remote.refresh(refresh);
    if (s == null) return false;
    final fullName = (await _session.getFullName()) ?? s.fullName;
    final userName = (await _session.getUserName()) ?? s.userName;
    await _session.saveSession(
      accessToken: s.accessToken,
      refreshToken: s.refreshToken,
      userId: s.userId,
      userName: userName,
      fullName: fullName,
    );
    return true;
  }

  @override
  Future<void> logout() async {
    final refresh = await _session.getRefreshToken();
    if (refresh != null) await _remote.logout(refresh);
    await _session.clear();
  }

  @override
  Future<bool> hasSession() => _session.hasSession();

  @override
  Future<SalesSession?> currentSession() async {
    final access = await _session.getAccessToken();
    final refresh = await _session.getRefreshToken();
    final userId = await _session.getUserId();
    final userName = await _session.getUserName();
    final fullName = await _session.getFullName();
    if (access == null || refresh == null || userId == null) return null;
    return SalesSession(
      userId: userId,
      userName: userName ?? '',
      fullName: fullName ?? '',
      accessToken: access,
      refreshToken: refresh,
    );
  }
}
