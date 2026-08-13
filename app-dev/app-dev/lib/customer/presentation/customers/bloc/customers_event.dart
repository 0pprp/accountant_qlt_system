part of 'customers_bloc.dart';

class InitialCustomersEvent extends DefaultEvent {
  final DateTime? lastDateTime;

  InitialCustomersEvent({this.lastDateTime});
}

class GetOrdersList extends DefaultEvent {}

class GetRecentSearches extends DefaultEvent {}

class AddRecentSearch extends DefaultEvent {
  final UserInfo customer;

  AddRecentSearch(this.customer);
}

class ClearRecentSearches extends DefaultEvent {}
