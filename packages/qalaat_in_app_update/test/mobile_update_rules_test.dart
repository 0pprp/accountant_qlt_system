import 'package:flutter_test/flutter_test.dart';
import 'package:qalaat_in_app_update/qalaat_in_app_update.dart';

void main() {
  group('MobileUpdateRules', () {
    test('none when current >= latest', () {
      expect(
        MobileUpdateRules.evaluate(
          currentVersionCode: 2,
          latestVersionCode: 2,
          minimumSupportedVersionCode: 1,
          forceUpdate: true,
        ),
        MobileUpdateKind.none,
      );
    });

    test('mandatory when below minimum', () {
      expect(
        MobileUpdateRules.evaluate(
          currentVersionCode: 1,
          latestVersionCode: 3,
          minimumSupportedVersionCode: 2,
          forceUpdate: false,
        ),
        MobileUpdateKind.mandatory,
      );
    });

    test('mandatory when forceUpdate', () {
      expect(
        MobileUpdateRules.evaluate(
          currentVersionCode: 1,
          latestVersionCode: 2,
          minimumSupportedVersionCode: 1,
          forceUpdate: true,
        ),
        MobileUpdateKind.mandatory,
      );
    });

    test('optional otherwise', () {
      expect(
        MobileUpdateRules.evaluate(
          currentVersionCode: 1,
          latestVersionCode: 2,
          minimumSupportedVersionCode: 1,
          forceUpdate: false,
        ),
        MobileUpdateKind.optional,
      );
    });
  });
}
