// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'admin_user.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

AdminUser _$AdminUserFromJson(Map<String, dynamic> json) {
  return _AdminUser.fromJson(json);
}

/// @nodoc
mixin _$AdminUser {
  int get id => throw _privateConstructorUsedError;
  String get fullName => throw _privateConstructorUsedError;
  List<BranchShortInfo> get branches => throw _privateConstructorUsedError;
  File? get profilePicture => throw _privateConstructorUsedError;
  String? get phoneNumber => throw _privateConstructorUsedError;
  OrderListInfo? get orderList => throw _privateConstructorUsedError;
  String? get address => throw _privateConstructorUsedError;
  DateTime get createdAt => throw _privateConstructorUsedError;

  /// Serializes this AdminUser to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of AdminUser
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $AdminUserCopyWith<AdminUser> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $AdminUserCopyWith<$Res> {
  factory $AdminUserCopyWith(AdminUser value, $Res Function(AdminUser) then) =
      _$AdminUserCopyWithImpl<$Res, AdminUser>;
  @useResult
  $Res call({
    int id,
    String fullName,
    List<BranchShortInfo> branches,
    File? profilePicture,
    String? phoneNumber,
    OrderListInfo? orderList,
    String? address,
    DateTime createdAt,
  });

  $FileCopyWith<$Res>? get profilePicture;
  $OrderListInfoCopyWith<$Res>? get orderList;
}

/// @nodoc
class _$AdminUserCopyWithImpl<$Res, $Val extends AdminUser>
    implements $AdminUserCopyWith<$Res> {
  _$AdminUserCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of AdminUser
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? fullName = null,
    Object? branches = null,
    Object? profilePicture = freezed,
    Object? phoneNumber = freezed,
    Object? orderList = freezed,
    Object? address = freezed,
    Object? createdAt = null,
  }) {
    return _then(
      _value.copyWith(
            id:
                null == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int,
            fullName:
                null == fullName
                    ? _value.fullName
                    : fullName // ignore: cast_nullable_to_non_nullable
                        as String,
            branches:
                null == branches
                    ? _value.branches
                    : branches // ignore: cast_nullable_to_non_nullable
                        as List<BranchShortInfo>,
            profilePicture:
                freezed == profilePicture
                    ? _value.profilePicture
                    : profilePicture // ignore: cast_nullable_to_non_nullable
                        as File?,
            phoneNumber:
                freezed == phoneNumber
                    ? _value.phoneNumber
                    : phoneNumber // ignore: cast_nullable_to_non_nullable
                        as String?,
            orderList:
                freezed == orderList
                    ? _value.orderList
                    : orderList // ignore: cast_nullable_to_non_nullable
                        as OrderListInfo?,
            address:
                freezed == address
                    ? _value.address
                    : address // ignore: cast_nullable_to_non_nullable
                        as String?,
            createdAt:
                null == createdAt
                    ? _value.createdAt
                    : createdAt // ignore: cast_nullable_to_non_nullable
                        as DateTime,
          )
          as $Val,
    );
  }

  /// Create a copy of AdminUser
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $FileCopyWith<$Res>? get profilePicture {
    if (_value.profilePicture == null) {
      return null;
    }

    return $FileCopyWith<$Res>(_value.profilePicture!, (value) {
      return _then(_value.copyWith(profilePicture: value) as $Val);
    });
  }

  /// Create a copy of AdminUser
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $OrderListInfoCopyWith<$Res>? get orderList {
    if (_value.orderList == null) {
      return null;
    }

    return $OrderListInfoCopyWith<$Res>(_value.orderList!, (value) {
      return _then(_value.copyWith(orderList: value) as $Val);
    });
  }
}

/// @nodoc
abstract class _$$AdminUserImplCopyWith<$Res>
    implements $AdminUserCopyWith<$Res> {
  factory _$$AdminUserImplCopyWith(
    _$AdminUserImpl value,
    $Res Function(_$AdminUserImpl) then,
  ) = __$$AdminUserImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int id,
    String fullName,
    List<BranchShortInfo> branches,
    File? profilePicture,
    String? phoneNumber,
    OrderListInfo? orderList,
    String? address,
    DateTime createdAt,
  });

  @override
  $FileCopyWith<$Res>? get profilePicture;
  @override
  $OrderListInfoCopyWith<$Res>? get orderList;
}

