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
  Future<int>? _running;
  final queueChanged = ValueNotifier<int>(0);

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

  Future<int> syncNow({SyncProgressCallback? onProgress}) {
    return _running ??= _run(onProgress);
  }

  Future<int> _run(SyncProgressCallback? onProgress) async {
    try {
      lastError = null;
      lastUploaded = 0;
      lastUploaded = await _syncUseCase(onProgress: onProgress);
      return lastUploaded;
    } catch (e) {
      lastError = '$e'.replaceFirst(RegExp(r'^[^:]+:\s*'), '');
      return 0;
    } finally {
      _running = null;
      if (lastUploaded > 0) queueChanged.value++;
    }
  }

  Future<void> dispose() async {
    WidgetsBinding.instance.removeObserver(this);
    await _sub?.cancel();
    _sub = null;
    _started = false;
  }
}
