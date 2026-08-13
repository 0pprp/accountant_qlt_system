// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'order_details.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

OrderDetails _$OrderDetailsFromJson(Map<String, dynamic> json) {
  return _OrderDetails.fromJson(json);
}

/// @nodoc
mixin _$OrderDetails {
  int get id => throw _privateConstructorUsedError;
  double get sellAmount => throw _privateConstructorUsedError;
  double get paidAmount => throw _privateConstructorUsedError;
  double get overdueAmount => throw _privateConstructorUsedError;
  double? get dailyInstallmentAmount => throw _privateConstructorUsedError;
  String? get creationAddress => throw _privateConstructorUsedError;
  double? get unpaidAmount => throw _privateConstructorUsedError;
  double? get buyAmount => throw _privateConstructorUsedError;
  DateTime get createdAt => throw _privateConstructorUsedError;
  @SaleDateConverter()
  DateTime? get saleDate => throw _privateConstructorUsedError;
  @SaleTimeConverter()
  TimeOfDay? get saleTime => throw _privateConstructorUsedError;
  OrderStep? get step => throw _privateConstructorUsedError;
  @JsonKey(includeToJson: false)
  ExecutionStatus? get executionStatus => throw _privateConstructorUsedError;
  @JsonKey(includeToJson: false)
  ApprovalStatus? get approvalStatus => throw _privateConstructorUsedError;
  List<OrderItemDetails>? get orderItems => throw _privateConstructorUsedError;
  List<AttachmentInformation>? get attachments =>
      throw _privateConstructorUsedError;

  /// Serializes this OrderDetails to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of OrderDetails
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $OrderDetailsCopyWith<OrderDetails> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $OrderDetailsCopyWith<$Res> {
  factory $OrderDetailsCopyWith(
    OrderDetails value,
    $Res Function(OrderDetails) then,
  ) = _$OrderDetailsCopyWithImpl<$Res, OrderDetails>;
  @useResult
  $Res call({
    int id,
    double sellAmount,
    double paidAmount,
    double overdueAmount,
    double? dailyInstallmentAmount,
    String? creationAddress,
    double? unpaidAmount,
    double? buyAmount,
    DateTime createdAt,
    @SaleDateConverter() DateTime? saleDate,
    @SaleTimeConverter() TimeOfDay? saleTime,
    OrderStep? step,
    @JsonKey(includeToJson: false) ExecutionStatus? executionStatus,
    @JsonKey(includeToJson: false) ApprovalStatus? approvalStatus,
    List<OrderItemDetails>? orderItems,
    List<AttachmentInformation>? attachments,
  });
}

/// @nodoc
class _$OrderDetailsCopyWithImpl<$Res, $Val extends OrderDetails>
    implements $OrderDetailsCopyWith<$Res> {
  _$OrderDetailsCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of OrderDetails
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? sellAmount = null,
    Object? paidAmount = null,
    Object? overdueAmount = null,
    Object? dailyInstallmentAmount = freezed,
    Object? creationAddress = freezed,
    Object? unpaidAmount = freezed,
    Object? buyAmount = freezed,
    Object? createdAt = null,
    Object? saleDate = freezed,
    Object? saleTime = freezed,
    Object? step = freezed,
    Object? executionStatus = freezed,
    Object? approvalStatus = freezed,
    Object? orderItems = freezed,
    Object? attachments = freezed,
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
                        as double,
            paidAmount:
                null == paidAmount
                    ? _value.paidAmount
                    : paidAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            overdueAmount:
                null == overdueAmount
                    ? _value.overdueAmount
                    : overdueAmount // ignore: cast_nullable_to_non_nullable
                        as double,
            dailyInstallmentAmount:
                freezed == dailyInstallmentAmount
                    ? _value.dailyInstallmentAmount
                    : dailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                        as double?,
            creationAddress:
                freezed == creationAddress
                    ? _value.creationAddress
                    : creationAddress // ignore: cast_nullable_to_non_nullable
                        as String?,
            unpaidAmount:
                freezed == unpaidAmount
                    ? _value.unpaidAmount
                    : unpaidAmount // ignore: cast_nullable_to_non_nullable
                        as double?,
            buyAmount:
                freezed == buyAmount
                    ? _value.buyAmount
                    : buyAmount // ignore: cast_nullable_to_non_nullable
                        as double?,
            createdAt:
                null == createdAt
                    ? _value.createdAt
                    : createdAt // ignore: cast_nullable_to_non_nullable
                        as DateTime,
            saleDate:
                freezed == saleDate
                    ? _value.saleDate
                    : saleDate // ignore: cast_nullable_to_non_nullable
                        as DateTime?,
            saleTime:
                freezed == saleTime
                    ? _value.saleTime
                    : saleTime // ignore: cast_nullable_to_non_nullable
                        as TimeOfDay?,
            step:
                freezed == step
                    ? _value.step
                    : step // ignore: cast_nullable_to_non_nullable
                        as OrderStep?,
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
            orderItems:
                freezed == orderItems
                    ? _value.orderItems
                    : orderItems // ignore: cast_nullable_to_non_nullable
                        as List<OrderItemDetails>?,
            attachments:
                freezed == attachments
                    ? _value.attachments
                    : attachments // ignore: cast_nullable_to_non_nullable
                        as List<AttachmentInformation>?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$OrderDetailsImplCopyWith<$Res>
    implements $OrderDetailsCopyWith<$Res> {
  factory _$$OrderDetailsImplCopyWith(
    _$OrderDetailsImpl value,
    $Res Function(_$OrderDetailsImpl) then,
  ) = __$$OrderDetailsImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int id,
    double sellAmount,
    double paidAmount,
    double overdueAmount,
    double? dailyInstallmentAmount,
    String? creationAddress,
    double? unpaidAmount,
    double? buyAmount,
    DateTime createdAt,
    @SaleDateConverter() DateTime? saleDate,
    @SaleTimeConverter() TimeOfDay? saleTime,
    OrderStep? step,
    @JsonKey(includeToJson: false) ExecutionStatus? executionStatus,
    @JsonKey(includeToJson: false) ApprovalStatus? approvalStatus,
    List<OrderItemDetails>? orderItems,
    List<AttachmentInformation>? attachments,
  });
}

