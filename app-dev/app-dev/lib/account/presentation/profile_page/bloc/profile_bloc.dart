import 'package:team/account/domain/admin_user/admin_user.dart';
import 'package:team/account/infrastructure/repository/account_repository.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';

part 'profile_event.dart';
part 'profile_state.dart';

class ProfileBloc extends DefaultBloc {
  final AccountRepository accountRepository;
  ProfileBloc({required this.accountRepository}) : super(ProfileInitial()) {
    safeOn<InitialProfileEvent>(_initialAccount);
  }

  AdminUser? user;

  void _initialAccount(InitialProfileEvent event, emit) async {
    user = await accountRepository.getCurrentAdminUser();
  }
}
