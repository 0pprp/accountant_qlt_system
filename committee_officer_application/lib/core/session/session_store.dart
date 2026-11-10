import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class SessionStore {
  SessionStore({FlutterSecureStorage? storage})
      : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;

  static const _kAccess = 'co_access_token';
  static const _kRefresh = 'co_refresh_token';
  static const _kUserId = 'co_user_id';
  static const _kUserName = 'co_user_name';
  static const _kFullName = 'co_full_name';
  static const _kDutyActive = 'co_duty_active';
  static const _kDutyDate = 'co_duty_date';

  Future<void> saveSession({
    required String accessToken,
    required String refreshToken,
    required int userId,
    required String userName,
    required String fullName,
  }) async {
    await Future.wait([
      _storage.write(key: _kAccess, value: accessToken),
      _storage.write(key: _kRefresh, value: refreshToken),
      _storage.write(key: _kUserId, value: '$userId'),
      _storage.write(key: _kUserName, value: userName),
      _storage.write(key: _kFullName, value: fullName),
    ]);
  }

  Future<String?> getAccessToken() => _storage.read(key: _kAccess);
  Future<String?> getRefreshToken() => _storage.read(key: _kRefresh);
  Future<String?> getUserName() => _storage.read(key: _kUserName);
  Future<String?> getFullName() => _storage.read(key: _kFullName);

  Future<int?> getUserId() async {
    final raw = await _storage.read(key: _kUserId);
    return raw == null ? null : int.tryParse(raw);
  }

  Future<bool> hasSession() async {
    final t = await getAccessToken();
    return t != null && t.isNotEmpty;
  }

  Future<void> markDutyStarted(String baghdadDate) async {
    await _storage.write(key: _kDutyActive, value: '1');
    await _storage.write(key: _kDutyDate, value: baghdadDate);
  }

  Future<void> markDutyEnded() async {
    await _storage.delete(key: _kDutyActive);
    await _storage.delete(key: _kDutyDate);
  }

  Future<bool> isDutyActiveToday(String baghdadDate) async {
    final active = await _storage.read(key: _kDutyActive);
    final date = await _storage.read(key: _kDutyDate);
    return active == '1' && date == baghdadDate;
  }

  Future<void> clear() async {
    await Future.wait([
      _storage.delete(key: _kAccess),
      _storage.delete(key: _kRefresh),
      _storage.delete(key: _kUserId),
      _storage.delete(key: _kUserName),
      _storage.delete(key: _kFullName),
      _storage.delete(key: _kDutyActive),
      _storage.delete(key: _kDutyDate),
    ]);
  }
}
