// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'installment_payments_summary.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

InstallmentPaymentsSummary _$InstallmentPaymentsSummaryFromJson(
  Map<String, dynamic> json,
) {
  return _InstallmentPaymentsSummary.fromJson(json);
}

/// @nodoc
mixin _$InstallmentPaymentsSummary {
  int get totalCount => throw _privateConstructorUsedError;
  double get totalAmount => throw _privateConstructorUsedError;

  /// Serializes this InstallmentPaymentsSummary to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of InstallmentPaymentsSummary
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $InstallmentPaymentsSummaryCopyWith<InstallmentPaymentsSummary>
  get copyWith => throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $InstallmentPaymentsSummaryCopyWith<$Res> {
  factory $InstallmentPaymentsSummaryCopyWith(
    InstallmentPaymentsSummary value,
    $Res Function(InstallmentPaymentsSummary) then,
  ) =
      _$InstallmentPaymentsSummaryCopyWithImpl<
        $Res,
        InstallmentPaymentsSummary
      >;
  @useResult
  $Res call({int totalCount, double totalAmount});
}

/// @nodoc
class _$InstallmentPaymentsSummaryCopyWithImpl<
  $Res,
  $Val extends InstallmentPaymentsSummary
>
    implements $InstallmentPaymentsSummaryCopyWith<$Res> {
  _$InstallmentPaymentsSummaryCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of InstallmentPaymentsSummary
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? totalCount = null, Object? totalAmount = null}) {
    return _then(
      _value.copyWith(
            totalCount:
                null == totalCount
                    ? _value.totalCount
                    : totalCount // ignore: cast_nullable_to_non_nullable
                        as int,
            totalAmount:
                null == totalAmount
                    ? _value.totalAmount
                    : totalAmount // ignore: cast_nullable_to_non_nullable
                        as double,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$InstallmentPaymentsSummaryImplCopyWith<$Res>
    implements $InstallmentPaymentsSummaryCopyWith<$Res> {
  factory _$$InstallmentPaymentsSummaryImplCopyWith(
    _$InstallmentPaymentsSummaryImpl value,
    $Res Function(_$InstallmentPaymentsSummaryImpl) then,
  ) = __$$InstallmentPaymentsSummaryImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int totalCount, double totalAmount});
}

/// @nodoc
class __$$InstallmentPaymentsSummaryImplCopyWithImpl<$Res>
    extends
        _$InstallmentPaymentsSummaryCopyWithImpl<
          $Res,
          _$InstallmentPaymentsSummaryImpl
        >
    implements _$$InstallmentPaymentsSummaryImplCopyWith<$Res> {
  __$$InstallmentPaymentsSummaryImplCopyWithImpl(
    _$InstallmentPaymentsSummaryImpl _value,
    $Res Function(_$InstallmentPaymentsSummaryImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of InstallmentPaymentsSummary
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? totalCount = null, Object? totalAmount = null}) {
    return _then(
      _$InstallmentPaymentsSummaryImpl(
        totalCount:
            null == totalCount
                ? _value.totalCount
                : totalCount // ignore: cast_nullable_to_non_nullable
                    as int,
        totalAmount:
            null == totalAmount
                ? _value.totalAmount
                : totalAmount // ignore: cast_nullable_to_non_nullable
                    as double,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$InstallmentPaymentsSummaryImpl implements _InstallmentPaymentsSummary {
  const _$InstallmentPaymentsSummaryImpl({
    required this.totalCount,
    required this.totalAmount,
  });

  factory _$InstallmentPaymentsSummaryImpl.fromJson(
    Map<String, dynamic> json,
  ) => _$$InstallmentPaymentsSummaryImplFromJson(json);

  @override
  final int totalCount;
  @override
  final double totalAmount;

  @override
  String toString() {
    return 'InstallmentPaymentsSummary(totalCount: $totalCount, totalAmount: $totalAmount)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$InstallmentPaymentsSummaryImpl &&
            (identical(other.totalCount, totalCount) ||
                other.totalCount == totalCount) &&
            (identical(other.totalAmount, totalAmount) ||
                other.totalAmount == totalAmount));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, totalCount, totalAmount);

  /// Create a copy of InstallmentPaymentsSummary
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$InstallmentPaymentsSummaryImplCopyWith<_$InstallmentPaymentsSummaryImpl>
  get copyWith => __$$InstallmentPaymentsSummaryImplCopyWithImpl<
    _$InstallmentPaymentsSummaryImpl
  >(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$InstallmentPaymentsSummaryImplToJson(this);
  }
}

abstract class _InstallmentPaymentsSummary
    implements InstallmentPaymentsSummary {
  const factory _InstallmentPaymentsSummary({
    required final int totalCount,
    required final double totalAmount,
  }) = _$InstallmentPaymentsSummaryImpl;

  factory _InstallmentPaymentsSummary.fromJson(Map<String, dynamic> json) =
      _$InstallmentPaymentsSummaryImpl.fromJson;

  @override
  int get totalCount;
  @override
  double get totalAmount;

  /// Create a copy of InstallmentPaymentsSummary
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$InstallmentPaymentsSummaryImplCopyWith<_$InstallmentPaymentsSummaryImpl>
  get copyWith => throw _privateConstructorUsedError;
}
