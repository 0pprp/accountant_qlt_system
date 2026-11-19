import 'package:delegate_app/core/time/iraq_date.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('03:00 Baghdad opens the next collection day across month end', () {
    final before = DateTime.utc(2026, 10, 31, 23, 59, 59, 999, 999);
    expect(IraqDate.isoDate(before), '2026-10-31');
    expect(
      IraqDate.delayUntilNextBusinessDayStart(before),
      const Duration(microseconds: 1),
    );
    expect(
      IraqDate.isoDate(before.add(const Duration(microseconds: 1))),
      '2026-11-01',
    );
  });
}
