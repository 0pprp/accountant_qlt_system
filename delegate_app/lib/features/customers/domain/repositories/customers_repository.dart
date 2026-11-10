import '../entities/customer.dart';

abstract class CustomersRepository {
  Future<List<Customer>> getLocalCustomers({String? query});
  Future<Customer?> getLocalById(int id);
  Future<int> refreshFromRemote({bool silent = true});
  DateTime? get lastRefreshAt;
}
