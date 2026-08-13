import 'package:flutter/foundation.dart';
import 'package:get_it/get_it.dart';
import 'package:share_plus/share_plus.dart';
import 'package:team/account/domain/admin_user/admin_user.dart';
import 'package:team/account/domain/login/login_model.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/account/infrastructure/provider/api_account_provider.dart';
import 'package:team/account/infrastructure/provider/local_account_provider.dart';
import 'package:team/home/infrastructure/providers/local_home_provider.dart';
import 'package:team/order/infrastructure/providers/local_order_provider.dart';
import 'package:team/payment/infrastructure/providers/local_payment_provider.dart';

class AccountRepository {
  final ApiAccountProvider apiAuthProvider;
  final LocalAccountProvider localAccountProvider;

  AccountRepository({required this.apiAuthProvider, required this.localAccountProvider});

  AdminUser? user;

  Future<void> ensureInitialized() => localAccountProvider.ensureInitialized();

  Future<LoginModel> login(String username, String password) async {
    final loginModel = await apiAuthProvider.login(username, password);
    await localAccountProvider.saveLoginModel(loginModel);
    return loginModel;
  }

  Future<void> saveToken(String token) async {
    await localAccountProvider.saveAccessToken(token);
    apiAuthProvider.setAuthToken(token);
  }

  Future<void> saveRefreshToken(String token) => localAccountProvider.saveRefreshToken(token);

  Future<void> savePermission(Permissions role) => localAccountProvider.savePermissions(role);

  Future<void> saveUser(AdminUser user) {
    this.user = user;
    return localAccountProvider.saveUserModel(user);
  }

  Future<AdminUser?> getCurrentAdminUser() async {
    try {
      if (user != null) {
        return user;
      }
      try {
        final savedUser = localAccountProvider.getUserModel();
        if (savedUser != null) {
          user = savedUser;

          return savedUser;
        }
      } catch (e) {
        debugPrint(e.toString());
      }
      final serverUser = await apiAuthProvider.getCurrentAdminFromServer();
      user = serverUser;
      await saveUser(serverUser);

      return serverUser;
    } catch (e) {
      debugPrint(e.toString());
    }
    return null;
  }

  Future<AdminUser> getCurrentAdminFromServer() async {
    final serverUser = await apiAuthProvider.getCurrentAdminFromServer();
    user = serverUser;
    await saveUser(serverUser);
    return serverUser;
  }

  Future<User> getUser(final int id, {bool fromServer = false}) async {
    User? localUser;
    if (!fromServer) {
      try {
        localUser = localAccountProvider.getUser(id);
      } catch (e) {
        debugPrint(e.toString());
      }
      if (localUser != null) {
        try {
          apiAuthProvider.getUser(id).then((serverUser) async {
            await localAccountProvider.saveUser(serverUser);
          });
        } catch (e) {
          debugPrint(e.toString());
        }
        return localUser;
      }
    }
    final serverUser = await apiAuthProvider.getUser(id);
    await localAccountProvider.saveUser(serverUser);
    return serverUser;
  }

  Permissions? getPermission() => localAccountProvider.getPermission();

  String? get getToken => localAccountProvider.getAccessToken();

  String? get getRefreshToken => localAccountProvider.getRefreshToken();

  bool get isLogged => localAccountProvider.getIsLogged();

  Permissions? get permissions => localAccountProvider.getLoginModel()?.permissions;

  List<Roles>? get roles => localAccountProvider.getLoginModel()?.roles;

  bool get isMotaba => localAccountProvider.getLoginModel()?.roles.any((element) => element == Roles.Motaba) ?? false;

  Future<void> saveIsLogged(bool isLogged) => localAccountProvider.saveIsLogged(isLogged);

  Future<void> refreshToken() async {
    final refreshToken = getRefreshToken;

    if (refreshToken != null) {
      final response = await apiAuthProvider.refreshToken(refreshToken);
      await saveToken(response.accessToken);
      await saveRefreshToken(response.refreshToken);
      await localAccountProvider.saveLoginModel(response);
      // try {
      //   if (tokenResponse.$3 != null) {
      //     // await saveRole(Permissions.values[tokenResponse.$3!]);
      //   }
      // } catch (e) {
      //   debugPrint(e.toString());
      // }
    } else {
      debugPrint('error');
      throw Exception();
    }
  }

  Future<void> deleteAvatar() => apiAuthProvider.deleteAvatar();

  Future<String> uploadAvatar(XFile file) => apiAuthProvider.uploadAvatar(file);

  Future<void> logout() async {
    user = null;
    final refreshToken = getRefreshToken;
    await localAccountProvider.clear();
    await GetIt.I.get<LocalOrderProvider>().clear();
    await GetIt.I.get<LocalPaymentProvider>().clear();
    await GetIt.I.get<LocalHomeProvider>().clear();
    if (refreshToken != null) {
      await apiAuthProvider.logout(refreshToken);
    }
  }
}
