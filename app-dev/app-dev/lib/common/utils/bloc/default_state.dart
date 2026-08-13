part of 'default_bloc.dart';

@immutable
abstract class DefaultState {
  const DefaultState({this.event});

  final DefaultEvent? event;
}

class LoadingState extends DefaultState {
  const LoadingState({super.event, this.data});
  final dynamic data;
}

class ErrorState extends DefaultState {
  const ErrorState({required this.error, super.event, this.data});
  final dynamic data;
  final String error;
}

class ResponseState extends DefaultState {
  const ResponseState({this.data, super.event});
  final dynamic data;
}

class ErrorMessageState extends DefaultState {
  const ErrorMessageState({this.error, super.event});
  final String? error;
}
