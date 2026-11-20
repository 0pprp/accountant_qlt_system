import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class SessionStore {
  SessionStore({FlutterSecureStorage? storage})
    : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;

  static const _kAccess = 'fp_access_token';
  static const _kRefresh = 'fp_refresh_token';
  static const _kUserName = 'fp_user_name';

  String? _accessCache;

  Future<void> saveTokens({
    required String accessToken,
    required String refreshToken,
  }) async {
    _accessCache = accessToken;
    await Future.wait([
      _storage.write(key: _kAccess, value: accessToken),
      _storage.write(key: _kRefresh, value: refreshToken),
    ]);
  }

  Future<void> saveUserName(String userName) =>
      _storage.write(key: _kUserName, value: userName);

  Future<String?> getAccessToken() async =>
      _accessCache ??= await _storage.read(key: _kAccess);

  Future<String?> getRefreshToken() => _storage.read(key: _kRefresh);

  Future<String?> getUserName() => _storage.read(key: _kUserName);

  Future<bool> hasSession() async {
    final t = await getAccessToken();
    return t != null && t.isNotEmpty;
  }

  Future<void> clear() async {
    _accessCache = null;
    await Future.wait([
      _storage.delete(key: _kAccess),
      _storage.delete(key: _kRefresh),
      _storage.delete(key: _kUserName),
    ]);
  }
}
