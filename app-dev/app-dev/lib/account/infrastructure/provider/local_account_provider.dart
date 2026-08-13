import 'dart:convert';

import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:hive_ce_flutter/hive_flutter.dart';
import 'package:team/account/domain/admin_user/admin_user.dart';
import 'package:team/account/domain/login/login_model.dart';
import 'package:team/account/domain/user/user.dart';

class LocalAccountProvider {
  late final FlutterSecureStorage _secureStorage;
  late final Box _box;
  final _encryptionKey = 'encryptionKey';
  final _userKey = 'userKey';
  final _accountBoxKey = 'accountBox';
  final _tokenKey = 'tokenKey';
  final _refreshTokenKey = 'refreshTokenKey';
  final _isLoggedKey = 'isLoggedKey';
  final _permissionKey = 'permissionKey';
  final _loginKey = 'loginKey';
  final _usersKey = 'user_';

  Future<void> ensureInitialized() async {
    _secureStorage = const FlutterSecureStorage(aOptions: AndroidOptions(encryptedSharedPreferences: true));
    var encryptionKey = await _secureStorage.read(key: _encryptionKey);

    if (encryptionKey == null) {
      encryptionKey = base64UrlEncode(Hive.generateSecureKey());
      await _secureStorage.write(key: _encryptionKey, value: encryptionKey);
    }

    _box = await Hive.openBox(_accountBoxKey, encryptionCipher: HiveAesCipher(base64Url.decode(encryptionKey)));
  }

  Future<void> saveAccessToken(String accessToken) async {
    return _box.put(_tokenKey, accessToken);
  }

  Future<void> saveRefreshToken(String refreshToken) async {
    return _box.put(_refreshTokenKey, refreshToken);
  }

  Future<void> savePermissions(Permissions? permission) async {
    return _box.put(_permissionKey, permission?.toJson());
  }

  Future<void> saveUserModel(AdminUser user) {
    return _box.put(_userKey, jsonEncode(user.toJson()));
  }

  Future<void> saveIsLogged(bool isLogged) {
    return _box.put(_isLoggedKey, isLogged);
  }

  Future<void> saveLoginModel(LoginModel loginModel) async {
    await _box.put(_loginKey, jsonEncode(loginModel.toJson()));
  }

  LoginModel? getLoginModel() {
    final loginData = _box.get(_loginKey);

    if (loginData == null) return null;

    final json = jsonDecode(loginData);
    return LoginModel.fromJson(json);
  }

  Future<void> saveUser(User user) async {
    await _box.put(_usersKey + user.id.toString(), jsonEncode(user.toJson()));
  }

  User? getUser(int userId) {
    final userData = _box.get(_usersKey + userId.toString());

    if (userData == null) return null;

    final json = jsonDecode(userData);
    return User.fromJson(json);
  }

  String? getAccessToken() {
    final data = _box.get(_tokenKey);

    if (data == null) return null;

    return data;
  }

  String? getRefreshToken() {
    final data = _box.get(_refreshTokenKey);

    if (data == null) return null;

    return data;
  }

  Permissions? getPermission() {
    final data = _box.get(_permissionKey);

    if (data == null) return null;

    return Permissions.fromJson(data);
  }

  AdminUser? getUserModel() {
    final data = _box.get(_userKey);

    if (data == null) return null;

    return AdminUser.fromJson(jsonDecode(data));
  }

  bool getIsLogged() {
    return _box.get(_isLoggedKey) ?? false;
  }

  Future clear() async {
    await _box.deleteAll([_userKey, _tokenKey, _refreshTokenKey, _isLoggedKey, _permissionKey]);
  }
}
