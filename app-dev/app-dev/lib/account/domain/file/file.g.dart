// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'file.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$FileImpl _$$FileImplFromJson(Map<String, dynamic> json) => _$FileImpl(
  id: (json['id'] as num).toInt(),
  originalFileName: json['originalFileName'] as String,
  relativePath: json['relativePath'] as String,
  fileSizeInByte: (json['fileSizeInByte'] as num).toInt(),
  type: (json['type'] as num).toInt(),
);

Map<String, dynamic> _$$FileImplToJson(_$FileImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'originalFileName': instance.originalFileName,
      'relativePath': instance.relativePath,
      'fileSizeInByte': instance.fileSizeInByte,
      'type': instance.type,
    };
