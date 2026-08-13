import 'package:get_it/get_it.dart';
import 'package:team/home/infrastructure/repositories/home_repository.dart';
import 'package:team/order/domain/order_list/order_list.dart';

class HomeMapper {
  late final homeRepository = GetIt.I.get<HomeRepository>();

  Future<void> ensureInitialized() => homeRepository.ensureInitialized();

  Future<void> saveIsDarkMode(bool isDarkMode) => homeRepository.saveIsDarkMode(isDarkMode);

  bool get getIsDarkMode => homeRepository.getIsDarkMode;

  Future<List<OrderList>> getOrderLists() => homeRepository.getOrderLists();
}
