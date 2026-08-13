import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/home/infrastructure/repositories/home_repository.dart';

part 'notifications_event.dart';
part 'notifications_state.dart';

class NotificationsBloc extends DefaultBloc {
  final HomeRepository homeRepository;
  NotificationsBloc({required this.homeRepository}) : super(NotificationsInitialState()) {
    safeOn<InitialNotificationsEvent>(_initialNotifications);
  }

  void _initialNotifications(InitialNotificationsEvent event, emit) async {}
}
