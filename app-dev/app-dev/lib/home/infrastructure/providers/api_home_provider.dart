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
    var response = await request.get(
      'api/v1/app/checkVersion',
      queryParameters: {
        'buildNumber': (await PackageInfo.fromPlatform()).buildNumber,
      },
    );
    return AppVersionResponse.fromJson(response.data);
  }
}
