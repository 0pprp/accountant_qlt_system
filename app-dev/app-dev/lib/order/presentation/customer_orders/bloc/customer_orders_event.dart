part of 'customer_orders_bloc.dart';

class InitialCustomerOrdersEvent extends DefaultEvent {
  final int id;

  InitialCustomerOrdersEvent(this.id);
}
