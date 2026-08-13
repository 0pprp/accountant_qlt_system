import 'package:freezed_annotation/freezed_annotation.dart';

part 'file.freezed.dart';
part 'file.g.dart';

@freezed
class File with _$File {
  const factory File({
    required int id,
    required String originalFileName,
    required String relativePath,
    required int fileSizeInByte,
    required int type, // Consider an enum if type has specific meanings
  }) = _File;

  factory File.fromJson(Map<String, Object?> json) => _$FileFromJson(json);
}
