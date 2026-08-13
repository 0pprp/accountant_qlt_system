import 'package:get_it/get_it.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/account/domain/user_info/user_info.dart';
import 'package:team/customer/infrastructure/repositories/customer_repository.dart';

class CustomerMapper {
  late final customerRepository = GetIt.I.get<CustomerRepository>();

  Future<void> ensureInitialized() => customerRepository.ensureInitialized();

  Future<List<UserInfo>> getCustomers({final String? searchText, final DateTime? lastDateTime}) =>
      customerRepository.getCustomers(searchText: searchText, lastDateTime: lastDateTime);

  Future<int> createCustomer(final User user) => customerRepository.createCustomer(user);

  Future<void> editCustomer(final User user) => customerRepository.editCustomer(user);
}
