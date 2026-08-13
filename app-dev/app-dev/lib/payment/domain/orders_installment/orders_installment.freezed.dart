// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'orders_installment.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

OrdersInstallment _$OrdersInstallmentFromJson(Map<String, dynamic> json) {
  return _OrdersInstallment.fromJson(json);
}

/// @nodoc
mixin _$OrdersInstallment {
  int get id => throw _privateConstructorUsedError;
  String get customerFullName => throw _privateConstructorUsedError;
  double get sellAmount => throw _privateConstructorUsedError;
  DateTime get createdAt =>
      throw _privateConstructorUsedError; // required String productName,
  List<Installment> get installmentPayments =>
      throw _privateConstructorUsedError;

  /// Serializes this OrdersInstallment to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of OrdersInstallment
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $OrdersInstallmentCopyWith<OrdersInstallment> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $OrdersInstallmentCopyWith<$Res> {
  factory $OrdersInstallmentCopyWith(
    OrdersInstallment value,
    $Res Function(OrdersInstallment) then,
  ) = _$OrdersInstallmentCopyWithImpl<$Res, OrdersInstallment>;
  @useResult
  $Res call({
    int id,
    String customerFullName,
    double sellAmount,
    DateTime createdAt,
    List<Installment> installmentPayments,
  });
}

/// @nodoc
class _$OrdersInstallmentCopyWithImpl<$Res, $Val extends OrdersInstallment>
    implements $OrdersInstallmentCopyWith<$Res> {
  _$OrdersInstallmentCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of OrdersInstallment
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? customerFullName = null,
    Object? sellAmount = null,
    Object? createdAt = null,
    Object? installmentPayments = null,
  }) {
    return _then(
      _value.copyWith(
            id:
                null == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int,
            customerFullName:
                null == customerFullName
                    ? _value.customerFullName
                    : customerFullName // ignore: cast_nullable_to_non_nullable
                        as String,
            sellAmount:
                null == sellAmount
                    ? _value.sellAmount
                    : sellAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            createdAt:
                null == createdAt
                    ? _value.createdAt
                    : createdAt // ignore: cast_nullable_to_non_nullable
                        as DateTime,
            installmentPayments:
                null == installmentPayments
                    ? _value.installmentPayments
                    : installmentPayments // ignore: cast_nullable_to_non_nullable
                        as List<Installment>,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$OrdersInstallmentImplCopyWith<$Res>
    implements $OrdersInstallmentCopyWith<$Res> {
  factory _$$OrdersInstallmentImplCopyWith(
    _$OrdersInstallmentImpl value,
    $Res Function(_$OrdersInstallmentImpl) then,
  ) = __$$OrdersInstallmentImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int id,
    String customerFullName,
    double sellAmount,
    DateTime createdAt,
    List<Installment> installmentPayments,
  });
}

/// @nodoc
class __$$OrdersInstallmentImplCopyWithImpl<$Res>
    extends _$OrdersInstallmentCopyWithImpl<$Res, _$OrdersInstallmentImpl>
    implements _$$OrdersInstallmentImplCopyWith<$Res> {
  __$$OrdersInstallmentImplCopyWithImpl(
    _$OrdersInstallmentImpl _value,
    $Res Function(_$OrdersInstallmentImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of OrdersInstallment
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? customerFullName = null,
    Object? sellAmount = null,
    Object? createdAt = null,
    Object? installmentPayments = null,
  }) {
    return _then(
      _$OrdersInstallmentImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        customerFullName:
            null == customerFullName
                ? _value.customerFullName
                : customerFullName // ignore: cast_nullable_to_non_nullable
                    as String,
        sellAmount:
            null == sellAmount
                ? _value.sellAmount
                : sellAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        createdAt:
            null == createdAt
                ? _value.createdAt
                : createdAt // ignore: cast_nullable_to_non_nullable
                    as DateTime,
        installmentPayments:
            null == installmentPayments
                ? _value._installmentPayments
                : installmentPayments // ignore: cast_nullable_to_non_nullable
                    as List<Installment>,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$OrdersInstallmentImpl implements _OrdersInstallment {
  const _$OrdersInstallmentImpl({
    required this.id,
    required this.customerFullName,
    required this.sellAmount,
    required this.createdAt,
    required final List<Installment> installmentPayments,
  }) : _installmentPayments = installmentPayments;

  factory _$OrdersInstallmentImpl.fromJson(Map<String, dynamic> json) =>
      _$$OrdersInstallmentImplFromJson(json);

  @override
  final int id;
  @override
  final String customerFullName;
  @override
  final double sellAmount;
  @override
  final DateTime createdAt;
  // required String productName,
  final List<Installment> _installmentPayments;
  // required String productName,
  @override
  List<Installment> get installmentPayments {
    if (_installmentPayments is EqualUnmodifiableListView)
      return _installmentPayments;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_installmentPayments);
  }

  @override
  String toString() {
    return 'OrdersInstallment(id: $id, customerFullName: $customerFullName, sellAmount: $sellAmount, createdAt: $createdAt, installmentPayments: $installmentPayments)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$OrdersInstallmentImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.customerFullName, customerFullName) ||
                other.customerFullName == customerFullName) &&
            (identical(other.sellAmount, sellAmount) ||
                other.sellAmount == sellAmount) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt) &&
            const DeepCollectionEquality().equals(
              other._installmentPayments,
              _installmentPayments,
            ));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    customerFullName,
    sellAmount,
    createdAt,
    const DeepCollectionEquality().hash(_installmentPayments),
  );

  /// Create a copy of OrdersInstallment
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$OrdersInstallmentImplCopyWith<_$OrdersInstallmentImpl> get copyWith =>
      __$$OrdersInstallmentImplCopyWithImpl<_$OrdersInstallmentImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$OrdersInstallmentImplToJson(this);
  }
}

