import 'package:team/account/domain/user/user.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/customer/domain/customer_financial_overview.dart';
import 'package:team/customer/infrastructure/repositories/customer_repository.dart';

part 'customer_details_event.dart';
part 'customer_details_state.dart';

class CustomerDetailsBloc extends DefaultBloc {
  final CustomerRepository customerRepository;
  final AccountMapper accountMapper;
  CustomerDetailsBloc({required this.customerRepository, required this.accountMapper})
    : super(CustomerDetailsInitialState()) {
    safeOn<InitialCustomerDetailsEvent>(_initialCustomerDetails);
    safeOn<GetUsersFinancialOverview>(_getUsersFinancialOverview);
  }

  User? user;
  CustomerFinancialOverview? customerFinancialOverview;

  void _initialCustomerDetails(InitialCustomerDetailsEvent event, emit) async {
    await Future.delayed(Duration(seconds: 1));
    add(GetUsersFinancialOverview(event.id));
    user = await accountMapper.getUser(event.id);
  }

  void _getUsersFinancialOverview(GetUsersFinancialOverview event, emit) async {
    customerFinancialOverview = await customerRepository.getUserFinancialOverview(event.id);
  }
}
