import 'package:dio/dio.dart';
import 'package:sales_employee_app/core/network/api_exception.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/network/json_map.dart';
import 'package:sales_employee_app/core/security/jwt_payload.dart';
import 'package:sales_employee_app/features/auth/domain/entities/sales_session.dart';

class AuthRemoteDataSource {
  AuthRemoteDataSource(this._dio);
  final DioClient _dio;

  Future<SalesSession> login({
    required String username,
    required String password,
  }) async {
    try {
      final res = await _dio.dio.post(
        'api/v1/app/Auth/login',
        data: {'username': username, 'password': password},
      );
      final data = res.data;
      if (data is! Map) throw ApiException('استجابة دخول غير متوقعة');

      final access = JsonMap.str(data, ['accessToken', 'AccessToken']);
      final refresh = JsonMap.str(data, ['refreshToken', 'RefreshToken']);
      if (access.isEmpty) {
        throw ApiException('اسم المستخدم أو كلمة السر غير صحيحة', statusCode: 401);
      }

      final uidRaw = JwtPayload.userId(access);
      final userId = int.tryParse(uidRaw ?? '') ?? 0;
      var fullName = JwtPayload.username(access) ?? username;

      try {
        final profile = await _dio.dio.get(
          'api/v1/app/Users/profile',
          options: Options(headers: {'Authorization': 'Bearer $access'}),
        );
        if (profile.data is Map) {
          final full = JsonMap.str(profile.data as Map, ['fullName', 'FullName']);
          if (full.isNotEmpty) fullName = full;
        }
      } catch (_) {}

      return SalesSession(
        userId: userId,
        userName: username,
        fullName: fullName,
        accessToken: access,
        refreshToken: refresh,
      );
    } on ApiException {
      rethrow;
    } on DioException catch (e) {
      final err = e.error;
      if (err is ApiException) throw err;
      throw ApiException(
        DioClient.mapError(e),
        statusCode: e.response?.statusCode,
      );
    }
  }

  Future<SalesSession?> refresh(String refreshToken) async {
    try {
      final res = await _dio.dio.post(
        'api/v1/app/Auth/refresh-token',
        data: {'refreshToken': refreshToken},
      );
      final data = res.data;
      if (data is! Map) return null;
      final access = JsonMap.str(data, ['accessToken', 'AccessToken']);
      final refresh = JsonMap.str(data, ['refreshToken', 'RefreshToken']);
      if (access.isEmpty) return null;
      final uid = int.tryParse(JwtPayload.userId(access) ?? '') ?? 0;
      final name = JwtPayload.username(access) ?? '';
      return SalesSession(
        userId: uid,
        userName: name,
        fullName: name,
        accessToken: access,
        refreshToken: refresh.isEmpty ? refreshToken : refresh,
      );
    } catch (_) {
      return null;
    }
  }

  Future<void> logout(String refreshToken) async {
    try {
      await _dio.dio.post(
        'api/v1/app/Auth/logout',
        data: {'refreshToken': refreshToken},
      );
    } catch (_) {}
  }
}
