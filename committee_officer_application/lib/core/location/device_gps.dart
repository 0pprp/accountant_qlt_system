import 'package:committee_officer_application/core/network/api_exception.dart';
import 'package:geolocator/geolocator.dart';

class DeviceGpsFix {
  const DeviceGpsFix({required this.latitude, required this.longitude});
  final double latitude;
  final double longitude;
}

Future<DeviceGpsFix> captureDeviceGps() async {
  if (!await Geolocator.isLocationServiceEnabled()) {
    throw ApiException('فعّل خدمة الموقع على الجهاز');
  }
  var perm = await Geolocator.checkPermission();
  if (perm == LocationPermission.denied) {
    perm = await Geolocator.requestPermission();
  }
  if (perm != LocationPermission.always &&
      perm != LocationPermission.whileInUse) {
    throw ApiException('اسمح للتطبيق بالوصول إلى الموقع');
  }
  final p = await Geolocator.getCurrentPosition(
    locationSettings: const LocationSettings(accuracy: LocationAccuracy.high),
  );
  if ((p.latitude.abs() < 1e-9 && p.longitude.abs() < 1e-9) ||
      !p.latitude.isFinite ||
      !p.longitude.isFinite) {
    throw ApiException('تعذر قراءة إحداثيات صالحة');
  }
  return DeviceGpsFix(latitude: p.latitude, longitude: p.longitude);
}
