/// Runtime / compile-time environment for مسؤول اللجنة (نظام قلعة الضمان — ليس SalesHaider/admincompany).
abstract final class AppEnv {
  /// AppArea API (JWT). Override: --dart-define=API_BASE=https://host/
  static const apiBaseUrl = String.fromEnvironment(
    'API_BASE',
    defaultValue: 'https://api.qalataldhaman.com/',
  );

  /// بوابة اختيارية لمهام/إعلانات الجوال داخل نفس منظومة قلعة الضمان فقط.
  /// فارغة افتراضياً — لا تستخدم admincompany.alsaaeidy.com.
  /// مثال لاحق: --dart-define=GATEWAY_BASE=https://api.qalataldhaman.com/
  static const gatewayBaseUrl = String.fromEnvironment(
    'GATEWAY_BASE',
    defaultValue: '',
  );

  static const appKey = String.fromEnvironment('APP_KEY', defaultValue: 'committee');

  static const requireHttps = bool.fromEnvironment('REQUIRE_HTTPS', defaultValue: false);

  static bool get hasGateway => gatewayBaseUrl.trim().isNotEmpty;
}