/// @nodoc
class __$$AdminUserImplCopyWithImpl<$Res>
    extends _$AdminUserCopyWithImpl<$Res, _$AdminUserImpl>
    implements _$$AdminUserImplCopyWith<$Res> {
  __$$AdminUserImplCopyWithImpl(
    _$AdminUserImpl _value,
    $Res Function(_$AdminUserImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of AdminUser
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? fullName = null,
    Object? branches = null,
    Object? profilePicture = freezed,
    Object? phoneNumber = freezed,
    Object? orderList = freezed,
    Object? address = freezed,
    Object? createdAt = null,
  }) {
    return _then(
      _$AdminUserImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        fullName:
            null == fullName
                ? _value.fullName
                : fullName // ignore: cast_nullable_to_non_nullable
                    as String,
        branches:
            null == branches
                ? _value._branches
                : branches // ignore: cast_nullable_to_non_nullable
                    as List<BranchShortInfo>,
        profilePicture:
            freezed == profilePicture
                ? _value.profilePicture
                : profilePicture // ignore: cast_nullable_to_non_nullable
                    as File?,
        phoneNumber:
            freezed == phoneNumber
                ? _value.phoneNumber
                : phoneNumber // ignore: cast_nullable_to_non_nullable
                    as String?,
        orderList:
            freezed == orderList
                ? _value.orderList
                : orderList // ignore: cast_nullable_to_non_nullable
                    as OrderListInfo?,
        address:
            freezed == address
                ? _value.address
                : address // ignore: cast_nullable_to_non_nullable
                    as String?,
        createdAt:
            null == createdAt
                ? _value.createdAt
                : createdAt // ignore: cast_nullable_to_non_nullable
                    as DateTime,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$AdminUserImpl implements _AdminUser {
  const _$AdminUserImpl({
    required this.id,
    required this.fullName,
    final List<BranchShortInfo> branches = const [],
    this.profilePicture,
    required this.phoneNumber,
    this.orderList,
    required this.address,
    required this.createdAt,
  }) : _branches = branches;

  factory _$AdminUserImpl.fromJson(Map<String, dynamic> json) =>
      _$$AdminUserImplFromJson(json);

  @override
  final int id;
  @override
  final String fullName;
  final List<BranchShortInfo> _branches;
  @override
  @JsonKey()
  List<BranchShortInfo> get branches {
    if (_branches is EqualUnmodifiableListView) return _branches;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_branches);
  }

  @override
  final File? profilePicture;
  @override
  final String? phoneNumber;
  @override
  final OrderListInfo? orderList;
  @override
  final String? address;
  @override
  final DateTime createdAt;

  @override
  String toString() {
    return 'AdminUser(id: $id, fullName: $fullName, branches: $branches, profilePicture: $profilePicture, phoneNumber: $phoneNumber, orderList: $orderList, address: $address, createdAt: $createdAt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$AdminUserImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.fullName, fullName) ||
                other.fullName == fullName) &&
            const DeepCollectionEquality().equals(other._branches, _branches) &&
            (identical(other.profilePicture, profilePicture) ||
                other.profilePicture == profilePicture) &&
            (identical(other.phoneNumber, phoneNumber) ||
                other.phoneNumber == phoneNumber) &&
            (identical(other.orderList, orderList) ||
                other.orderList == orderList) &&
            (identical(other.address, address) || other.address == address) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    fullName,
    const DeepCollectionEquality().hash(_branches),
    profilePicture,
    phoneNumber,
    orderList,
    address,
    createdAt,
  );

  /// Create a copy of AdminUser
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$AdminUserImplCopyWith<_$AdminUserImpl> get copyWith =>
      __$$AdminUserImplCopyWithImpl<_$AdminUserImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$AdminUserImplToJson(this);
  }
}

abstract class _AdminUser implements AdminUser {
  const factory _AdminUser({
    required final int id,
    required final String fullName,
    final List<BranchShortInfo> branches,
    final File? profilePicture,
    required final String? phoneNumber,
    final OrderListInfo? orderList,
    required final String? address,
    required final DateTime createdAt,
  }) = _$AdminUserImpl;

  factory _AdminUser.fromJson(Map<String, dynamic> json) =
      _$AdminUserImpl.fromJson;

  @override
  int get id;
  @override
  String get fullName;
  @override
  List<BranchShortInfo> get branches;
  @override
  File? get profilePicture;
  @override
  String? get phoneNumber;
  @override
  OrderListInfo? get orderList;
  @override
  String? get address;
  @override
  DateTime get createdAt;

  /// Create a copy of AdminUser
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$AdminUserImplCopyWith<_$AdminUserImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
