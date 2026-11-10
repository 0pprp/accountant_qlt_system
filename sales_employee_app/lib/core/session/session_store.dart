import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// جلسة الموظف.
///
/// على **Web** لا نعتمد flutter_secure_storage للـJWT: قراءة AES-GCM تفشل أحيانًا
/// بـ DomException/OperationError بعد الكتابة الناجحة، فيظهر Dio كـ `unknown`
/// قبل إرسال الطلب (`session_read_failed`). نستخدم SharedPreferences + ذاكرة.
class SessionStore {
  SessionStore({FlutterSecureStorage? storage})
      : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;

  static const _kAccess = 'access_token';
  static const _kRefresh = 'refresh_token';
  static const _kUserId = 'user_id';
  static const _kUserName = 'user_name';
  static const _kFullName = 'full_name';
  static const _webPrefix = 'sales_session:';

  String? _accessMem;
  String? _refreshMem;
  String? _userIdMem;
  String? _userNameMem;
  String? _fullNameMem;

  Future<void> saveSession({
    required String accessToken,
    required String refreshToken,
    required int userId,
    required String userName,
    required String fullName,
  }) async {
    _accessMem = accessToken;
    _refreshMem = refreshToken;
    _userIdMem = '$userId';
    _userNameMem = userName;
    _fullNameMem = fullName;

    if (kIsWeb) {
      final p = await SharedPreferences.getInstance();
      await Future.wait([
        p.setString('$_webPrefix$_kAccess', accessToken),
        p.setString('$_webPrefix$_kRefresh', refreshToken),
        p.setString('$_webPrefix$_kUserId', '$userId'),
        p.setString('$_webPrefix$_kUserName', userName),
        p.setString('$_webPrefix$_kFullName', fullName),
      ]);
      return;
    }

    try {
      await Future.wait([
        _storage.write(key: _kAccess, value: accessToken),
        _storage.write(key: _kRefresh, value: refreshToken),
        _storage.write(key: _kUserId, value: '$userId'),
        _storage.write(key: _kUserName, value: userName),
        _storage.write(key: _kFullName, value: fullName),
      ]);
    } catch (_) {
      // الذاكرة كافية لجلسة التشغيل الحالية إن فشل التخزين الآمن.
    }
  }

  Future<String?> getAccessToken() => _read(_kAccess, () => _accessMem, (v) => _accessMem = v);
  Future<String?> getRefreshToken() => _read(_kRefresh, () => _refreshMem, (v) => _refreshMem = v);
  Future<String?> getUserName() => _read(_kUserName, () => _userNameMem, (v) => _userNameMem = v);
  Future<String?> getFullName() => _read(_kFullName, () => _fullNameMem, (v) => _fullNameMem = v);

  Future<int?> getUserId() async {
    final raw = await _read(_kUserId, () => _userIdMem, (v) => _userIdMem = v);
    return raw == null ? null : int.tryParse(raw);
  }

  Future<bool> hasSession() async {
    final t = await getAccessToken();
    return t != null && t.isNotEmpty;
  }

  Future<void> clear() async {
    _accessMem = null;
    _refreshMem = null;
    _userIdMem = null;
    _userNameMem = null;
    _fullNameMem = null;

    if (kIsWeb) {
      final p = await SharedPreferences.getInstance();
      await Future.wait([
        p.remove('$_webPrefix$_kAccess'),
        p.remove('$_webPrefix$_kRefresh'),
        p.remove('$_webPrefix$_kUserId'),
        p.remove('$_webPrefix$_kUserName'),
        p.remove('$_webPrefix$_kFullName'),
      ]);
      return;
    }

    try {
      await Future.wait([
        _storage.delete(key: _kAccess),
        _storage.delete(key: _kRefresh),
        _storage.delete(key: _kUserId),
        _storage.delete(key: _kUserName),
        _storage.delete(key: _kFullName),
      ]);
    } catch (_) {}
  }

  Future<String?> _read(
    String key,
    String? Function() memGet,
    void Function(String?) memSet,
  ) async {
    final cached = memGet();
    if (cached != null && cached.isNotEmpty) return cached;

    if (kIsWeb) {
      final p = await SharedPreferences.getInstance();
      final v = p.getString('$_webPrefix$key');
      memSet(v);
      return v;
    }

    try {
      final v = await _storage.read(key: key);
      memSet(v);
      return v;
    } catch (_) {
      // DomException / plugin gaps — لا تُسقط طلبات Dio كـ unknown صامت.
      return memGet();
    }
  }
}
