part of 'main_bloc.dart';

class UserLogedIn extends DefaultEvent {}

class ChangeNotificationBadge extends DefaultEvent {
  final bool hasNotification;

  ChangeNotificationBadge(this.hasNotification);
}

class UpdateProfileData extends DefaultEvent {}

class OrderPaymentEvent extends DefaultEvent {}

class OrderCreateEvent extends DefaultEvent {}

class GetOrderLists extends DefaultEvent {}
