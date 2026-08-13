import 'package:get_it/get_it.dart';
import 'package:team/account/infrastructure/repository/account_repository.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';

part 'entry_event.dart';
part 'entry_state.dart';

class EntryBloc extends DefaultBloc {
  final AccountRepository accountRepository;

  EntryBloc({required this.accountRepository}) : super(LoginInitial()) {
    safeOn<LoginEvent>(_login);
  }

  void _login(LoginEvent event, emit) async {
    final result = await accountRepository.login(event.username, event.password);
    await accountRepository.saveIsLogged(true);
    await accountRepository.saveToken(result.accessToken);
    await accountRepository.saveRefreshToken(result.refreshToken);
    await accountRepository.savePermission(result.permissions);
    GetIt.I.get<MainBloc>().add(UserLogedIn());
  }
}
