import 'dart:io';

import 'package:flutter/services.dart' show rootBundle;
import 'package:intl/intl.dart' as intl;
import 'package:open_file/open_file.dart';
import 'package:path_provider/path_provider.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:team/payment/domain/installment_payment/installment_payment.dart';

/// Enhanced PDF Generator with Logo Support - Single Page Version
/// This version fits all content on a single page with automatic scaling
class ReceiptPdfGenerator {
  /// Generate a professional receipt PDF (single page)
  static Future<String> generateReceipt({
    required int orderId,
    required String buyerName,
    required String orderItemNames,
    required double totalAmount,
    required double paidAmount,
    required double unpaidAmount,
    required double latestPaidAmount,
    required DateTime latestPaidDate,
    required String orderListName,
    required String mandoobName,
    required List<InstallmentPayment> paymentHistory,
    required PdfPageFormat pdfPageFormat,
    required bool isLandScape,
    String? logoPath, // Optional logo path
  }) async {
    // Create PDF document
    final pdf = pw.Document();

    // Load Arabic fonts
    final arabicFont = await rootBundle.load("assets/fonts/ElMessiri-Regular.ttf");
    final arabicFontBold = await rootBundle.load("assets/fonts/ElMessiri-Bold.ttf");

    final ttf = pw.Font.ttf(arabicFont);
    final ttfBold = pw.Font.ttf(arabicFontBold);

    // Load logo if provided
    pw.MemoryImage? logo;
    if (logoPath != null) {
      try {
        final logoData = await rootBundle.load(logoPath);
        logo = pw.MemoryImage(logoData.buffer.asUint8List());
      } catch (e) {
        print('Error loading logo: $e');
      }
    }

    // Format date and time
    final dateFormatter = intl.DateFormat('d MMMM yyyy', 'ar');
    final timeFormatter = intl.DateFormat('hh:mm a', 'en');

    final formattedDate = dateFormatter.format(latestPaidDate);
    final formattedTime = timeFormatter.format(latestPaidDate);

    // Number formatter
    final numberFormat = intl.NumberFormat('#,###', 'ar');

    // Add single page with all content
    pdf.addPage(
      pw.Page(
        pageFormat: pdfPageFormat,
        orientation: isLandScape ? pw.PageOrientation.landscape : pw.PageOrientation.portrait,
        textDirection: pw.TextDirection.rtl,
        margin: const pw.EdgeInsets.all(0),
        theme: pw.ThemeData.withFont(
          base: ttf,
          bold: ttfBold,
        ),
        build: (context) {
          return pw.Column(
            crossAxisAlignment: pw.CrossAxisAlignment.start,
            children: [
              // Header
              _buildHeader(logo, ttf, ttfBold),

              // Main content
              pw.Expanded(
                child: pw.Container(
                  margin: const pw.EdgeInsets.symmetric(horizontal: 20, vertical: 16),
                  child: pw.Column(
                    crossAxisAlignment: pw.CrossAxisAlignment.start,
                    children: [
                      // Main receipt table
                      _buildReceiptTable(
                        orderId: orderId,
                        buyerName: buyerName,
                        orderItemNames: orderItemNames,
                        totalAmount: totalAmount,
                        paidAmount: paidAmount,
                        unpaidAmount: unpaidAmount,
                        latestPaidAmount: latestPaidAmount,
                        formattedDate: formattedDate,
                        formattedTime: formattedTime,
                        orderListName: orderListName,
                        mandoobName: mandoobName,
                        ttf: ttf,
                        ttfBold: ttfBold,
                        numberFormat: numberFormat,
                      ),

                      pw.SizedBox(height: 12),

                      // Payment history section
                      pw.Expanded(
                        child: _buildPaymentHistory(
                          paymentHistory,
                          ttf,
                          ttfBold,
                          numberFormat,
                        ),
                      ),
                    ],
                  ),
                ),
              ),

              // Footer
              _buildFooter(ttf),
            ],
          );
        },
      ),
    );

    // Save PDF
    final savePath = await _savePdf(pdf, orderId);
    return savePath;
  }

