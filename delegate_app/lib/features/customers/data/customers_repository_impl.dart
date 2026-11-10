import 'package:flutter/foundation.dart';

import '../../../core/security/secure_session_store.dart';
import '../domain/entities/customer.dart';
import '../domain/repositories/customers_repository.dart';
import 'customers_local_datasource.dart';
import 'customers_remote_datasource.dart';

class CustomersRepositoryImpl implements CustomersRepository {
  CustomersRepositoryImpl({
    required CustomersRemoteDataSource remote,
    required CustomersLocalDataSource local,
    required SecureSessionStore session,
  })  : _remote = remote,
        _local = local,
        _session = session;

  final CustomersRemoteDataSource _remote;
  final CustomersLocalDataSource _local;
  final SecureSessionStore _session;

  DateTime? _lastRefreshAt;
  bool _refreshing = false;

  @override
  DateTime? get lastRefreshAt => _lastRefreshAt;

  @override
  Future<List<Customer>> getLocalCustomers({String? query}) =>
      _local.list(query: query);

  @override
  Future<Customer?> getLocalById(int id) => _local.byId(id);

  @override
  Future<int> refreshFromRemote({bool silent = true}) async {
    if (_refreshing) return 0;
    _refreshing = true;
    try {
      if (!await _session.hasSession) return 0;
      final remote = await _remote.fetchAllForDelegate();
      await _local.replaceAll(remote);
      _lastRefreshAt = DateTime.now();
      return remote.length;
    } catch (e, st) {
      if (!silent) rethrow;
      debugPrint('silent customer refresh failed: $e\n$st');
      return 0;
    } finally {
      _refreshing = false;
    }
  }
}
