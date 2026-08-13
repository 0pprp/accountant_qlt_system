// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'order_item_details.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$OrderItemDetailsImpl _$$OrderItemDetailsImplFromJson(
  Map<String, dynamic> json,
) => _$OrderItemDetailsImpl(
  id: (json['id'] as num?)?.toInt(),
  productId: (json['productId'] as num?)?.toInt(),
  productName: json['productName'] as String?,
  productType: (json['productType'] as num).toInt(),
  quantity: (json['quantity'] as num).toInt(),
  buyAmount: (json['buyAmount'] as num).toDouble(),
  sellAmount: (json['sellAmount'] as num).toDouble(),
  prepaymentAmount: (json['prepaymentAmount'] as num).toDouble(),
  dailyInstallmentAmount: (json['dailyInstallmentAmount'] as num).toDouble(),
);

Map<String, dynamic> _$$OrderItemDetailsImplToJson(
  _$OrderItemDetailsImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'productId': instance.productId,
  'productName': instance.productName,
  'productType': instance.productType,
  'quantity': instance.quantity,
  'buyAmount': instance.buyAmount,
  'sellAmount': instance.sellAmount,
  'prepaymentAmount': instance.prepaymentAmount,
  'dailyInstallmentAmount': instance.dailyInstallmentAmount,
};
