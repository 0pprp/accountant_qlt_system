import 'dart:async';

import 'package:committee_officer_application/core/network/api_exception.dart';
import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/session/session_store.dart';
import 'package:committee_officer_application/core/time/baghdad_time.dart';
import 'package:geolocator/geolocator.dart';

/// تتبع موقع المتابع مرتبط بالدوام — إرسال في المقدمة فقط (لا ادّعاء خلفية).
class ShiftRemote {
  ShiftRemote(this._dio, this._session);

  final DioClient _dio;
  final SessionStore _session;
  Timer? _liveTimer;

  Future<bool> currentActive() async {
    final res = await _dio.dio.get('api/v1/app/committee/shifts/current');
    final data = res.data;
    if (data is! Map) throw ApiException('تعذر التحقق من حالة الدوام');
    final active = data['hasActiveShift'] ?? data['HasActiveShift'];
    if (active is! bool) throw ApiException('استجابة حالة الدوام غير صالحة');
    if (active) {
      _ensureLiveLoop();
    } else {
      stopLiveLoop();
    }
    return active;
  }

  Future<void> start() async {
    await _ensureGps();
    await _dio.dio.post('api/v1/app/committee/shifts/start');
    await _session.markDutyStarted(baghdadDateKey());
    await _pingLive();
    _ensureLiveLoop();
  }

  Future<void> end() async {
    stopLiveLoop();
    try {
      await _pingLive();
    } catch (_) {}
    await _dio.dio.post('api/v1/app/committee/shifts/end');
    await _session.markDutyEnded();
  }

  void stopLiveLoop() {
    _liveTimer?.cancel();
    _liveTimer = null;
  }

  void _ensureLiveLoop() {
    if (_liveTimer != null) return;
    // مقدمة التطبيق فقط — كل 60 ثانية أثناء بقاء الجلسة/الدوام
    _liveTimer = Timer.periodic(const Duration(seconds: 60), (_) {
      unawaited(_pingLive());
    });
  }

  Future<void> _pingLive() async {
    try {
      final p = await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.high,
        ),
      );
      final eventId =
          'live-${DateTime.now().toUtc().millisecondsSinceEpoch}-${p.latitude.toStringAsFixed(5)}';
      await _dio.dio.post(
        'api/v1/app/committee/locations/live',
        data: {
          'lat': p.latitude,
          'lng': p.longitude,
          'accuracyMeters': p.accuracy.isFinite ? p.accuracy : null,
          'capturedAtUtc': DateTime.now().toUtc().toIso8601String(),
          'clientEventId': eventId,
        },
      );
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
