import 'core/database/app_database.dart';
import 'core/location/location_service.dart';
import 'core/network/dio_client.dart';
import 'core/security/secure_session_store.dart';
import 'features/auth/data/auth_remote_datasource.dart';
import 'features/auth/data/auth_repository_impl.dart';
import 'features/auth/domain/repositories/auth_repository.dart';
import 'features/auth/domain/usecases/login_usecase.dart';
import 'features/auth/domain/usecases/logout_usecase.dart';
import 'features/customers/data/customers_local_datasource.dart';
import 'features/customers/data/customers_remote_datasource.dart';
import 'features/customers/data/customers_repository_impl.dart';
import 'features/customers/domain/entities/customer.dart';
import 'features/customers/domain/repositories/customers_repository.dart';
import 'features/customers/domain/usecases/get_local_customers_usecase.dart';
import 'features/customers/domain/usecases/refresh_customers_usecase.dart';
import 'features/home/data/announcements_remote_datasource.dart';
import 'features/home/data/home_repository_impl.dart';
import 'features/home/domain/entities/delegate_home_snapshot.dart';
import 'features/home/domain/repositories/home_repository.dart';
import 'features/home/domain/usecases/get_home_snapshot_usecase.dart';
import 'features/payments/data/first_collection_eval_remote.dart';
import 'features/payments/data/payment_sync_service.dart';
import 'features/payments/data/payments_local_datasource.dart';
import 'features/payments/data/payments_remote_datasource.dart';
import 'features/payments/data/payments_repository_impl.dart';
import 'features/payments/domain/repositories/payments_repository.dart';
import 'features/payments/domain/usecases/collect_payment_usecase.dart';
import 'features/payments/domain/usecases/sync_pending_payments_usecase.dart';

/// Manual service locator — no get_it.
abstract final class Injection {
  static late final SecureSessionStore sessionStore;
  static late final AppDatabase database;
  static late final DioClient dioClient;
  static late final LocationService locationService;

  static late final AuthRemoteDataSource authRemote;
  static late final AuthRepository authRepository;
  static late final LoginUseCase loginUseCase;
  static late final LogoutUseCase logoutUseCase;

  static late final CustomersLocalDataSource customersLocal;
  static late final CustomersRemoteDataSource customersRemote;
  static late final CustomersRepository customersRepository;
  static late final GetLocalCustomersUseCase getLocalCustomersUseCase;
  static late final RefreshCustomersUseCase refreshCustomersUseCase;

  static late final PaymentsLocalDataSource paymentsLocal;
  static late final PaymentsRemoteDataSource paymentsRemote;
  static late final PaymentsRepository paymentsRepository;
  static late final CollectPaymentUseCase collectPaymentUseCase;
  static late final SyncPendingPaymentsUseCase syncPendingPaymentsUseCase;
  static late final PaymentSyncService paymentSyncService;
  static late final FirstCollectionEvalRemote firstCollectionEvalRemote;

  static late final HomeRepository homeRepository;
  static late final GetHomeSnapshotUseCase getHomeSnapshotUseCase;

  static bool _ready = false;

  static Future<void> init() async {
    if (_ready) return;

    sessionStore = SecureSessionStore();
    database = AppDatabase();
    dioClient = DioClient(sessionStore);
    locationService = LocationService();

    paymentsLocal = PaymentsLocalDataSource(database);
    paymentsRemote = PaymentsRemoteDataSource(dioClient, sessionStore);
    customersLocal = CustomersLocalDataSource(database);
    customersRemote = CustomersRemoteDataSource(dioClient, sessionStore);

    authRemote = AuthRemoteDataSource(dioClient);
    authRepository = AuthRepositoryImpl(
      remote: authRemote,
      session: sessionStore,
      paymentsLocal: paymentsLocal,
    );
    loginUseCase = LoginUseCase(authRepository);
    logoutUseCase = LogoutUseCase(authRepository);

    customersRepository = CustomersRepositoryImpl(
      remote: customersRemote,
      local: customersLocal,
      session: sessionStore,
    );
    getLocalCustomersUseCase = GetLocalCustomersUseCase(customersRepository);
    refreshCustomersUseCase = RefreshCustomersUseCase(customersRepository);

    paymentsRepository = PaymentsRepositoryImpl(
      local: paymentsLocal,
      remote: paymentsRemote,
      customersLocal: customersLocal,
      session: sessionStore,
      location: locationService,
    );
    collectPaymentUseCase = CollectPaymentUseCase(paymentsRepository);
    syncPendingPaymentsUseCase =
        SyncPendingPaymentsUseCase(paymentsRepository);
    paymentSyncService = PaymentSyncService(syncPendingPaymentsUseCase);
    firstCollectionEvalRemote = FirstCollectionEvalRemote(dioClient);

    homeRepository = HomeRepositoryImpl(
      customersLocal: customersLocal,
      paymentsLocal: paymentsLocal,
      session: sessionStore,
      announcementsRemote: AnnouncementsRemoteDataSource(dioClient),
    );
    getHomeSnapshotUseCase = GetHomeSnapshotUseCase(homeRepository);

    _ready = true;
  }

  static Future<DelegateHomeSnapshot> getHomeSnapshot() =>
      getHomeSnapshotUseCase();

  static Future<List<Customer>> getLocalCustomers({String? query}) =>
      getLocalCustomersUseCase(query: query);

  static Future<int> refreshCustomers({bool force = false}) =>
      refreshCustomersUseCase(force: force);

  static Future<bool> collectPayment({
    required int customerId,
    required String customerName,
    required double amount,
  }) =>
      collectPaymentUseCase(
        customerId: customerId,
        customerName: customerName,
        amount: amount,
      );
}
