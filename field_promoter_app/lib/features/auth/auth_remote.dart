import '../../core/network/api_exception.dart';
import '../../core/network/dio_client.dart';
import '../../core/session/session_store.dart';

class AuthRemote {
  AuthRemote(this._client, this._session);

  final DioClient _client;
  final SessionStore _session;

  Future<void> login({required String username, required String password}) async {
    final res = await _client.dio.post(
      'api/v1/app/Auth/login',
      data: {'username': username.trim(), 'password': password},
    );
    final data = res.data;
    if (data is! Map) throw ApiException('استجابة دخول غير صالحة');
    final access = '${data['accessToken'] ?? ''}';
    final refresh = '${data['refreshToken'] ?? ''}';
    if (access.isEmpty) throw ApiException('لم يُرجع الخادم رمز الدخول');
    await _session.saveTokens(accessToken: access, refreshToken: refresh);
    await _session.saveUserName(username.trim());
  }

  Future<RefreshOutcome> refresh() async {
    final token = await _session.getRefreshToken();
    if (token == null || token.isEmpty) return RefreshOutcome.rejected;
    try {
      final res = await _client.dio.post(
        'api/v1/app/Auth/refresh-token',
        data: {'refreshToken': token},
      );
      final data = res.data;
      if (data is! Map) return RefreshOutcome.unreachable;
      final access = '${data['accessToken'] ?? ''}';
      final next = '${data['refreshToken'] ?? ''}';
      if (access.isEmpty) return RefreshOutcome.rejected;
      await _session.saveTokens(
        accessToken: access,
        refreshToken: next.isEmpty ? token : next,
      );
      return RefreshOutcome.ok;
    } catch (e) {
      return DioClient.classifyRefreshFailure(e);
    }
  }

  Future<void> logout() async {
    try {
      final token = await _session.getRefreshToken();
      if (token != null && token.isNotEmpty) {
        await _client.dio.post(
          'api/v1/app/Auth/logout',
          data: {'refreshToken': token},
        );
      }
    } catch (_) {}
    await _session.clear();
  }
}
