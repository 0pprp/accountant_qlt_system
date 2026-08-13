// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_version_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$AppVersionResponseImpl _$$AppVersionResponseImplFromJson(
  Map<String, dynamic> json,
) => _$AppVersionResponseImpl(
  shouldUpdate: json['shouldUpdate'] as bool,
  isForce: json['isForce'] as bool,
  url: json['url'] as String,
);

Map<String, dynamic> _$$AppVersionResponseImplToJson(
  _$AppVersionResponseImpl instance,
) => <String, dynamic>{
  'shouldUpdate': instance.shouldUpdate,
  'isForce': instance.isForce,
  'url': instance.url,
};
