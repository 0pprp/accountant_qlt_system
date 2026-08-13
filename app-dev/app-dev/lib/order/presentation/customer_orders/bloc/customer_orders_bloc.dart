import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/order/domain/order_details/order_details.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';

part 'customer_orders_event.dart';
part 'customer_orders_state.dart';

class CustomerOrdersBloc extends DefaultBloc {
  final OrderRepository orderRepository;
  CustomerOrdersBloc({required this.orderRepository}) : super(CustomerOrdersInitialState()) {
    safeOn<InitialCustomerOrdersEvent>(_initialCustomerOrders);
  }

  List<OrderDetails>? customerOrders;

  void _initialCustomerOrders(InitialCustomerOrdersEvent event, emit) async {
    customerOrders = await orderRepository.getUserOrders(event.id);
  }
}
