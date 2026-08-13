import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/payment/domain/orders_installment/orders_installment.dart';
import 'package:team/payment/infrastructure/repositories/payment_repository.dart';

part 'order_payment_details_event.dart';
part 'order_payment_details_state.dart';

class OrderPaymentDetailsBloc extends DefaultBloc {
  final PaymentRepository paymentRepository;
  OrderPaymentDetailsBloc({required this.paymentRepository}) : super(OrderPaymentDetailsInitialState()) {
    safeOn<InitialOrderPaymentDetailsEvent>(_initialOrderPaymentDetails);
  }

  OrdersInstallment? ordersInstallment;

  void _initialOrderPaymentDetails(InitialOrderPaymentDetailsEvent event, emit) async {
    ordersInstallment = await paymentRepository.getOrderInstallments(event.id);
  }
}
