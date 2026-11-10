/// Production-only configuration. No demo mode.
abstract final class AppEnv {
  /// Central Qalaat API (same host as team app).
  static const apiBaseUrl = String.fromEnvironment('API_BASE_URL', defaultValue: 'https://api.qalataldhaman.com/');

  static const gatewayBase =
      'https://admincompany.alsaaeidy.com/sales-gw/api/';
  static const appKey = 'delegate';
  static const requireHttps = bool.fromEnvironment('REQUIRE_HTTPS', defaultValue: true);
}
