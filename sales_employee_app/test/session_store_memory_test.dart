import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/core/session/session_store.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  test('saveSession caches access token in memory for immediate Dio use', () async {
    final store = SessionStore();
    await store.saveSession(
      accessToken: 'tok-abc',
      refreshToken: 'ref-xyz',
      userId: 3,
      userName: 'emp_a_04f',
      fullName: 'Employee A',
    );
    // Even if platform storage throws, memory must satisfy interceptor.
    expect(await store.getAccessToken(), 'tok-abc');
    expect(await store.getRefreshToken(), 'ref-xyz');
    expect(await store.getUserId(), 3);
    expect(await store.hasSession(), isTrue);

    await store.clear();
    expect(await store.getAccessToken(), isNull);
    expect(await store.hasSession(), isFalse);
  });
}
