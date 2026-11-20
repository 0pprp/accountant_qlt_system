import 'package:geolocator/geolocator.dart';

import '../network/api_exception.dart';

abstract final class Gps {
  static Future<void> ensurePermission() async {
    if (!await Geolocator.isLocationServiceEnabled()) {
      throw ApiException('فعّل خدمة الموقع (GPS) في الهاتف');
    }
    var perm = await Geolocator.checkPermission();
    if (perm == LocationPermission.denied) {
      perm = await Geolocator.requestPermission();
    }
    if (perm == LocationPermission.deniedForever) {
      throw ApiException('صلاحية الموقع مرفوضة — فعّلها من إعدادات التطبيق');
    }
    if (perm != LocationPermission.always &&
        perm != LocationPermission.whileInUse) {
      throw ApiException('اسمح للتطبيق بالوصول إلى الموقع');
    }
  }

  static Future<Position> current() async {
    await ensurePermission();
    try {
      return await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.high,
          timeLimit: Duration(seconds: 20),
        ),
      );
    } catch (_) {
      final last = await Geolocator.getLastKnownPosition();
      if (last != null) return last;
      throw ApiException('تعذر تحديد موقعك الحالي — تأكد من تشغيل GPS');
    }
  }

  static bool usable(Position p) =>
      p.latitude.isFinite &&
      p.longitude.isFinite &&
      !(p.latitude.abs() < 1e-9 && p.longitude.abs() < 1e-9);
}
