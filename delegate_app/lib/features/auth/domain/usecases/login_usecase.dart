import '../entities/delegate_session.dart';
import '../repositories/auth_repository.dart';

class LoginUseCase {
  LoginUseCase(this._repo);

  final AuthRepository _repo;

  Future<DelegateSession> call({
    required String username,
    required String password,
  }) {
    final u = username.trim();
    final p = password.trim();
    if (u.isEmpty) {
      throw ArgumentError('أدخل اسم المستخدم');
    }
    if (p.isEmpty) {
      throw ArgumentError('أدخل كلمة السر');
    }
    return _repo.login(username: u, password: p);
  }
}