abstract class _OrdersInstallment implements OrdersInstallment {
  const factory _OrdersInstallment({
    required final int id,
    required final String customerFullName,
    required final double sellAmount,
    required final DateTime createdAt,
    required final List<Installment> installmentPayments,
  }) = _$OrdersInstallmentImpl;

  factory _OrdersInstallment.fromJson(Map<String, dynamic> json) =
      _$OrdersInstallmentImpl.fromJson;

  @override
  int get id;
  @override
  String get customerFullName;
  @override
  double get sellAmount;
  @override
  DateTime get createdAt; // required String productName,
  @override
  List<Installment> get installmentPayments;

  /// Create a copy of OrdersInstallment
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$OrdersInstallmentImplCopyWith<_$OrdersInstallmentImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

Installment _$InstallmentFromJson(Map<String, dynamic> json) {
  return _Installment.fromJson(json);
}

/// @nodoc
mixin _$Installment {
  int? get id => throw _privateConstructorUsedError;
  DateTime get date => throw _privateConstructorUsedError;
  double? get amount =>
      throw _privateConstructorUsedError; // required InstallmentStatus status,
  bool get hasPayment => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;

  /// Serializes this Installment to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of Installment
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $InstallmentCopyWith<Installment> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $InstallmentCopyWith<$Res> {
  factory $InstallmentCopyWith(
    Installment value,
    $Res Function(Installment) then,
  ) = _$InstallmentCopyWithImpl<$Res, Installment>;
  @useResult
  $Res call({
    int? id,
    DateTime date,
    double? amount,
    bool hasPayment,
    String? description,
  });
}

/// @nodoc
class _$InstallmentCopyWithImpl<$Res, $Val extends Installment>
    implements $InstallmentCopyWith<$Res> {
  _$InstallmentCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of Installment
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? date = null,
    Object? amount = freezed,
    Object? hasPayment = null,
    Object? description = freezed,
  }) {
    return _then(
      _value.copyWith(
            id:
                freezed == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int?,
            date:
                null == date
                    ? _value.date
                    : date // ignore: cast_nullable_to_non_nullable
                        as DateTime,
            amount:
                freezed == amount
                    ? _value.amount
                    : amount // ignore: cast_nullable_to_non_nullable
                        as double?,
            hasPayment:
                null == hasPayment
                    ? _value.hasPayment
                    : hasPayment // ignore: cast_nullable_to_non_nullable
                        as bool,
            description:
                freezed == description
                    ? _value.description
                    : description // ignore: cast_nullable_to_non_nullable
                        as String?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$InstallmentImplCopyWith<$Res>
    implements $InstallmentCopyWith<$Res> {
  factory _$$InstallmentImplCopyWith(
    _$InstallmentImpl value,
    $Res Function(_$InstallmentImpl) then,
  ) = __$$InstallmentImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int? id,
    DateTime date,
    double? amount,
    bool hasPayment,
    String? description,
  });
}

/// @nodoc
class __$$InstallmentImplCopyWithImpl<$Res>
    extends _$InstallmentCopyWithImpl<$Res, _$InstallmentImpl>
    implements _$$InstallmentImplCopyWith<$Res> {
  __$$InstallmentImplCopyWithImpl(
    _$InstallmentImpl _value,
    $Res Function(_$InstallmentImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of Installment
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? date = null,
    Object? amount = freezed,
    Object? hasPayment = null,
    Object? description = freezed,
  }) {
    return _then(
      _$InstallmentImpl(
        id:
            freezed == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int?,
        date:
            null == date
                ? _value.date
                : date // ignore: cast_nullable_to_non_nullable
                    as DateTime,
        amount:
            freezed == amount
                ? _value.amount
                : amount // ignore: cast_nullable_to_non_nullable
                    as double?,
        hasPayment:
            null == hasPayment
                ? _value.hasPayment
                : hasPayment // ignore: cast_nullable_to_non_nullable
                    as bool,
        description:
            freezed == description
                ? _value.description
                : description // ignore: cast_nullable_to_non_nullable
                    as String?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$InstallmentImpl implements _Installment {
  const _$InstallmentImpl({
    this.id,
    required this.date,
    this.amount,
    this.hasPayment = false,
    this.description,
  });

  factory _$InstallmentImpl.fromJson(Map<String, dynamic> json) =>
      _$$InstallmentImplFromJson(json);

  @override
  final int? id;
  @override
  final DateTime date;
  @override
  final double? amount;
  // required InstallmentStatus status,
  @override
  @JsonKey()
  final bool hasPayment;
  @override
  final String? description;

  @override
  String toString() {
    return 'Installment(id: $id, date: $date, amount: $amount, hasPayment: $hasPayment, description: $description)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$InstallmentImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.date, date) || other.date == date) &&
            (identical(other.amount, amount) || other.amount == amount) &&
            (identical(other.hasPayment, hasPayment) ||
                other.hasPayment == hasPayment) &&
            (identical(other.description, description) ||
                other.description == description));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, id, date, amount, hasPayment, description);

  /// Create a copy of Installment
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$InstallmentImplCopyWith<_$InstallmentImpl> get copyWith =>
      __$$InstallmentImplCopyWithImpl<_$InstallmentImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$InstallmentImplToJson(this);
  }
}

abstract class _Installment implements Installment {
  const factory _Installment({
    final int? id,
    required final DateTime date,
    final double? amount,
    final bool hasPayment,
    final String? description,
  }) = _$InstallmentImpl;

  factory _Installment.fromJson(Map<String, dynamic> json) =
      _$InstallmentImpl.fromJson;

  @override
  int? get id;
  @override
  DateTime get date;
  @override
  double? get amount; // required InstallmentStatus status,
  @override
  bool get hasPayment;
  @override
  String? get description;

  /// Create a copy of Installment
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$InstallmentImplCopyWith<_$InstallmentImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
