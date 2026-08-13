// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'user.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

User _$UserFromJson(Map<String, dynamic> json) {
  return _User.fromJson(json);
}

/// @nodoc
mixin _$User {
  int get id => throw _privateConstructorUsedError;
  String get fullName => throw _privateConstructorUsedError;
  String get motherName => throw _privateConstructorUsedError;
  String get nationalCode => throw _privateConstructorUsedError;
  @DateOnlyConverter()
  DateTime get birthDate => throw _privateConstructorUsedError;
  String get phoneNumber => throw _privateConstructorUsedError;
  String get whatsAppPhoneNumber => throw _privateConstructorUsedError;
  BusinessInformation get business => throw _privateConstructorUsedError;
  BranchInformation get branch => throw _privateConstructorUsedError;
  List<AttachmentInformation> get attachments =>
      throw _privateConstructorUsedError;

  /// Serializes this User to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of User
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $UserCopyWith<User> get copyWith => throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $UserCopyWith<$Res> {
  factory $UserCopyWith(User value, $Res Function(User) then) =
      _$UserCopyWithImpl<$Res, User>;
  @useResult
  $Res call({
    int id,
    String fullName,
    String motherName,
    String nationalCode,
    @DateOnlyConverter() DateTime birthDate,
    String phoneNumber,
    String whatsAppPhoneNumber,
    BusinessInformation business,
    BranchInformation branch,
    List<AttachmentInformation> attachments,
  });

  $BusinessInformationCopyWith<$Res> get business;
  $BranchInformationCopyWith<$Res> get branch;
}

/// @nodoc
class _$UserCopyWithImpl<$Res, $Val extends User>
    implements $UserCopyWith<$Res> {
  _$UserCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of User
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? fullName = null,
    Object? motherName = null,
    Object? nationalCode = null,
    Object? birthDate = null,
    Object? phoneNumber = null,
    Object? whatsAppPhoneNumber = null,
    Object? business = null,
    Object? branch = null,
    Object? attachments = null,
  }) {
    return _then(
      _value.copyWith(
            id:
                null == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int,
            fullName:
                null == fullName
                    ? _value.fullName
                    : fullName // ignore: cast_nullable_to_non_nullable
                        as String,
            motherName:
                null == motherName
                    ? _value.motherName
                    : motherName // ignore: cast_nullable_to_non_nullable
                        as String,
            nationalCode:
                null == nationalCode
                    ? _value.nationalCode
                    : nationalCode // ignore: cast_nullable_to_non_nullable
                        as String,
            birthDate:
                null == birthDate
                    ? _value.birthDate
                    : birthDate // ignore: cast_nullable_to_non_nullable
                        as DateTime,
            phoneNumber:
                null == phoneNumber
                    ? _value.phoneNumber
                    : phoneNumber // ignore: cast_nullable_to_non_nullable
                        as String,
            whatsAppPhoneNumber:
                null == whatsAppPhoneNumber
                    ? _value.whatsAppPhoneNumber
                    : whatsAppPhoneNumber // ignore: cast_nullable_to_non_nullable
                        as String,
            business:
                null == business
                    ? _value.business
                    : business // ignore: cast_nullable_to_non_nullable
                        as BusinessInformation,
            branch:
                null == branch
                    ? _value.branch
                    : branch // ignore: cast_nullable_to_non_nullable
                        as BranchInformation,
            attachments:
                null == attachments
                    ? _value.attachments
                    : attachments // ignore: cast_nullable_to_non_nullable
                        as List<AttachmentInformation>,
          )
          as $Val,
    );
  }

  /// Create a copy of User
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $BusinessInformationCopyWith<$Res> get business {
    return $BusinessInformationCopyWith<$Res>(_value.business, (value) {
      return _then(_value.copyWith(business: value) as $Val);
    });
  }

  /// Create a copy of User
  /// with the given fields replaced by the non-null parameter values.
  @override
  @pragma('vm:prefer-inline')
  $BranchInformationCopyWith<$Res> get branch {
    return $BranchInformationCopyWith<$Res>(_value.branch, (value) {
      return _then(_value.copyWith(branch: value) as $Val);
    });
  }
}

