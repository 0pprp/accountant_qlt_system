// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'order_item_details.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

OrderItemDetails _$OrderItemDetailsFromJson(Map<String, dynamic> json) {
  return _OrderItemDetails.fromJson(json);
}

/// @nodoc
mixin _$OrderItemDetails {
  int? get id => throw _privateConstructorUsedError;
  int? get productId => throw _privateConstructorUsedError;
  String? get productName => throw _privateConstructorUsedError;
  int get productType => throw _privateConstructorUsedError;
  int get quantity => throw _privateConstructorUsedError;
  double get buyAmount => throw _privateConstructorUsedError;
  double get sellAmount => throw _privateConstructorUsedError;
  double get prepaymentAmount => throw _privateConstructorUsedError;
  double get dailyInstallmentAmount => throw _privateConstructorUsedError;

  /// Serializes this OrderItemDetails to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of OrderItemDetails
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $OrderItemDetailsCopyWith<OrderItemDetails> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $OrderItemDetailsCopyWith<$Res> {
  factory $OrderItemDetailsCopyWith(
    OrderItemDetails value,
    $Res Function(OrderItemDetails) then,
  ) = _$OrderItemDetailsCopyWithImpl<$Res, OrderItemDetails>;
  @useResult
  $Res call({
    int? id,
    int? productId,
    String? productName,
    int productType,
    int quantity,
    double buyAmount,
    double sellAmount,
    double prepaymentAmount,
    double dailyInstallmentAmount,
  });
}

/// @nodoc
class _$OrderItemDetailsCopyWithImpl<$Res, $Val extends OrderItemDetails>
    implements $OrderItemDetailsCopyWith<$Res> {
  _$OrderItemDetailsCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of OrderItemDetails
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? productId = freezed,
    Object? productName = freezed,
    Object? productType = null,
    Object? quantity = null,
    Object? buyAmount = null,
    Object? sellAmount = null,
    Object? prepaymentAmount = null,
    Object? dailyInstallmentAmount = null,
  }) {
    return _then(
      _value.copyWith(
            id:
                freezed == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int?,
            productId:
                freezed == productId
                    ? _value.productId
                    : productId // ignore: cast_nullable_to_non_nullable
                        as int?,
            productName:
                freezed == productName
                    ? _value.productName
                    : productName // ignore: cast_nullable_to_non_nullable
                        as String?,
            productType:
                null == productType
                    ? _value.productType
                    : productType // ignore: cast_nullable_to_non_nullable
                        as int,
            quantity:
                null == quantity
                    ? _value.quantity
                    : quantity // ignore: cast_nullable_to_non_nullable
                        as int,
            buyAmount:
                null == buyAmount
                    ? _value.buyAmount
                    : buyAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            sellAmount:
                null == sellAmount
                    ? _value.sellAmount
                    : sellAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            prepaymentAmount:
                null == prepaymentAmount
                    ? _value.prepaymentAmount
                    : prepaymentAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            dailyInstallmentAmount:
                null == dailyInstallmentAmount
                    ? _value.dailyInstallmentAmount
                    : dailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                        as double,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$OrderItemDetailsImplCopyWith<$Res>
    implements $OrderItemDetailsCopyWith<$Res> {
  factory _$$OrderItemDetailsImplCopyWith(
    _$OrderItemDetailsImpl value,
    $Res Function(_$OrderItemDetailsImpl) then,
  ) = __$$OrderItemDetailsImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int? id,
    int? productId,
    String? productName,
    int productType,
    int quantity,
    double buyAmount,
    double sellAmount,
    double prepaymentAmount,
    double dailyInstallmentAmount,
  });
}

/// @nodoc
class __$$OrderItemDetailsImplCopyWithImpl<$Res>
    extends _$OrderItemDetailsCopyWithImpl<$Res, _$OrderItemDetailsImpl>
    implements _$$OrderItemDetailsImplCopyWith<$Res> {
  __$$OrderItemDetailsImplCopyWithImpl(
    _$OrderItemDetailsImpl _value,
    $Res Function(_$OrderItemDetailsImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of OrderItemDetails
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? productId = freezed,
    Object? productName = freezed,
    Object? productType = null,
    Object? quantity = null,
    Object? buyAmount = null,
    Object? sellAmount = null,
    Object? prepaymentAmount = null,
    Object? dailyInstallmentAmount = null,
  }) {
    return _then(
      _$OrderItemDetailsImpl(
        id:
            freezed == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int?,
        productId:
            freezed == productId
                ? _value.productId
                : productId // ignore: cast_nullable_to_non_nullable
                    as int?,
        productName:
            freezed == productName
                ? _value.productName
                : productName // ignore: cast_nullable_to_non_nullable
                    as String?,
        productType:
            null == productType
                ? _value.productType
                : productType // ignore: cast_nullable_to_non_nullable
                    as int,
        quantity:
            null == quantity
                ? _value.quantity
                : quantity // ignore: cast_nullable_to_non_nullable
                    as int,
        buyAmount:
            null == buyAmount
                ? _value.buyAmount
                : buyAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        sellAmount:
            null == sellAmount
                ? _value.sellAmount
                : sellAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        prepaymentAmount:
            null == prepaymentAmount
                ? _value.prepaymentAmount
                : prepaymentAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        dailyInstallmentAmount:
            null == dailyInstallmentAmount
                ? _value.dailyInstallmentAmount
                : dailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                    as double,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$OrderItemDetailsImpl implements _OrderItemDetails {
  const _$OrderItemDetailsImpl({
    this.id,
    this.productId,
    this.productName,
    required this.productType,
    required this.quantity,
    required this.buyAmount,
    required this.sellAmount,
    required this.prepaymentAmount,
    required this.dailyInstallmentAmount,
  });

  factory _$OrderItemDetailsImpl.fromJson(Map<String, dynamic> json) =>
      _$$OrderItemDetailsImplFromJson(json);

  @override
  final int? id;
  @override
  final int? productId;
  @override
  final String? productName;
  @override
  final int productType;
  @override
  final int quantity;
  @override
  final double buyAmount;
  @override
  final double sellAmount;
  @override
  final double prepaymentAmount;
  @override
  final double dailyInstallmentAmount;

  @override
  String toString() {
    return 'OrderItemDetails(id: $id, productId: $productId, productName: $productName, productType: $productType, quantity: $quantity, buyAmount: $buyAmount, sellAmount: $sellAmount, prepaymentAmount: $prepaymentAmount, dailyInstallmentAmount: $dailyInstallmentAmount)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$OrderItemDetailsImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.productId, productId) ||
                other.productId == productId) &&
            (identical(other.productName, productName) ||
                other.productName == productName) &&
            (identical(other.productType, productType) ||
                other.productType == productType) &&
            (identical(other.quantity, quantity) ||
                other.quantity == quantity) &&
            (identical(other.buyAmount, buyAmount) ||
                other.buyAmount == buyAmount) &&
            (identical(other.sellAmount, sellAmount) ||
                other.sellAmount == sellAmount) &&
            (identical(other.prepaymentAmount, prepaymentAmount) ||
                other.prepaymentAmount == prepaymentAmount) &&
            (identical(other.dailyInstallmentAmount, dailyInstallmentAmount) ||
                other.dailyInstallmentAmount == dailyInstallmentAmount));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    productId,
    productName,
    productType,
    quantity,
    buyAmount,
    sellAmount,
    prepaymentAmount,
    dailyInstallmentAmount,
  );

  /// Create a copy of OrderItemDetails
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$OrderItemDetailsImplCopyWith<_$OrderItemDetailsImpl> get copyWith =>
      __$$OrderItemDetailsImplCopyWithImpl<_$OrderItemDetailsImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$OrderItemDetailsImplToJson(this);
  }
}

abstract class _OrderItemDetails implements OrderItemDetails {
  const factory _OrderItemDetails({
    final int? id,
    final int? productId,
    final String? productName,
    required final int productType,
    required final int quantity,
    required final double buyAmount,
    required final double sellAmount,
    required final double prepaymentAmount,
    required final double dailyInstallmentAmount,
  }) = _$OrderItemDetailsImpl;

  factory _OrderItemDetails.fromJson(Map<String, dynamic> json) =
      _$OrderItemDetailsImpl.fromJson;

  @override
  int? get id;
  @override
  int? get productId;
  @override
  String? get productName;
  @override
  int get productType;
  @override
  int get quantity;
  @override
  double get buyAmount;
  @override
  double get sellAmount;
  @override
  double get prepaymentAmount;
  @override
  double get dailyInstallmentAmount;

  /// Create a copy of OrderItemDetails
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$OrderItemDetailsImplCopyWith<_$OrderItemDetailsImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
