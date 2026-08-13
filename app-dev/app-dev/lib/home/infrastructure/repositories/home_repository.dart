import 'package:flutter/cupertino.dart';
import 'package:get_it/get_it.dart';
import 'package:team/account/mapper.dart';
import 'package:team/home/domain/app_version_response/app_version_response.dart';
import 'package:team/home/infrastructure/providers/api_home_provider.dart';
import 'package:team/home/infrastructure/providers/local_home_provider.dart';
import 'package:team/order/domain/order_list/order_list.dart';

class HomeRepository {
  final ApiHomeProvider apiHomeProvider;
  final LocalHomeProvider localHomeProvider;

  HomeRepository({required this.apiHomeProvider, required this.localHomeProvider});

  Future<void> ensureInitialized() => localHomeProvider.ensureInitialized();

  Future<List<OrderList>> getOrderLists() async {
    try {
      final response = await apiHomeProvider.getDailyReports();

      try {
        await localHomeProvider.saveDailyReports(response);
      } catch (e) {
        debugPrint('Failed to save daily report locally: ${e.toString()}');
      }

      return response;
    } catch (e) {
      debugPrint('Failed to fetch daily report from API: ${e.toString()}');

      if (localHomeProvider.isDataFromToday() && !GetIt.I.get<AccountMapper>().isMotaba) {
        final localDailyReport = localHomeProvider.getDailyReports();
        if (localDailyReport != null) return localDailyReport;
      }

      rethrow;
    }
  }

  Future<void> saveIsDarkMode(bool isDarkMode) => localHomeProvider.saveIsDarkMode(isDarkMode);

  Future<AppVersionResponse> checkForUpdates() => apiHomeProvider.checkForUpdates();

  bool get getIsDarkMode => localHomeProvider.getIsDarkMode();
}
