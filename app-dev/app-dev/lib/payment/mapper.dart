import 'package:get_it/get_it.dart';
import 'package:team/order/domain/order_list/order_list.dart';
import 'package:team/payment/infrastructure/repositories/payment_repository.dart';

class PaymentMapper {
  late final paymentRepository = GetIt.I.get<PaymentRepository>();

  Future<void> ensureInitialized() => paymentRepository.ensureInitialized();

  Future<List<OrderList>> getOrdersList() => paymentRepository.getOrdersList();
}
