import 'package:sales_employee_app/core/cache/app_cache.dart';
import 'package:sales_employee_app/core/database/app_database.dart';
import 'package:sales_employee_app/core/location/location_service.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/session/session_store.dart';
import 'package:sales_employee_app/features/auth/data/auth_remote_datasource.dart';
import 'package:sales_employee_app/features/auth/data/auth_repository_impl.dart';
import 'package:sales_employee_app/features/auth/domain/repositories/auth_repository.dart';
import 'package:sales_employee_app/features/catalog/data/catalog_remote_datasource.dart';
import 'package:sales_employee_app/features/customers/data/customers_remote_datasource.dart';
import 'package:sales_employee_app/features/ratings/data/ratings_remote_datasource.dart';
import 'package:sales_employee_app/features/sales/data/demo_orders_store.dart';
import 'package:sales_employee_app/features/sales/data/sales_remote_datasource.dart';
import 'package:sales_employee_app/features/sales/realtime/sales_request_realtime_client.dart';
import 'package:sales_employee_app/features/shift/data/duty_location_tracker.dart';
import 'package:sales_employee_app/features/shift/data/duty_session_store.dart';
import 'package:sales_employee_app/features/shift/data/location_queue_datasource.dart';
import 'package:sales_employee_app/features/shift/data/shift_remote_datasource.dart';
import 'package:sales_employee_app/features/updates/data/app_version_datasource.dart';

abstract final class Injection {
  static late final SessionStore sessionStore;
  static late final AppDatabase database;
  static late final AppCache appCache;
  static late final DioClient dioClient;
  static late final LocationService locationService;

  static late final AuthRemoteDataSource authRemote;
  static late final AuthRepository authRepository;

  static late final CustomersRemoteDataSource customersRemote;
  static late final CatalogRemoteDataSource catalogRemote;
  static late final SalesRemoteDataSource salesRemote;
  static late final SalesRequestRealtimeClient salesRealtime;
  static late final DemoOrdersStore demoOrders;
  static late final ShiftRemoteDataSource shiftRemote;
  static late final DutySessionStore dutySession;
  static late final LocationQueueDataSource locationQueue;
  static late final DutyLocationTracker dutyTracker;
  static late final RatingsRemoteDataSource ratingsRemote;
  static late final AppVersionDataSource appVersion;

  static bool _ready = false;

  static Future<void> init() async {
    if (_ready) return;

    sessionStore = SessionStore();
    database = AppDatabase();
    appCache = AppCache(database);
    locationService = LocationService();

    dioClient = DioClient(
      sessionStore: sessionStore,
      onRefresh: () => authRepository.refresh(),
    );
    authRemote = AuthRemoteDataSource(dioClient);
    authRepository = AuthRepositoryImpl(remote: authRemote, session: sessionStore);

    customersRemote = CustomersRemoteDataSource(dioClient);
    catalogRemote = CatalogRemoteDataSource(dioClient);
    salesRemote = SalesRemoteDataSource(dioClient, cache: appCache);
    salesRealtime = SalesRequestRealtimeClient();
    demoOrders = DemoOrdersStore(database);
    shiftRemote = ShiftRemoteDataSource(dioClient);
    dutySession = DutySessionStore();
    locationQueue = LocationQueueDataSource(database, dioClient);
    dutyTracker = DutyLocationTracker(location: locationService, queue: locationQueue);
    ratingsRemote = RatingsRemoteDataSource(dioClient);
    appVersion = AppVersionDataSource(dioClient);

    _ready = true;
  }
}
