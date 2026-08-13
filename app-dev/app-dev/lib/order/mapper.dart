import 'package:get_it/get_it.dart';
import 'package:team/order/domain/collect_installment/collect_installment.dart';
import 'package:team/order/domain/order/order.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';

class OrderMapper {
  late final customerRepository = GetIt.I.get<OrderRepository>();

  Future<void> ensureInitialized() => customerRepository.ensureInitialized();

  List<CollectInstallment> getOfflineCollects() => customerRepository.getOfflineCollects();

  Future<void> collectInstallment(CollectInstallment collectInstallment, Order order) =>
      customerRepository.collectInstallment(collectInstallment, order);

  Future<void> saveOfflineCollects(List<CollectInstallment> collects) =>
      customerRepository.saveOfflineCollects(collects);

  Future<void> collectOfflineInstallments(List<CollectInstallment> collects) =>
      customerRepository.collectOfflineInstallments(collects);
}
