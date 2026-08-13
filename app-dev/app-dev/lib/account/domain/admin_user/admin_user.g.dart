// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'admin_user.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$AdminUserImpl _$$AdminUserImplFromJson(Map<String, dynamic> json) =>
    _$AdminUserImpl(
      id: (json['id'] as num).toInt(),
      fullName: json['fullName'] as String,
      branches:
          (json['branches'] as List<dynamic>?)
              ?.map((e) => BranchShortInfo.fromJson(e as Map<String, dynamic>))
              .toList() ??
          const [],
      profilePicture:
          json['profilePicture'] == null
              ? null
              : File.fromJson(json['profilePicture'] as Map<String, dynamic>),
      phoneNumber: json['phoneNumber'] as String?,
      orderList:
          json['orderList'] == null
              ? null
              : OrderListInfo.fromJson(
                json['orderList'] as Map<String, dynamic>,
              ),
      address: json['address'] as String?,
      createdAt: DateTime.parse(json['createdAt'] as String),
    );

Map<String, dynamic> _$$AdminUserImplToJson(_$AdminUserImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'fullName': instance.fullName,
      'branches': instance.branches,
      'profilePicture': instance.profilePicture,
      'phoneNumber': instance.phoneNumber,
      'orderList': instance.orderList,
      'address': instance.address,
      'createdAt': instance.createdAt.toIso8601String(),
    };
