import 'dart:async';
import 'dart:convert';
import 'dart:io' show Platform;

import 'package:committee_officer_application/core/network/api_exception.dart';
import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/session/session_store.dart';
import 'package:committee_officer_application/core/time/baghdad_time.dart';
import 'package:geolocator/geolocator.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ShiftRemote {
  ShiftRemote(this._dio, this._session);

  static const sendInterval = Duration(seconds: 45);
  static const watchdogInterval = Duration(seconds: 60);
  static const maxQueuedPoints = 500;
  static const _kQueue = 'co_location_queue';

  final DioClient _dio;
  final SessionStore _session;
  StreamSubscription<Position>? _stream;
  Timer? _watchdog;
  DateTime? _lastSentAt;
  DateTime? _lastFixAt;
  bool _sending = false;

  bool get isTracking => _stream != null;

  Future<bool> currentActive() async {
    final res = await _dio.dio.get('api/v1/app/committee/shifts/current');
    final data = res.data;
    if (data is! Map) throw ApiException('تعذر التحقق من حالة الدوام');
    final active = data['hasActiveShift'] ?? data['HasActiveShift'];
    if (active is! bool) throw ApiException('استجابة حالة الدوام غير صالحة');
    if (active) {
      try {
        await _ensureGps();
        _startTracking();
      } catch (_) {}
      unawaited(_flushQueue());
    } else {
      stopLiveLoop();
      await _clearQueue();
    }
    return active;
  }

  Future<void> start() async {
    await _ensureGps();
    await _dio.dio.post('api/v1/app/committee/shifts/start');
    await _session.markDutyStarted(baghdadDateKey());
    await _clearQueue();
    _lastSentAt = null;
    await _sendOneShot();
    _startTracking();
  }

  Future<void> end() async {
    stopLiveLoop();
    try {
      await _sendOneShot();
      await _flushQueue();
    } catch (_) {}
    await _dio.dio.post('api/v1/app/committee/shifts/end');
    await _session.markDutyEnded();
    await _clearQueue();
  }

  void stopLiveLoop() {
    _watchdog?.cancel();
    _watchdog = null;
    unawaited(_stream?.cancel());
    _stream = null;
  }

  void _startTracking() {
    if (_stream != null) return;
    _stream = Geolocator.getPositionStream(locationSettings: _streamSettings()).listen(
      (p) => unawaited(_onFix(p)),
      onError: (_) {},
    );
    _watchdog ??= Timer.periodic(watchdogInterval, (_) {
      final last = _lastFixAt;
      if (last == null || DateTime.now().difference(last) >= watchdogInterval) {
        unawaited(_sendOneShot());
      }
      unawaited(_flushQueue());
    });
  }

  LocationSettings _streamSettings() {
    if (Platform.isAndroid) {
      return AndroidSettings(
        accuracy: LocationAccuracy.high,
        distanceFilter: 0,
        intervalDuration: const Duration(seconds: 20),
        foregroundNotificationConfig: const ForegroundNotificationConfig(
          notificationTitle: 'الدوام فعّال',
          notificationText: 'يتم تسجيل موقعك ومسارك أثناء الدوام',
          notificationChannelName: 'تتبع الدوام',
          enableWakeLock: true,
          setOngoing: true,
        ),
      );
    }
    return const LocationSettings(accuracy: LocationAccuracy.high, distanceFilter: 0);
  }

  Future<void> _onFix(Position p) async {
    _lastFixAt = DateTime.now();
    final last = _lastSentAt;
    if (last != null && DateTime.now().difference(last) < sendInterval) return;
    await _send(p);
  }

  Future<void> _sendOneShot() async {
    try {
      final p = await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.high,
          timeLimit: Duration(seconds: 15),
        ),
      );
      _lastFixAt = DateTime.now();
      await _send(p);
    } catch (_) {}
  }

  bool _usable(Position p) =>
      p.latitude.isFinite &&
      p.longitude.isFinite &&
      !(p.latitude.abs() < 1e-9 && p.longitude.abs() < 1e-9);

  Future<void> _send(Position p) async {
    if (!_usable(p)) return;
    final capturedAt = DateTime.now().toUtc();
    _lastSentAt = DateTime.now();
    final point = <String, dynamic>{
      'lat': p.latitude,
      'lng': p.longitude,
      'accuracyMeters': p.accuracy.isFinite && p.accuracy >= 0 ? p.accuracy : null,
      'capturedAtUtc': capturedAt.toIso8601String(),
      'clientEventId':
          'live-${capturedAt.millisecondsSinceEpoch}-${p.latitude.toStringAsFixed(5)}-${p.longitude.toStringAsFixed(5)}',
    };
    try {
      await _dio.dio.post('api/v1/app/committee/locations/live', data: point);
      unawaited(_flushQueue());
    } catch (_) {
      await _enqueue(point);
    }
  }

  Future<void> _enqueue(Map<String, dynamic> point) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final queue = _readQueue(prefs)..add(point);
      final trimmed = queue.length > maxQueuedPoints
          ? queue.sublist(queue.length - maxQueuedPoints)
          : queue;
      await prefs.setString(_kQueue, jsonEncode(trimmed));
    } catch (_) {}
  }

  List<Map<String, dynamic>> _readQueue(SharedPreferences prefs) {
    final raw = prefs.getString(_kQueue);
    if (raw == null || raw.isEmpty) return [];
    try {
      final decoded = jsonDecode(raw);
      if (decoded is! List) return [];
      return decoded.whereType<Map>().map((e) => Map<String, dynamic>.from(e)).toList();
    } catch (_) {
      return [];
    }
  }

  Future<void> _flushQueue() async {
    if (_sending) return;
    _sending = true;
    try {
      final prefs = await SharedPreferences.getInstance();
      final queue = _readQueue(prefs);
      if (queue.isEmpty) return;
      final batch = [
        for (final q in queue)
          {
            'lat': q['lat'],
            'lng': q['lng'],
            'recordedAtUtc': q['capturedAtUtc'],
            'accuracyMeters': q['accuracyMeters'],
            'clientEventId': q['clientEventId'],
          },
      ];
      await _dio.dio.post('api/v1/app/committee/locations/batch', data: batch);
      final after = _readQueue(prefs);
      final sentIds = queue.map((q) => q['clientEventId']).toSet();
      final remaining = after.where((q) => !sentIds.contains(q['clientEventId'])).toList();
      await prefs.setString(_kQueue, jsonEncode(remaining));
    } catch (_) {
    } finally {
      _sending = false;
    }
  }

  Future<void> _clearQueue() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove(_kQueue);
    } catch (_) {}
  }

  Future<void> _ensureGps() async {
    if (!await Geolocator.isLocationServiceEnabled()) {
      throw ApiException('فعّل خدمة الموقع لبدء الدوام');
    }
    var perm = await Geolocator.checkPermission();
    if (perm == LocationPermission.denied) {
      perm = await Geolocator.requestPermission();
    }
    if (perm != LocationPermission.always &&
        perm != LocationPermission.whileInUse) {
      throw ApiException('اسمح للتطبيق بالوصول إلى الموقع لبدء الدوام');
    }
  }
}
