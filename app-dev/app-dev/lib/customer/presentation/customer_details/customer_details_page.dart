import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/customer/infrastructure/repositories/customer_repository.dart';
import 'package:team/customer/presentation/customer_details/bloc/customer_details_bloc.dart';
import 'package:team/customer/presentation/customer_details/widgets/customer_financial_overview_widget.dart';
import 'package:team/customer/presentation/customer_details/widgets/customer_header_buttons.dart';
import 'package:team/customer/presentation/customer_details/widgets/customer_info_view.dart';

import 'widgets/business_info_view.dart';

class CustomerDetailsPage extends StatelessWidget {
  final int id;
  const CustomerDetailsPage({
    super.key,
    required this.id,
  });

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create:
          (BuildContext context) => CustomerDetailsBloc(
            customerRepository: GetIt.I.get<CustomerRepository>(),
            accountMapper: GetIt.I.get<AccountMapper>(),
          ),
      child: CustomerDetailsPageView(
        id: id,
      ),
    );
  }
}

class CustomerDetailsPageView extends StatefulWidget {
  final int id;
  const CustomerDetailsPageView({super.key, required this.id});

  @override
  State<CustomerDetailsPageView> createState() => _CustomerDetailsPageViewState();
}

class _CustomerDetailsPageViewState extends State<CustomerDetailsPageView> {
  late final _bloc = context.read<CustomerDetailsBloc>();

  @override
  void initState() {
    _bloc.add(InitialCustomerDetailsEvent(id: widget.id));
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: DefaultAppBar(
        title: 'العملاء',
        firstIcon: SizedBox(width: 40),
      ),
      body: ListView(
        padding: EdgeInsets.symmetric(horizontal: 22, vertical: 24),
        children: [
          CustomerHeaderButtons(),
          SizedBox(height: 16),
          DefaultBuilder<CustomerDetailsBloc>(
            buildWhen: (previous, state) => state.event is InitialCustomerDetailsEvent,
            builder: (context, state) {
              if (state is ErrorState) return SizedBox();
              return CustomerInfoView(user: _bloc.user);
            },
          ),

          DefaultBuilder<CustomerDetailsBloc>(
            buildWhen: (previous, state) => state.event is InitialCustomerDetailsEvent,
            builder: (context, state) {
              if (state is ErrorState) return SizedBox();
              return BusinessInfoView(user: _bloc.user);
            },
          ),

          DefaultBuilder<CustomerDetailsBloc>(
            buildWhen: (previous, state) => state.event is GetUsersFinancialOverview,
            builder: (context, state) {
              if (state is ErrorState) return SizedBox();
              return CustomerFinancialOverviewWidget(customerFinancialOverview: _bloc.customerFinancialOverview);
            },
          ),
        ],
      ),
    );
  }
}
