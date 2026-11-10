import '../entities/delegate_home_snapshot.dart';

abstract class HomeRepository {
  Future<DelegateHomeSnapshot> loadHomeSnapshot();
}
