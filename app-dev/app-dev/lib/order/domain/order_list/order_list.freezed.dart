// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'order_list.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

OrderList _$OrderListFromJson(Map<String, dynamic> json) {
  return _OrderList.fromJson(json);
}

/// @nodoc
mixin _$OrderList {
  int get id => throw _privateConstructorUsedError;
  String get name => throw _privateConstructorUsedError;
  String get mandobFullName => throw _privateConstructorUsedError;
  String? get mandobPhoneNumber => throw _privateConstructorUsedError;
  int get totalOrderCount => throw _privateConstructorUsedError;
  int get todayCollectedOrderCount => throw _privateConstructorUsedError;
  double get totalSellAmount =>
      throw _privateConstructorUsedError; // Assuming this can be a decimal value
  double get totalDailyInstallmentAmount =>
      throw _privateConstructorUsedError; // Assuming this can be a decimal value
  double get totalOverdueInstallmentAmount =>
      throw _privateConstructorUsedError; // Assuming this can be a decimal value
  double get totalCollectedInstallmentAmount =>
      throw _privateConstructorUsedError; // Assuming this can be a decimal value
  double get totalUnpaidInstallmentAmount => throw _privateConstructorUsedError;

  /// Serializes this OrderList to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of OrderList
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $OrderListCopyWith<OrderList> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $OrderListCopyWith<$Res> {
  factory $OrderListCopyWith(OrderList value, $Res Function(OrderList) then) =
      _$OrderListCopyWithImpl<$Res, OrderList>;
  @useResult
  $Res call({
    int id,
    String name,
    String mandobFullName,
    String? mandobPhoneNumber,
    int totalOrderCount,
    int todayCollectedOrderCount,
    double totalSellAmount,
    double totalDailyInstallmentAmount,
    double totalOverdueInstallmentAmount,
    double totalCollectedInstallmentAmount,
    double totalUnpaidInstallmentAmount,
  });
}

/// @nodoc
class _$OrderListCopyWithImpl<$Res, $Val extends OrderList>
    implements $OrderListCopyWith<$Res> {
  _$OrderListCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of OrderList
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? name = null,
    Object? mandobFullName = null,
    Object? mandobPhoneNumber = freezed,
    Object? totalOrderCount = null,
    Object? todayCollectedOrderCount = null,
    Object? totalSellAmount = null,
    Object? totalDailyInstallmentAmount = null,
    Object? totalOverdueInstallmentAmount = null,
    Object? totalCollectedInstallmentAmount = null,
    Object? totalUnpaidInstallmentAmount = null,
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
                null == mandobFullName
                    ? _value.mandobFullName
                    : mandobFullName // ignore: cast_nullable_to_non_nullable
                        as String,
            mandobPhoneNumber:
                freezed == mandobPhoneNumber
                    ? _value.mandobPhoneNumber
                    : mandobPhoneNumber // ignore: cast_nullable_to_non_nullable
                        as String?,
            totalOrderCount:
                null == totalOrderCount
                    ? _value.totalOrderCount
                    : totalOrderCount // ignore: cast_nullable_to_non_nullable
                        as int,
            todayCollectedOrderCount:
                null == todayCollectedOrderCount
                    ? _value.todayCollectedOrderCount
                    : todayCollectedOrderCount // ignore: cast_nullable_to_non_nullable
                        as int,
            totalSellAmount:
                null == totalSellAmount
                    ? _value.totalSellAmount
                    : totalSellAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            totalDailyInstallmentAmount:
                null == totalDailyInstallmentAmount
                    ? _value.totalDailyInstallmentAmount
                    : totalDailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            totalOverdueInstallmentAmount:
                null == totalOverdueInstallmentAmount
                    ? _value.totalOverdueInstallmentAmount
                    : totalOverdueInstallmentAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            totalCollectedInstallmentAmount:
                null == totalCollectedInstallmentAmount
                    ? _value.totalCollectedInstallmentAmount
                    : totalCollectedInstallmentAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            totalUnpaidInstallmentAmount:
                null == totalUnpaidInstallmentAmount
                    ? _value.totalUnpaidInstallmentAmount
                    : totalUnpaidInstallmentAmount // ignore: cast_nullable_to_non_nullable
                        as double,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$OrderListImplCopyWith<$Res>
    implements $OrderListCopyWith<$Res> {
  factory _$$OrderListImplCopyWith(
    _$OrderListImpl value,
    $Res Function(_$OrderListImpl) then,
  ) = __$$OrderListImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int id,
    String name,
    String mandobFullName,
    String? mandobPhoneNumber,
    int totalOrderCount,
    int todayCollectedOrderCount,
    double totalSellAmount,
    double totalDailyInstallmentAmount,
    double totalOverdueInstallmentAmount,
    double totalCollectedInstallmentAmount,
    double totalUnpaidInstallmentAmount,
  });
}

