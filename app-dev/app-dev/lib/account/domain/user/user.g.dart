// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'user.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$UserImpl _$$UserImplFromJson(Map<String, dynamic> json) => _$UserImpl(
  id: (json['id'] as num).toInt(),
  fullName: json['fullName'] as String,
  motherName: json['motherName'] as String,
  nationalCode: json['nationalCode'] as String,
  birthDate: const DateOnlyConverter().fromJson(json['birthDate'] as String),
  phoneNumber: json['phoneNumber'] as String,
  whatsAppPhoneNumber: json['whatsAppPhoneNumber'] as String,
  business: BusinessInformation.fromJson(
    json['business'] as Map<String, dynamic>,
  ),
  branch: BranchInformation.fromJson(json['branch'] as Map<String, dynamic>),
  attachments:
      (json['attachments'] as List<dynamic>)
          .map((e) => AttachmentInformation.fromJson(e as Map<String, dynamic>))
          .toList(),
);

Map<String, dynamic> _$$UserImplToJson(_$UserImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'fullName': instance.fullName,
      'motherName': instance.motherName,
      'nationalCode': instance.nationalCode,
      'birthDate': const DateOnlyConverter().toJson(instance.birthDate),
      'phoneNumber': instance.phoneNumber,
      'whatsAppPhoneNumber': instance.whatsAppPhoneNumber,
      'business': instance.business,
      'branch': instance.branch,
      'attachments': instance.attachments,
    };

_$BusinessInformationImpl _$$BusinessInformationImplFromJson(
  Map<String, dynamic> json,
) => _$BusinessInformationImpl(
  name: json['name'] as String,
  address: json['address'] as String,
  nearestKnownLocation: json['nearestKnownLocation'] as String,
);

Map<String, dynamic> _$$BusinessInformationImplToJson(
  _$BusinessInformationImpl instance,
) => <String, dynamic>{
  'name': instance.name,
  'address': instance.address,
  'nearestKnownLocation': instance.nearestKnownLocation,
};

_$BranchInformationImpl _$$BranchInformationImplFromJson(
  Map<String, dynamic> json,
) => _$BranchInformationImpl(
  id: (json['id'] as num).toInt(),
  name: json['name'] as String,
);

Map<String, dynamic> _$$BranchInformationImplToJson(
  _$BranchInformationImpl instance,
) => <String, dynamic>{'id': instance.id, 'name': instance.name};

_$AttachmentInformationImpl _$$AttachmentInformationImplFromJson(
  Map<String, dynamic> json,
) => _$AttachmentInformationImpl(
  id: (json['id'] as num?)?.toInt(),
  originalFileName: json['originalFileName'] as String?,
  relativePath: json['relativePath'] as String?,
  type: $enumDecode(_$AttachmentTypeEnumMap, json['type']),
);

Map<String, dynamic> _$$AttachmentInformationImplToJson(
  _$AttachmentInformationImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'originalFileName': instance.originalFileName,
  'relativePath': instance.relativePath,
  'type': _$AttachmentTypeEnumMap[instance.type]!,
};

const _$AttachmentTypeEnumMap = {
  AttachmentType.nationalCard: 0,
  AttachmentType.residenceCard: 1,
  AttachmentType.rationCard: 2,
  AttachmentType.personalPicture: 3,
  AttachmentType.purchaseReceipt: 4,
  AttachmentType.trustReceipt: 5,
  AttachmentType.saleContract: 6,
  AttachmentType.profilePicture: 7,
  AttachmentType.factor: 8,
};
