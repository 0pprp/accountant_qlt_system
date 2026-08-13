// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'login_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

LoginModel _$LoginModelFromJson(Map<String, dynamic> json) {
  return _LoginModel.fromJson(json);
}

/// @nodoc
mixin _$LoginModel {
  String get accessToken => throw _privateConstructorUsedError;
  String get refreshToken => throw _privateConstructorUsedError;
  Permissions get permissions => throw _privateConstructorUsedError;
  List<Roles> get roles => throw _privateConstructorUsedError;

  /// Serializes this LoginModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of LoginModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $LoginModelCopyWith<LoginModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $LoginModelCopyWith<$Res> {
  factory $LoginModelCopyWith(
    LoginModel value,
    $Res Function(LoginModel) then,
  ) = _$LoginModelCopyWithImpl<$Res, LoginModel>;
  @useResult
  $Res call({
    String accessToken,
    String refreshToken,
    Permissions permissions,
    List<Roles> roles,
  });

  $PermissionsCopyWith<$Res> get permissions;
}

/// @nodoc
class _$LoginModelCopyWithImpl<$Res, $Val extends LoginModel>
    implements $LoginModelCopyWith<$Res> {
  _$LoginModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of LoginModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? accessToken = null,
    Object? refreshToken = null,
    Object? permissions = null,
    Object? roles = null,
  }) {
    return _then(
      _value.copyWith(
            accessToken:
                null == accessToken
                    ? _value.accessToken
                    : accessToken // ignore: cast_nullable_to_non_nullable
                        as String,
            refreshToken:
                null == refreshToken
                    ? _value.refreshToken
                    : refreshToken // ignore: cast_nullable_to_non_nullable
                        as String,
            permissions:
                null == permissions
                    ? _value.permissions
                    : permissions // ignore: cast_nullable_to_non_nullable
                        as Permissions,
            roles:
                null == roles
                    ? _value.roles
                    : roles // ignore: cast_nullable_to_non_nullable
                        as List<Roles>,
          )
          as $Val,
    );
  }

  /// Create a copy of LoginModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $PermissionsCopyWith<$Res> get permissions {
    return $PermissionsCopyWith<$Res>(_value.permissions, (value) {
      return _then(_value.copyWith(permissions: value) as $Val);
    });
  }
}

/// @nodoc
abstract class _$$LoginModelImplCopyWith<$Res>
    implements $LoginModelCopyWith<$Res> {
  factory _$$LoginModelImplCopyWith(
    _$LoginModelImpl value,
    $Res Function(_$LoginModelImpl) then,
  ) = __$$LoginModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String accessToken,
    String refreshToken,
    Permissions permissions,
    List<Roles> roles,
  });

  @override
  $PermissionsCopyWith<$Res> get permissions;
}

