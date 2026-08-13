import 'package:freezed_annotation/freezed_annotation.dart';

part 'branch_short_info.freezed.dart';
part 'branch_short_info.g.dart';

@freezed
class BranchShortInfo with _$BranchShortInfo {
  const factory BranchShortInfo({required int id, required String name}) = _BranchShortInfo;

  factory BranchShortInfo.fromJson(Map<String, Object?> json) => _$BranchShortInfoFromJson(json);
}
