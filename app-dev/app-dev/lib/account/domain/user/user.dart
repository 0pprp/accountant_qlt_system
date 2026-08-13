import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:image_picker/image_picker.dart';

part 'user.freezed.dart';
part 'user.g.dart';

@freezed
class User with _$User {
  const factory User({
    required int id,
    required String fullName,
    required String motherName,
    required String nationalCode,
    @DateOnlyConverter() required DateTime birthDate,
    required String phoneNumber,
    required String whatsAppPhoneNumber,
    required BusinessInformation business,
    required BranchInformation branch,
    required List<AttachmentInformation> attachments,
  }) = _User;

  factory User.fromJson(Map<String, dynamic> json) => _$UserFromJson(json);
}

@freezed
class BusinessInformation with _$BusinessInformation {
  const factory BusinessInformation({
    required String name,
    required String address,
    required String nearestKnownLocation,
  }) = _BusinessInformation;

  factory BusinessInformation.fromJson(Map<String, dynamic> json) => _$BusinessInformationFromJson(json);
}

@freezed
class BranchInformation with _$BranchInformation {
  const factory BranchInformation({
    required int id,
    required String name,
  }) = _BranchInformation;

  factory BranchInformation.fromJson(Map<String, dynamic> json) => _$BranchInformationFromJson(json);
}

@freezed
class AttachmentInformation with _$AttachmentInformation {
  const factory AttachmentInformation({
    int? id,
    String? originalFileName,
    String? relativePath,
    required AttachmentType type,
    @JsonKey(includeFromJson: false, includeToJson: false) XFile? localFile,
  }) = _AttachmentInformation;

  factory AttachmentInformation.fromJson(Map<String, dynamic> json) => _$AttachmentInformationFromJson(json);
}

enum AttachmentType {
  @JsonValue(0)
  nationalCard,
  @JsonValue(1)
  residenceCard,
  @JsonValue(2)
  rationCard,
  @JsonValue(3)
  personalPicture,
  @JsonValue(4)
  purchaseReceipt,
  @JsonValue(5)
  trustReceipt,
  @JsonValue(6)
  saleContract,
  @JsonValue(7)
  profilePicture,
  @JsonValue(8)
  factor,
}

class DateOnlyConverter implements JsonConverter<DateTime, String> {
  const DateOnlyConverter();

  @override
  DateTime fromJson(String json) => DateTime.parse(json);

  @override
  String toJson(DateTime date) => date.toIso8601String().split('T').first;
}
