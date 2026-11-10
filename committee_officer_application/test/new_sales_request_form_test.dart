import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:committee_officer_application/features/sales/data/sales_request_submission.dart';
import 'package:committee_officer_application/features/sales/presentation/new_sales_request_page.dart';

class FakeSubmission extends SalesRequestSubmission {
  FakeSubmission() : super(Dio(), 'Follower');
  final payloads = <Map<String, dynamic>>[];
  bool failFirst = false;
  @override
  Future<List<Map<String, dynamic>>> branches() async => [{'id': 3, 'name': 'فرع الاختبار'}];
  @override
  Future<int> submit(Map<String, dynamic> payload) async {
    payloads.add(Map.of(payload));
    if (failFirst && payloads.length == 1) {
      throw DioException(requestOptions: RequestOptions(path: '/test'), type: DioExceptionType.receiveTimeout);
    }
    return 42;
  }
}

void main() {
  testWidgets('Form sends all four fields and retries the same operation after timeout', (tester) async {
    final remote = FakeSubmission()..failFirst = true;
    await tester.pumpWidget(MaterialApp(home: NewSalesRequestPage(submission: remote)));
    await tester.pumpAndSettle();
    final fields = find.byType(TextFormField);
    for (final entry in <String>['أحمد محمد علي', '٠٧٨٠١٢٣٤٥٦٧', 'بغداد حي الجامعة', 'ثلاجة', 'اتصل مساءً'].asMap().entries) {
      await tester.ensureVisible(fields.at(entry.key));
      await tester.enterText(fields.at(entry.key), entry.value);
    }
    tester.testTextInput.hide();
    await tester.pumpAndSettle();
    await tester.ensureVisible(find.byType(FilledButton));
    await tester.pumpAndSettle();
    await tester.tap(find.byType(FilledButton));
    await tester.pumpAndSettle();
    expect(remote.payloads.length, 1);
    expect(remote.payloads.single['customerPhone'], '07801234567');
    expect(remote.payloads.single['branchId'], 3);
    expect(remote.payloads.single['requestedProduct'], 'ثلاجة');
    expect(remote.payloads.single.containsKey('targetEmployeeId'), isFalse);
    expect(remote.payloads.single.containsKey('status'), isFalse);
    tester.testTextInput.hide();
    await tester.pumpAndSettle();
    await tester.ensureVisible(find.byType(FilledButton));
    await tester.pumpAndSettle();
    await tester.tap(find.byType(FilledButton));
    await tester.pumpAndSettle();
    expect(remote.payloads.length, 2);
    expect(remote.payloads[0], remote.payloads[1]);
    expect(find.text('تم إرسال الطلب للتدقيق'), findsOneWidget);
  });
}
