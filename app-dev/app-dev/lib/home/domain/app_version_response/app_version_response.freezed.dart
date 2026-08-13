// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'app_version_response.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

AppVersionResponse _$AppVersionResponseFromJson(Map<String, dynamic> json) {
  return _AppVersionResponse.fromJson(json);
}

/// @nodoc
mixin _$AppVersionResponse {
  bool get shouldUpdate => throw _privateConstructorUsedError;
  bool get isForce => throw _privateConstructorUsedError;
  String get url => throw _privateConstructorUsedError;

  /// Serializes this AppVersionResponse to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of AppVersionResponse
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $AppVersionResponseCopyWith<AppVersionResponse> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $AppVersionResponseCopyWith<$Res> {
  factory $AppVersionResponseCopyWith(
    AppVersionResponse value,
    $Res Function(AppVersionResponse) then,
  ) = _$AppVersionResponseCopyWithImpl<$Res, AppVersionResponse>;
  @useResult
  $Res call({bool shouldUpdate, bool isForce, String url});
}

/// @nodoc
class _$AppVersionResponseCopyWithImpl<$Res, $Val extends AppVersionResponse>
    implements $AppVersionResponseCopyWith<$Res> {
  _$AppVersionResponseCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of AppVersionResponse
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? shouldUpdate = null,
    Object? isForce = null,
    Object? url = null,
  }) {
    return _then(
      _value.copyWith(
            shouldUpdate:
                null == shouldUpdate
                    ? _value.shouldUpdate
                    : shouldUpdate // ignore: cast_nullable_to_non_nullable
                        as bool,
            isForce:
                null == isForce
                    ? _value.isForce
                    : isForce // ignore: cast_nullable_to_non_nullable
                        as bool,
            url:
                null == url
                    ? _value.url
                    : url // ignore: cast_nullable_to_non_nullable
                        as String,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$AppVersionResponseImplCopyWith<$Res>
    implements $AppVersionResponseCopyWith<$Res> {
  factory _$$AppVersionResponseImplCopyWith(
    _$AppVersionResponseImpl value,
    $Res Function(_$AppVersionResponseImpl) then,
  ) = __$$AppVersionResponseImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({bool shouldUpdate, bool isForce, String url});
}

/// @nodoc
class __$$AppVersionResponseImplCopyWithImpl<$Res>
    extends _$AppVersionResponseCopyWithImpl<$Res, _$AppVersionResponseImpl>
    implements _$$AppVersionResponseImplCopyWith<$Res> {
  __$$AppVersionResponseImplCopyWithImpl(
    _$AppVersionResponseImpl _value,
    $Res Function(_$AppVersionResponseImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of AppVersionResponse
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? shouldUpdate = null,
    Object? isForce = null,
    Object? url = null,
  }) {
    return _then(
      _$AppVersionResponseImpl(
        shouldUpdate:
            null == shouldUpdate
                ? _value.shouldUpdate
                : shouldUpdate // ignore: cast_nullable_to_non_nullable
                    as bool,
        isForce:
            null == isForce
                ? _value.isForce
                : isForce // ignore: cast_nullable_to_non_nullable
                    as bool,
        url:
            null == url
                ? _value.url
                : url // ignore: cast_nullable_to_non_nullable
                    as String,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$AppVersionResponseImpl implements _AppVersionResponse {
  const _$AppVersionResponseImpl({
    required this.shouldUpdate,
    required this.isForce,
    required this.url,
  });

  factory _$AppVersionResponseImpl.fromJson(Map<String, dynamic> json) =>
      _$$AppVersionResponseImplFromJson(json);

  @override
  final bool shouldUpdate;
  @override
  final bool isForce;
  @override
  final String url;

  @override
  String toString() {
    return 'AppVersionResponse(shouldUpdate: $shouldUpdate, isForce: $isForce, url: $url)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$AppVersionResponseImpl &&
            (identical(other.shouldUpdate, shouldUpdate) ||
                other.shouldUpdate == shouldUpdate) &&
            (identical(other.isForce, isForce) || other.isForce == isForce) &&
            (identical(other.url, url) || other.url == url));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, shouldUpdate, isForce, url);

  /// Create a copy of AppVersionResponse
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$AppVersionResponseImplCopyWith<_$AppVersionResponseImpl> get copyWith =>
      __$$AppVersionResponseImplCopyWithImpl<_$AppVersionResponseImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$AppVersionResponseImplToJson(this);
  }
}

abstract class _AppVersionResponse implements AppVersionResponse {
  const factory _AppVersionResponse({
    required final bool shouldUpdate,
    required final bool isForce,
    required final String url,
  }) = _$AppVersionResponseImpl;

  factory _AppVersionResponse.fromJson(Map<String, dynamic> json) =
      _$AppVersionResponseImpl.fromJson;

  @override
  bool get shouldUpdate;
  @override
  bool get isForce;
  @override
  String get url;

  /// Create a copy of AppVersionResponse
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$AppVersionResponseImplCopyWith<_$AppVersionResponseImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
