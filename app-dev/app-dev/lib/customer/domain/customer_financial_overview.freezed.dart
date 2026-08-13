// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'customer_financial_overview.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

CustomerFinancialOverview _$CustomerFinancialOverviewFromJson(
  Map<String, dynamic> json,
) {
  return _CustomerFinancialOverview.fromJson(json);
}

/// @nodoc
mixin _$CustomerFinancialOverview {
  int get customerId => throw _privateConstructorUsedError;
  double get totalPaidAmount => throw _privateConstructorUsedError;
  double get totalOverdueAmount => throw _privateConstructorUsedError;
  double get totalSellAmount => throw _privateConstructorUsedError;
  double get totalRemainingAmount => throw _privateConstructorUsedError;
  double get totalDailyInstallmentAmount => throw _privateConstructorUsedError;
  int get activeOrdersCount => throw _privateConstructorUsedError;
  int get completedOrdersCount => throw _privateConstructorUsedError;

  /// Serializes this CustomerFinancialOverview to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of CustomerFinancialOverview
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CustomerFinancialOverviewCopyWith<CustomerFinancialOverview> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CustomerFinancialOverviewCopyWith<$Res> {
  factory $CustomerFinancialOverviewCopyWith(
    CustomerFinancialOverview value,
    $Res Function(CustomerFinancialOverview) then,
  ) = _$CustomerFinancialOverviewCopyWithImpl<$Res, CustomerFinancialOverview>;
  @useResult
  $Res call({
    int customerId,
    double totalPaidAmount,
    double totalOverdueAmount,
    double totalSellAmount,
    double totalRemainingAmount,
    double totalDailyInstallmentAmount,
    int activeOrdersCount,
    int completedOrdersCount,
  });
}

/// @nodoc
class _$CustomerFinancialOverviewCopyWithImpl<
  $Res,
  $Val extends CustomerFinancialOverview
>
    implements $CustomerFinancialOverviewCopyWith<$Res> {
  _$CustomerFinancialOverviewCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of CustomerFinancialOverview
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? customerId = null,
    Object? totalPaidAmount = null,
    Object? totalOverdueAmount = null,
    Object? totalSellAmount = null,
    Object? totalRemainingAmount = null,
    Object? totalDailyInstallmentAmount = null,
    Object? activeOrdersCount = null,
    Object? completedOrdersCount = null,
  }) {
    return _then(
      _value.copyWith(
            customerId:
                null == customerId
                    ? _value.customerId
                    : customerId // ignore: cast_nullable_to_non_nullable
                        as int,
            totalPaidAmount:
                null == totalPaidAmount
                    ? _value.totalPaidAmount
                    : totalPaidAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            totalOverdueAmount:
                null == totalOverdueAmount
                    ? _value.totalOverdueAmount
                    : totalOverdueAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            totalSellAmount:
                null == totalSellAmount
                    ? _value.totalSellAmount
                    : totalSellAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            totalRemainingAmount:
                null == totalRemainingAmount
                    ? _value.totalRemainingAmount
                    : totalRemainingAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            totalDailyInstallmentAmount:
                null == totalDailyInstallmentAmount
                    ? _value.totalDailyInstallmentAmount
                    : totalDailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            activeOrdersCount:
                null == activeOrdersCount
                    ? _value.activeOrdersCount
                    : activeOrdersCount // ignore: cast_nullable_to_non_nullable
                        as int,
            completedOrdersCount:
                null == completedOrdersCount
                    ? _value.completedOrdersCount
                    : completedOrdersCount // ignore: cast_nullable_to_non_nullable
                        as int,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CustomerFinancialOverviewImplCopyWith<$Res>
    implements $CustomerFinancialOverviewCopyWith<$Res> {
  factory _$$CustomerFinancialOverviewImplCopyWith(
    _$CustomerFinancialOverviewImpl value,
    $Res Function(_$CustomerFinancialOverviewImpl) then,
  ) = __$$CustomerFinancialOverviewImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int customerId,
    double totalPaidAmount,
    double totalOverdueAmount,
    double totalSellAmount,
    double totalRemainingAmount,
    double totalDailyInstallmentAmount,
    int activeOrdersCount,
    int completedOrdersCount,
  });
}

/// @nodoc
class __$$CustomerFinancialOverviewImplCopyWithImpl<$Res>
    extends
        _$CustomerFinancialOverviewCopyWithImpl<
          $Res,
          _$CustomerFinancialOverviewImpl
        >
    implements _$$CustomerFinancialOverviewImplCopyWith<$Res> {
  __$$CustomerFinancialOverviewImplCopyWithImpl(
    _$CustomerFinancialOverviewImpl _value,
    $Res Function(_$CustomerFinancialOverviewImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of CustomerFinancialOverview
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? customerId = null,
    Object? totalPaidAmount = null,
    Object? totalOverdueAmount = null,
    Object? totalSellAmount = null,
    Object? totalRemainingAmount = null,
    Object? totalDailyInstallmentAmount = null,
    Object? activeOrdersCount = null,
    Object? completedOrdersCount = null,
  }) {
    return _then(
      _$CustomerFinancialOverviewImpl(
        customerId:
            null == customerId
                ? _value.customerId
                : customerId // ignore: cast_nullable_to_non_nullable
                    as int,
        totalPaidAmount:
            null == totalPaidAmount
                ? _value.totalPaidAmount
                : totalPaidAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        totalOverdueAmount:
            null == totalOverdueAmount
                ? _value.totalOverdueAmount
                : totalOverdueAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        totalSellAmount:
            null == totalSellAmount
                ? _value.totalSellAmount
                : totalSellAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        totalRemainingAmount:
            null == totalRemainingAmount
                ? _value.totalRemainingAmount
                : totalRemainingAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        totalDailyInstallmentAmount:
            null == totalDailyInstallmentAmount
                ? _value.totalDailyInstallmentAmount
                : totalDailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        activeOrdersCount:
            null == activeOrdersCount
                ? _value.activeOrdersCount
                : activeOrdersCount // ignore: cast_nullable_to_non_nullable
                    as int,
        completedOrdersCount:
            null == completedOrdersCount
                ? _value.completedOrdersCount
                : completedOrdersCount // ignore: cast_nullable_to_non_nullable
                    as int,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CustomerFinancialOverviewImpl implements _CustomerFinancialOverview {
  const _$CustomerFinancialOverviewImpl({
    required this.customerId,
    required this.totalPaidAmount,
    required this.totalOverdueAmount,
    required this.totalSellAmount,
    required this.totalRemainingAmount,
    required this.totalDailyInstallmentAmount,
    required this.activeOrdersCount,
    required this.completedOrdersCount,
  });

  factory _$CustomerFinancialOverviewImpl.fromJson(Map<String, dynamic> json) =>
      _$$CustomerFinancialOverviewImplFromJson(json);

  @override
  final int customerId;
  @override
  final double totalPaidAmount;
  @override
  final double totalOverdueAmount;
  @override
  final double totalSellAmount;
  @override
  final double totalRemainingAmount;
  @override
  final double totalDailyInstallmentAmount;
  @override
  final int activeOrdersCount;
  @override
  final int completedOrdersCount;

  @override
  String toString() {
    return 'CustomerFinancialOverview(customerId: $customerId, totalPaidAmount: $totalPaidAmount, totalOverdueAmount: $totalOverdueAmount, totalSellAmount: $totalSellAmount, totalRemainingAmount: $totalRemainingAmount, totalDailyInstallmentAmount: $totalDailyInstallmentAmount, activeOrdersCount: $activeOrdersCount, completedOrdersCount: $completedOrdersCount)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CustomerFinancialOverviewImpl &&
            (identical(other.customerId, customerId) ||
                other.customerId == customerId) &&
            (identical(other.totalPaidAmount, totalPaidAmount) ||
                other.totalPaidAmount == totalPaidAmount) &&
            (identical(other.totalOverdueAmount, totalOverdueAmount) ||
                other.totalOverdueAmount == totalOverdueAmount) &&
            (identical(other.totalSellAmount, totalSellAmount) ||
                other.totalSellAmount == totalSellAmount) &&
            (identical(other.totalRemainingAmount, totalRemainingAmount) ||
                other.totalRemainingAmount == totalRemainingAmount) &&
            (identical(
                  other.totalDailyInstallmentAmount,
                  totalDailyInstallmentAmount,
                ) ||
                other.totalDailyInstallmentAmount ==
                    totalDailyInstallmentAmount) &&
            (identical(other.activeOrdersCount, activeOrdersCount) ||
                other.activeOrdersCount == activeOrdersCount) &&
            (identical(other.completedOrdersCount, completedOrdersCount) ||
                other.completedOrdersCount == completedOrdersCount));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    customerId,
    totalPaidAmount,
    totalOverdueAmount,
    totalSellAmount,
    totalRemainingAmount,
    totalDailyInstallmentAmount,
    activeOrdersCount,
    completedOrdersCount,
  );

  /// Create a copy of CustomerFinancialOverview
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CustomerFinancialOverviewImplCopyWith<_$CustomerFinancialOverviewImpl>
  get copyWith => __$$CustomerFinancialOverviewImplCopyWithImpl<
    _$CustomerFinancialOverviewImpl
  >(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$CustomerFinancialOverviewImplToJson(this);
  }
}

abstract class _CustomerFinancialOverview implements CustomerFinancialOverview {
  const factory _CustomerFinancialOverview({
    required final int customerId,
    required final double totalPaidAmount,
    required final double totalOverdueAmount,
    required final double totalSellAmount,
    required final double totalRemainingAmount,
    required final double totalDailyInstallmentAmount,
    required final int activeOrdersCount,
    required final int completedOrdersCount,
  }) = _$CustomerFinancialOverviewImpl;

  factory _CustomerFinancialOverview.fromJson(Map<String, dynamic> json) =
      _$CustomerFinancialOverviewImpl.fromJson;

  @override
  int get customerId;
  @override
  double get totalPaidAmount;
  @override
  double get totalOverdueAmount;
  @override
  double get totalSellAmount;
  @override
  double get totalRemainingAmount;
  @override
  double get totalDailyInstallmentAmount;
  @override
  int get activeOrdersCount;
  @override
  int get completedOrdersCount;

  /// Create a copy of CustomerFinancialOverview
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CustomerFinancialOverviewImplCopyWith<_$CustomerFinancialOverviewImpl>
  get copyWith => throw _privateConstructorUsedError;
}
