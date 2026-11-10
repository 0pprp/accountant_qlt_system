import '../entities/customer.dart';
import '../repositories/customers_repository.dart';

class GetLocalCustomersUseCase {
  GetLocalCustomersUseCase(this._repo);

  final CustomersRepository _repo;

  Future<List<Customer>> call({String? query}) =>
      _repo.getLocalCustomers(query: query);
}
