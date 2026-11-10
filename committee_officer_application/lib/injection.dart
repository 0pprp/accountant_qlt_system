import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/session/session_store.dart';
import 'package:committee_officer_application/features/auth/data/auth_remote.dart';
import 'package:committee_officer_application/features/home/data/announcements_remote.dart';
import 'package:committee_officer_application/features/home/data/committee_remote.dart';
import 'package:committee_officer_application/features/shift/data/shift_remote.dart';

abstract final class Injection {
  static late final SessionStore session;
  static late final DioClient dio;
  static late final AuthRemote auth;
  static late final CommitteeRemote committee;
  static late final ShiftRemote shift;
  static late final AnnouncementsRemote announcements;
  static bool _ready = false;

  static Future<void> init() async {
    if (_ready) return;
    session = SessionStore();
    dio = DioClient(
      sessionStore: session,
      onRefresh: () => auth.refresh(),
    );
    auth = AuthRemote(dio, session);
    committee = CommitteeRemote(dio);
    shift = ShiftRemote(dio, session);
    announcements = AnnouncementsRemote();
    _ready = true;
  }
}
