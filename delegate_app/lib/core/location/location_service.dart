import 'package:geolocator/geolocator.dart';

class LocationService {
  /// Fast GPS for payment — never blocks long; null if unavailable.
  Future<String?> tryGetLocation() async {
    try {
      final enabled = await Geolocator.isLocationServiceEnabled().timeout(
        const Duration(milliseconds: 400),
        onTimeout: () => false,
      );
      if (!enabled) return null;
      final perm = await Geolocator.checkPermission().timeout(
        const Duration(milliseconds: 400),
        onTimeout: () => LocationPermission.denied,
      );
      if (perm == LocationPermission.denied ||
          perm == LocationPermission.deniedForever) {
        return null;
      }
      final last = await Geolocator.getLastKnownPosition().timeout(
        const Duration(milliseconds: 500),
        onTimeout: () => null,
      );
      if (last != null) return '${last.latitude},${last.longitude}';
      final pos = await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.low,
          timeLimit: Duration(seconds: 1),
        ),
      ).timeout(const Duration(milliseconds: 1200));
      return '${pos.latitude},${pos.longitude}';
    } catch (_) {
      return null;
    }
  }
}
