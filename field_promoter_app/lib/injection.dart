import 'core/network/dio_client.dart';
import 'core/session/session_store.dart';
import 'features/auth/auth_remote.dart';
import 'features/orders/order_draft.dart';
import 'features/promoter/promoter_api.dart';
import 'features/promoter/promoter_store.dart';
import 'features/shift/location_queue.dart';
import 'features/shift/shift_tracker.dart';

abstract final class Injection {
  static late final SessionStore session;
  static late final DioClient client;
  static late final AuthRemote auth;
  static late final PromoterApi api;
  static late final ShiftTracker tracker;
  static late final PromoterStore store;
  static late final OrderDraftStore drafts;
  static bool _ready = false;

  static Future<void> init() async {
    if (_ready) return;
    session = SessionStore();
    client = DioClient(sessionStore: session, onRefresh: () => auth.refresh());
    auth = AuthRemote(client, session);
    api = PromoterApi(client);
    tracker = ShiftTracker(api, LocationQueue(PrefsQueueStorage()));
    store = PromoterStore(api, tracker);
    drafts = OrderDraftStore();
    await tracker.init();
    _ready = true;
  }

  static Future<void> signOut() async {
    await tracker.stop(clearQueue: true);
    await auth.logout();
    await drafts.clear();
    store.reset();
  }
}
