// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'product.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ProductImpl _$$ProductImplFromJson(
  Map<String, dynamic> json,
) => _$ProductImpl(
  id: (json['id'] as num).toInt(),
  name: json['name'] as String,
  buyAmount: (json['buyAmount'] as num).toDouble(),
  sellAmount: (json['sellAmount'] as num).toDouble(),
  dailyInstallmentAmount: (json['dailyInstallmentAmount'] as num).toDouble(),
  category: ProductCategory.fromJson(json['category'] as Map<String, dynamic>),
  createdAt: DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$$ProductImplToJson(_$ProductImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'buyAmount': instance.buyAmount,
      'sellAmount': instance.sellAmount,
      'dailyInstallmentAmount': instance.dailyInstallmentAmount,
      'category': instance.category,
      'createdAt': instance.createdAt.toIso8601String(),
    };
