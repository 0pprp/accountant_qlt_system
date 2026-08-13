import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:team/account/domain/permission_actions/permission_actions.dart';

part 'login_model.freezed.dart';
part 'login_model.g.dart';

@freezed
class LoginModel with _$LoginModel {
  const factory LoginModel({
    required String accessToken,
    required String refreshToken,
    required Permissions permissions,
    required List<Roles> roles,
  }) = _LoginModel;

  factory LoginModel.fromJson(Map<String, Object?> json) => _$LoginModelFromJson(json);
}

@JsonEnum()
enum Roles { Admin, ChiefAccountant, MainAccountant, BranchAccountant, BranchManager, Ceo, Mandob, Motaba }

@freezed
class Permissions with _$Permissions {
  const factory Permissions({
    @JsonKey(name: 'InstallmentPayment')
    @PermissionActionsConverter()
    @Default(PermissionActions())
    PermissionActions? installmentPayment,
    @JsonKey(name: 'Order') @PermissionActionsConverter() @Default(PermissionActions()) PermissionActions? order,
    @JsonKey(name: 'OrderList')
    @PermissionActionsConverter()
    @Default(PermissionActions())
    PermissionActions? orderList,
    @JsonKey(name: 'User') @PermissionActionsConverter() @Default(PermissionActions()) PermissionActions? user,
    @JsonKey(name: 'Warehouse')
    @PermissionActionsConverter()
    @Default(PermissionActions())
    PermissionActions? warehouse,
  }) = _Permissions;

  factory Permissions.fromJson(Map<String, Object?> json) => _$PermissionsFromJson(json);
}

class PermissionActionsConverter implements JsonConverter<PermissionActions, List<dynamic>> {
  const PermissionActionsConverter();

  @override
  PermissionActions fromJson(List<dynamic> jsonList) {
    bool canCreate = false;
    bool canRead = false;
    bool canUpdate = false;
    bool canDelete = false;

    for (var item in jsonList) {
      if (item['name'] is String) {
        switch (item['name']?.toLowerCase()) {
          // Use toLowerCase for case-insensitivity
          case 'create':
            canCreate = true;
            break;
          case 'read':
            canRead = true;
            break;
          case 'update':
            canUpdate = true;
            break;
          case 'delete':
            canDelete = true;
            break;
        }
      }
    }
    return PermissionActions(canCreate: canCreate, canRead: canRead, canUpdate: canUpdate, canDelete: canDelete);
  }

  @override
  List<Map<String, String>> toJson(PermissionActions object) {
    // Convert PermissionActions back to List<String> if you need to serialize it
    // This might not be strictly necessary if you only deserialize this structure
    final List<Map<String, String>> permissions = [];

    if (object.canCreate) permissions.add({'name': 'Create'});
    if (object.canRead) permissions.add({'name': 'Read'});
    if (object.canUpdate) permissions.add({'name': 'Update'});
    if (object.canDelete) permissions.add({'name': 'Delete'});
    return permissions;
  }
}
