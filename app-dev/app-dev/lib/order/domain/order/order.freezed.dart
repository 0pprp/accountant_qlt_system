// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'order.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

Order _$OrderFromJson(Map<String, dynamic> json) {
  return _Order.fromJson(json);
}

/// @nodoc
mixin _$Order {
  int get id => throw _privateConstructorUsedError;
  int get sellAmount => throw _privateConstructorUsedError;
  int get paidAmount => throw _privateConstructorUsedError;
  int get dailyInstallmentAmount => throw _privateConstructorUsedError;
  DateTime get createdAt => throw _privateConstructorUsedError;
  String? get creationAddress => throw _privateConstructorUsedError;
  int? get orderListId => throw _privateConstructorUsedError;
  Customer get customer =>
      throw _privateConstructorUsedError; // final InstallmentPaymentInfo? installmentPayment,
  int? get installmentPaymentId => throw _privateConstructorUsedError;
  List<OrderItem> get orderItems => throw _privateConstructorUsedError;
  ExecutionStatus? get executionStatus => throw _privateConstructorUsedError;
  ApprovalStatus? get approvalStatus => throw _privateConstructorUsedError;
  OrderStep? get step => throw _privateConstructorUsedError;
  @JsonKey(includeFromJson: false, includeToJson: false, defaultValue: false)
  bool? get isCollectedOffline => throw _privateConstructorUsedError;

  /// Serializes this Order to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of Order
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $OrderCopyWith<Order> get copyWith => throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $OrderCopyWith<$Res> {
  factory $OrderCopyWith(Order value, $Res Function(Order) then) =
      _$OrderCopyWithImpl<$Res, Order>;
  @useResult
  $Res call({
    int id,
    int sellAmount,
    int paidAmount,
    int dailyInstallmentAmount,
    DateTime createdAt,
    String? creationAddress,
    int? orderListId,
    Customer customer,
    int? installmentPaymentId,
    List<OrderItem> orderItems,
    ExecutionStatus? executionStatus,
    ApprovalStatus? approvalStatus,
    OrderStep? step,
    @JsonKey(includeFromJson: false, includeToJson: false, defaultValue: false)
    bool? isCollectedOffline,
  });

  $CustomerCopyWith<$Res> get customer;
}

/// @nodoc
class _$OrderCopyWithImpl<$Res, $Val extends Order>
    implements $OrderCopyWith<$Res> {
  _$OrderCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of Order
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? sellAmount = null,
    Object? paidAmount = null,
    Object? dailyInstallmentAmount = null,
    Object? createdAt = null,
    Object? creationAddress = freezed,
    Object? orderListId = freezed,
    Object? customer = null,
    Object? installmentPaymentId = freezed,
    Object? orderItems = null,
    Object? executionStatus = freezed,
    Object? approvalStatus = freezed,
    Object? step = freezed,
    Object? isCollectedOffline = freezed,
  }) {
    return _then(
      _value.copyWith(
            id:
                null == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int,
            sellAmount:
                null == sellAmount
                    ? _value.sellAmount
                    : sellAmount // ignore: cast_nullable_to_non_nullable
                        as int,
            paidAmount:
                null == paidAmount
                    ? _value.paidAmount
                    : paidAmount // ignore: cast_nullable_to_non_nullable
                        as int,
            dailyInstallmentAmount:
                null == dailyInstallmentAmount
                    ? _value.dailyInstallmentAmount
                    : dailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                        as int,
            createdAt:
                null == createdAt
                    ? _value.createdAt
                    : createdAt // ignore: cast_nullable_to_non_nullable
                        as DateTime,
            creationAddress:
                freezed == creationAddress
                    ? _value.creationAddress
                    : creationAddress // ignore: cast_nullable_to_non_nullable
                        as String?,
            orderListId:
                freezed == orderListId
                    ? _value.orderListId
                    : orderListId // ignore: cast_nullable_to_non_nullable
                        as int?,
            customer:
                null == customer
                    ? _value.customer
                    : customer // ignore: cast_nullable_to_non_nullable
                        as Customer,
            installmentPaymentId:
                freezed == installmentPaymentId
                    ? _value.installmentPaymentId
                    : installmentPaymentId // ignore: cast_nullable_to_non_nullable
                        as int?,
            orderItems:
                null == orderItems
                    ? _value.orderItems
                    : orderItems // ignore: cast_nullable_to_non_nullable
                        as List<OrderItem>,
            executionStatus:
                freezed == executionStatus
                    ? _value.executionStatus
                    : executionStatus // ignore: cast_nullable_to_non_nullable
                        as ExecutionStatus?,
            approvalStatus:
                freezed == approvalStatus
                    ? _value.approvalStatus
                    : approvalStatus // ignore: cast_nullable_to_non_nullable
                        as ApprovalStatus?,
            step:
                freezed == step
                    ? _value.step
                    : step // ignore: cast_nullable_to_non_nullable
                        as OrderStep?,
            isCollectedOffline:
                freezed == isCollectedOffline
                    ? _value.isCollectedOffline
                    : isCollectedOffline // ignore: cast_nullable_to_non_nullable
                        as bool?,
          )
          as $Val,
    );
  }

  /// Create a copy of Order
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $CustomerCopyWith<$Res> get customer {
    return $CustomerCopyWith<$Res>(_value.customer, (value) {
      return _then(_value.copyWith(customer: value) as $Val);
    });
  }
}