/// @nodoc
class __$$OrderDetailsImplCopyWithImpl<$Res>
    extends _$OrderDetailsCopyWithImpl<$Res, _$OrderDetailsImpl>
    implements _$$OrderDetailsImplCopyWith<$Res> {
  __$$OrderDetailsImplCopyWithImpl(
    _$OrderDetailsImpl _value,
    $Res Function(_$OrderDetailsImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of OrderDetails
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? sellAmount = null,
    Object? paidAmount = null,
    Object? overdueAmount = null,
    Object? dailyInstallmentAmount = freezed,
    Object? creationAddress = freezed,
    Object? unpaidAmount = freezed,
    Object? buyAmount = freezed,
    Object? createdAt = null,
    Object? saleDate = freezed,
    Object? saleTime = freezed,
    Object? step = freezed,
    Object? executionStatus = freezed,
    Object? approvalStatus = freezed,
    Object? orderItems = freezed,
    Object? attachments = freezed,
  }) {
    return _then(
      _$OrderDetailsImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        sellAmount:
            null == sellAmount
                ? _value.sellAmount
                : sellAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        paidAmount:
            null == paidAmount
                ? _value.paidAmount
                : paidAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        overdueAmount:
            null == overdueAmount
                ? _value.overdueAmount
                : overdueAmount // ignore: cast_nullable_to_non_nullable
                    as double,
        dailyInstallmentAmount:
            freezed == dailyInstallmentAmount
                ? _value.dailyInstallmentAmount
                : dailyInstallmentAmount // ignore: cast_nullable_to_non_nullable
                    as double?,
        creationAddress:
            freezed == creationAddress
                ? _value.creationAddress
                : creationAddress // ignore: cast_nullable_to_non_nullable
                    as String?,
        unpaidAmount:
            freezed == unpaidAmount
                ? _value.unpaidAmount
                : unpaidAmount // ignore: cast_nullable_to_non_nullable
                    as double?,
        buyAmount:
            freezed == buyAmount
                ? _value.buyAmount
                : buyAmount // ignore: cast_nullable_to_non_nullable
                    as double?,
        createdAt:
            null == createdAt
                ? _value.createdAt
                : createdAt // ignore: cast_nullable_to_non_nullable
                    as DateTime,
        saleDate:
            freezed == saleDate
                ? _value.saleDate
                : saleDate // ignore: cast_nullable_to_non_nullable
                    as DateTime?,
        saleTime:
            freezed == saleTime
                ? _value.saleTime
                : saleTime // ignore: cast_nullable_to_non_nullable
                    as TimeOfDay?,
        step:
            freezed == step
                ? _value.step
                : step // ignore: cast_nullable_to_non_nullable
                    as OrderStep?,
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
        orderItems:
            freezed == orderItems
                ? _value._orderItems
                : orderItems // ignore: cast_nullable_to_non_nullable
                    as List<OrderItemDetails>?,
        attachments:
            freezed == attachments
                ? _value._attachments
                : attachments // ignore: cast_nullable_to_non_nullable
                    as List<AttachmentInformation>?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$OrderDetailsImpl implements _OrderDetails {
  const _$OrderDetailsImpl({
    required this.id,
    required this.sellAmount,
    required this.paidAmount,
    required this.overdueAmount,
    this.dailyInstallmentAmount,
    this.creationAddress,
    this.unpaidAmount,
    this.buyAmount,
    required this.createdAt,
    @SaleDateConverter() this.saleDate,
    @SaleTimeConverter() this.saleTime,
    this.step,
    @JsonKey(includeToJson: false) this.executionStatus,
    @JsonKey(includeToJson: false) this.approvalStatus,
    final List<OrderItemDetails>? orderItems,
    final List<AttachmentInformation>? attachments,
  }) : _orderItems = orderItems,
       _attachments = attachments;

  factory _$OrderDetailsImpl.fromJson(Map<String, dynamic> json) =>
      _$$OrderDetailsImplFromJson(json);

  @override
  final int id;
  @override
  final double sellAmount;
  @override
  final double paidAmount;
  @override
  final double overdueAmount;
  @override
  final double? dailyInstallmentAmount;
  @override
  final String? creationAddress;
  @override
  final double? unpaidAmount;
  @override
  final double? buyAmount;
  @override
  final DateTime createdAt;
  @override
  @SaleDateConverter()
  final DateTime? saleDate;
  @override
  @SaleTimeConverter()
  final TimeOfDay? saleTime;
  @override
  final OrderStep? step;
  @override
  @JsonKey(includeToJson: false)
  final ExecutionStatus? executionStatus;
  @override
  @JsonKey(includeToJson: false)
  final ApprovalStatus? approvalStatus;
  final List<OrderItemDetails>? _orderItems;
  @override
  List<OrderItemDetails>? get orderItems {
    final value = _orderItems;
    if (value == null) return null;
    if (_orderItems is EqualUnmodifiableListView) return _orderItems;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(value);
  }

  final List<AttachmentInformation>? _attachments;
  @override
  List<AttachmentInformation>? get attachments {
    final value = _attachments;
    if (value == null) return null;
    if (_attachments is EqualUnmodifiableListView) return _attachments;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(value);
  }

  @override
  String toString() {
    return 'OrderDetails(id: $id, sellAmount: $sellAmount, paidAmount: $paidAmount, overdueAmount: $overdueAmount, dailyInstallmentAmount: $dailyInstallmentAmount, creationAddress: $creationAddress, unpaidAmount: $unpaidAmount, buyAmount: $buyAmount, createdAt: $createdAt, saleDate: $saleDate, saleTime: $saleTime, step: $step, executionStatus: $executionStatus, approvalStatus: $approvalStatus, orderItems: $orderItems, attachments: $attachments)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$OrderDetailsImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.sellAmount, sellAmount) ||
                other.sellAmount == sellAmount) &&
            (identical(other.paidAmount, paidAmount) ||
                other.paidAmount == paidAmount) &&
            (identical(other.overdueAmount, overdueAmount) ||
                other.overdueAmount == overdueAmount) &&
            (identical(other.dailyInstallmentAmount, dailyInstallmentAmount) ||
                other.dailyInstallmentAmount == dailyInstallmentAmount) &&
            (identical(other.creationAddress, creationAddress) ||
                other.creationAddress == creationAddress) &&
            (identical(other.unpaidAmount, unpaidAmount) ||
                other.unpaidAmount == unpaidAmount) &&
            (identical(other.buyAmount, buyAmount) ||
                other.buyAmount == buyAmount) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt) &&
            (identical(other.saleDate, saleDate) ||
                other.saleDate == saleDate) &&
            (identical(other.saleTime, saleTime) ||
                other.saleTime == saleTime) &&
            (identical(other.step, step) || other.step == step) &&
            (identical(other.executionStatus, executionStatus) ||
                other.executionStatus == executionStatus) &&
            (identical(other.approvalStatus, approvalStatus) ||
                other.approvalStatus == approvalStatus) &&
            const DeepCollectionEquality().equals(
              other._orderItems,
              _orderItems,
            ) &&
            const DeepCollectionEquality().equals(
              other._attachments,
              _attachments,
            ));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    sellAmount,
    paidAmount,
    overdueAmount,
    dailyInstallmentAmount,
    creationAddress,
    unpaidAmount,
    buyAmount,
    createdAt,
    saleDate,
    saleTime,
    step,
    executionStatus,
    approvalStatus,
    const DeepCollectionEquality().hash(_orderItems),
    const DeepCollectionEquality().hash(_attachments),
  );

  /// Create a copy of OrderDetails
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$OrderDetailsImplCopyWith<_$OrderDetailsImpl> get copyWith =>
      __$$OrderDetailsImplCopyWithImpl<_$OrderDetailsImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$OrderDetailsImplToJson(this);
  }
}

