// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'order_list_info.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$OrderListInfoImpl _$$OrderListInfoImplFromJson(Map<String, dynamic> json) =>
    _$OrderListInfoImpl(
      id: (json['id'] as num).toInt(),
      name: json['name'] as String,
      mandobFullName: json['mandobFullName'] as String?,
    );

Map<String, dynamic> _$$OrderListInfoImplToJson(_$OrderListInfoImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'mandobFullName': instance.mandobFullName,
    };
