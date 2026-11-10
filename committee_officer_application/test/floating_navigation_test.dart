import 'package:committee_officer_application/core/widgets/glassy_bottom_bar.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

const items = [
  GlassyBottomBarItem(icon: Icons.home, label: 'الرئيسية'),
  GlassyBottomBarItem(icon: Icons.payment, label: 'الجباية'),
  GlassyBottomBarItem(icon: Icons.receipt, label: 'التسديدات'),
  GlassyBottomBarItem(icon: Icons.trending_down, label: 'المتدنية'),
];

void main() {
  for (final direction in TextDirection.values) {
    testWidgets(
      'floating selection follows $direction and stays above system inset',
      (tester) async {
        var selected = 0;
        await tester.pumpWidget(
          MaterialApp(
            home: MediaQuery(
              data: const MediaQueryData(padding: EdgeInsets.only(bottom: 34)),
              child: Directionality(
                textDirection: direction,
                child: StatefulBuilder(
                  builder: (context, setState) {
                    return Scaffold(
                      bottomNavigationBar: SafeArea(
                        top: false,
                        child: GlassyBottomBar(
                          items: items,
                          index: selected,
                          onTap: (value) => setState(() => selected = value),
                        ),
                      ),
                    );
                  },
                ),
              ),
            ),
          ),
        );
        final circle = find.byKey(const ValueKey('floating-tab-circle'));
        final first = tester.getCenter(circle);
        final tab = tester.getRect(find.byKey(const ValueKey('bottom-tab-0')));
        expect(first.dx, closeTo(tab.center.dx, .1));
        expect(tester.getRect(circle).top, greaterThanOrEqualTo(tab.top));
        expect(tester.getRect(circle).bottom, lessThan(tab.bottom));
        await tester.tap(find.byKey(const ValueKey('bottom-tab-3')));
        await tester.pumpAndSettle();
        expect(selected, 3);
        final last = tester.getCenter(circle);
        expect(
          last.dx,
          closeTo(
            tester.getCenter(find.byKey(const ValueKey('bottom-tab-3'))).dx,
            .1,
          ),
        );
        expect(
          direction == TextDirection.rtl
              ? last.dx < first.dx
              : last.dx > first.dx,
          isTrue,
        );
        expect(tester.takeException(), isNull);
      },
    );
  }

  testWidgets('reduced motion switches selection immediately', (tester) async {
    var selected = 0;
    await tester.pumpWidget(
      MaterialApp(
        home: MediaQuery(
          data: const MediaQueryData(disableAnimations: true),
          child: StatefulBuilder(
            builder: (context, setState) => Scaffold(
              bottomNavigationBar: GlassyBottomBar(
                items: items,
                index: selected,
                onTap: (i) => setState(() => selected = i),
              ),
            ),
          ),
        ),
      ),
    );
    await tester.tap(find.byKey(const ValueKey('bottom-tab-2')));
    await tester.pump();
    expect(
      tester.getCenter(find.byKey(const ValueKey('floating-tab-circle'))).dx,
      closeTo(
        tester.getCenter(find.byKey(const ValueKey('bottom-tab-2'))).dx,
        .1,
      ),
    );
    expect(tester.takeException(), isNull);
  });
}