/// @nodoc
abstract class _$$OrderImplCopyWith<$Res> implements $OrderCopyWith<$Res> {
  factory _$$OrderImplCopyWith(
    _$OrderImpl value,
    $Res Function(_$OrderImpl) then,
  ) = __$$OrderImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int id,
    int sellAmount,
    int paidAmount,
    int dailyInstallmentAmount,
    DateTime createdAt,
    String? creationAddress,
    int? orderListId,
    Customer customer,
    int? installmentPaymentId,
    List<OrderItem> orderItems,
    ExecutionStatus? executionStatus,
    ApprovalStatus? approvalStatus,
    OrderStep? step,
    @JsonKey(includeFromJson: false, includeToJson: false, defaultValue: false)
    bool? isCollectedOffline,
  });

  @override
  $CustomerCopyWith<$Res> get customer;
}

/// @nodoc
class __$$OrderImplCopyWithImpl<$Res>
    extends _$OrderCopyWithImpl<$Res, _$OrderImpl>
    implements _$$OrderImplCopyWith<$Res> {
  __$$OrderImplCopyWithImpl(
    _$OrderImpl _value,
    $Res Function(_$OrderImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of Order
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? sellAmount = null,
    Object? paidAmount = null,
    Object? dailyInstallmentAmount = null,
    Object? createdAt = null,
    Object? creationAddress = freezed,
    Object? orderListId = freezed,
    Object? customer = null,
    Object? installmentPaymentId = freezed,
    Object? orderItems = null,
    Object? executionStatus = freezed,
    Object? approvalStatus = freezed,
    Object? step = freezed,
    Object? isCollectedOffline = freezed,
  }) {
    return _then(
      _$OrderImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        sellAmount:
            null == sellAmount
                ? _value.sellAmount
                : sellAmount // ignore: cast_nullable_to_non_nullable
                    as int,
        paidAmount:
            null == paidAmount
                ? _value.paidAmount
                : paidAmount // ignore: cast_nullable_to_non_nullable
                    as int,
        dailyInstallmentAmount:
            null == dailyInstallmentAmount
                ? _value.dailyInstallmentAmount
                : dailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                    as int,
        createdAt:
            null == createdAt
                ? _value.createdAt
                : createdAt // ignore: cast_nullable_to_non_nullable
                    as DateTime,
        creationAddress:
            freezed == creationAddress
                ? _value.creationAddress
                : creationAddress // ignore: cast_nullable_to_non_nullable
                    as String?,
        orderListId:
            freezed == orderListId
                ? _value.orderListId
                : orderListId // ignore: cast_nullable_to_non_nullable
                    as int?,
        customer:
            null == customer
                ? _value.customer
                : customer // ignore: cast_nullable_to_non_nullable
                    as Customer,
        installmentPaymentId:
            freezed == installmentPaymentId
                ? _value.installmentPaymentId
                : installmentPaymentId // ignore: cast_nullable_to_non_nullable
                    as int?,
        orderItems:
            null == orderItems
                ? _value._orderItems
                : orderItems // ignore: cast_nullable_to_non_nullable
                    as List<OrderItem>,
        executionStatus:
            freezed == executionStatus
                ? _value.executionStatus
                : executionStatus // ignore: cast_nullable_to_non_nullable
                    as ExecutionStatus?,
        approvalStatus:
            freezed == approvalStatus
                ? _value.approvalStatus
                : approvalStatus // ignore: cast_nullable_to_non_nullable
                    as ApprovalStatus?,
        step:
            freezed == step
                ? _value.step
                : step // ignore: cast_nullable_to_non_nullable
                    as OrderStep?,
        isCollectedOffline:
            freezed == isCollectedOffline
                ? _value.isCollectedOffline
                : isCollectedOffline // ignore: cast_nullable_to_non_nullable
                    as bool?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$OrderImpl extends _Order {
  const _$OrderImpl({
    required this.id,
    required this.sellAmount,
    required this.paidAmount,
    required this.dailyInstallmentAmount,
    required this.createdAt,
    this.creationAddress,
    this.orderListId,
    required this.customer,
    this.installmentPaymentId,
    required final List<OrderItem> orderItems,
    this.executionStatus,
    this.approvalStatus,
    this.step,
    @JsonKey(includeFromJson: false, includeToJson: false, defaultValue: false)
    this.isCollectedOffline,
  }) : _orderItems = orderItems,
       super._();

  factory _$OrderImpl.fromJson(Map<String, dynamic> json) =>
      _$$OrderImplFromJson(json);

  @override
  final int id;
  @override
  final int sellAmount;
  @override
  final int paidAmount;
  @override
  final int dailyInstallmentAmount;
  @override
  final DateTime createdAt;
  @override
  final String? creationAddress;
  @override
  final int? orderListId;
  @override
  final Customer customer;
  // final InstallmentPaymentInfo? installmentPayment,
  @override
  final int? installmentPaymentId;
  final List<OrderItem> _orderItems;
  @override
  List<OrderItem> get orderItems {
    if (_orderItems is EqualUnmodifiableListView) return _orderItems;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_orderItems);
  }

  @override
  final ExecutionStatus? executionStatus;
  @override
  final ApprovalStatus? approvalStatus;
  @override
  final OrderStep? step;
  @override
  @JsonKey(includeFromJson: false, includeToJson: false, defaultValue: false)
  final bool? isCollectedOffline;

  @override
  String toString() {
    return 'Order(id: $id, sellAmount: $sellAmount, paidAmount: $paidAmount, dailyInstallmentAmount: $dailyInstallmentAmount, createdAt: $createdAt, creationAddress: $creationAddress, orderListId: $orderListId, customer: $customer, installmentPaymentId: $installmentPaymentId, orderItems: $orderItems, executionStatus: $executionStatus, approvalStatus: $approvalStatus, step: $step, isCollectedOffline: $isCollectedOffline)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$OrderImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.sellAmount, sellAmount) ||
                other.sellAmount == sellAmount) &&
            (identical(other.paidAmount, paidAmount) ||
                other.paidAmount == paidAmount) &&
            (identical(other.dailyInstallmentAmount, dailyInstallmentAmount) ||
                other.dailyInstallmentAmount == dailyInstallmentAmount) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt) &&
            (identical(other.creationAddress, creationAddress) ||
                other.creationAddress == creationAddress) &&
            (identical(other.orderListId, orderListId) ||
                other.orderListId == orderListId) &&
            (identical(other.customer, customer) ||
                other.customer == customer) &&
            (identical(other.installmentPaymentId, installmentPaymentId) ||
                other.installmentPaymentId == installmentPaymentId) &&
            const DeepCollectionEquality().equals(
              other._orderItems,
              _orderItems,
            ) &&
            (identical(other.executionStatus, executionStatus) ||
                other.executionStatus == executionStatus) &&
            (identical(other.approvalStatus, approvalStatus) ||
                other.approvalStatus == approvalStatus) &&
            (identical(other.step, step) || other.step == step) &&
            (identical(other.isCollectedOffline, isCollectedOffline) ||
                other.isCollectedOffline == isCollectedOffline));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    sellAmount,
    paidAmount,
    dailyInstallmentAmount,
    createdAt,
    creationAddress,
    orderListId,
    customer,
    installmentPaymentId,
    const DeepCollectionEquality().hash(_orderItems),
    executionStatus,
    approvalStatus,
    step,
    isCollectedOffline,
  );

  /// Create a copy of Order
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$OrderImplCopyWith<_$OrderImpl> get copyWith =>
      __$$OrderImplCopyWithImpl<_$OrderImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$OrderImplToJson(this);
  }
}

