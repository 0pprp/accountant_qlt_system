import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';
import 'package:team/payment/infrastructure/repositories/payment_repository.dart';

part 'customer_payments_event.dart';
part 'customer_payments_state.dart';

class CustomerPaymentsBloc extends DefaultBloc {
  final PaymentRepository paymentRepository;
  CustomerPaymentsBloc({required this.paymentRepository}) : super(CustomerPaymentsInitialState()) {
    safeOn<InitialCustomerPaymentsEvent>(_initialCustomerPayments);
  }

  List<InstallmentPayment>? installmentPayments;
  double sumOfAllPayments = 0;

  void _initialCustomerPayments(InitialCustomerPaymentsEvent event, emit) async {
    installmentPayments = await paymentRepository.getUserInstallmentPayments(event.userId);
    _calculateSumOfAllPayments();
  }

  String _calculateSumOfAllPayments() {
    for (final installment in installmentPayments!) {
      sumOfAllPayments += installment.amount;
    }
    if (sumOfAllPayments % 1 == 0) {
      return sumOfAllPayments.toInt().toString();
    }
    return sumOfAllPayments.toString();
  }
}
