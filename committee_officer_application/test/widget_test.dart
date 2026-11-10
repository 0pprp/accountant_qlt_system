import 'package:committee_officer_application/core/theme/app_theme.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets('Committee officer brand smoke', (WidgetTester tester) async {
    await tester.pumpWidget(
      MaterialApp(
        theme: AppTheme.light(),
        home: const Scaffold(
          body: Center(child: Text('تطبيق مسؤول اللجنة')),
        ),
      ),
    );

    expect(find.text('تطبيق مسؤول اللجنة'), findsOneWidget);
  });
}