/// @nodoc
abstract class _$$UserImplCopyWith<$Res> implements $UserCopyWith<$Res> {
  factory _$$UserImplCopyWith(
    _$UserImpl value,
    $Res Function(_$UserImpl) then,
  ) = __$$UserImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int id,
    String fullName,
    String motherName,
    String nationalCode,
    @DateOnlyConverter() DateTime birthDate,
    String phoneNumber,
    String whatsAppPhoneNumber,
    BusinessInformation business,
    BranchInformation branch,
    List<AttachmentInformation> attachments,
  });

  @override
  $BusinessInformationCopyWith<$Res> get business;
  @override
  $BranchInformationCopyWith<$Res> get branch;
}

/// @nodoc
class __$$UserImplCopyWithImpl<$Res>
    extends _$UserCopyWithImpl<$Res, _$UserImpl>
    implements _$$UserImplCopyWith<$Res> {
  __$$UserImplCopyWithImpl(_$UserImpl _value, $Res Function(_$UserImpl) _then)
    : super(_value, _then);

  /// Create a copy of User
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? fullName = null,
    Object? motherName = null,
    Object? nationalCode = null,
    Object? birthDate = null,
    Object? phoneNumber = null,
    Object? whatsAppPhoneNumber = null,
    Object? business = null,
    Object? branch = null,
    Object? attachments = null,
  }) {
    return _then(
      _$UserImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        fullName:
            null == fullName
                ? _value.fullName
                : fullName // ignore: cast_nullable_to_non_nullable
                    as String,
        motherName:
            null == motherName
                ? _value.motherName
                : motherName // ignore: cast_nullable_to_non_nullable
                    as String,
        nationalCode:
            null == nationalCode
                ? _value.nationalCode
                : nationalCode // ignore: cast_nullable_to_non_nullable
                    as String,
        birthDate:
            null == birthDate
                ? _value.birthDate
                : birthDate // ignore: cast_nullable_to_non_nullable
                    as DateTime,
        phoneNumber:
            null == phoneNumber
                ? _value.phoneNumber
                : phoneNumber // ignore: cast_nullable_to_non_nullable
                    as String,
        whatsAppPhoneNumber:
            null == whatsAppPhoneNumber
                ? _value.whatsAppPhoneNumber
                : whatsAppPhoneNumber // ignore: cast_nullable_to_non_nullable
                    as String,
        business:
            null == business
                ? _value.business
                : business // ignore: cast_nullable_to_non_nullable
                    as BusinessInformation,
        branch:
            null == branch
                ? _value.branch
                : branch // ignore: cast_nullable_to_non_nullable
                    as BranchInformation,
        attachments:
            null == attachments
                ? _value._attachments
                : attachments // ignore: cast_nullable_to_non_nullable
                    as List<AttachmentInformation>,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$UserImpl implements _User {
  const _$UserImpl({
    required this.id,
    required this.fullName,
    required this.motherName,
    required this.nationalCode,
    @DateOnlyConverter() required this.birthDate,
    required this.phoneNumber,
    required this.whatsAppPhoneNumber,
    required this.business,
    required this.branch,
    required final List<AttachmentInformation> attachments,
  }) : _attachments = attachments;

  factory _$UserImpl.fromJson(Map<String, dynamic> json) =>
      _$$UserImplFromJson(json);

  @override
  final int id;
  @override
  final String fullName;
  @override
  final String motherName;
  @override
  final String nationalCode;
  @override
  @DateOnlyConverter()
  final DateTime birthDate;
  @override
  final String phoneNumber;
  @override
  final String whatsAppPhoneNumber;
  @override
  final BusinessInformation business;
  @override
  final BranchInformation branch;
  final List<AttachmentInformation> _attachments;
  @override
  List<AttachmentInformation> get attachments {
    if (_attachments is EqualUnmodifiableListView) return _attachments;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_attachments);
  }

  @override
  String toString() {
    return 'User(id: $id, fullName: $fullName, motherName: $motherName, nationalCode: $nationalCode, birthDate: $birthDate, phoneNumber: $phoneNumber, whatsAppPhoneNumber: $whatsAppPhoneNumber, business: $business, branch: $branch, attachments: $attachments)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$UserImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.fullName, fullName) ||
                other.fullName == fullName) &&
            (identical(other.motherName, motherName) ||
                other.motherName == motherName) &&
            (identical(other.nationalCode, nationalCode) ||
                other.nationalCode == nationalCode) &&
            (identical(other.birthDate, birthDate) ||
                other.birthDate == birthDate) &&
            (identical(other.phoneNumber, phoneNumber) ||
                other.phoneNumber == phoneNumber) &&
            (identical(other.whatsAppPhoneNumber, whatsAppPhoneNumber) ||
                other.whatsAppPhoneNumber == whatsAppPhoneNumber) &&
            (identical(other.business, business) ||
                other.business == business) &&
            (identical(other.branch, branch) || other.branch == branch) &&
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
    fullName,
    motherName,
    nationalCode,
    birthDate,
    phoneNumber,
    whatsAppPhoneNumber,
    business,
    branch,
    const DeepCollectionEquality().hash(_attachments),
  );

  /// Create a copy of User
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$UserImplCopyWith<_$UserImpl> get copyWith =>
      __$$UserImplCopyWithImpl<_$UserImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$UserImplToJson(this);
  }
}

