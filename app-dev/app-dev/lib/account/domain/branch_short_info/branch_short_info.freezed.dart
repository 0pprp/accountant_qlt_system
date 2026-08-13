// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'branch_short_info.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

BranchShortInfo _$BranchShortInfoFromJson(Map<String, dynamic> json) {
  return _BranchShortInfo.fromJson(json);
}

/// @nodoc
mixin _$BranchShortInfo {
  int get id => throw _privateConstructorUsedError;
  String get name => throw _privateConstructorUsedError;

  /// Serializes this BranchShortInfo to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of BranchShortInfo
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $BranchShortInfoCopyWith<BranchShortInfo> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $BranchShortInfoCopyWith<$Res> {
  factory $BranchShortInfoCopyWith(
    BranchShortInfo value,
    $Res Function(BranchShortInfo) then,
  ) = _$BranchShortInfoCopyWithImpl<$Res, BranchShortInfo>;
  @useResult
  $Res call({int id, String name});
}

/// @nodoc
class _$BranchShortInfoCopyWithImpl<$Res, $Val extends BranchShortInfo>
    implements $BranchShortInfoCopyWith<$Res> {
  _$BranchShortInfoCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of BranchShortInfo
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? id = null, Object? name = null}) {
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
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$BranchShortInfoImplCopyWith<$Res>
    implements $BranchShortInfoCopyWith<$Res> {
  factory _$$BranchShortInfoImplCopyWith(
    _$BranchShortInfoImpl value,
    $Res Function(_$BranchShortInfoImpl) then,
  ) = __$$BranchShortInfoImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int id, String name});
}

/// @nodoc
class __$$BranchShortInfoImplCopyWithImpl<$Res>
    extends _$BranchShortInfoCopyWithImpl<$Res, _$BranchShortInfoImpl>
    implements _$$BranchShortInfoImplCopyWith<$Res> {
  __$$BranchShortInfoImplCopyWithImpl(
    _$BranchShortInfoImpl _value,
    $Res Function(_$BranchShortInfoImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of BranchShortInfo
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? id = null, Object? name = null}) {
    return _then(
      _$BranchShortInfoImpl(
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
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$BranchShortInfoImpl implements _BranchShortInfo {
  const _$BranchShortInfoImpl({required this.id, required this.name});

  factory _$BranchShortInfoImpl.fromJson(Map<String, dynamic> json) =>
      _$$BranchShortInfoImplFromJson(json);

  @override
  final int id;
  @override
  final String name;

  @override
  String toString() {
    return 'BranchShortInfo(id: $id, name: $name)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$BranchShortInfoImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.name, name) || other.name == name));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, id, name);

  /// Create a copy of BranchShortInfo
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$BranchShortInfoImplCopyWith<_$BranchShortInfoImpl> get copyWith =>
      __$$BranchShortInfoImplCopyWithImpl<_$BranchShortInfoImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$BranchShortInfoImplToJson(this);
  }
}

abstract class _BranchShortInfo implements BranchShortInfo {
  const factory _BranchShortInfo({
    required final int id,
    required final String name,
  }) = _$BranchShortInfoImpl;

  factory _BranchShortInfo.fromJson(Map<String, dynamic> json) =
      _$BranchShortInfoImpl.fromJson;

  @override
  int get id;
  @override
  String get name;

  /// Create a copy of BranchShortInfo
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$BranchShortInfoImplCopyWith<_$BranchShortInfoImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
