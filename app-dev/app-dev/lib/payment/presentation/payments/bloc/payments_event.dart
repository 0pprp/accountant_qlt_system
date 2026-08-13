part of 'payments_bloc.dart';

class InitialPaymentsEvent extends DefaultEvent {
  final DateTime? lastId;
  final int? orderListId;

  InitialPaymentsEvent({this.lastId, this.orderListId});
}

class GetRecentSearches extends DefaultEvent {}

class AddRecentSearch extends DefaultEvent {
  final InstallmentPayment payment;

  AddRecentSearch(this.payment);
}

class ClearRecentSearches extends DefaultEvent {}
