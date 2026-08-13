// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'order_list_info.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

OrderListInfo _$OrderListInfoFromJson(Map<String, dynamic> json) {
  return _OrderListInfo.fromJson(json);
}

/// @nodoc
mixin _$OrderListInfo {
  int get id => throw _privateConstructorUsedError;
  String get name => throw _privateConstructorUsedError;
  String? get mandobFullName => throw _privateConstructorUsedError;

  /// Serializes this OrderListInfo to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of OrderListInfo
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $OrderListInfoCopyWith<OrderListInfo> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $OrderListInfoCopyWith<$Res> {
  factory $OrderListInfoCopyWith(
    OrderListInfo value,
    $Res Function(OrderListInfo) then,
  ) = _$OrderListInfoCopyWithImpl<$Res, OrderListInfo>;
  @useResult
  $Res call({int id, String name, String? mandobFullName});
}

/// @nodoc
class _$OrderListInfoCopyWithImpl<$Res, $Val extends OrderListInfo>
    implements $OrderListInfoCopyWith<$Res> {
  _$OrderListInfoCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of OrderListInfo
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? name = null,
    Object? mandobFullName = freezed,
  }) {
    return _then(
      _value.copyWith(
            id:
                null == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int,
            name:
                null == name
                    ? _value.name
                    : name // ignore: cast_nullable_to_non_nullable
                        as String,
            mandobFullName:
                freezed == mandobFullName
                    ? _value.mandobFullName
                    : mandobFullName // ignore: cast_nullable_to_non_nullable
                        as String?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$OrderListInfoImplCopyWith<$Res>
    implements $OrderListInfoCopyWith<$Res> {
  factory _$$OrderListInfoImplCopyWith(
    _$OrderListInfoImpl value,
    $Res Function(_$OrderListInfoImpl) then,
  ) = __$$OrderListInfoImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int id, String name, String? mandobFullName});
}

/// @nodoc
class __$$OrderListInfoImplCopyWithImpl<$Res>
    extends _$OrderListInfoCopyWithImpl<$Res, _$OrderListInfoImpl>
    implements _$$OrderListInfoImplCopyWith<$Res> {
  __$$OrderListInfoImplCopyWithImpl(
    _$OrderListInfoImpl _value,
    $Res Function(_$OrderListInfoImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of OrderListInfo
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? name = null,
    Object? mandobFullName = freezed,
  }) {
    return _then(
      _$OrderListInfoImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        name:
            null == name
                ? _value.name
                : name // ignore: cast_nullable_to_non_nullable
                    as String,
        mandobFullName:
            freezed == mandobFullName
                ? _value.mandobFullName
                : mandobFullName // ignore: cast_nullable_to_non_nullable
                    as String?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$OrderListInfoImpl implements _OrderListInfo {
  const _$OrderListInfoImpl({
    required this.id,
    required this.name,
    this.mandobFullName,
  });

  factory _$OrderListInfoImpl.fromJson(Map<String, dynamic> json) =>
      _$$OrderListInfoImplFromJson(json);

  @override
  final int id;
  @override
  final String name;
  @override
  final String? mandobFullName;

  @override
  String toString() {
    return 'OrderListInfo(id: $id, name: $name, mandobFullName: $mandobFullName)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$OrderListInfoImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.mandobFullName, mandobFullName) ||
                other.mandobFullName == mandobFullName));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, id, name, mandobFullName);

  /// Create a copy of OrderListInfo
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$OrderListInfoImplCopyWith<_$OrderListInfoImpl> get copyWith =>
      __$$OrderListInfoImplCopyWithImpl<_$OrderListInfoImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$OrderListInfoImplToJson(this);
  }
}

abstract class _OrderListInfo implements OrderListInfo {
  const factory _OrderListInfo({
    required final int id,
    required final String name,
    final String? mandobFullName,
  }) = _$OrderListInfoImpl;

  factory _OrderListInfo.fromJson(Map<String, dynamic> json) =
      _$OrderListInfoImpl.fromJson;

  @override
  int get id;
  @override
  String get name;
  @override
  String? get mandobFullName;

  /// Create a copy of OrderListInfo
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$OrderListInfoImplCopyWith<_$OrderListInfoImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