abstract class _User implements User {
  const factory _User({
    required final int id,
    required final String fullName,
    required final String motherName,
    required final String nationalCode,
    @DateOnlyConverter() required final DateTime birthDate,
    required final String phoneNumber,
    required final String whatsAppPhoneNumber,
    required final BusinessInformation business,
    required final BranchInformation branch,
    required final List<AttachmentInformation> attachments,
  }) = _$UserImpl;

  factory _User.fromJson(Map<String, dynamic> json) = _$UserImpl.fromJson;

  @override
  int get id;
  @override
  String get fullName;
  @override
  String get motherName;
  @override
  String get nationalCode;
  @override
  @DateOnlyConverter()
  DateTime get birthDate;
  @override
  String get phoneNumber;
  @override
  String get whatsAppPhoneNumber;
  @override
  BusinessInformation get business;
  @override
  BranchInformation get branch;
  @override
  List<AttachmentInformation> get attachments;

  /// Create a copy of User
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$UserImplCopyWith<_$UserImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

BusinessInformation _$BusinessInformationFromJson(Map<String, dynamic> json) {
  return _BusinessInformation.fromJson(json);
}

/// @nodoc
mixin _$BusinessInformation {
  String get name => throw _privateConstructorUsedError;
  String get address => throw _privateConstructorUsedError;
  String get nearestKnownLocation => throw _privateConstructorUsedError;

  /// Serializes this BusinessInformation to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of BusinessInformation
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $BusinessInformationCopyWith<BusinessInformation> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $BusinessInformationCopyWith<$Res> {
  factory $BusinessInformationCopyWith(
    BusinessInformation value,
    $Res Function(BusinessInformation) then,
  ) = _$BusinessInformationCopyWithImpl<$Res, BusinessInformation>;
  @useResult
  $Res call({String name, String address, String nearestKnownLocation});
}

/// @nodoc
class _$BusinessInformationCopyWithImpl<$Res, $Val extends BusinessInformation>
    implements $BusinessInformationCopyWith<$Res> {
  _$BusinessInformationCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of BusinessInformation
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = null,
    Object? address = null,
    Object? nearestKnownLocation = null,
  }) {
    return _then(
      _value.copyWith(
            name:
                null == name
                    ? _value.name
                    : name // ignore: cast_nullable_to_non_nullable
                        as String,
            address:
                null == address
                    ? _value.address
                    : address // ignore: cast_nullable_to_non_nullable
                        as String,
            nearestKnownLocation:
                null == nearestKnownLocation
                    ? _value.nearestKnownLocation
                    : nearestKnownLocation // ignore: cast_nullable_to_non_nullable
                        as String,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$BusinessInformationImplCopyWith<$Res>
    implements $BusinessInformationCopyWith<$Res> {
  factory _$$BusinessInformationImplCopyWith(
    _$BusinessInformationImpl value,
    $Res Function(_$BusinessInformationImpl) then,
  ) = __$$BusinessInformationImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({String name, String address, String nearestKnownLocation});
}

/// @nodoc
class __$$BusinessInformationImplCopyWithImpl<$Res>
    extends _$BusinessInformationCopyWithImpl<$Res, _$BusinessInformationImpl>
    implements _$$BusinessInformationImplCopyWith<$Res> {
  __$$BusinessInformationImplCopyWithImpl(
    _$BusinessInformationImpl _value,
    $Res Function(_$BusinessInformationImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of BusinessInformation
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = null,
    Object? address = null,
    Object? nearestKnownLocation = null,
  }) {
    return _then(
      _$BusinessInformationImpl(
        name:
            null == name
                ? _value.name
                : name // ignore: cast_nullable_to_non_nullable
                    as String,
        address:
            null == address
                ? _value.address
                : address // ignore: cast_nullable_to_non_nullable
                    as String,
        nearestKnownLocation:
            null == nearestKnownLocation
                ? _value.nearestKnownLocation
                : nearestKnownLocation // ignore: cast_nullable_to_non_nullable
                    as String,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$BusinessInformationImpl implements _BusinessInformation {
  const _$BusinessInformationImpl({
    required this.name,
    required this.address,
    required this.nearestKnownLocation,
  });

  factory _$BusinessInformationImpl.fromJson(Map<String, dynamic> json) =>
      _$$BusinessInformationImplFromJson(json);

  @override
  final String name;
  @override
  final String address;
  @override
  final String nearestKnownLocation;

  @override
  String toString() {
    return 'BusinessInformation(name: $name, address: $address, nearestKnownLocation: $nearestKnownLocation)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$BusinessInformationImpl &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.address, address) || other.address == address) &&
            (identical(other.nearestKnownLocation, nearestKnownLocation) ||
                other.nearestKnownLocation == nearestKnownLocation));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, name, address, nearestKnownLocation);

  /// Create a copy of BusinessInformation
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$BusinessInformationImplCopyWith<_$BusinessInformationImpl> get copyWith =>
      __$$BusinessInformationImplCopyWithImpl<_$BusinessInformationImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$BusinessInformationImplToJson(this);
  }
}

abstract class _BusinessInformation implements BusinessInformation {
  const factory _BusinessInformation({
    required final String name,
    required final String address,
    required final String nearestKnownLocation,
  }) = _$BusinessInformationImpl;

  factory _BusinessInformation.fromJson(Map<String, dynamic> json) =
      _$BusinessInformationImpl.fromJson;

  @override
  String get name;
  @override
  String get address;
  @override
  String get nearestKnownLocation;

  /// Create a copy of BusinessInformation
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$BusinessInformationImplCopyWith<_$BusinessInformationImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

BranchInformation _$BranchInformationFromJson(Map<String, dynamic> json) {
  return _BranchInformation.fromJson(json);
}

/// @nodoc
mixin _$BranchInformation {
  int get id => throw _privateConstructorUsedError;
  String get name => throw _privateConstructorUsedError;

  /// Serializes this BranchInformation to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of BranchInformation
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $BranchInformationCopyWith<BranchInformation> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $BranchInformationCopyWith<$Res> {
  factory $BranchInformationCopyWith(
    BranchInformation value,
    $Res Function(BranchInformation) then,
  ) = _$BranchInformationCopyWithImpl<$Res, BranchInformation>;
  @useResult
  $Res call({int id, String name});
}

/// @nodoc
class _$BranchInformationCopyWithImpl<$Res, $Val extends BranchInformation>
    implements $BranchInformationCopyWith<$Res> {
  _$BranchInformationCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of BranchInformation
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? id = null, Object? name = null}) {
    return _then(
      _value.copyWith(
            id:
                null == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int,
            name:
                null == name
                    ? _value.name
                    : name // ignore: cast_nullable_to_non_nullable
                        as String,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$BranchInformationImplCopyWith<$Res>
    implements $BranchInformationCopyWith<$Res> {
  factory _$$BranchInformationImplCopyWith(
    _$BranchInformationImpl value,
    $Res Function(_$BranchInformationImpl) then,
  ) = __$$BranchInformationImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int id, String name});
}

/// @nodoc
class __$$BranchInformationImplCopyWithImpl<$Res>
    extends _$BranchInformationCopyWithImpl<$Res, _$BranchInformationImpl>
    implements _$$BranchInformationImplCopyWith<$Res> {
  __$$BranchInformationImplCopyWithImpl(
    _$BranchInformationImpl _value,
    $Res Function(_$BranchInformationImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of BranchInformation
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? id = null, Object? name = null}) {
    return _then(
      _$BranchInformationImpl(
        id:
            null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int,
        name:
            null == name
                ? _value.name
                : name // ignore: cast_nullable_to_non_nullable
                    as String,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$BranchInformationImpl implements _BranchInformation {
  const _$BranchInformationImpl({required this.id, required this.name});

  factory _$BranchInformationImpl.fromJson(Map<String, dynamic> json) =>
      _$$BranchInformationImplFromJson(json);

  @override
  final int id;
  @override
  final String name;

  @override
  String toString() {
    return 'BranchInformation(id: $id, name: $name)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$BranchInformationImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.name, name) || other.name == name));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, id, name);

  /// Create a copy of BranchInformation
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$BranchInformationImplCopyWith<_$BranchInformationImpl> get copyWith =>
      __$$BranchInformationImplCopyWithImpl<_$BranchInformationImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$BranchInformationImplToJson(this);
  }
}

abstract class _BranchInformation implements BranchInformation {
  const factory _BranchInformation({
    required final int id,
    required final String name,
  }) = _$BranchInformationImpl;

  factory _BranchInformation.fromJson(Map<String, dynamic> json) =
      _$BranchInformationImpl.fromJson;

  @override
  int get id;
  @override
  String get name;

  /// Create a copy of BranchInformation
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$BranchInformationImplCopyWith<_$BranchInformationImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

AttachmentInformation _$AttachmentInformationFromJson(
  Map<String, dynamic> json,
) {
  return _AttachmentInformation.fromJson(json);
}

/// @nodoc
mixin _$AttachmentInformation {
  int? get id => throw _privateConstructorUsedError;
  String? get originalFileName => throw _privateConstructorUsedError;
  String? get relativePath => throw _privateConstructorUsedError;
  AttachmentType get type => throw _privateConstructorUsedError;
  @JsonKey(includeFromJson: false, includeToJson: false)
  XFile? get localFile => throw _privateConstructorUsedError;

  /// Serializes this AttachmentInformation to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of AttachmentInformation
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $AttachmentInformationCopyWith<AttachmentInformation> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $AttachmentInformationCopyWith<$Res> {
  factory $AttachmentInformationCopyWith(
    AttachmentInformation value,
    $Res Function(AttachmentInformation) then,
  ) = _$AttachmentInformationCopyWithImpl<$Res, AttachmentInformation>;
  @useResult
  $Res call({
    int? id,
    String? originalFileName,
    String? relativePath,
    AttachmentType type,
    @JsonKey(includeFromJson: false, includeToJson: false) XFile? localFile,
  });
}

/// @nodoc
class _$AttachmentInformationCopyWithImpl<
  $Res,
  $Val extends AttachmentInformation
>
    implements $AttachmentInformationCopyWith<$Res> {
  _$AttachmentInformationCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of AttachmentInformation
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? originalFileName = freezed,
    Object? relativePath = freezed,
    Object? type = null,
    Object? localFile = freezed,
  }) {
    return _then(
      _value.copyWith(
            id:
                freezed == id
                    ? _value.id
                    : id // ignore: cast_nullable_to_non_nullable
                        as int?,
            originalFileName:
                freezed == originalFileName
                    ? _value.originalFileName
                    : originalFileName // ignore: cast_nullable_to_non_nullable
                        as String?,
            relativePath:
                freezed == relativePath
                    ? _value.relativePath
                    : relativePath // ignore: cast_nullable_to_non_nullable
                        as String?,
            type:
                null == type
                    ? _value.type
                    : type // ignore: cast_nullable_to_non_nullable
                        as AttachmentType,
            localFile:
                freezed == localFile
                    ? _value.localFile
                    : localFile // ignore: cast_nullable_to_non_nullable
                        as XFile?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$AttachmentInformationImplCopyWith<$Res>
    implements $AttachmentInformationCopyWith<$Res> {
  factory _$$AttachmentInformationImplCopyWith(
    _$AttachmentInformationImpl value,
    $Res Function(_$AttachmentInformationImpl) then,
  ) = __$$AttachmentInformationImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int? id,
    String? originalFileName,
    String? relativePath,
    AttachmentType type,
    @JsonKey(includeFromJson: false, includeToJson: false) XFile? localFile,
  });
}

/// @nodoc
class __$$AttachmentInformationImplCopyWithImpl<$Res>
    extends
        _$AttachmentInformationCopyWithImpl<$Res, _$AttachmentInformationImpl>
    implements _$$AttachmentInformationImplCopyWith<$Res> {
  __$$AttachmentInformationImplCopyWithImpl(
    _$AttachmentInformationImpl _value,
    $Res Function(_$AttachmentInformationImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of AttachmentInformation
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? originalFileName = freezed,
    Object? relativePath = freezed,
    Object? type = null,
    Object? localFile = freezed,
  }) {
    return _then(
      _$AttachmentInformationImpl(
        id:
            freezed == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                    as int?,
        originalFileName:
            freezed == originalFileName
                ? _value.originalFileName
                : originalFileName // ignore: cast_nullable_to_non_nullable
                    as String?,
        relativePath:
            freezed == relativePath
                ? _value.relativePath
                : relativePath // ignore: cast_nullable_to_non_nullable
                    as String?,
        type:
            null == type
                ? _value.type
                : type // ignore: cast_nullable_to_non_nullable
                    as AttachmentType,
        localFile:
            freezed == localFile
                ? _value.localFile
                : localFile // ignore: cast_nullable_to_non_nullable
                    as XFile?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$AttachmentInformationImpl implements _AttachmentInformation {
  const _$AttachmentInformationImpl({
    this.id,
    this.originalFileName,
    this.relativePath,
    required this.type,
    @JsonKey(includeFromJson: false, includeToJson: false) this.localFile,
  });

  factory _$AttachmentInformationImpl.fromJson(Map<String, dynamic> json) =>
      _$$AttachmentInformationImplFromJson(json);

  @override
  final int? id;
  @override
  final String? originalFileName;
  @override
  final String? relativePath;
  @override
  final AttachmentType type;
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  final XFile? localFile;

  @override
  String toString() {
    return 'AttachmentInformation(id: $id, originalFileName: $originalFileName, relativePath: $relativePath, type: $type, localFile: $localFile)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$AttachmentInformationImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.originalFileName, originalFileName) ||
                other.originalFileName == originalFileName) &&
            (identical(other.relativePath, relativePath) ||
                other.relativePath == relativePath) &&
            (identical(other.type, type) || other.type == type) &&
            (identical(other.localFile, localFile) ||
                other.localFile == localFile));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    originalFileName,
    relativePath,
    type,
    localFile,
  );

  /// Create a copy of AttachmentInformation
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$AttachmentInformationImplCopyWith<_$AttachmentInformationImpl>
  get copyWith =>
      __$$AttachmentInformationImplCopyWithImpl<_$AttachmentInformationImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$AttachmentInformationImplToJson(this);
  }
}

abstract class _AttachmentInformation implements AttachmentInformation {
  const factory _AttachmentInformation({
    final int? id,
    final String? originalFileName,
    final String? relativePath,
    required final AttachmentType type,
    @JsonKey(includeFromJson: false, includeToJson: false)
    final XFile? localFile,
  }) = _$AttachmentInformationImpl;

  factory _AttachmentInformation.fromJson(Map<String, dynamic> json) =
      _$AttachmentInformationImpl.fromJson;

  @override
  int? get id;
  @override
  String? get originalFileName;
  @override
  String? get relativePath;
  @override
  AttachmentType get type;
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  XFile? get localFile;

  /// Create a copy of AttachmentInformation
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$AttachmentInformationImplCopyWith<_$AttachmentInformationImpl>
  get copyWith => throw _privateConstructorUsedError;
}
