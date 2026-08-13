import 'package:get_it/get_it.dart';
import 'package:team/account/domain/admin_user/admin_user.dart';
import 'package:team/account/domain/login/login_model.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/account/infrastructure/repository/account_repository.dart';

class AccountMapper {
  late final authRepository = GetIt.I.get<AccountRepository>();

  bool get isLoggedIn => authRepository.isLogged;

  AdminUser? get user => authRepository.user;

  Permissions? get permissions => authRepository.permissions;

  List<Roles>? get roles => authRepository.roles;

  bool get isMotaba => authRepository.isMotaba;

  Future<void> ensureInitialized() => authRepository.ensureInitialized();

  Future<AdminUser?> getCurrentAdminUser() => authRepository.getCurrentAdminUser();

  Future<AdminUser?> getCurrentAdminUserFromServer() => authRepository.getCurrentAdminFromServer();

  Future<User> getUser(final int id, {bool fromServer = false}) => authRepository.getUser(id, fromServer: fromServer);

  Future<void> saveUser(AdminUser user) => authRepository.saveUser(user);

  Future<void> refreshToken() => authRepository.refreshToken();

  String? authToken() => authRepository.getToken;

  Future<void> logout() => authRepository.logout();
}
