import 'package:team/account/domain/user/user.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/error_handler.dart';
import 'package:team/order/domain/collect_installment/collect_installment.dart';
import 'package:team/order/domain/order/order.dart';
import 'package:team/order/domain/order_details/order_details.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';

part 'order_details_event.dart';
part 'order_details_state.dart';

class OrderDetailsBloc extends DefaultBloc {
  final OrderRepository orderRepository;
  final AccountMapper accountMapper;
  OrderDetailsBloc({required this.orderRepository, required this.accountMapper}) : super(OrderDetailsInitialState()) {
    safeOn<InitialOrderDetailsEvent>(_initialOrderDetails);
    safeOn<SendUserPayment>(_sendUserPayment);
    safeOn<GetUserEvent>(_getUserEvent);
    safeOn<GetOrderDetailsEvent>(_getOrderDetailsEvent);
  }

  OrderDetails? orderDetails;
  User? user;
  int? orderId;
  bool isTodayCollected = false;

  void _initialOrderDetails(InitialOrderDetailsEvent event, emit) async {
    orderId = event.orderId;
    add(GetUserEvent(event.userId));
    add(GetOrderDetailsEvent(event.orderId));
  }

  void _getUserEvent(GetUserEvent event, emit) async {
    user = await accountMapper.getUser(event.id);
  }

  void _getOrderDetailsEvent(GetOrderDetailsEvent event, emit) async {
    orderDetails = await orderRepository.getOrderDetails(event.id);
  }

  void _sendUserPayment(SendUserPayment event, emit) async {
    if (!isTodayCollected) {
      await orderRepository.collectInstallment(event.collectInstallment, event.order);
      isTodayCollected = true;
      if (orderId != null) {
        add(GetOrderDetailsEvent(orderId!));
      }
    } else {
      throw MessageException('تم جمعها بالفعل');
    }
  }
}
