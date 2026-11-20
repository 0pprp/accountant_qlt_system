import 'package:flutter/foundation.dart';

abstract final class AppEnv {
  static const productionBaseUrl = 'https://api.qalataldhaman.com/';
  static const debugBaseUrl = 'http://10.0.2.2:15999/';
  static const _override = String.fromEnvironment('API_BASE_URL');
  static const requireHttpsFlag = bool.fromEnvironment('REQUIRE_HTTPS');
  static const appKey = 'field-promoter';
  static const appTitle = 'المروّج الميداني';
  static const mapUserAgent = 'iq.qalaa.field_promoter';
  static const tileUrl = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';

  static String get apiBaseUrl {
    final raw = _override.trim().isNotEmpty
        ? _override.trim()
        : (kReleaseMode ? productionBaseUrl : debugBaseUrl);
    return raw.endsWith('/') ? raw : '$raw/';
  }

  static bool get requireHttps => kReleaseMode || requireHttpsFlag;
}
