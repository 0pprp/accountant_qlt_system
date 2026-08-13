import 'package:team/account/domain/user/user.dart';
import 'package:team/account/domain/user_info/user_info.dart';
import 'package:team/customer/domain/customer_financial_overview.dart';
import 'package:team/customer/infrastructure/providers/api_customer_provider.dart';
import 'package:team/customer/infrastructure/providers/local_customer_provider.dart';

class CustomerRepository {
  final ApiCustomerProvider apiCustomerProvider;
  final LocalCustomerProvider localCustomerProvider;

  CustomerRepository({required this.apiCustomerProvider, required this.localCustomerProvider});

  Future<void> ensureInitialized() => localCustomerProvider.ensureInitialized();

  Future<List<UserInfo>> getCustomers({final String? searchText, final DateTime? lastDateTime}) async {
    return await apiCustomerProvider.getCustomers(
      searchText: searchText,
      lastDateTime: lastDateTime,
    );
  }

  List<UserInfo> getRecentSearches() => localCustomerProvider.getRecentSearches();

  Future<int> createCustomer(final User user) => apiCustomerProvider.createCustomer(user);

  Future<CustomerFinancialOverview> getUserFinancialOverview(final int userId) =>
      apiCustomerProvider.getUserFinancialOverview(userId);

  Future<void> editCustomer(final User user) => apiCustomerProvider.editCustomer(user);

  Future<void> saveRecentSearches(final List<UserInfo> recentSearches) =>
      localCustomerProvider.saveRecentSearches(recentSearches);

  Future<User> getCustomerDetails(final int id) => apiCustomerProvider.getCustomerDetails(id);
}
