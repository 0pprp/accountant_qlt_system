import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:get_it/get_it.dart';
import 'package:team/account/presentation/entry/entry_page.dart';
import 'package:team/account/presentation/splash/splash_page.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/services/navigation/routes.dart';
import 'package:team/common/ui/theme/app_theme.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/customer/presentation/customer_details/customer_details_page.dart';
import 'package:team/customer/presentation/customers/customers_page.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';
import 'package:team/home/presentation/main/main_page.dart';
import 'package:team/home/presentation/notifications/notification_page.dart';
import 'package:team/order/presentation/create_order/create_order_page.dart';
import 'package:team/order/presentation/customer_orders/customer_orders.dart';
import 'package:team/order/presentation/order_details/order_details_page.dart';
import 'package:team/order/presentation/orders/orders_page.dart';
import 'package:team/payment/presentation/payments/payments_page.dart';

import 'common/ui/theme/app_color.dart';
import 'payment/presentation/customer_payments/customer_payments.dart';
import 'payment/presentation/order_payment_details/order_payment_details_page.dart';

class App extends StatelessWidget {
  const App({super.key});

  Route? _onGenerateRoute(RouteSettings settings) {
    FocusManager.instance.primaryFocus?.unfocus();
    final Widget child;

    switch (settings.name) {
      case RouteNames.main:
        child = SplashPage();
        break;

      case RouteNames.entry:
        child = EntryPage();
        break;

      case RouteNames.home:
        child = MainPage();
        break;

      case RouteNames.notifications:
        child = NotificationsPage();
        break;

      case RouteNames.ordersDetails:
        child = OrderDetailsPage(
          order: (settings.arguments as Map)['order'],
          userId: (settings.arguments as Map)['userId'],
          installmentPaymentId: (settings.arguments as Map)['installmentPaymentId'],
          dailyInstallmentAmount: (settings.arguments as Map)['dailyInstallmentAmount'],
        );
        break;

      case RouteNames.customerDetails:
        child = CustomerDetailsPage(id: settings.arguments as int);
        break;

      case RouteNames.customerOrders:
        child = CustomerOrdersPage(
          isCurrentAdmin: (settings.arguments as Map)['isCurrentAdmin'] ?? false,
          userId: (settings.arguments as Map)['userId'],
        );
        break;

      case RouteNames.customerPayments:
        child = CustomerPaymentsPage(userId: settings.arguments as int);
        break;

      case RouteNames.customerPaymentDetails:
        child = OrderPaymentDetailsPage(id: settings.arguments as int);
        break;

      case RouteNames.ordersSearch:
        child = OrdersPage(
          isSearching: true,
          orderListId: settings.arguments as int?,
        );
        break;

      case RouteNames.customersSearch:
        child = CustomersPage(isSearching: true);
        break;

      case RouteNames.paymentsSearch:
        child = PaymentsPage(
          isSearching: true,
          orderListId: settings.arguments as int?,
        );
        break;

      case RouteNames.createOrder:
        child = CreateOrderPage(
          order: (settings.arguments as Map)['order'],
          currentOrderListId: (settings.arguments as Map)['currentOrderListId'],
        );
        break;

      default:
        child = const SizedBox();
    }

    return MaterialPageRoute(builder: (context) => child, settings: settings);
  }

  @override
  Widget build(BuildContext context) {
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: const SystemUiOverlayStyle(
        statusBarColor: AppColor.surface1,
        statusBarIconBrightness: Brightness.dark,
        statusBarBrightness: Brightness.dark,
      ),
      child: DefaultBlocProvider.value(
        value: GetIt.I.get<MainBloc>(),
        child: SafeArea(
          top: false,
          child: MaterialApp(
            title: 'فريق قلعه الضمان',
            scrollBehavior: MyScrollBehavior().copyWith(physics: ClampingScrollPhysics()),
            debugShowCheckedModeBanner: false,
            navigatorKey: Routes.navigationKey,
            theme: AppTheme.lightTheme,
            locale: const Locale("ar"),
            supportedLocales: const [Locale("ar")],
            localizationsDelegates: [
              GlobalCupertinoLocalizations.delegate,
              GlobalMaterialLocalizations.delegate,
              GlobalWidgetsLocalizations.delegate,
            ],
            onGenerateRoute: _onGenerateRoute,
          ),
        ),
      ),
    );
  }
}

class MyScrollBehavior extends MaterialScrollBehavior {
  @override
  Set<PointerDeviceKind> get dragDevices => {PointerDeviceKind.touch, PointerDeviceKind.mouse};
}
