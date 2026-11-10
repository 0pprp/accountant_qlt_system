import 'dart:typed_data';

import 'package:flutter_test/flutter_test.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:pdf/pdf.dart';
import 'package:team/common/utils/helpers/receipt_pdf_generator_enhanced.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() async {
    await initializeDateFormatting('ar');
  });

  Future<ReceiptPdfResult> buildSample({required PdfPageFormat format}) {
    final now = DateTime(2026, 8, 23, 16, 45);
    return ReceiptPdfGenerator.buildReceiptPdf(
      orderId: 45,
      buyerName: 'زبون تجريبي',
      orderItemNames: 'تلفزيون',
      totalAmount: 500000,
      paidAmount: 120000,
      unpaidAmount: 380000,
      latestPaidAmount: 25000,
      latestPaidDate: now,
      orderListName: 'قائمة الكرادة',
      mandoobName: 'أحمد الجابي',
      paymentHistory: [
        InstallmentPayment(
          id: 1,
          orderId: 45,
          customerFullName: 'زبون تجريبي',
          amount: 25000,
          date: now,
          lastUpdatedAt: now,
        ),
      ],
      pdfPageFormat: format,
      isLandScape: false,
    );
  }

  ({double width, double height}) mediaBox(Uint8List bytes) {
    final text = String.fromCharCodes(bytes);
    final match = RegExp(r'/MediaBox\s*\[\s*([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s*\]').firstMatch(text);
    expect(match, isNotNull, reason: 'PDF MediaBox missing');
    return (width: double.parse(match!.group(3)!), height: double.parse(match.group(4)!));
  }

  test('thermal 58mm PDF uses finite roll size close to paper width', () async {
    final result = await buildSample(
      format: PdfPageFormat(58 * PdfPageFormat.mm, double.infinity),
    );
    final box = mediaBox(result.bytes);
    final widthMm = box.width / PdfPageFormat.mm;
    final heightMm = box.height / PdfPageFormat.mm;

    expect(result.isThermal, isTrue);
    expect(result.pageFormat.height.isFinite, isTrue);
    expect(widthMm, closeTo(58, 0.2));
    expect(heightMm, greaterThan(80));
    expect(heightMm, lessThan(280));
    expect(box.width, isNot(closeTo(PdfPageFormat.a4.width, 1)));
  });

  test('thermal 80mm PDF keeps 80mm width and finite height', () async {
    final result = await buildSample(
      format: PdfPageFormat(80 * PdfPageFormat.mm, double.infinity),
    );
    final box = mediaBox(result.bytes);
    expect(box.width / PdfPageFormat.mm, closeTo(80, 0.2));
    expect(box.height.isFinite, isTrue);
    expect(box.height / PdfPageFormat.mm, lessThan(320));
  });

  test('A4 export stays A4 and is not thermal', () async {
    final result = await buildSample(format: PdfPageFormat.a4);
    expect(result.isThermal, isFalse);
    expect(result.pageFormat.width, closeTo(PdfPageFormat.a4.width, 0.1));
  });
}