abstract class _Order extends Order {
  const factory _Order({
    required final int id,
    required final int sellAmount,
    required final int paidAmount,
    required final int dailyInstallmentAmount,
    required final DateTime createdAt,
    final String? creationAddress,
    final int? orderListId,
    required final Customer customer,
    final int? installmentPaymentId,
    required final List<OrderItem> orderItems,
    final ExecutionStatus? executionStatus,
    final ApprovalStatus? approvalStatus,
    final OrderStep? step,
    @JsonKey(includeFromJson: false, includeToJson: false, defaultValue: false)
    final bool? isCollectedOffline,
  }) = _$OrderImpl;
  const _Order._() : super._();

  factory _Order.fromJson(Map<String, dynamic> json) = _$OrderImpl.fromJson;

  @override
  int get id;
  @override
  int get sellAmount;
  @override
  int get paidAmount;
  @override
  int get dailyInstallmentAmount;
  @override
  DateTime get createdAt;
  @override
  String? get creationAddress;
  @override
  int? get orderListId;
  @override
  Customer get customer; // final InstallmentPaymentInfo? installmentPayment,
  @override
  int? get installmentPaymentId;
  @override
  List<OrderItem> get orderItems;
  @override
  ExecutionStatus? get executionStatus;
  @override
  ApprovalStatus? get approvalStatus;
  @override
  OrderStep? get step;
  @override
  @JsonKey(includeFromJson: false, includeToJson: false, defaultValue: false)
  bool? get isCollectedOffline;

