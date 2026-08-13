part of 'customer_payments_bloc.dart';

class InitialCustomerPaymentsEvent extends DefaultEvent {
  final int userId;

  InitialCustomerPaymentsEvent(this.userId);
}
