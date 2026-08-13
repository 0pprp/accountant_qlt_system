// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'installment_payment.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

InstallmentPayment _$InstallmentPaymentFromJson(Map<String, dynamic> json) {
  return _InstallmentPayment.fromJson(json);
}

/// @nodoc
mixin _$InstallmentPayment {
  int get id => throw _privateConstructorUsedError;
  int get orderId => throw _privateConstructorUsedError;
  String get customerFullName => throw _privateConstructorUsedError;
  int? get orderListId => throw _privateConstructorUsedError;
  double get amount => throw _privateConstructorUsedError;
  DateTime get date => throw _privateConstructorUsedError;
  DateTime get lastUpdatedAt => throw _privateConstructorUsedError;

  /// Serializes this InstallmentPayment to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of InstallmentPayment
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $InstallmentPaymentCopyWith<InstallmentPayment> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $InstallmentPaymentCopyWith<$Res> {
  factory $InstallmentPaymentCopyWith(
    InstallmentPayment value,
    $Res Function(InstallmentPayment) then,
  ) = _$InstallmentPaymentCopyWithImpl<$Res, InstallmentPayment>;
  @useResult
  $Res call({
    int id,
    int orderId,
    String customerFullName,
    int? orderListId,
    double amount,
    DateTime date,
    DateTime lastUpdatedAt,
  });
}

/// @nodoc
class _$InstallmentPaymentCopyWithImpl<$Res, $Val extends InstallmentPayment>
    implements $InstallmentPaymentCopyWith<$Res> {
  _$InstallmentPaymentCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of InstallmentPayment
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? orderId = null,
    Object? customerFullName = null,
    Object? orderListId = freezed,
    Object? amount = null,
    Object? date = null,
    Object? lastUpdatedAt = null,
  }) {
    return _then(
      _value.copyWith(
            id:
                null == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int,
            orderId:
                null == orderId
                    ? _value.orderId
                    : orderId // ignore: cast_nullable_to_non_nullable
                        as int,
            customerFullName:
                null == customerFullName
                    ? _value.customerFullName
                    : customerFullName // ignore: cast_nullable_to_non_nullable
                        as String,
            orderListId:
                freezed == orderListId
                    ? _value.orderListId
                    : orderListId // ignore: cast_nullable_to_non_nullable
                        as int?,
            amount:
                null == amount
                    ? _value.amount
                    : amount // ignore: cast_nullable_to_non_nullable
                        as double,
            date:
                null == date
                    ? _value.date
                    : date // ignore: cast_nullable_to_non_nullable
                        as DateTime,
            lastUpdatedAt:
                null == lastUpdatedAt
                    ? _value.lastUpdatedAt
                    : lastUpdatedAt // ignore: cast_nullable_to_non_nullable
                        as DateTime,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$InstallmentPaymentImplCopyWith<$Res>
    implements $InstallmentPaymentCopyWith<$Res> {
  factory _$$InstallmentPaymentImplCopyWith(
    _$InstallmentPaymentImpl value,
    $Res Function(_$InstallmentPaymentImpl) then,
  ) = __$$InstallmentPaymentImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int id,
    int orderId,
    String customerFullName,
    int? orderListId,
    double amount,
    DateTime date,
    DateTime lastUpdatedAt,
  });
}

/// @nodoc
class __$$InstallmentPaymentImplCopyWithImpl<$Res>
    extends _$InstallmentPaymentCopyWithImpl<$Res, _$InstallmentPaymentImpl>
    implements _$$InstallmentPaymentImplCopyWith<$Res> {
  __$$InstallmentPaymentImplCopyWithImpl(
    _$InstallmentPaymentImpl _value,
    $Res Function(_$InstallmentPaymentImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of InstallmentPayment
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? orderId = null,
    Object? customerFullName = null,
    Object? orderListId = freezed,
    Object? amount = null,
    Object? date = null,
    Object? lastUpdatedAt = null,
  }) {
    return _then(
      _$InstallmentPaymentImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        orderId:
            null == orderId
                ? _value.orderId
                : orderId // ignore: cast_nullable_to_non_nullable
                    as int,
        customerFullName:
            null == customerFullName
                ? _value.customerFullName
                : customerFullName // ignore: cast_nullable_to_non_nullable
                    as String,
        orderListId:
            freezed == orderListId
                ? _value.orderListId
                : orderListId // ignore: cast_nullable_to_non_nullable
                    as int?,
        amount:
            null == amount
                ? _value.amount
                : amount // ignore: cast_nullable_to_non_nullable
                    as double,
        date:
            null == date
                ? _value.date
                : date // ignore: cast_nullable_to_non_nullable
                    as DateTime,
        lastUpdatedAt:
            null == lastUpdatedAt
                ? _value.lastUpdatedAt
                : lastUpdatedAt // ignore: cast_nullable_to_non_nullable
                    as DateTime,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$InstallmentPaymentImpl implements _InstallmentPayment {
  const _$InstallmentPaymentImpl({
    required this.id,
    required this.orderId,
    required this.customerFullName,
    this.orderListId,
    required this.amount,
    required this.date,
    required this.lastUpdatedAt,
  });

  factory _$InstallmentPaymentImpl.fromJson(Map<String, dynamic> json) =>
      _$$InstallmentPaymentImplFromJson(json);

  @override
  final int id;
  @override
  final int orderId;
  @override
  final String customerFullName;
  @override
  final int? orderListId;
  @override
  final double amount;
  @override
  final DateTime date;
  @override
  final DateTime lastUpdatedAt;

  @override
  String toString() {
    return 'InstallmentPayment(id: $id, orderId: $orderId, customerFullName: $customerFullName, orderListId: $orderListId, amount: $amount, date: $date, lastUpdatedAt: $lastUpdatedAt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$InstallmentPaymentImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.orderId, orderId) || other.orderId == orderId) &&
            (identical(other.customerFullName, customerFullName) ||
                other.customerFullName == customerFullName) &&
            (identical(other.orderListId, orderListId) ||
                other.orderListId == orderListId) &&
            (identical(other.amount, amount) || other.amount == amount) &&
            (identical(other.date, date) || other.date == date) &&
            (identical(other.lastUpdatedAt, lastUpdatedAt) ||
                other.lastUpdatedAt == lastUpdatedAt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    orderId,
    customerFullName,
    orderListId,
    amount,
    date,
    lastUpdatedAt,
  );

  /// Create a copy of InstallmentPayment
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$InstallmentPaymentImplCopyWith<_$InstallmentPaymentImpl> get copyWith =>
      __$$InstallmentPaymentImplCopyWithImpl<_$InstallmentPaymentImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$InstallmentPaymentImplToJson(this);
  }
}

abstract class _InstallmentPayment implements InstallmentPayment {
  const factory _InstallmentPayment({
    required final int id,
    required final int orderId,
    required final String customerFullName,
    final int? orderListId,
    required final double amount,
    required final DateTime date,
    required final DateTime lastUpdatedAt,
  }) = _$InstallmentPaymentImpl;

  factory _InstallmentPayment.fromJson(Map<String, dynamic> json) =
      _$InstallmentPaymentImpl.fromJson;

  @override
  int get id;
  @override
  int get orderId;
  @override
  String get customerFullName;
  @override
  int? get orderListId;
  @override
  double get amount;
  @override
  DateTime get date;
  @override
  DateTime get lastUpdatedAt;

  /// Create a copy of InstallmentPayment
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$InstallmentPaymentImplCopyWith<_$InstallmentPaymentImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
