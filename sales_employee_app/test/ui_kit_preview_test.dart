import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/core/widgets/ui/ui_kit_preview_page.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sales_request_item.dart';
import 'package:sales_employee_app/features/sales/presentation/widgets/sales_request_card.dart';

void main() {
  testWidgets('UiKit preview shows long Arabic and states', (tester) async {
    tester.view.physicalSize = const Size(360, 800);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.resetPhysicalSize);

    await tester.pumpWidget(const MaterialApp(home: UiKitPreviewPage()));
    await tester.pump(); // loading indicator animates — avoid pumpAndSettle timeout
    expect(find.textContaining('اسم طويل'), findsOneWidget);
    expect(find.text('فشل'), findsOneWidget);
    expect(find.text('ممنوع'), findsOneWidget);
  });

  testWidgets('SalesRequestCard keeps SourceParity fields at 360', (tester) async {
    tester.view.physicalSize = const Size(360, 900);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.resetPhysicalSize);

    const item = SalesRequestItem(
      requestId: 7,
      customerName: 'زبون تجريبي باسم طويل للالتفاف الكامل دون إخفاء',
      customerPhone: '07801112233',
      address: 'عنوان طويل جداً يجب أن يظهر كاملاً في الكارت',
      requestedProduct: 'منتج مطلوب طويل',
      status: 1,
      statusLabel: 'قيد التنفيذ',
      tabKey: 'incoming',
      branchName: 'فرع الاختبار',
      sourceTypeLabel: 'مندوب',
      submissionNote: 'ملاحظة\nمتعددة\nالأسطر',
      lastStatusNote: 'آخر ملاحظة',
      createdAt: null,
      statusChangedAtUtc: null,
      isReturned: false,
      linkedOrderId: null,
    );

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: SingleChildScrollView(
            child: SalesRequestCard(item: item, onOpen: () {}),
          ),
        ),
      ),
    );
    await tester.pumpAndSettle();
    expect(find.textContaining('زبون تجريبي'), findsOneWidget);
    expect(find.text('07801112233'), findsOneWidget);
    expect(find.textContaining('ملاحظة'), findsWidgets);
    expect(tester.takeException(), isNull);
  });
}
