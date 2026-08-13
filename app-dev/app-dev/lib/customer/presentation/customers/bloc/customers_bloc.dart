import 'package:team/account/domain/user_info/user_info.dart';
import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/customer/infrastructure/repositories/customer_repository.dart';
import 'package:team/order/domain/order_list/order_list.dart';
import 'package:team/payment/mapper.dart';

part 'customers_event.dart';
part 'customers_state.dart';

class CustomersBloc extends DefaultBloc {
  final CustomerRepository customerRepository;
  final PaymentMapper paymentMapper;
  CustomersBloc({required this.customerRepository, required this.paymentMapper}) : super(CustomersInitialState()) {
    safeOn<InitialCustomersEvent>(
      _initialCustomers,
      onCatch: (event, emit) async => isFetchingInitialCustomers = false,
    );
    safeOn<GetOrdersList>(_getOrdersList);
    safeOn<AddRecentSearch>(_addRecentSearches);
    safeOn<GetRecentSearches>(_getRecentSearches);
    safeOn<ClearRecentSearches>(_clearRecentSearches);
  }

  List<UserInfo>? customers;
  List<UserInfo>? searchedCustomers;
  List<UserInfo>? recentSearches;
  bool isFetchingInitialCustomers = false;
  List<OrderList>? orderListItem;
  ExpansionOption? selectedOrderItem;
  DateTime? startDate;
  DateTime? endDate;
  String? searchText;

  void _initialCustomers(InitialCustomersEvent event, emit) async {
    if (!isFetchingInitialCustomers) {
      isFetchingInitialCustomers = true;
      List<UserInfo> response = await customerRepository.getCustomers(
        searchText: searchText,
        lastDateTime: event.lastDateTime,
      );

      if (searchText == null) {
        customers = response;
      } else {
        searchedCustomers = response;
      }

      isFetchingInitialCustomers = false;
    }
  }

  void _getOrdersList(GetOrdersList event, emit) async {
    orderListItem = await paymentMapper.getOrdersList();
  }

  void _clearRecentSearches(ClearRecentSearches event, emit) async {
    await customerRepository.saveRecentSearches([]);
    add(GetRecentSearches());
  }

  void _getRecentSearches(GetRecentSearches event, emit) async {
    searchedCustomers = null;
    searchText = null;
    recentSearches = customerRepository.getRecentSearches();
  }

  void _addRecentSearches(AddRecentSearch event, emit) async {
    final recentSearchesList = List<UserInfo>.from(recentSearches ?? []);

    // Remove any existing item with the same ID
    recentSearchesList.removeWhere((item) => item.id == event.customer.id);

    // Insert the new/updated item at the beginning
    recentSearchesList.insert(0, event.customer);

    // Save the updated list
    await customerRepository.saveRecentSearches(recentSearchesList);
  }
}
