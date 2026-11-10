/// إعدادات التشغيل — الإنتاج افتراضيًا؛ يُتجاوز بـ `--dart-define` للاختبار المعزول.
/// مثال 04G (Windows / جهاز يصل للحاسوب) — المسارات في Dio تتضمن `api/v1/...`،
/// لذا القاعدة = أصل المضيف فقط:
/// `flutter run -d windows --dart-define=API_BASE_URL=http://127.0.0.1:4200/ --dart-define=REQUIRE_HTTPS=false`
/// محاكي Android: `http://10.0.2.2:4200/` (localhost داخل المحاكي ≠ الحاسوب).
abstract final class AppEnv {
  static const apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'https://api.qalataldhaman.com/',
  );

  static const appKey = 'sales-employee';

  static const requireHttps = bool.fromEnvironment(
    'REQUIRE_HTTPS',
    defaultValue: true,
  );

  static bool get isLoopbackHost {
    try {
      final host = Uri.parse(apiBaseUrl).host.toLowerCase();
      return host == 'localhost' ||
          host == '127.0.0.1' ||
          host == '::1' ||
          host == '10.0.2.2';
    } catch (_) {
      return false;
    }
  }

  /// عزل demo عن مسار الإنتاج: لا دمج ولا fallback إلا صراحةً للاختبار المعزول.
  static const enableDemoOrders = bool.fromEnvironment(
    'ENABLE_DEMO_ORDERS',
    defaultValue: false,
  );

  static bool get allowDemoOrders => enableDemoOrders && isLoopbackHost;
}
