import 'package:freezed_annotation/freezed_annotation.dart';

part 'app_version_response.freezed.dart';
part 'app_version_response.g.dart';

@freezed
class AppVersionResponse with _$AppVersionResponse {
  const factory AppVersionResponse({
    required bool shouldUpdate,
    required bool isForce,
    required String url,
  }) = _AppVersionResponse;

  factory AppVersionResponse.fromJson(Map<String, Object?> json) => _$AppVersionResponseFromJson(json);
}