/// @nodoc
class __$$OrderListImplCopyWithImpl<$Res>
    extends _$OrderListCopyWithImpl<$Res, _$OrderListImpl>
    implements _$$OrderListImplCopyWith<$Res> {
  __$$OrderListImplCopyWithImpl(
    _$OrderListImpl _value,
    $Res Function(_$OrderListImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of OrderList
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? name = null,
    Object? mandobFullName = null,
    Object? mandobPhoneNumber = freezed,
    Object? totalOrderCount = null,
    Object? todayCollectedOrderCount = null,
    Object? totalSellAmount = null,
    Object? totalDailyInstallmentAmount = null,
    Object? totalOverdueInstallmentAmount = null,
    Object? totalCollectedInstallmentAmount = null,
    Object? totalUnpaidInstallmentAmount = null,
  }) {
    return _then(
      _$OrderListImpl(
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
            null == mandobFullName
                ? _value.mandobFullName
                : mandobFullName // ignore: cast_nullable_to_non_nullable
                    as String,
        mandobPhoneNumber:
            freezed == mandobPhoneNumber
                ? _value.mandobPhoneNumber
                : mandobPhoneNumber // ignore: cast_nullable_to_non_nullable
                    as String?,
        totalOrderCount:
            null == totalOrderCount
                ? _value.totalOrderCount
                : totalOrderCount // ignore: cast_nullable_to_non_nullable
                    as int,
        todayCollectedOrderCount:
            null == todayCollectedOrderCount
                ? _value.todayCollectedOrderCount
                : todayCollectedOrderCount // ignore: cast_nullable_to_non_nullable
                    as int,
        totalSellAmount:
            null == totalSellAmount
                ? _value.totalSellAmount
                : totalSellAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        totalDailyInstallmentAmount:
            null == totalDailyInstallmentAmount
                ? _value.totalDailyInstallmentAmount
                : totalDailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        totalOverdueInstallmentAmount:
            null == totalOverdueInstallmentAmount
                ? _value.totalOverdueInstallmentAmount
                : totalOverdueInstallmentAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        totalCollectedInstallmentAmount:
            null == totalCollectedInstallmentAmount
                ? _value.totalCollectedInstallmentAmount
                : totalCollectedInstallmentAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        totalUnpaidInstallmentAmount:
            null == totalUnpaidInstallmentAmount
                ? _value.totalUnpaidInstallmentAmount
                : totalUnpaidInstallmentAmount // ignore: cast_nullable_to_non_nullable
                    as double,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$OrderListImpl implements _OrderList {
  const _$OrderListImpl({
    required this.id,
    required this.name,
    required this.mandobFullName,
    this.mandobPhoneNumber,
    required this.totalOrderCount,
    required this.todayCollectedOrderCount,
    required this.totalSellAmount,
    required this.totalDailyInstallmentAmount,
    required this.totalOverdueInstallmentAmount,
    required this.totalCollectedInstallmentAmount,
    required this.totalUnpaidInstallmentAmount,
  });

  factory _$OrderListImpl.fromJson(Map<String, dynamic> json) =>
      _$$OrderListImplFromJson(json);

  @override
  final int id;
  @override
  final String name;
  @override
  final String mandobFullName;
  @override
  final String? mandobPhoneNumber;
  @override
  final int totalOrderCount;
  @override
  final int todayCollectedOrderCount;
  @override
  final double totalSellAmount;
  // Assuming this can be a decimal value
  @override
  final double totalDailyInstallmentAmount;
  // Assuming this can be a decimal value
  @override
  final double totalOverdueInstallmentAmount;
  // Assuming this can be a decimal value
  @override
  final double totalCollectedInstallmentAmount;
  // Assuming this can be a decimal value
  @override
  final double totalUnpaidInstallmentAmount;

  @override
  String toString() {
    return 'OrderList(id: $id, name: $name, mandobFullName: $mandobFullName, mandobPhoneNumber: $mandobPhoneNumber, totalOrderCount: $totalOrderCount, todayCollectedOrderCount: $todayCollectedOrderCount, totalSellAmount: $totalSellAmount, totalDailyInstallmentAmount: $totalDailyInstallmentAmount, totalOverdueInstallmentAmount: $totalOverdueInstallmentAmount, totalCollectedInstallmentAmount: $totalCollectedInstallmentAmount, totalUnpaidInstallmentAmount: $totalUnpaidInstallmentAmount)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$OrderListImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.mandobFullName, mandobFullName) ||
                other.mandobFullName == mandobFullName) &&
            (identical(other.mandobPhoneNumber, mandobPhoneNumber) ||
                other.mandobPhoneNumber == mandobPhoneNumber) &&
            (identical(other.totalOrderCount, totalOrderCount) ||
                other.totalOrderCount == totalOrderCount) &&
            (identical(
                  other.todayCollectedOrderCount,
                  todayCollectedOrderCount,
                ) ||
                other.todayCollectedOrderCount == todayCollectedOrderCount) &&
            (identical(other.totalSellAmount, totalSellAmount) ||
                other.totalSellAmount == totalSellAmount) &&
            (identical(
                  other.totalDailyInstallmentAmount,
                  totalDailyInstallmentAmount,
                ) ||
                other.totalDailyInstallmentAmount ==
                    totalDailyInstallmentAmount) &&
            (identical(
                  other.totalOverdueInstallmentAmount,
                  totalOverdueInstallmentAmount,
                ) ||
                other.totalOverdueInstallmentAmount ==
                    totalOverdueInstallmentAmount) &&
            (identical(
                  other.totalCollectedInstallmentAmount,
                  totalCollectedInstallmentAmount,
                ) ||
                other.totalCollectedInstallmentAmount ==
                    totalCollectedInstallmentAmount) &&
            (identical(
                  other.totalUnpaidInstallmentAmount,
                  totalUnpaidInstallmentAmount,
                ) ||
                other.totalUnpaidInstallmentAmount ==
                    totalUnpaidInstallmentAmount));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    name,
    mandobFullName,
    mandobPhoneNumber,
    totalOrderCount,
    todayCollectedOrderCount,
    totalSellAmount,
    totalDailyInstallmentAmount,
    totalOverdueInstallmentAmount,
    totalCollectedInstallmentAmount,
    totalUnpaidInstallmentAmount,
  );

  /// Create a copy of OrderList
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$OrderListImplCopyWith<_$OrderListImpl> get copyWith =>
      __$$OrderListImplCopyWithImpl<_$OrderListImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$OrderListImplToJson(this);
  }
}

abstract class _OrderList implements OrderList {
  const factory _OrderList({
    required final int id,
    required final String name,
    required final String mandobFullName,
    final String? mandobPhoneNumber,
    required final int totalOrderCount,
    required final int todayCollectedOrderCount,
    required final double totalSellAmount,
    required final double totalDailyInstallmentAmount,
    required final double totalOverdueInstallmentAmount,
    required final double totalCollectedInstallmentAmount,
    required final double totalUnpaidInstallmentAmount,
  }) = _$OrderListImpl;

  factory _OrderList.fromJson(Map<String, dynamic> json) =
      _$OrderListImpl.fromJson;

  @override
  int get id;
  @override
  String get name;
  @override
  String get mandobFullName;
  @override
  String? get mandobPhoneNumber;
  @override
  int get totalOrderCount;
  @override
  int get todayCollectedOrderCount;
  @override
  double get totalSellAmount; // Assuming this can be a decimal value
  @override
  double get totalDailyInstallmentAmount; // Assuming this can be a decimal value
  @override
  double get totalOverdueInstallmentAmount; // Assuming this can be a decimal value
  @override
  double get totalCollectedInstallmentAmount; // Assuming this can be a decimal value
  @override
  double get totalUnpaidInstallmentAmount;

  /// Create a copy of OrderList
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$OrderListImplCopyWith<_$OrderListImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
