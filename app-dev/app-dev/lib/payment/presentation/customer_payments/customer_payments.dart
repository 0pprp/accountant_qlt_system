import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/ui/widgets/payment_info_card.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/text_util.dart';
import 'package:team/payment/infrastructure/repositories/payment_repository.dart';
import 'package:vector_graphics/vector_graphics.dart';

import 'bloc/customer_payments_bloc.dart';

class CustomerPaymentsPage extends StatelessWidget {
  final int userId;
  const CustomerPaymentsPage({super.key, required this.userId});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create: (BuildContext context) => CustomerPaymentsBloc(paymentRepository: GetIt.I.get<PaymentRepository>()),
      child: CustomerPaymentsPageView(userId: userId),
    );
  }
}

class CustomerPaymentsPageView extends StatefulWidget {
  final int userId;
  const CustomerPaymentsPageView({super.key, required this.userId});

  @override
  State<CustomerPaymentsPageView> createState() => _CustomerPaymentsPageViewState();
}

class _CustomerPaymentsPageViewState extends State<CustomerPaymentsPageView> {
  late final _bloc = context.read<CustomerPaymentsBloc>();

  @override
  void initState() {
    _bloc.add(InitialCustomerPaymentsEvent(widget.userId));
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: DefaultAppBar(title: 'تسديدات العميل', firstIcon: SizedBox(width: 40)),
      body: DefaultBuilder<CustomerPaymentsBloc>(
        buildWhen: (previous, state) => state.event is InitialCustomerPaymentsEvent,
        builder: (context, state) {
          if (state is ErrorState) {
            return DefaultErrorWidget(
              error: state.error,
              onRetry: () => _bloc.add(InitialCustomerPaymentsEvent(widget.userId)),
            );
          }
          if (_bloc.installmentPayments != null) {
            return Padding(
              padding: EdgeInsets.symmetric(horizontal: 22, vertical: 24),
              child: Column(
                children: [
                  Container(
                    decoration: BoxDecoration(borderRadius: BorderRadius.circular(16)),
                    child: ClipRRect(
                      borderRadius: BorderRadius.circular(16),
                      child: Stack(
                        alignment: Alignment.center,
                        children: [
                          Positioned.fill(
                            child: VectorGraphic(
                              loader: AssetBytesLoader('assets/svg/row_background.svg'),
                              fit: BoxFit.cover,
                            ),
                          ),
                          Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Container(
                                margin: EdgeInsets.only(right: 16, bottom: 16, top: 16),
                                padding: EdgeInsets.symmetric(vertical: 10, horizontal: 16),
                                decoration: BoxDecoration(
                                  color: AppColor.surface2,
                                  borderRadius: BorderRadius.circular(16),
                                ),
                                child: Row(
                                  children: [
                                    Text(
                                      TextUtil.addComma(_bloc.installmentPayments!.length.toString()),
                                      style: AppTextStyle.numberLarge,
                                    ),
                                    Padding(
                                      padding: const EdgeInsets.only(right: 8, top: 2),
                                      child: Text('تسديد', style: AppTextStyle.bodyLarge.withColor(AppColor.text2)),
                                    ),
                                  ],
                                ),
                              ),
                              Expanded(
                                child: Container(
                                  margin: EdgeInsets.only(left: 16, right: 8),
                                  padding: EdgeInsets.symmetric(vertical: 10),
                                  decoration: BoxDecoration(
                                    color: AppColor.surface2,
                                    borderRadius: BorderRadius.circular(16),
                                  ),
                                  child: Center(
                                    child: Row(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        Text(
                                          TextUtil.addComma(_bloc.sumOfAllPayments.toString()),
                                          style: AppTextStyle.numberLarge,
                                        ),
                                        Padding(
                                          padding: const EdgeInsets.only(right: 4, top: 2),
                                          child: Text('د.ع', style: AppTextStyle.bodyLarge.withColor(AppColor.text2)),
                                        ),
                                      ],
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ),
                  SizedBox(height: 40),
                  Expanded(
                    child: ListView.builder(
                      itemCount: _bloc.installmentPayments!.length,
                      itemBuilder: (context, index) {
                        return Padding(
                          padding: const EdgeInsets.only(bottom: 10),
                          child: PaymentInfoCard(
                            installmentPayment: _bloc.installmentPayments![index],
                            onTap: () {
                              Navigator.pushNamed(
                                context,
                                RouteNames.customerPaymentDetails,
                                arguments: _bloc.installmentPayments![index].orderId,
                              );
                            },
                          ),
                        );
                      },
                    ),
                  ),
                ],
              ),
            );
          }
          return const DefaultLoadingWidget();
        },
      ),
    );
  }
}
