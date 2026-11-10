import '../entities/delegate_home_snapshot.dart';
import '../repositories/home_repository.dart';

class GetHomeSnapshotUseCase {
  GetHomeSnapshotUseCase(this._repo);
  final HomeRepository _repo;

  Future<DelegateHomeSnapshot> call() => _repo.loadHomeSnapshot();
}
