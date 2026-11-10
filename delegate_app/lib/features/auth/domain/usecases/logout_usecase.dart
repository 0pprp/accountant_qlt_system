import '../repositories/auth_repository.dart';

class LogoutBlockedException implements Exception {
  LogoutBlockedException(this.pendingCount);
  final int pendingCount;

  @override
  String toString() =>
      'لا يمكن الخروج: يوجد $pendingCount تسديد معلّق. أرسلها أولاً أو اختر الخروج القسري.';
}

class LogoutUseCase {
  LogoutUseCase(this._repo);

  final AuthRepository _repo;

  Future<void> call({bool force = false}) async {
    if (!force) {
      final pending = await _repo.pendingPaymentsCount();
      if (pending > 0) throw LogoutBlockedException(pending);
    }
    await _repo.logout(force: force);
  }
}