  /// Build header with logo and company name (compact version)
  static pw.Widget _buildHeader(
    pw.MemoryImage? logo,
    pw.Font ttf,
    pw.Font ttfBold,
  ) {
    return pw.Container(
      padding: const pw.EdgeInsets.symmetric(horizontal: 20, vertical: 16),
      color: PdfColor.fromHex('#006A64'),
      child: pw.Row(
        mainAxisAlignment: pw.MainAxisAlignment.spaceBetween,
        crossAxisAlignment: pw.CrossAxisAlignment.center,
        children: [
          // Logo
          if (logo != null)
            pw.Container(
              width: 60,
              height: 60,
              child: pw.Image(logo, fit: pw.BoxFit.contain),
            )
          else
            pw.Container(
              width: 60,
              height: 60,
              decoration: pw.BoxDecoration(
                color: PdfColors.teal,
                shape: pw.BoxShape.circle,
              ),
              child: pw.Center(
                child: pw.Text(
                  'قلعة',
                  style: pw.TextStyle(
                    font: ttfBold,
                    fontSize: 18,
                    color: PdfColors.white,
                  ),
                  textDirection: pw.TextDirection.rtl,
                ),
              ),
            ),

          // Company name and title
          pw.Column(
            crossAxisAlignment: pw.CrossAxisAlignment.end,
            mainAxisSize: pw.MainAxisSize.min,
            children: [
              pw.Text(
                'شركة قلعة الضمان',
                style: pw.TextStyle(
                  font: ttfBold,
                  fontSize: 22,
                  color: PdfColors.white,
                ),
                textDirection: pw.TextDirection.rtl,
              ),
              pw.SizedBox(height: 4),
              pw.Text(
                'وصل استلام',
                style: pw.TextStyle(
                  font: ttf,
                  fontSize: 16,
                  color: PdfColors.grey200,
                ),
                textDirection: pw.TextDirection.rtl,
              ),
            ],
          ),
        ],
      ),
    );
  }

  /// Build main receipt table (compact version)
  static pw.Widget _buildReceiptTable({
    required int orderId,
    required String buyerName,
    required String orderItemNames,
    required double totalAmount,
    required double paidAmount,
    required double unpaidAmount,
    required double latestPaidAmount,
    required String formattedDate,
    required String formattedTime,
    required String orderListName,
    required String mandoobName,
    required pw.Font ttf,
    required pw.Font ttfBold,
    required intl.NumberFormat numberFormat,
  }) {
    return pw.Table(
      border: pw.TableBorder.all(
        color: PdfColors.grey600,
        width: 1,
      ),
      columnWidths: {
        0: const pw.FlexColumnWidth(2),
        1: const pw.FlexColumnWidth(1),
      },
      children: [
        _buildTableRow('رقم الوصل', orderId.toString(), ttf, ttfBold),
        _buildTableRow('الزبون', buyerName, ttf, ttfBold),
        _buildTableRow('المادة المباعة', orderItemNames, ttf, ttfBold),
        _buildTableRow(
          'المبلغ الكلي',
          '${numberFormat.format(totalAmount)} دع',
          ttf,
          ttfBold,
        ),
        _buildTableRow(
          'المبلغ الواصل',
          '${numberFormat.format(paidAmount)} دع',
          ttf,
          ttfBold,
        ),
        _buildTableRow(
          'المبلغ المتبقي',
          '${numberFormat.format(unpaidAmount)} دع',
          ttf,
          ttfBold,
        ),
        _buildTableRow(
          'المبلغ المدفوع',
          '${numberFormat.format(latestPaidAmount)} دع',
          ttf,
          ttfBold,
          highlightValue: true,
        ),
        _buildTableRow('تاريخ الدفع', formattedDate, ttf, ttfBold),
        _buildTableRow('الوقت', formattedTime, ttf, ttfBold),
        _buildTableRow('اسم القائمة', orderListName, ttf, ttfBold),
        _buildTableRow(
          'اسم الجابي',
          mandoobName.isEmpty ? 'لا يوجد' : mandoobName,
          ttf,
          ttfBold,
        ),
      ],
    );
  }

  /// Build a table row (compact version)
  static pw.TableRow _buildTableRow(
    String label,
    String value,
    pw.Font ttf,
    pw.Font ttfBold, {
    bool highlightValue = false,
  }) {
    return pw.TableRow(
      children: [
        pw.Container(
          padding: const pw.EdgeInsets.all(6),
          alignment: pw.Alignment.centerRight,
          decoration: highlightValue ? const pw.BoxDecoration(color: PdfColors.green50) : null,
          child: pw.Text(
            value,
            style: pw.TextStyle(
              font: highlightValue ? ttfBold : ttf,
              fontSize: 10,
              color: highlightValue ? PdfColors.green900 : PdfColors.black,
            ),
            textDirection: pw.TextDirection.rtl,
          ),
        ),
        pw.Container(
          padding: const pw.EdgeInsets.all(6),
          alignment: pw.Alignment.centerRight,
          decoration: const pw.BoxDecoration(
            color: PdfColors.grey200,
          ),
          child: pw.Text(
            label,
            style: pw.TextStyle(
              font: ttfBold,
              fontSize: 10,
              color: PdfColors.black,
            ),
            textDirection: pw.TextDirection.rtl,
          ),
        ),
      ],
    );
  }

