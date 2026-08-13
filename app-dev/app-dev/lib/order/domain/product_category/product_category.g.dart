// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'product_category.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ProductCategoryImpl _$$ProductCategoryImplFromJson(
  Map<String, dynamic> json,
) => _$ProductCategoryImpl(
  id: (json['id'] as num).toInt(),
  name: json['name'] as String,
  createdAt:
      json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$$ProductCategoryImplToJson(
  _$ProductCategoryImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'name': instance.name,
  'createdAt': instance.createdAt?.toIso8601String(),
};
