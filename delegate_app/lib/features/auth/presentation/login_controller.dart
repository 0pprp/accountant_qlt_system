import 'package:flutter/foundation.dart';

import '../domain/usecases/login_usecase.dart';

class LoginController extends ChangeNotifier {
  LoginController(this._login);

  final LoginUseCase _login;

  bool loading = false;
  String? error;

  Future<bool> submit(String username, String password) async {
    loading = true;
    error = null;
    notifyListeners();
    try {
      await _login(username: username, password: password);
      return true;
    } catch (e) {
      error = _clean(e);
      return false;
    } finally {
      loading = false;
      notifyListeners();
    }
  }

  String _clean(Object e) {
    final raw = e.toString();
    if (raw.contains('This exception was thrown because the response') ||
        raw.contains('validateStatus') ||
        raw.contains('DioException')) {
      return 'اسم المستخدم أو كلمة السر غير صحيحة';
    }
    if (raw.contains('connection error') || raw.contains('SocketException')) {
      return 'لا يوجد اتصال بالإنترنت';
    }
    return raw
        .replaceFirst('Exception: ', '')
        .replaceFirst('ApiException: ', '')
        .replaceFirst('Invalid argument(s): ', '')
        .replaceFirst(RegExp(r'DioException\s*\[[^\]]*\]:\s*'), '')
        .trim();
  }
}
