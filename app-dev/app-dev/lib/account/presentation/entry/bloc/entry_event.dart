part of 'entry_bloc.dart';

class LoginEvent extends DefaultEvent {
  final String username;
  final String password;

  LoginEvent(this.username, this.password);
}
