part of 'customer_details_bloc.dart';

class InitialCustomerDetailsEvent extends DefaultEvent {
  final int id;

  InitialCustomerDetailsEvent({required this.id});
}

class GetUsersFinancialOverview extends DefaultEvent {
  final int id;

  GetUsersFinancialOverview(this.id);
}

class GetUserEvent extends DefaultEvent {
  final int id;

  GetUserEvent(this.id);
}