/// @nodoc
class __$$LoginModelImplCopyWithImpl<$Res>
    extends _$LoginModelCopyWithImpl<$Res, _$LoginModelImpl>
    implements _$$LoginModelImplCopyWith<$Res> {
  __$$LoginModelImplCopyWithImpl(
    _$LoginModelImpl _value,
    $Res Function(_$LoginModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of LoginModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? accessToken = null,
    Object? refreshToken = null,
    Object? permissions = null,
    Object? roles = null,
  }) {
    return _then(
      _$LoginModelImpl(
        accessToken:
            null == accessToken
                ? _value.accessToken
                : accessToken // ignore: cast_nullable_to_non_nullable
                    as String,
        refreshToken:
            null == refreshToken
                ? _value.refreshToken
                : refreshToken // ignore: cast_nullable_to_non_nullable
                    as String,
        permissions:
            null == permissions
                ? _value.permissions
                : permissions // ignore: cast_nullable_to_non_nullable
                    as Permissions,
        roles:
            null == roles
                ? _value._roles
                : roles // ignore: cast_nullable_to_non_nullable
                    as List<Roles>,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$LoginModelImpl implements _LoginModel {
  const _$LoginModelImpl({
    required this.accessToken,
    required this.refreshToken,
    required this.permissions,
    required final List<Roles> roles,
  }) : _roles = roles;

  factory _$LoginModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$LoginModelImplFromJson(json);

  @override
  final String accessToken;
  @override
  final String refreshToken;
  @override
  final Permissions permissions;
  final List<Roles> _roles;
  @override
  List<Roles> get roles {
    if (_roles is EqualUnmodifiableListView) return _roles;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_roles);
  }

  @override
  String toString() {
    return 'LoginModel(accessToken: $accessToken, refreshToken: $refreshToken, permissions: $permissions, roles: $roles)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$LoginModelImpl &&
            (identical(other.accessToken, accessToken) ||
                other.accessToken == accessToken) &&
            (identical(other.refreshToken, refreshToken) ||
                other.refreshToken == refreshToken) &&
            (identical(other.permissions, permissions) ||
                other.permissions == permissions) &&
            const DeepCollectionEquality().equals(other._roles, _roles));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    accessToken,
    refreshToken,
    permissions,
    const DeepCollectionEquality().hash(_roles),
  );

  /// Create a copy of LoginModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$LoginModelImplCopyWith<_$LoginModelImpl> get copyWith =>
      __$$LoginModelImplCopyWithImpl<_$LoginModelImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$LoginModelImplToJson(this);
  }
}

abstract class _LoginModel implements LoginModel {
  const factory _LoginModel({
    required final String accessToken,
    required final String refreshToken,
    required final Permissions permissions,
    required final List<Roles> roles,
  }) = _$LoginModelImpl;

  factory _LoginModel.fromJson(Map<String, dynamic> json) =
      _$LoginModelImpl.fromJson;

  @override
  String get accessToken;
  @override
  String get refreshToken;
  @override
  Permissions get permissions;
  @override
  List<Roles> get roles;

  /// Create a copy of LoginModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$LoginModelImplCopyWith<_$LoginModelImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

Permissions _$PermissionsFromJson(Map<String, dynamic> json) {
  return _Permissions.fromJson(json);
}

/// @nodoc
mixin _$Permissions {
  @JsonKey(name: 'InstallmentPayment')
  @PermissionActionsConverter()
  PermissionActions? get installmentPayment =>
      throw _privateConstructorUsedError;
  @JsonKey(name: 'Order')
  @PermissionActionsConverter()
  PermissionActions? get order => throw _privateConstructorUsedError;
  @JsonKey(name: 'OrderList')
  @PermissionActionsConverter()
  PermissionActions? get orderList => throw _privateConstructorUsedError;
  @JsonKey(name: 'User')
  @PermissionActionsConverter()
  PermissionActions? get user => throw _privateConstructorUsedError;
  @JsonKey(name: 'Warehouse')
  @PermissionActionsConverter()
  PermissionActions? get warehouse => throw _privateConstructorUsedError;

  /// Serializes this Permissions to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of Permissions
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $PermissionsCopyWith<Permissions> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $PermissionsCopyWith<$Res> {
  factory $PermissionsCopyWith(
    Permissions value,
    $Res Function(Permissions) then,
  ) = _$PermissionsCopyWithImpl<$Res, Permissions>;
  @useResult
  $Res call({
    @JsonKey(name: 'InstallmentPayment')
    @PermissionActionsConverter()
    PermissionActions? installmentPayment,
    @JsonKey(name: 'Order')
    @PermissionActionsConverter()
    PermissionActions? order,
    @JsonKey(name: 'OrderList')
    @PermissionActionsConverter()
    PermissionActions? orderList,
    @JsonKey(name: 'User')
    @PermissionActionsConverter()
    PermissionActions? user,
    @JsonKey(name: 'Warehouse')
    @PermissionActionsConverter()
    PermissionActions? warehouse,
  });
}

/// @nodoc
class _$PermissionsCopyWithImpl<$Res, $Val extends Permissions>
    implements $PermissionsCopyWith<$Res> {
  _$PermissionsCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of Permissions
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? installmentPayment = freezed,
    Object? order = freezed,
    Object? orderList = freezed,
    Object? user = freezed,
    Object? warehouse = freezed,
  }) {
    return _then(
      _value.copyWith(
            installmentPayment:
                freezed == installmentPayment
                    ? _value.installmentPayment
                    : installmentPayment // ignore: cast_nullable_to_non_nullable
                        as PermissionActions?,
            order:
                freezed == order
                    ? _value.order
                    : order // ignore: cast_nullable_to_non_nullable
                        as PermissionActions?,
            orderList:
                freezed == orderList
                    ? _value.orderList
                    : orderList // ignore: cast_nullable_to_non_nullable
                        as PermissionActions?,
            user:
                freezed == user
                    ? _value.user
                    : user // ignore: cast_nullable_to_non_nullable
                        as PermissionActions?,
            warehouse:
                freezed == warehouse
                    ? _value.warehouse
                    : warehouse // ignore: cast_nullable_to_non_nullable
                        as PermissionActions?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$PermissionsImplCopyWith<$Res>
    implements $PermissionsCopyWith<$Res> {
  factory _$$PermissionsImplCopyWith(
    _$PermissionsImpl value,
    $Res Function(_$PermissionsImpl) then,
  ) = __$$PermissionsImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    @JsonKey(name: 'InstallmentPayment')
    @PermissionActionsConverter()
    PermissionActions? installmentPayment,
    @JsonKey(name: 'Order')
    @PermissionActionsConverter()
    PermissionActions? order,
    @JsonKey(name: 'OrderList')
    @PermissionActionsConverter()
    PermissionActions? orderList,
    @JsonKey(name: 'User')
    @PermissionActionsConverter()
    PermissionActions? user,
    @JsonKey(name: 'Warehouse')
    @PermissionActionsConverter()
    PermissionActions? warehouse,
  });
}

/// @nodoc
class __$$PermissionsImplCopyWithImpl<$Res>
    extends _$PermissionsCopyWithImpl<$Res, _$PermissionsImpl>
    implements _$$PermissionsImplCopyWith<$Res> {
  __$$PermissionsImplCopyWithImpl(
    _$PermissionsImpl _value,
    $Res Function(_$PermissionsImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of Permissions
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? installmentPayment = freezed,
    Object? order = freezed,
    Object? orderList = freezed,
    Object? user = freezed,
    Object? warehouse = freezed,
  }) {
    return _then(
      _$PermissionsImpl(
        installmentPayment:
            freezed == installmentPayment
                ? _value.installmentPayment
                : installmentPayment // ignore: cast_nullable_to_non_nullable
                    as PermissionActions?,
        order:
            freezed == order
                ? _value.order
                : order // ignore: cast_nullable_to_non_nullable
                    as PermissionActions?,
        orderList:
            freezed == orderList
                ? _value.orderList
                : orderList // ignore: cast_nullable_to_non_nullable
                    as PermissionActions?,
        user:
            freezed == user
                ? _value.user
                : user // ignore: cast_nullable_to_non_nullable
                    as PermissionActions?,
        warehouse:
            freezed == warehouse
                ? _value.warehouse
                : warehouse // ignore: cast_nullable_to_non_nullable
                    as PermissionActions?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$PermissionsImpl implements _Permissions {
  const _$PermissionsImpl({
    @JsonKey(name: 'InstallmentPayment')
    @PermissionActionsConverter()
    this.installmentPayment = const PermissionActions(),
    @JsonKey(name: 'Order')
    @PermissionActionsConverter()
    this.order = const PermissionActions(),
    @JsonKey(name: 'OrderList')
    @PermissionActionsConverter()
    this.orderList = const PermissionActions(),
    @JsonKey(name: 'User')
    @PermissionActionsConverter()
    this.user = const PermissionActions(),
    @JsonKey(name: 'Warehouse')
    @PermissionActionsConverter()
    this.warehouse = const PermissionActions(),
  });

  factory _$PermissionsImpl.fromJson(Map<String, dynamic> json) =>
      _$$PermissionsImplFromJson(json);

  @override
  @JsonKey(name: 'InstallmentPayment')
  @PermissionActionsConverter()
  final PermissionActions? installmentPayment;
  @override
  @JsonKey(name: 'Order')
  @PermissionActionsConverter()
  final PermissionActions? order;
  @override
  @JsonKey(name: 'OrderList')
  @PermissionActionsConverter()
  final PermissionActions? orderList;
  @override
  @JsonKey(name: 'User')
  @PermissionActionsConverter()
  final PermissionActions? user;
  @override
  @JsonKey(name: 'Warehouse')
  @PermissionActionsConverter()
  final PermissionActions? warehouse;

  @override
  String toString() {
    return 'Permissions(installmentPayment: $installmentPayment, order: $order, orderList: $orderList, user: $user, warehouse: $warehouse)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$PermissionsImpl &&
            (identical(other.installmentPayment, installmentPayment) ||
                other.installmentPayment == installmentPayment) &&
            (identical(other.order, order) || other.order == order) &&
            (identical(other.orderList, orderList) ||
                other.orderList == orderList) &&
            (identical(other.user, user) || other.user == user) &&
            (identical(other.warehouse, warehouse) ||
                other.warehouse == warehouse));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    installmentPayment,
    order,
    orderList,
    user,
    warehouse,
  );

  /// Create a copy of Permissions
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$PermissionsImplCopyWith<_$PermissionsImpl> get copyWith =>
      __$$PermissionsImplCopyWithImpl<_$PermissionsImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$PermissionsImplToJson(this);
  }
}

abstract class _Permissions implements Permissions {
  const factory _Permissions({
    @JsonKey(name: 'InstallmentPayment')
    @PermissionActionsConverter()
    final PermissionActions? installmentPayment,
    @JsonKey(name: 'Order')
    @PermissionActionsConverter()
    final PermissionActions? order,
    @JsonKey(name: 'OrderList')
    @PermissionActionsConverter()
    final PermissionActions? orderList,
    @JsonKey(name: 'User')
    @PermissionActionsConverter()
    final PermissionActions? user,
    @JsonKey(name: 'Warehouse')
    @PermissionActionsConverter()
    final PermissionActions? warehouse,
  }) = _$PermissionsImpl;

  factory _Permissions.fromJson(Map<String, dynamic> json) =
      _$PermissionsImpl.fromJson;

  @override
  @JsonKey(name: 'InstallmentPayment')
  @PermissionActionsConverter()
  PermissionActions? get installmentPayment;
  @override
  @JsonKey(name: 'Order')
  @PermissionActionsConverter()
  PermissionActions? get order;
  @override
  @JsonKey(name: 'OrderList')
  @PermissionActionsConverter()
  PermissionActions? get orderList;
  @override
  @JsonKey(name: 'User')
  @PermissionActionsConverter()
  PermissionActions? get user;
  @override
  @JsonKey(name: 'Warehouse')
  @PermissionActionsConverter()
  PermissionActions? get warehouse;

  /// Create a copy of Permissions
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$PermissionsImplCopyWith<_$PermissionsImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
