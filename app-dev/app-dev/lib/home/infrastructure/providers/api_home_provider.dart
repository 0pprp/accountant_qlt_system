import 'package:package_info_plus/package_info_plus.dart';
import 'package:team/common/services/request/request.dart';
import 'package:team/home/domain/app_version_response/app_version_response.dart';
import 'package:team/order/domain/order_list/order_list.dart';

class ApiHomeProvider {
  final Request request;

  ApiHomeProvider({required this.request});

  Future<List<OrderList>> getDailyReports() async {
    final response = await request.get('api/v1/app/OrderLists/');
    return (response.data as List).map((e) => OrderList.fromJson(e)).toList();
  }

  Future<AppVersionResponse> checkForUpdates() async {
    final build = int.tryParse((await PackageInfo.fromPlatform()).buildNumber) ?? 0;
    try {
      final response = await request.get(
        'api/v1/app/mobile-updates/follower',
        queryParameters: {'currentVersionCode': build},
      );
      final m = Map<String, dynamic>.from(response.data as Map);
      final available = m['updateAvailable'] == true || m['UpdateAvailable'] == true;
      final mandatory = m['mandatory'] == true || m['Mandatory'] == true || m['forceUpdate'] == true;
      final url = '${m['apkUrl'] ?? m['ApkUrl'] ?? m['url'] ?? ''}';
      return AppVersionResponse(
        shouldUpdate: available,
        isForce: mandatory,
        url: url,
      );
    } catch (_) {
      // توافق قديم
      final response = await request.get(
        'api/v1/app/checkVersion',
        queryParameters: {
          'buildNumber': build,
          'appKey': 'follower',
        },
      );
      return AppVersionResponse.fromJson(
        Map<String, Object?>.from(response.data as Map),
      );
    }
  }
}
