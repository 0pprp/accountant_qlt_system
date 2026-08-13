import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/payment/infrastructure/repositories/payment_repository.dart';

part 'print_payment_details_event.dart';
part 'print_payment_details_state.dart';

class PrintPaymentDetailsBloc extends DefaultBloc {
  final PaymentRepository paymentRepository;
  PrintPaymentDetailsBloc({required this.paymentRepository}) : super(PrintPaymentDetailsInitialState()) {
    safeOn<InitialPrintPaymentDetailsEvent>(_initialPrintPaymentDetails);
  }

  void _initialPrintPaymentDetails(InitialPrintPaymentDetailsEvent event, emit) async {}
}
