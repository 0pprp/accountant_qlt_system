// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'login_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$LoginModelImpl _$$LoginModelImplFromJson(Map<String, dynamic> json) =>
    _$LoginModelImpl(
      accessToken: json['accessToken'] as String,
      refreshToken: json['refreshToken'] as String,
      permissions: Permissions.fromJson(
        json['permissions'] as Map<String, dynamic>,
      ),
      roles:
          (json['roles'] as List<dynamic>)
              .map((e) => $enumDecode(_$RolesEnumMap, e))
              .toList(),
    );

Map<String, dynamic> _$$LoginModelImplToJson(_$LoginModelImpl instance) =>
    <String, dynamic>{
      'accessToken': instance.accessToken,
      'refreshToken': instance.refreshToken,
      'permissions': instance.permissions,
      'roles': instance.roles.map((e) => _$RolesEnumMap[e]!).toList(),
    };

const _$RolesEnumMap = {
  Roles.Admin: 'Admin',
  Roles.ChiefAccountant: 'ChiefAccountant',
  Roles.MainAccountant: 'MainAccountant',
  Roles.BranchAccountant: 'BranchAccountant',
  Roles.BranchManager: 'BranchManager',
  Roles.Ceo: 'Ceo',
  Roles.Mandob: 'Mandob',
  Roles.Motaba: 'Motaba',
};

_$PermissionsImpl _$$PermissionsImplFromJson(Map<String, dynamic> json) =>
    _$PermissionsImpl(
      installmentPayment:
          _$JsonConverterFromJson<List<dynamic>, PermissionActions>(
            json['InstallmentPayment'],
            const PermissionActionsConverter().fromJson,
          ) ??
          const PermissionActions(),
      order:
          _$JsonConverterFromJson<List<dynamic>, PermissionActions>(
            json['Order'],
            const PermissionActionsConverter().fromJson,
          ) ??
          const PermissionActions(),
      orderList:
          _$JsonConverterFromJson<List<dynamic>, PermissionActions>(
            json['OrderList'],
            const PermissionActionsConverter().fromJson,
          ) ??
          const PermissionActions(),
      user:
          _$JsonConverterFromJson<List<dynamic>, PermissionActions>(
            json['User'],
            const PermissionActionsConverter().fromJson,
          ) ??
          const PermissionActions(),
      warehouse:
          _$JsonConverterFromJson<List<dynamic>, PermissionActions>(
            json['Warehouse'],
            const PermissionActionsConverter().fromJson,
          ) ??
          const PermissionActions(),
    );

Map<String, dynamic> _$$PermissionsImplToJson(
  _$PermissionsImpl instance,
) => <String, dynamic>{
  'InstallmentPayment': _$JsonConverterToJson<List<dynamic>, PermissionActions>(
    instance.installmentPayment,
    const PermissionActionsConverter().toJson,
  ),
  'Order': _$JsonConverterToJson<List<dynamic>, PermissionActions>(
    instance.order,
    const PermissionActionsConverter().toJson,
  ),
  'OrderList': _$JsonConverterToJson<List<dynamic>, PermissionActions>(
    instance.orderList,
    const PermissionActionsConverter().toJson,
  ),
  'User': _$JsonConverterToJson<List<dynamic>, PermissionActions>(
    instance.user,
    const PermissionActionsConverter().toJson,
  ),
  'Warehouse': _$JsonConverterToJson<List<dynamic>, PermissionActions>(
    instance.warehouse,
    const PermissionActionsConverter().toJson,
  ),
};

Value? _$JsonConverterFromJson<Json, Value>(
  Object? json,
  Value? Function(Json json) fromJson,
) => json == null ? null : fromJson(json as Json);

Json? _$JsonConverterToJson<Json, Value>(
  Value? value,
  Json? Function(Value value) toJson,
) => value == null ? null : toJson(value);
