import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';
import 'package:team/order/presentation/customer_orders/bloc/customer_orders_bloc.dart';
import 'package:team/order/presentation/customer_orders/widgets/order_info_card.dart';

class CustomerOrdersPage extends StatelessWidget {
  final int userId;
  final bool? isCurrentAdmin;
  const CustomerOrdersPage({super.key, this.isCurrentAdmin, required this.userId});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create: (BuildContext context) => CustomerOrdersBloc(orderRepository: GetIt.I.get<OrderRepository>()),
      child: CustomerOrdersPageView(isCurrentAdmin: isCurrentAdmin ?? false, userId: userId),
    );
  }
}

class CustomerOrdersPageView extends StatefulWidget {
  final int userId;
  final bool isCurrentAdmin;
  const CustomerOrdersPageView({super.key, required this.userId, required this.isCurrentAdmin});

  @override
  State<CustomerOrdersPageView> createState() => _CustomerOrdersPageViewState();
}

class _CustomerOrdersPageViewState extends State<CustomerOrdersPageView> {
  late final _bloc = context.read<CustomerOrdersBloc>();

  @override
  void initState() {
    _bloc.add(InitialCustomerOrdersEvent(widget.userId));
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: DefaultAppBar(title: widget.isCurrentAdmin ? 'الديون' : 'مبيعات العميل', firstIcon: SizedBox(width: 40)),
      body: DefaultBuilder<CustomerOrdersBloc>(
        buildWhen: (previous, state) => state.event is InitialCustomerOrdersEvent,
        builder: (context, state) {
          if (state is ErrorState) {
            return DefaultErrorWidget(
              error: state.error,
              onRetry: () => _bloc.add(InitialCustomerOrdersEvent(widget.userId)),
            );
          }
          if (_bloc.customerOrders != null) {
            return ListView.builder(
              padding: EdgeInsets.symmetric(horizontal: 22, vertical: 24),
              itemCount: _bloc.customerOrders!.length,
              itemBuilder: (context, index) {
                return Padding(
                  padding: const EdgeInsets.only(bottom: 24),
                  child: OrderInfoCard(orderDetails: _bloc.customerOrders![index]),
                );
              },
            );
          }
          return const DefaultLoadingWidget();
        },
      ),
    );
  }
}
