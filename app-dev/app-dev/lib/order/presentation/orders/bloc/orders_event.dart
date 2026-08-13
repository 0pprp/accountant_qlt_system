part of 'orders_bloc.dart';

class InitialOrdersEvent extends DefaultEvent {
  final int? orderListId;

  InitialOrdersEvent({this.orderListId});
}

class SendUserPayment extends DefaultEvent {
  final CollectInstallment collectInstallment;
  final Order order;

  SendUserPayment({required this.collectInstallment, required this.order});
}

class GetRecentSearches extends DefaultEvent {}

class AddRecentSearch extends DefaultEvent {
  final Order order;

  AddRecentSearch(this.order);
}

class ClearRecentSearches extends DefaultEvent {}

class DeleteOrderEvent extends DefaultEvent {
  final int orderId;

  DeleteOrderEvent({required this.orderId});
}
