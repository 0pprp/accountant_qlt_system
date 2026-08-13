part of 'main_bloc.dart';

class MainInitial extends DefaultState {}

class UserLogedInState extends DefaultState {}

class UpdatedProfileData extends DefaultState {}

class OrderPayment extends DefaultState {}

class OrderCreated extends DefaultState {}

class ChangedNotificationBadge extends DefaultState {
  final bool hasNewNotification;

  const ChangedNotificationBadge(this.hasNewNotification);
}