  /// Create a copy of Order
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$OrderImplCopyWith<_$OrderImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

OrderItem _$OrderItemFromJson(Map<String, dynamic> json) {
  return _OrderItem.fromJson(json);
}

/// @nodoc
mixin _$OrderItem {
  int get id => throw _privateConstructorUsedError;
  String get productName => throw _privateConstructorUsedError;
  int get quantity => throw _privateConstructorUsedError;
  int get sellAmount => throw _privateConstructorUsedError;

  /// Serializes this OrderItem to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of OrderItem
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $OrderItemCopyWith<OrderItem> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $OrderItemCopyWith<$Res> {
  factory $OrderItemCopyWith(OrderItem value, $Res Function(OrderItem) then) =
      _$OrderItemCopyWithImpl<$Res, OrderItem>;
  @useResult
  $Res call({int id, String productName, int quantity, int sellAmount});
}

/// @nodoc
class _$OrderItemCopyWithImpl<$Res, $Val extends OrderItem>
    implements $OrderItemCopyWith<$Res> {
  _$OrderItemCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of OrderItem
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? productName = null,
    Object? quantity = null,
    Object? sellAmount = null,
  }) {
    return _then(
      _value.copyWith(
            id:
                null == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int,
            productName:
                null == productName
                    ? _value.productName
                    : productName // ignore: cast_nullable_to_non_nullable
                        as String,
            quantity:
                null == quantity
                    ? _value.quantity
                    : quantity // ignore: cast_nullable_to_non_nullable
                        as int,
            sellAmount:
                null == sellAmount
                    ? _value.sellAmount
                    : sellAmount // ignore: cast_nullable_to_non_nullable
                        as int,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$OrderItemImplCopyWith<$Res>
    implements $OrderItemCopyWith<$Res> {
  factory _$$OrderItemImplCopyWith(
    _$OrderItemImpl value,
    $Res Function(_$OrderItemImpl) then,
  ) = __$$OrderItemImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int id, String productName, int quantity, int sellAmount});
}

/// @nodoc
class __$$OrderItemImplCopyWithImpl<$Res>
    extends _$OrderItemCopyWithImpl<$Res, _$OrderItemImpl>
    implements _$$OrderItemImplCopyWith<$Res> {
  __$$OrderItemImplCopyWithImpl(
    _$OrderItemImpl _value,
    $Res Function(_$OrderItemImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of OrderItem
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? productName = null,
    Object? quantity = null,
    Object? sellAmount = null,
  }) {
    return _then(
      _$OrderItemImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        productName:
            null == productName
                ? _value.productName
                : productName // ignore: cast_nullable_to_non_nullable
                    as String,
        quantity:
            null == quantity
                ? _value.quantity
                : quantity // ignore: cast_nullable_to_non_nullable
                    as int,
        sellAmount:
            null == sellAmount
                ? _value.sellAmount
                : sellAmount // ignore: cast_nullable_to_non_nullable
                    as int,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$OrderItemImpl implements _OrderItem {
  const _$OrderItemImpl({
    required this.id,
    required this.productName,
    required this.quantity,
    required this.sellAmount,
  });

  factory _$OrderItemImpl.fromJson(Map<String, dynamic> json) =>
      _$$OrderItemImplFromJson(json);

  @override
  final int id;
  @override
  final String productName;
  @override
  final int quantity;
  @override
  final int sellAmount;

  @override
  String toString() {
    return 'OrderItem(id: $id, productName: $productName, quantity: $quantity, sellAmount: $sellAmount)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$OrderItemImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.productName, productName) ||
                other.productName == productName) &&
            (identical(other.quantity, quantity) ||
                other.quantity == quantity) &&
            (identical(other.sellAmount, sellAmount) ||
                other.sellAmount == sellAmount));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, id, productName, quantity, sellAmount);

  /// Create a copy of OrderItem
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$OrderItemImplCopyWith<_$OrderItemImpl> get copyWith =>
      __$$OrderItemImplCopyWithImpl<_$OrderItemImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$OrderItemImplToJson(this);
  }
}

abstract class _OrderItem implements OrderItem {
  const factory _OrderItem({
    required final int id,
    required final String productName,
    required final int quantity,
    required final int sellAmount,
  }) = _$OrderItemImpl;

  factory _OrderItem.fromJson(Map<String, dynamic> json) =
      _$OrderItemImpl.fromJson;

  @override
  int get id;
  @override
  String get productName;
  @override
  int get quantity;
  @override
  int get sellAmount;

  /// Create a copy of OrderItem
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$OrderItemImplCopyWith<_$OrderItemImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

Customer _$CustomerFromJson(Map<String, dynamic> json) {
  return _Customer.fromJson(json);
}

/// @nodoc
mixin _$Customer {
  int get id => throw _privateConstructorUsedError;
  String get fullName => throw _privateConstructorUsedError;
  String get phoneNumber => throw _privateConstructorUsedError;

  /// Serializes this Customer to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of Customer
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CustomerCopyWith<Customer> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CustomerCopyWith<$Res> {
  factory $CustomerCopyWith(Customer value, $Res Function(Customer) then) =
      _$CustomerCopyWithImpl<$Res, Customer>;
  @useResult
  $Res call({int id, String fullName, String phoneNumber});
}

/// @nodoc
class _$CustomerCopyWithImpl<$Res, $Val extends Customer>
    implements $CustomerCopyWith<$Res> {
  _$CustomerCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of Customer
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? fullName = null,
    Object? phoneNumber = null,
  }) {
    return _then(
      _value.copyWith(
            id:
                null == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int,
            fullName:
                null == fullName
                    ? _value.fullName
                    : fullName // ignore: cast_nullable_to_non_nullable
                        as String,
            phoneNumber:
                null == phoneNumber
                    ? _value.phoneNumber
                    : phoneNumber // ignore: cast_nullable_to_non_nullable
                        as String,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CustomerImplCopyWith<$Res>
    implements $CustomerCopyWith<$Res> {
  factory _$$CustomerImplCopyWith(
    _$CustomerImpl value,
    $Res Function(_$CustomerImpl) then,
  ) = __$$CustomerImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int id, String fullName, String phoneNumber});
}

/// @nodoc
class __$$CustomerImplCopyWithImpl<$Res>
    extends _$CustomerCopyWithImpl<$Res, _$CustomerImpl>
    implements _$$CustomerImplCopyWith<$Res> {
  __$$CustomerImplCopyWithImpl(
    _$CustomerImpl _value,
    $Res Function(_$CustomerImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of Customer
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? fullName = null,
    Object? phoneNumber = null,
  }) {
    return _then(
      _$CustomerImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        fullName:
            null == fullName
                ? _value.fullName
                : fullName // ignore: cast_nullable_to_non_nullable
                    as String,
        phoneNumber:
            null == phoneNumber
                ? _value.phoneNumber
                : phoneNumber // ignore: cast_nullable_to_non_nullable
                    as String,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CustomerImpl implements _Customer {
  const _$CustomerImpl({
    required this.id,
    required this.fullName,
    required this.phoneNumber,
  });

  factory _$CustomerImpl.fromJson(Map<String, dynamic> json) =>
      _$$CustomerImplFromJson(json);

  @override
  final int id;
  @override
  final String fullName;
  @override
  final String phoneNumber;

  @override
  String toString() {
    return 'Customer(id: $id, fullName: $fullName, phoneNumber: $phoneNumber)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CustomerImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.fullName, fullName) ||
                other.fullName == fullName) &&
            (identical(other.phoneNumber, phoneNumber) ||
                other.phoneNumber == phoneNumber));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, id, fullName, phoneNumber);

  /// Create a copy of Customer
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CustomerImplCopyWith<_$CustomerImpl> get copyWith =>
      __$$CustomerImplCopyWithImpl<_$CustomerImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$CustomerImplToJson(this);
  }
}

abstract class _Customer implements Customer {
  const factory _Customer({
    required final int id,
    required final String fullName,
    required final String phoneNumber,
  }) = _$CustomerImpl;

  factory _Customer.fromJson(Map<String, dynamic> json) =
      _$CustomerImpl.fromJson;

  @override
  int get id;
  @override
  String get fullName;
  @override
  String get phoneNumber;

  /// Create a copy of Customer
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CustomerImplCopyWith<_$CustomerImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
