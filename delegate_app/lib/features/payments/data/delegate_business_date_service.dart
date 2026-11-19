import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../../core/time/iraq_date.dart';
import 'payment_sync_service.dart';
import 'payments_local_datasource.dart';

/// دخول يوم العمل الجديد عند 3 صباحاً بغداد — يصفر مخزن المسددين ويحدّث اللوكال.
class DelegateBusinessDateService with WidgetsBindingObserver {
  DelegateBusinessDateService({
    required PaymentsLocalDataSource local,
    required PaymentSyncService syncService,
    this.onRollover,
  })  : _local = local,
        _syncService = syncService;

  static const prefsKey = 'lastBusinessDate';

  /// يزيد عند كل ترحيل يوم — الشاشات تعيد التحميل.
  static final ValueNotifier<int> dayRevision = ValueNotifier(0);

  final PaymentsLocalDataSource _local;
  final PaymentSyncService _syncService;
  final FutureOr<void> Function()? onRollover;

  bool _started = false;
  bool _rolling = false;
  Timer? _nextRolloverTimer;

  Future<void> start() async {
    if (_started) return;
    _started = true;
    WidgetsBinding.instance.addObserver(this);
    await ensureCurrentBusinessDay();
    _scheduleNextRollover();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      unawaited(_onResume());
    }
  }

  Future<void> _onResume() async {
    await ensureCurrentBusinessDay();
    _scheduleNextRollover();
  }

  Future<void> dispose() async {
    WidgetsBinding.instance.removeObserver(this);
    _nextRolloverTimer?.cancel();
    _nextRolloverTimer = null;
    _started = false;
  }

  /// true عند تغيّر يوم العمل أو أول ختم.
  Future<bool> ensureCurrentBusinessDay({DateTime? utcNow}) async {
    if (_rolling) return false;
    _rolling = true;
    try {
      final current = IraqDate.isoDate(utcNow);
      final prefs = await SharedPreferences.getInstance();
      final last = prefs.getString(prefsKey);
      if (last == current) return false;

      final isFirstStamp = last == null || last.isEmpty;

      await _local.clearPaidCollectionsBefore(current);
      await _local.clearDayLocksBefore(current);
      await prefs.setString(prefsKey, current);
      dayRevision.value = dayRevision.value + 1;

      if (!isFirstStamp) {
        unawaited(_syncService.syncNow());
        try {
          await onRollover?.call();
        } catch (_) {}
      }
      return true;
    } catch (_) {
      return false;
    } finally {
      _rolling = false;
    }
  }

  void _scheduleNextRollover({DateTime? utcNow}) {
    _nextRolloverTimer?.cancel();
    final delay = IraqDate.delayUntilNextBusinessDayStart(utcNow);
    final safe = delay < const Duration(seconds: 1)
        ? const Duration(seconds: 1)
        : delay;
    debugPrint(
      '[BusinessDay] next rollover in ${safe.inMinutes}m '
      '(${IraqDate.isoDate(utcNow)})',
    );
    _nextRolloverTimer = Timer(safe, () async {
      await ensureCurrentBusinessDay();
      _scheduleNextRollover();
    });
  }
}
