import '../repositories/customers_repository.dart';

class RefreshCustomersUseCase {
  RefreshCustomersUseCase(this._repo);

  final CustomersRepository _repo;

  Future<int> call({bool force = false}) =>
      _repo.refreshFromRemote(silent: !force);
}
