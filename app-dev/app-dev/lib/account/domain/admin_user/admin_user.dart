import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:team/account/domain/branch_short_info/branch_short_info.dart';
import 'package:team/account/domain/file/file.dart';
import 'package:team/order/domain/order_list_info/order_list_info.dart';

part 'admin_user.freezed.dart';
part 'admin_user.g.dart';

@freezed
class AdminUser with _$AdminUser {
  const factory AdminUser({
    required int id,
    required String fullName,
    @Default([]) List<BranchShortInfo> branches,
    File? profilePicture,
    required String? phoneNumber,
    OrderListInfo? orderList,
    required String? address,
    required DateTime createdAt,
  }) = _AdminUser;

  factory AdminUser.fromJson(Map<String, Object?> json) => _$AdminUserFromJson(json);
}
