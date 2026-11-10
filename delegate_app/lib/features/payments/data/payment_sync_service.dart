import 'dart:async';

import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter/widgets.dart';

import '../domain/repositories/payments_repository.dart';
import '../domain/usecases/sync_pending_payments_usecase.dart';

/// Background sync on connectivity restore and app resume — no polling loop.
class PaymentSyncService with WidgetsBindingObserver {
  PaymentSyncService(this._syncUseCase);

  final SyncPendingPaymentsUseCase _syncUseCase;
  final Connectivity _connectivity = Connectivity();
  StreamSubscription<List<ConnectivityResult>>? _sub;
  bool _started = false;
  bool _wasOffline = true;
  bool _running = false;

  int lastUploaded = 0;
  String? lastError;

  Future<void> start() async {
    if (_started) return;
    _started = true;
    WidgetsBinding.instance.addObserver(this);
    _sub = _connectivity.onConnectivityChanged.listen((results) async {
      final online = results.any((r) => r != ConnectivityResult.none);
      if (online && _wasOffline) {
        await syncNow();
      }
      _wasOffline = !online;
    });
    final current = await _connectivity.checkConnectivity();
    _wasOffline = current.every((r) => r == ConnectivityResult.none);
    unawaited(syncNow());
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      unawaited(syncNow());
    }
  }

  Future<int> syncNow({SyncProgressCallback? onProgress}) async {
    if (_running) return lastUploaded;
    _running = true;
    try {
      lastError = null;
      lastUploaded = await _syncUseCase(onProgress: onProgress);
      return lastUploaded;
    } catch (e) {
      lastError = '$e'.replaceFirst(RegExp(r'^[^:]+:\s*'), '');
      return 0;
    } finally {
      _running = false;
    }
  }

  Future<void> dispose() async {
    WidgetsBinding.instance.removeObserver(this);
    await _sub?.cancel();
    _sub = null;
    _started = false;
  }
}
