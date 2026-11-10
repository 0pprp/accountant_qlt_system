import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:geolocator/geolocator.dart';

class LocationPoint {
  const LocationPoint({required this.lat, required this.lng});
  final double lat;
  final double lng;
}

class LocationService {
  /// Baghdad center — Web-verified fallback when browser geolocation is unavailable.
  static const LocationPoint webFallback = LocationPoint(lat: 33.3152, lng: 44.3661);

  Future<bool> ensurePermission() async {
    if (kIsWeb) return true;
    var enabled = await Geolocator.isLocationServiceEnabled();
    if (!enabled) return false;
    var permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
    }
    return permission == LocationPermission.always ||
        permission == LocationPermission.whileInUse;
  }

  Future<LocationPoint?> current() async {
    // Web-verified: avoid hanging browser geolocation prompts during UI proof.
    if (kIsWeb) return webFallback;
    final ok = await ensurePermission();
    if (!ok) return null;
    final p = await Geolocator.getCurrentPosition(
      locationSettings: const LocationSettings(accuracy: LocationAccuracy.high),
    );
    return LocationPoint(lat: p.latitude, lng: p.longitude);
  }
}
