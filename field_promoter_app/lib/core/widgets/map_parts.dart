import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

import '../../config/app_env.dart';
import '../../features/promoter/models.dart';
import '../../features/promoter/promoter_api.dart';
import '../theme/app_theme.dart';

abstract final class MapParts {
  static const baghdad = LatLng(33.3152, 44.3661);

  static TileLayer tiles() => TileLayer(
    urlTemplate: AppEnv.tileUrl,
    userAgentPackageName: AppEnv.mapUserAgent,
    maxNativeZoom: 19,
  );

  static Widget attribution() => const Align(
    alignment: AlignmentDirectional.bottomStart,
    child: Padding(
      padding: EdgeInsets.all(4),
      child: DecoratedBox(
        decoration: BoxDecoration(color: Color(0xCCFFFFFF)),
        child: Padding(
          padding: EdgeInsets.symmetric(horizontal: 4, vertical: 1),
          child: Text(
            '© OpenStreetMap',
            textDirection: TextDirection.ltr,
            style: TextStyle(fontSize: 10, color: AppColors.muted),
          ),
        ),
      ),
    ),
  );

  static Marker pinMarker(
    MapPin pin, {
    VoidCallback? onTap,
    double size = 30,
  }) => Marker(
    key: ValueKey('pin-${pin.id}'),
    point: LatLng(pin.lat, pin.lng),
    width: size,
    height: size,
    alignment: Alignment.topCenter,
    child: GestureDetector(
      onTap: onTap,
      child: PinGlyph(interest: pin.interest, mine: pin.mine, size: size),
    ),
  );

  static Marker meMarker(LatLng point) => Marker(
    point: point,
    width: 26,
    height: 26,
    child: Container(
      decoration: BoxDecoration(
        color: const Color(0xFF1565C0),
        shape: BoxShape.circle,
        border: Border.all(color: Colors.white, width: 3),
        boxShadow: const [BoxShadow(color: Color(0x55000000), blurRadius: 6)],
      ),
    ),
  );
}

class PinGlyph extends StatelessWidget {
  const PinGlyph({super.key, required this.interest, required this.mine, this.size = 30});
  final int interest;
  final bool mine;
  final double size;

  @override
  Widget build(BuildContext context) {
    final color = Interest.color(interest);
    return Stack(
      alignment: Alignment.center,
      children: [
        if (mine)
          Icon(Icons.location_on, color: AppColors.ink, size: size + 4),
        Icon(Icons.location_on, color: color, size: size),
        Positioned(
          top: size * 0.2,
          child: Container(
            width: size * 0.28,
            height: size * 0.28,
            decoration: const BoxDecoration(
              color: Colors.white,
              shape: BoxShape.circle,
            ),
          ),
        ),
      ],
    );
  }
}

class PinBoxLoader {
  PinBoxLoader(
    this._api, {
    this.debounce = const Duration(milliseconds: 600),
    this.minZoom = 12,
  });

  final PromoterApi _api;
  final Duration debounce;
  final double minZoom;
  final ValueNotifier<List<MapPin>> pins = ValueNotifier(const []);
  final ValueNotifier<bool> zoomTooLow = ValueNotifier(false);

  Timer? _timer;
  CancelToken? _cancel;
  String? _lastKey;
  LatLngBounds? _lastBounds;
  bool _disposed = false;

  void onCamera(MapCamera camera) {
    _timer?.cancel();
    if (camera.zoom < minZoom) {
      zoomTooLow.value = true;
      return;
    }
    zoomTooLow.value = false;
    final bounds = camera.visibleBounds;
    _timer = Timer(debounce, () => unawaited(_load(bounds)));
  }

  Future<void> refresh() async {
    final b = _lastBounds;
    if (b == null) return;
    _lastKey = null;
    await _load(b);
  }

  Future<void> _load(LatLngBounds b) async {
    if (_disposed) return;
    final key = [
      b.south,
      b.west,
      b.north,
      b.east,
    ].map((v) => v.toStringAsFixed(3)).join(',');
    if (key == _lastKey) return;
    _cancel?.cancel();
    final token = CancelToken();
    _cancel = token;
    try {
      final result = await _api.pinsInBox(
        minLat: b.south,
        minLng: b.west,
        maxLat: b.north,
        maxLng: b.east,
        cancelToken: token,
      );
      if (_disposed || token.isCancelled) return;
      _lastKey = key;
      _lastBounds = b;
      pins.value = result;
    } catch (_) {}
  }

  void dispose() {
    _disposed = true;
    _timer?.cancel();
    _cancel?.cancel();
    pins.dispose();
    zoomTooLow.dispose();
  }
}
