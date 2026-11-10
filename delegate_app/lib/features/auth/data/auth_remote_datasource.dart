import 'package:dio/dio.dart';

import '../../../core/config/app_env.dart';
import '../../../core/network/api_exception.dart';
import '../../../core/network/dio_client.dart';
import '../../../core/network/json_map.dart';
import '../../../core/security/jwt_payload.dart';
import '../domain/entities/delegate_session.dart';

class AuthLoginResult {
  const AuthLoginResult({
    required this.session,
    required this.refreshToken,
  });

  final DelegateSession session;
  final String refreshToken;
}

class AuthRemoteDataSource {
  AuthRemoteDataSource(this._dio);

  final DioClient _dio;

  Future<AuthLoginResult> login({
    required String username,
    required String password,
  }) async {
    try {
      final res = await _dio.dio.post(
        'api/v1/app/Auth/login',
        data: {'username': username, 'password': password},
      );
      final data = res.data;
      if (data is! Map) {
        throw ApiException('استجابة دخول غير متوقعة');
      }
      final access = JsonMap.str(data, ['accessToken', 'AccessToken']);
      final refresh = JsonMap.str(data, ['refreshToken', 'RefreshToken']);
      if (access.isEmpty) {
        throw ApiException(
          'اسم المستخدم أو كلمة السر غير صحيحة',
          statusCode: 401,
        );
      }

      final userId = JwtPayload.userId(access) ?? '';
      if (userId.isEmpty) {
        throw ApiException('تعذر قراءة معرّف المستخدم من الجلسة');
      }

      var displayName = JwtPayload.username(access) ?? username;
      String? listName;
      int? listId;
      var followerCollectionEnabled = false;

      try {
        final profile = await _dio.dio.get(
          'api/v1/app/Users/profile',
          options: Options(headers: {'Authorization': 'Bearer $access'}),
        );
        if (profile.data is Map) {
          final m = profile.data as Map;
          final full = JsonMap.str(m, ['fullName', 'FullName']);
          if (full.isNotEmpty) displayName = full;
          final branches = m['branches'] ?? m['Branches'];
          if (branches is List &&
              branches.isNotEmpty &&
              branches.first is Map) {
            final n = JsonMap.str(branches.first as Map, ['name', 'Name']);
            if (n.isNotEmpty) listName = n;
          }
          final orderList = m['orderList'] ?? m['OrderList'];
          if (orderList is Map) {
            final olMap = orderList as Map;
            final ol = JsonMap.str(olMap, ['name', 'Name']);
            if (ol.isNotEmpty) listName = ol;
            listId = JsonMap.asInt(olMap, ['id', 'Id']);
            followerCollectionEnabled =
                olMap['followerCollectionEnabled'] == true ||
                    olMap['FollowerCollectionEnabled'] == true;
          }
        }
      } catch (_) {}

      return AuthLoginResult(
        refreshToken: refresh,
        session: DelegateSession(
          delegateId: userId,
          asyncId: userId,
          apiBase: AppEnv.apiBaseUrl,
          delegateName: displayName,
          cityName: listName,
          orderListId: listId,
          followerCollectionEnabled: followerCollectionEnabled,
          username: username,
          accessToken: access,
        ),
      );
    } on ApiException {
      rethrow;
    } on DioException catch (e) {
      final err = e.error;
      if (err is ApiException) throw err;
      throw ApiException(
        'اسم المستخدم أو كلمة السر غير صحيحة',
        statusCode: e.response?.statusCode ?? 401,
      );
    }
  }

  Future<void> logoutRemote(String refreshToken) async {
    try {
      await _dio.dio.post(
        'api/v1/app/Auth/logout',
        data: {'refreshToken': refreshToken},
      );
    } catch (_) {}
  }
}
