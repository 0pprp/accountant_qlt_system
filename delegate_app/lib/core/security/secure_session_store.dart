import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../config/app_env.dart';

class SecureSessionStore {
  SecureSessionStore({FlutterSecureStorage? storage})
      : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;

  static const _kAccessToken = 'access_token';
  static const _kRefreshToken = 'refresh_token';
  static const _kUserId = 'user_id';
  static const _kUsername = 'username';
  static const _kDelegateName = 'delegate_name';
  static const _kCityName = 'city_name';
  static const _kOrderListId = 'order_list_id';
  static const _kFollowerCollectionEnabled = 'follower_collection_enabled';
  /// Owner key for offline payment queue (equals userId).
  static const _kOwnerKey = 'owner_key';
  /// Legacy key from pre-JWT login (city/asyncId flow).
  static const _kLegacyAsyncId = 'async_id';

  Future<String?> get accessToken => _storage.read(key: _kAccessToken);
  Future<String?> get refreshToken => _storage.read(key: _kRefreshToken);
  Future<String?> get userId => _storage.read(key: _kUserId);
  Future<String?> get username => _storage.read(key: _kUsername);
  Future<String?> get delegateName => _storage.read(key: _kDelegateName);
  Future<String?> get cityName => _storage.read(key: _kCityName);
  Future<String?> get orderListId => _storage.read(key: _kOrderListId);
  Future<bool> get followerCollectionEnabled async {
    final v = await _storage.read(key: _kFollowerCollectionEnabled);
    return v == '1' || v == 'true';
  }

  Future<String?> get ownerKey async {
    final current = await _storage.read(key: _kOwnerKey);
    if (current != null && current.isNotEmpty) return current;

    // Migrate: prefer userId, then legacy async_id.
    final uid = await _storage.read(key: _kUserId);
    if (uid != null && uid.isNotEmpty) {
      await _storage.write(key: _kOwnerKey, value: uid);
      return uid;
    }
    final legacy = await _storage.read(key: _kLegacyAsyncId);
    if (legacy != null && legacy.isNotEmpty) {
      await _storage.write(key: _kOwnerKey, value: legacy);
      return legacy;
    }
    return null;
  }

  /// Backward-compatible aliases used across the app.
  Future<String?> get delegateId => userId;
  Future<String?> get asyncId => ownerKey;
  Future<String?> get apiBase async => AppEnv.apiBaseUrl;

  Future<bool> get hasSession async {
    final token = await accessToken;
    final id = await userId;
    return token != null &&
        token.isNotEmpty &&
        id != null &&
        id.isNotEmpty;
  }

  Future<void> save({
    required String accessToken,
    required String refreshToken,
    required String userId,
    String? username,
    String? delegateName,
    String? cityName,
    String? orderListId,
    bool? followerCollectionEnabled,
  }) async {
    await _storage.write(key: _kAccessToken, value: accessToken);
    await _storage.write(key: _kRefreshToken, value: refreshToken);
    await _storage.write(key: _kUserId, value: userId);
    await _storage.write(key: _kOwnerKey, value: userId);
    if (username != null) {
      await _storage.write(key: _kUsername, value: username);
    }
    if (delegateName != null) {
      await _storage.write(key: _kDelegateName, value: delegateName);
    }
    if (cityName != null) {
      await _storage.write(key: _kCityName, value: cityName);
    }
    if (orderListId != null && orderListId.isNotEmpty) {
      await _storage.write(key: _kOrderListId, value: orderListId);
    }
    if (followerCollectionEnabled != null) {
      await _storage.write(
        key: _kFollowerCollectionEnabled,
        value: followerCollectionEnabled ? '1' : '0',
      );
    }
  }

  Future<void> saveTokens({
    required String accessToken,
    required String refreshToken,
  }) async {
    await _storage.write(key: _kAccessToken, value: accessToken);
    await _storage.write(key: _kRefreshToken, value: refreshToken);
  }

  Future<void> clear() async {
    await _storage.deleteAll();
  }
}
