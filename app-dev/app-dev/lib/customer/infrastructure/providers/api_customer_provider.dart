import 'package:team/account/domain/user/user.dart';
import 'package:team/account/domain/user_info/user_info.dart';
import 'package:team/common/services/request/request.dart';
import 'package:team/customer/domain/customer_financial_overview.dart';

class ApiCustomerProvider {
  final Request request;

  ApiCustomerProvider({required this.request});

  Future<List<UserInfo>> getCustomers({final String? searchText, final DateTime? lastDateTime}) async {
    final response = await request.get(
      'api/v1/app/Customers',
      queryParameters: {
        'SearchTerm': searchText,
        'lastId': lastDateTime,
      },
    );

    return (response.data as List).map((e) => UserInfo.fromJson(e)).toList();
  }

  Future<User> getCustomerDetails(final int id) async {
    final response = await request.get('api/v1/app/Customers/$id');

    return User.fromJson(response.data);
  }

  Future<CustomerFinancialOverview> getUserFinancialOverview(final int id) async {
    final response = await request.get('api/v1/app/Customers/$id/financial-overview');

    return CustomerFinancialOverview.fromJson(response.data);
  }

  Future<int> createCustomer(final User user) async {
    final response = await request.post(
      'api/v1/app/Customers',
      data:
          user.toJson()
            ..remove('id')
            ..remove('attachments')
            ..remove('branch'),
    );

    return response.data['id'];
  }

  Future<void> editCustomer(final User user) async {
    await request.put(
      'api/v1/app/Customers/${user.id}',
      data:
          user.toJson()
            ..remove('id')
            ..remove('attachments')
            ..remove('branch'),
    );
  }
}
