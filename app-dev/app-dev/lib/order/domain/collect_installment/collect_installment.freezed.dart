// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'collect_installment.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

CollectInstallment _$CollectInstallmentFromJson(Map<String, dynamic> json) {
  return _CollectInstallment.fromJson(json);
}

/// @nodoc
mixin _$CollectInstallment {
  int get orderId => throw _privateConstructorUsedError;
  int get amount => throw _privateConstructorUsedError;
  @DateOnlyConverter()
  DateTime? get date => throw _privateConstructorUsedError;

  /// Serializes this CollectInstallment to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of CollectInstallment
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CollectInstallmentCopyWith<CollectInstallment> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CollectInstallmentCopyWith<$Res> {
  factory $CollectInstallmentCopyWith(
    CollectInstallment value,
    $Res Function(CollectInstallment) then,
  ) = _$CollectInstallmentCopyWithImpl<$Res, CollectInstallment>;
  @useResult
  $Res call({int orderId, int amount, @DateOnlyConverter() DateTime? date});
}

/// @nodoc
class _$CollectInstallmentCopyWithImpl<$Res, $Val extends CollectInstallment>
    implements $CollectInstallmentCopyWith<$Res> {
  _$CollectInstallmentCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of CollectInstallment
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? orderId = null,
    Object? amount = null,
    Object? date = freezed,
  }) {
    return _then(
      _value.copyWith(
            orderId:
                null == orderId
                    ? _value.orderId
                    : orderId // ignore: cast_nullable_to_non_nullable
                        as int,
            amount:
                null == amount
                    ? _value.amount
                    : amount // ignore: cast_nullable_to_non_nullable
                        as int,
            date:
                freezed == date
                    ? _value.date
                    : date // ignore: cast_nullable_to_non_nullable
                        as DateTime?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CollectInstallmentImplCopyWith<$Res>
    implements $CollectInstallmentCopyWith<$Res> {
  factory _$$CollectInstallmentImplCopyWith(
    _$CollectInstallmentImpl value,
    $Res Function(_$CollectInstallmentImpl) then,
  ) = __$$CollectInstallmentImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int orderId, int amount, @DateOnlyConverter() DateTime? date});
}

/// @nodoc
class __$$CollectInstallmentImplCopyWithImpl<$Res>
    extends _$CollectInstallmentCopyWithImpl<$Res, _$CollectInstallmentImpl>
    implements _$$CollectInstallmentImplCopyWith<$Res> {
  __$$CollectInstallmentImplCopyWithImpl(
    _$CollectInstallmentImpl _value,
    $Res Function(_$CollectInstallmentImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of CollectInstallment
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? orderId = null,
    Object? amount = null,
    Object? date = freezed,
  }) {
    return _then(
      _$CollectInstallmentImpl(
        orderId:
            null == orderId
                ? _value.orderId
                : orderId // ignore: cast_nullable_to_non_nullable
                    as int,
        amount:
            null == amount
                ? _value.amount
                : amount // ignore: cast_nullable_to_non_nullable
                    as int,
        date:
            freezed == date
                ? _value.date
                : date // ignore: cast_nullable_to_non_nullable
                    as DateTime?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CollectInstallmentImpl implements _CollectInstallment {
  const _$CollectInstallmentImpl({
    required this.orderId,
    required this.amount,
    @DateOnlyConverter() this.date,
  });

  factory _$CollectInstallmentImpl.fromJson(Map<String, dynamic> json) =>
      _$$CollectInstallmentImplFromJson(json);

  @override
  final int orderId;
  @override
  final int amount;
  @override
  @DateOnlyConverter()
  final DateTime? date;

  @override
  String toString() {
    return 'CollectInstallment(orderId: $orderId, amount: $amount, date: $date)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CollectInstallmentImpl &&
            (identical(other.orderId, orderId) || other.orderId == orderId) &&
            (identical(other.amount, amount) || other.amount == amount) &&
            (identical(other.date, date) || other.date == date));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, orderId, amount, date);

  /// Create a copy of CollectInstallment
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CollectInstallmentImplCopyWith<_$CollectInstallmentImpl> get copyWith =>
      __$$CollectInstallmentImplCopyWithImpl<_$CollectInstallmentImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$CollectInstallmentImplToJson(this);
  }
}

abstract class _CollectInstallment implements CollectInstallment {
  const factory _CollectInstallment({
    required final int orderId,
    required final int amount,
    @DateOnlyConverter() final DateTime? date,
  }) = _$CollectInstallmentImpl;

  factory _CollectInstallment.fromJson(Map<String, dynamic> json) =
      _$CollectInstallmentImpl.fromJson;

  @override
  int get orderId;
  @override
  int get amount;
  @override
  @DateOnlyConverter()
  DateTime? get date;

  /// Create a copy of CollectInstallment
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CollectInstallmentImplCopyWith<_$CollectInstallmentImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
