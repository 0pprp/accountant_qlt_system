import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:share_plus/share_plus.dart';
import 'package:team/account/domain/admin_user/admin_user.dart';
import 'package:team/account/domain/login/login_model.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/common/services/request/request.dart';

class ApiAccountProvider {
  final Request request;

  ApiAccountProvider({required this.request});

  void setAuthToken(String token) => request.setAuthToken(token);

  Future<LoginModel> login(String username, String password) async {
    final response = await request.post('api/v1/app/Auth/login', data: {"username": username, "password": password});
    return LoginModel.fromJson(response.data);
  }

  Future<void> logout(String refreshToken) async {
    await request.post('api/v1/app/Auth/logout', data: {'refreshToken': refreshToken});
  }

  Future<LoginModel> refreshToken(String refreshToken) async {
    setAuthToken(refreshToken);
    final response = await request.post('api/v1/app/Auth/refresh-token', data: {'refreshToken': refreshToken});
    final loginModel = LoginModel.fromJson(response.data);
    setAuthToken(loginModel.accessToken);

    return loginModel;
  }

  Future<AdminUser> getCurrentAdminFromServer() async {
    var response = await request.get('api/v1/app/Users/profile');

    return AdminUser.fromJson(response.data);
  }

  Future<User> getUser(final int id) async {
    var response = await request.get('api/v1/app/customers/$id');

    return User.fromJson(response.data);
  }

  Future<void> updateUserData({required String firstName, required String lastName, required XFile? image}) async {
    await request.put(
      'api/v1/users',
      data: FormData.fromMap({
        'firstName': firstName,
        'lastName': lastName,
        if (image != null)
          'profileImage':
              (kIsWeb)
                  ? MultipartFile.fromBytes(
                    await image.readAsBytes(),
                    filename: image.name,
                    contentType: DioMediaType.parse('image/png'),
                  )
                  : await MultipartFile.fromFile(
                    image.path,
                    filename: image.name,
                    contentType: DioMediaType.parse('image/png'),
                  ),
      }),
    );
  }

  Future<void> deleteAvatar() async {
    await request.put('api/v1/userProfile/deleteAvatar');
  }

  Future<String> uploadAvatar(XFile file) async {
    final response = await request.post(
      'api/v1/userProfile/uploadAvatar',
      data: FormData.fromMap({'AvatarFile': await MultipartFile.fromFile(file.path, filename: file.name)}),
    );

    return response.data['avatarFilePath'];
  }
}