  /// Build payment history section (compact version)
  static pw.Widget _buildPaymentHistory(
    List<InstallmentPayment> paymentHistory,
    pw.Font ttf,
    pw.Font ttfBold,
    intl.NumberFormat numberFormat,
  ) {
    return pw.Column(
      crossAxisAlignment: pw.CrossAxisAlignment.end,
      children: [
        pw.Text(
          'تسديدات اخر سبعة ايام',
          style: pw.TextStyle(
            font: ttfBold,
            fontSize: 12,
            color: PdfColors.black,
          ),
          textDirection: pw.TextDirection.rtl,
        ),
        pw.SizedBox(height: 6),
        pw.Table(
          border: pw.TableBorder.all(
            color: PdfColors.grey600,
            width: 1,
          ),
          columnWidths: {
            0: const pw.FlexColumnWidth(1),
            1: const pw.FlexColumnWidth(2),
          },
          children: _buildPaymentHistoryRows(
            paymentHistory,
            ttf,
            numberFormat,
          ),
        ),
      ],
    );
  }

  /// Build payment history rows (compact version)
  static List<pw.TableRow> _buildPaymentHistoryRows(
    List<InstallmentPayment> paymentHistory,
    pw.Font ttf,
    intl.NumberFormat numberFormat,
  ) {
    final now = DateTime.now();
    final last7Days = List.generate(
      7,
      (index) => now.subtract(Duration(days: index)),
    );

    return last7Days.map((date) {
      final payment = paymentHistory.firstWhere(
        (p) => p.date.year == date.year && p.date.month == date.month && p.date.day == date.day,
        orElse:
            () =>
                InstallmentPayment(id: 0, orderId: 0, customerFullName: '', amount: 0, date: date, lastUpdatedAt: date),
      );
      final dateStr = intl.DateFormat('yyyy-MM-dd').format(date);
      final amountStr = payment.amount > 0 ? '${numberFormat.format(payment.amount)} دع' : '0 دع';

      return pw.TableRow(
        children: [
          pw.Container(
            padding: const pw.EdgeInsets.all(5),
            alignment: pw.Alignment.centerRight,
            decoration: payment.amount > 0 ? const pw.BoxDecoration(color: PdfColors.green50) : null,
            child: pw.Text(
              amountStr,
              style: pw.TextStyle(
                font: ttf,
                fontSize: 9,
                color: payment.amount > 0 ? PdfColors.green900 : PdfColors.grey700,
              ),
              textDirection: pw.TextDirection.rtl,
            ),
          ),
          pw.Container(
            padding: const pw.EdgeInsets.all(5),
            alignment: pw.Alignment.center,
            decoration: const pw.BoxDecoration(
              color: PdfColors.grey100,
            ),
            child: pw.Text(
              dateStr,
              style: pw.TextStyle(
                font: ttf,
                fontSize: 9,
                color: PdfColors.black,
              ),
            ),
          ),
        ],
      );
    }).toList();
  }

  /// Build footer with contact info (compact version)
  static pw.Widget _buildFooter(pw.Font ttf) {
    return pw.Container(
      padding: const pw.EdgeInsets.symmetric(vertical: 10, horizontal: 20),
      decoration: const pw.BoxDecoration(
        border: pw.Border(
          top: pw.BorderSide(
            color: PdfColors.grey400,
            width: 1,
          ),
        ),
      ),
      child: pw.Column(
        crossAxisAlignment: pw.CrossAxisAlignment.center,
        children: [
          pw.Text(
            'رقم الشكاوى: 6757 - 07740827170',
            style: pw.TextStyle(
              font: ttf,
              fontSize: 9,
              color: PdfColors.grey800,
            ),
            textDirection: pw.TextDirection.rtl,
          ),
          pw.SizedBox(height: 3),
          pw.Text(
            'وقت الدوام: من الساعة 9 صباحا الى الساعة 8 مساء',
            style: pw.TextStyle(
              font: ttf,
              fontSize: 9,
              color: PdfColors.grey800,
            ),
            textDirection: pw.TextDirection.rtl,
          ),
        ],
      ),
    );
  }

  /// Save PDF to device
  static Future<String> _savePdf(pw.Document pdf, int orderId) async {
    // Get downloads directory
    Directory? downloadsDir;

    if (Platform.isAndroid) {
      // App-specific external storage — no runtime permission needed
      downloadsDir = await getExternalStorageDirectory();
    } else if (Platform.isIOS) {
      downloadsDir = await getApplicationDocumentsDirectory();
    }

    if (downloadsDir == null) {
      throw Exception('لا يمكن الوصول إلى مجلد التنزيلات');
    }

    // Create filename
    final timestamp = DateTime.now().millisecondsSinceEpoch;
    final fileName = 'receipt_${orderId}_$timestamp.pdf';
    final savePath = '${downloadsDir.path}/$fileName';

    // Save file
    final file = File(savePath);
    await file.writeAsBytes(await pdf.save());

    return savePath;
  }

  /// Open the generated PDF
  static Future<void> openPdf(String path) async {
    final result = await OpenFile.open(path);
    if (result.type != ResultType.done) {
      throw Exception('لا يمكن فتح الملف: ${result.message}');
    }
  }
}
