import 'dart:async';
import 'dart:io' show Platform;

import 'package:flutter/foundation.dart';
import 'package:geolocator/geolocator.dart';
import 'package:uuid/uuid.dart';

import '../../core/location/gps.dart';
import '../../core/validation/validators.dart';
import '../promoter/models.dart';
import '../promoter/promoter_api.dart';
import 'location_queue.dart';

class ShiftTracker {
  ShiftTracker(this._api, this._queue);

  static const minPointGap = Duration(seconds: 20);
  static const flushInterval = Duration(seconds: 60);
  static const flushThreshold = 200;
  static const maxAccuracyMeters = 5000.0;

  final PromoterApi _api;
  final LocationQueue _queue;
  final _uuid = const Uuid();

  final ValueNotifier<int> pending = ValueNotifier(0);
  final ValueNotifier<Position?> lastPosition = ValueNotifier(null);
  final ValueNotifier<bool> tracking = ValueNotifier(false);

  void Function()? onServerStopped;

  StreamSubscription<Position>? _stream;
  Timer? _timer;
  DateTime? _lastRecordedAt;

  Future<void> init() async {
    pending.value = await _queue.length();
  }

  Future<void> ensureRunning() async {
    if (_stream != null) return;
    await Gps.ensurePermission();
    if (_stream != null) return;
    _stream = Geolocator.getPositionStream(
      locationSettings: _settings(),
    ).listen((p) => unawaited(_onFix(p)), onError: (_) {});
    _timer ??= Timer.periodic(flushInterval, (_) => unawaited(flush()));
    tracking.value = true;
  }

  Future<void> stop({bool clearQueue = false}) async {
    _timer?.cancel();
    _timer = null;
    final s = _stream;
    _stream = null;
    tracking.value = false;
    await s?.cancel();
    if (clearQueue) {
      await _queue.clear();
      pending.value = 0;
    }
  }

  Future<void> syncWithServer(bool isWorking) async {
    if (isWorking) {
      try {
        await ensureRunning();
      } catch (_) {}
      unawaited(flush());
    } else if (_stream != null) {
      await stop(clearQueue: true);
    } else if (pending.value > 0) {
      await _queue.clear();
      pending.value = 0;
    }
  }

  Future<ShiftState> startShift() async {
    await Gps.ensurePermission();
    final state = await _api.startShift();
    if (state.isWorking) {
      await ensureRunning();
      unawaited(_recordOneShot());
    }
    return state;
  }

  Future<ShiftState> endShift() async {
    try {
      await _recordOneShot();
      await _queue.flushAll(_api.sendLocations);
    } catch (_) {}
    final state = await _api.endShift();
    await stop(clearQueue: true);
    return state;
  }

  Future<void> flush() async {
    final outcome = await _queue.flushAll(_api.sendLocations);
    pending.value = await _queue.length();
    if (outcome == FlushOutcome.stopped) {
      await stop(clearQueue: true);
      onServerStopped?.call();
    }
  }

  LocationSettings _settings() {
    if (Platform.isAndroid) {
      return AndroidSettings(
        accuracy: LocationAccuracy.high,
        distanceFilter: 0,
        intervalDuration: minPointGap,
        foregroundNotificationConfig: const ForegroundNotificationConfig(
          notificationTitle: 'المروّج الميداني',
          notificationText: 'الدوام مستمر — يتم تسجيل موقعك أثناء الدوام',
          notificationChannelName: 'تتبع الدوام',
          enableWakeLock: true,
          setOngoing: true,
        ),
      );
    }
    return const LocationSettings(
      accuracy: LocationAccuracy.high,
      distanceFilter: 0,
    );
  }

  Future<void> _recordOneShot() async {
    try {
      final p = await Gps.current();
      await _record(p, force: true);
    } catch (_) {}
  }

  Future<void> _onFix(Position p) => _record(p);

  Future<void> _record(Position p, {bool force = false}) async {
    if (!Gps.usable(p)) return;
    lastPosition.value = p;
    final now = DateTime.now().toUtc();
    final last = _lastRecordedAt;
    if (!force && last != null && now.difference(last) < minPointGap) return;
    if (!Validators.insideIraq(p.latitude, p.longitude)) return;
    final accuracy = p.accuracy.isFinite && p.accuracy >= 0 ? p.accuracy : null;
    if (accuracy != null && accuracy > maxAccuracyMeters) return;
    _lastRecordedAt = now;
    final stamp = p.timestamp.toUtc();
    final recordedAt =
        stamp.isAfter(now.add(const Duration(minutes: 1))) ||
            now.difference(stamp) > const Duration(minutes: 10)
        ? now
        : stamp;
    final count = await _queue.add(
      QueuedPoint(
        lat: p.latitude,
        lng: p.longitude,
        accuracy: accuracy,
        recordedAt: recordedAt,
        clientEventId: _uuid.v4(),
      ),
    );
    pending.value = count;
    if (count >= flushThreshold) unawaited(flush());
  }
}
