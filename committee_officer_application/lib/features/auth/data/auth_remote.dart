import 'package:committee_officer_application/core/network/api_exception.dart';
import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/session/session_store.dart';

class AuthRemote {
  AuthRemote(this._dio, this._session);

  final DioClient _dio;
  final SessionStore _session;

  Future<void> login({
    required String userName,
    required String password,
  }) async {
    final res = await _dio.dio.post(
      'api/v1/app/Auth/login',
      data: {'userName': userName, 'password': password},
    );
    final data = res.data;
    if (data is! Map) throw ApiException('استجابة دخول غير صالحة');
    final access = '${data['accessToken'] ?? data['AccessToken'] ?? ''}';
    final refresh = '${data['refreshToken'] ?? data['RefreshToken'] ?? ''}';
    final userId =
        int.tryParse('${data['userId'] ?? data['UserId'] ?? 0}') ?? 0;
    final name = '${data['userName'] ?? data['UserName'] ?? userName}';
    final full = '${data['fullName'] ?? data['FullName'] ?? name}';
    if (access.isEmpty) throw ApiException('لم يُرجع الخادم رمز الدخول');
    await _session.saveSession(
      accessToken: access,
      refreshToken: refresh,
      userId: userId,
      userName: name,
      fullName: full,
    );

    try {
      await verifyRole();
    } catch (_) {
      await _session.clear();
      rethrow;
    }
  }

  Future<void> verifyRole() async {
    final response = await _dio.dio.get('api/v1/app/committee/me');
    final data = response.data;
    if (data is! Map ||
        (data['isCommitteeOfficer'] ?? data['IsCommitteeOfficer']) != true) {
      throw ApiException('الحساب غير مخوّل لتطبيق مسؤول اللجنة');
    }
  }

  Future<bool> refresh() async {
    final refresh = await _session.getRefreshToken();
    if (refresh == null || refresh.isEmpty) return false;
    try {
      final res = await _dio.dio.post(
        'api/v1/app/Auth/refresh-token',
        data: {'refreshToken': refresh},
      );
      final data = res.data;
      if (data is! Map) return false;
      final access = '${data['accessToken'] ?? data['AccessToken'] ?? ''}';
      final newRefresh =
          '${data['refreshToken'] ?? data['RefreshToken'] ?? refresh}';
      if (access.isEmpty) return false;
      final uid = await _session.getUserId() ?? 0;
      final un = await _session.getUserName() ?? '';
      final fn = await _session.getFullName() ?? un;
      await _session.saveSession(
        accessToken: access,
        refreshToken: newRefresh,
        userId: uid,
        userName: un,
        fullName: fn,
      );
      return true;
    } catch (_) {
      return false;
    }
  }

  Future<void> logout() async {
    try {
      final r = await _session.getRefreshToken();
      if (r != null) {
        await _dio.dio.post(
          'api/v1/app/Auth/logout',
          data: {'refreshToken': r},
        );
      }
    } catch (_) {}
    await _session.clear();
  }
}