abstract class _OrderDetails implements OrderDetails {
  const factory _OrderDetails({
    required final int id,
    required final double sellAmount,
    required final double paidAmount,
    required final double overdueAmount,
    final double? dailyInstallmentAmount,
    final String? creationAddress,
    final double? unpaidAmount,
    final double? buyAmount,
    required final DateTime createdAt,
    @SaleDateConverter() final DateTime? saleDate,
    @SaleTimeConverter() final TimeOfDay? saleTime,
    final OrderStep? step,
    @JsonKey(includeToJson: false) final ExecutionStatus? executionStatus,
    @JsonKey(includeToJson: false) final ApprovalStatus? approvalStatus,
    final List<OrderItemDetails>? orderItems,
    final List<AttachmentInformation>? attachments,
  }) = _$OrderDetailsImpl;

  factory _OrderDetails.fromJson(Map<String, dynamic> json) =
      _$OrderDetailsImpl.fromJson;

  @override
  int get id;
  @override
  double get sellAmount;
  @override
  double get paidAmount;
  @override
  double get overdueAmount;
  @override
  double? get dailyInstallmentAmount;
  @override
  String? get creationAddress;
  @override
  double? get unpaidAmount;
  @override
  double? get buyAmount;
  @override
  DateTime get createdAt;
  @override
  @SaleDateConverter()
  DateTime? get saleDate;
  @override
  @SaleTimeConverter()
  TimeOfDay? get saleTime;
  @override
  OrderStep? get step;
  @override
  @JsonKey(includeToJson: false)
  ExecutionStatus? get executionStatus;
  @override
  @JsonKey(includeToJson: false)
  ApprovalStatus? get approvalStatus;
  @override
  List<OrderItemDetails>? get orderItems;
  @override
  List<AttachmentInformation>? get attachments;

  /// Create a copy of OrderDetails
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$OrderDetailsImplCopyWith<_$OrderDetailsImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
