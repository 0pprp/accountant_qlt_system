import 'dart:async';

import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:geolocator/geolocator.dart';
import 'package:sales_employee_app/core/location/location_service.dart';
import 'package:sales_employee_app/core/time/baghdad_time.dart';
import 'package:sales_employee_app/features/shift/data/location_queue_datasource.dart';

/// Continuous GPS trail while duty is active (Waze-style point stream → server).
class DutyLocationTracker {
  DutyLocationTracker({
    required LocationService location,
    required LocationQueueDataSource queue,
  })  : _location = location,
        _queue = queue;

  final LocationService _location;
  final LocationQueueDataSource _queue;

  StreamSubscription<Position>? _sub;
  Timer? _flushTimer;
  Timer? _heartbeat;
  bool _running = false;
  DateTime? _lastSentAt;
  double? _lastLat;
  double? _lastLng;

  bool get isRunning => _running;

  Future<void> start() async {
    if (_running) return;
    final ok = await _location.ensurePermission();
    if (!ok) return;
    _running = true;

    // Immediate first point with Baghdad wall-clock stamp in queue.
    await _captureAndPersist(force: true);

    if (!kIsWeb) {
      _sub = Geolocator.getPositionStream(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.bestForNavigation,
          distanceFilter: 12,
        ),
      ).listen(
        (pos) => _onPosition(pos.latitude, pos.longitude, force: false),
        onError: (_) {},
      );
    }

    // Heartbeat every 10s even if stationary (trail continuity).
    _heartbeat = Timer.periodic(const Duration(seconds: 10), (_) async {
      await _captureAndPersist(force: false);
    });

    _flushTimer = Timer.periodic(const Duration(seconds: 20), (_) async {
      await _queue.flush();
    });
  }

  Future<void> stop() async {
    _running = false;
    await _sub?.cancel();
    _sub = null;
    _heartbeat?.cancel();
    _heartbeat = null;
    _flushTimer?.cancel();
    _flushTimer = null;
    await _queue.flush();
  }

  Future<void> _captureAndPersist({required bool force}) async {
    final point = await _location.current();
    if (point == null) return;
    await _onPosition(point.lat, point.lng, force: force);
  }

  Future<void> _onPosition(double lat, double lng, {required bool force}) async {
    if (!_running) return;
    final now = DateTime.now().toUtc();
    if (!force && _lastLat != null && _lastLng != null && _lastSentAt != null) {
      final moved = Geolocator.distanceBetween(_lastLat!, _lastLng!, lat, lng);
      final age = now.difference(_lastSentAt!);
      if (moved < 8 && age < const Duration(seconds: 10)) return;
    }
    _lastLat = lat;
    _lastLng = lng;
    _lastSentAt = now;

    // Always enqueue with explicit UTC timestamp (Baghdad wall-clock recoverable later).
    final stamp = now;
    await _queue.enqueue(lat: lat, lng: lng, recordedAtUtc: stamp);
    // Live ping for realtime + keep Baghdad note in local queue for batch history.
    await _queue.sendLive(lat: lat, lng: lng, recordedAtUtc: stamp, enqueueOnFailOnly: true);
  }

  /// Debug/status: last Baghdad time string of a point send.
  String? get lastBaghdadStamp {
    if (_lastSentAt == null) return null;
    return formatBaghdadDateTime(_lastSentAt!);
  }
}
