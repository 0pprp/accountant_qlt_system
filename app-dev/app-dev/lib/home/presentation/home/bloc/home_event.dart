part of 'home_bloc.dart';

class InitialHomeEvent extends DefaultEvent {}

class UpdateDataEvent extends DefaultEvent {}

class GetUserEvent extends DefaultEvent {}

class CheckPendingCollects extends DefaultEvent {}

class CheckForUpdates extends DefaultEvent {}

class ChangeOrderListEvent extends DefaultEvent {
  final int index;
  ChangeOrderListEvent(this.index);
}
