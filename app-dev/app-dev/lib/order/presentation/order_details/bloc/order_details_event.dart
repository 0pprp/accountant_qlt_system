part of 'order_details_bloc.dart';

class InitialOrderDetailsEvent extends DefaultEvent {
  final int userId;
  final int orderId;

  InitialOrderDetailsEvent({required this.userId, required this.orderId});
}

class GetUserEvent extends DefaultEvent {
  final int id;

  GetUserEvent(this.id);
}

class GetOrderDetailsEvent extends DefaultEvent {
  final int id;

  GetOrderDetailsEvent(this.id);
}

class SendUserPayment extends DefaultEvent {
  final CollectInstallment collectInstallment;
  final Order order;

  SendUserPayment({required this.collectInstallment, required this.order});
}
