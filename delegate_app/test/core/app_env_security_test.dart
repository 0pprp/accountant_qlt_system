import 'package:delegate_app/core/config/app_env.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('production API is HTTPS', () {
    expect(AppEnv.requireHttps, isTrue);
    expect(Uri.parse(AppEnv.apiBaseUrl).scheme, 'https');
    expect(Uri.parse(AppEnv.apiBaseUrl).host, 'api.qalataldhaman.com');
  });
}
