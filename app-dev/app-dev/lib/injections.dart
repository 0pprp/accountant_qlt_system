import 'package:get_it/get_it.dart';
import 'package:team/account/infrastructure/provider/api_account_provider.dart';
import 'package:team/account/infrastructure/provider/local_account_provider.dart';
import 'package:team/account/infrastructure/repository/account_repository.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/services/request/request.dart';
import 'package:team/customer/infrastructure/providers/api_customer_provider.dart';
import 'package:team/customer/infrastructure/providers/local_customer_provider.dart';
import 'package:team/customer/infrastructure/repositories/customer_repository.dart';
import 'package:team/customer/mapper.dart';
import 'package:team/home/infrastructure/providers/api_home_provider.dart';
import 'package:team/home/infrastructure/providers/local_home_provider.dart';
import 'package:team/home/infrastructure/repositories/home_repository.dart';
import 'package:team/home/mapper.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';
import 'package:team/order/infrastructure/providers/api_order_provider.dart';
import 'package:team/order/infrastructure/providers/local_order_provider.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';
import 'package:team/order/mapper.dart';
import 'package:team/payment/mapper.dart';

import 'payment/infrastructure/providers/api_payment_provider.dart';
import 'payment/infrastructure/providers/local_payment_provider.dart';
import 'payment/infrastructure/repositories/payment_repository.dart';

class Injections {
  static final _serviceLocator = GetIt.I;

  // static const String baseUrl = 'http://77.238.108.116:6075/';
  static const String baseUrl = 'https://api.qalataldhaman.com/';

  static Future<void> ensureInitialized(HandleTokenExpireException handleTokenExpireException) async {
    /// common
    _ensureInitializedCommon(handleTokenExpireException);

    /// provider
    _ensureInitializedProviders();

    /// repository
    _ensureInitializedRepositories();

    /// mapper
    _ensureInitializedMappers();

    /// initializing some classes that need to be initializing asynchronously
    await _prepare();
  }

  /// provider
  static void _ensureInitializedProviders() {
    _serviceLocator.registerLazySingleton(() => LocalHomeProvider());
    _serviceLocator.registerLazySingleton(() => ApiHomeProvider(request: _serviceLocator()));
    _serviceLocator.registerLazySingleton(() => LocalAccountProvider());
    _serviceLocator.registerLazySingleton(() => ApiAccountProvider(request: _serviceLocator()));
    _serviceLocator.registerLazySingleton(() => LocalOrderProvider());
    _serviceLocator.registerLazySingleton(() => ApiOrderProvider(request: _serviceLocator()));
    _serviceLocator.registerLazySingleton(() => LocalPaymentProvider());
    _serviceLocator.registerLazySingleton(() => ApiPaymentProvider(request: _serviceLocator()));
    _serviceLocator.registerLazySingleton(() => LocalCustomerProvider());
    _serviceLocator.registerLazySingleton(() => ApiCustomerProvider(request: _serviceLocator()));
  }

  /// repository
  static void _ensureInitializedRepositories() {
    _serviceLocator.registerLazySingleton(
      () => HomeRepository(apiHomeProvider: _serviceLocator(), localHomeProvider: _serviceLocator()),
    );
    _serviceLocator.registerLazySingleton(
      () => AccountRepository(apiAuthProvider: _serviceLocator(), localAccountProvider: _serviceLocator()),
    );
    _serviceLocator.registerLazySingleton(
      () => OrderRepository(apiOrderProvider: _serviceLocator(), localOrderProvider: _serviceLocator()),
    );
    _serviceLocator.registerLazySingleton(
      () => PaymentRepository(apiPaymentProvider: _serviceLocator(), localPaymentProvider: _serviceLocator()),
    );
    _serviceLocator.registerLazySingleton(
      () => CustomerRepository(apiCustomerProvider: _serviceLocator(), localCustomerProvider: _serviceLocator()),
    );
  }

  /// mapper
  static void _ensureInitializedMappers() {
    _serviceLocator.registerLazySingleton(() => HomeMapper());
    _serviceLocator.registerLazySingleton(() => AccountMapper());
    _serviceLocator.registerLazySingleton(() => OrderMapper());
    _serviceLocator.registerLazySingleton(() => PaymentMapper());
    _serviceLocator.registerLazySingleton(() => CustomerMapper());
  }

  /// common
  static void _ensureInitializedCommon(HandleTokenExpireException handleTokenExpireException) {
    _serviceLocator.registerSingleton(MainBloc());
    _serviceLocator.registerLazySingleton(
      () => Request(baseUrl: baseUrl, handleTokenExpireException: handleTokenExpireException),
    );
  }

  static Future<void> _prepare() async {
    await _serviceLocator.get<AccountMapper>().ensureInitialized();
    await _serviceLocator.get<HomeMapper>().ensureInitialized();
    await _serviceLocator.get<PaymentMapper>().ensureInitialized();
    await _serviceLocator.get<OrderMapper>().ensureInitialized();
    await _serviceLocator.get<CustomerMapper>().ensureInitialized();
  }
}
