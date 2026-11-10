import 'dart:io';
import 'dart:typed_data';

import 'package:flutter/services.dart' show rootBundle;
import 'package:intl/intl.dart' as intl;
import 'package:open_file/open_file.dart';
import 'package:path_provider/path_provider.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:printing/printing.dart';
import 'package:team/common/utils/helpers/receipt_thermal_printer.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';

class ReceiptPdfResult {
  const ReceiptPdfResult({
    required this.bytes,
    required this.pageFormat,
    required this.isThermal,
  });

  final Uint8List bytes;
  final PdfPageFormat pageFormat;
  final bool isThermal;
}

class ReceiptPdfGenerator {
  static Future<ReceiptPdfResult> buildReceiptPdf({
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
    String? logoPath,
  }) async {
    final style = _ReceiptStyle.fromPageWidth(pdfPageFormat.width);
    final pageFormat = _resolvePageFormat(pdfPageFormat, isLandScape, style.isThermal);

    final arabicFont = await rootBundle.load('assets/fonts/ElMessiri-Regular.ttf');
    final arabicFontBold = await rootBundle.load('assets/fonts/ElMessiri-Bold.ttf');
    final ttf = pw.Font.ttf(arabicFont);
    final ttfBold = pw.Font.ttf(arabicFontBold);

    pw.MemoryImage? logo;
    if (logoPath != null && style.isThermal == false) {
      try {
        final logoData = await rootBundle.load(logoPath);
        logo = pw.MemoryImage(logoData.buffer.asUint8List());
      } catch (_) {}
    }

    final formattedDate = intl.DateFormat('d MMMM yyyy', 'ar').format(latestPaidDate);
    final formattedTime = intl.DateFormat('hh:mm a', 'en').format(latestPaidDate);
    final numberFormat = intl.NumberFormat('#,###', 'en');

    final pdf = pw.Document();
    pdf.addPage(
      pw.MultiPage(
        pageTheme: pw.PageTheme(
          pageFormat: pageFormat,
          textDirection: pw.TextDirection.rtl,
          theme: pw.ThemeData.withFont(base: ttf, bold: ttfBold),
          buildBackground: (context) => pw.FullPage(
            ignoreMargins: true,
            child: pw.Container(color: PdfColors.white),
          ),
        ),
        build: (context) {
          return [
            _buildHeader(logo, ttf, ttfBold, style),
            pw.SizedBox(height: style.sectionGap),
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
              style: style,
            ),
            pw.SizedBox(height: style.sectionGap),
            _buildPaymentHistory(
              paymentHistory,
              ttf,
              ttfBold,
              numberFormat,
              style,
            ),
            pw.SizedBox(height: style.sectionGap),
            _buildFooter(ttf, ttfBold, style),
          ];
        },
      ),
    );

    return ReceiptPdfResult(
      bytes: await pdf.save(),
      pageFormat: pageFormat,
      isThermal: style.isThermal,
    );
  }

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
    String? logoPath,
  }) async {
    final result = await buildReceiptPdf(
      orderId: orderId,
      buyerName: buyerName,
      orderItemNames: orderItemNames,
      totalAmount: totalAmount,
      paidAmount: paidAmount,
      unpaidAmount: unpaidAmount,
      latestPaidAmount: latestPaidAmount,
      latestPaidDate: latestPaidDate,
      orderListName: orderListName,
      mandoobName: mandoobName,
      paymentHistory: paymentHistory,
      pdfPageFormat: pdfPageFormat,
      isLandScape: isLandScape,
      logoPath: logoPath,
    );
    return _writePdf(result.bytes, orderId);
  }

  static Future<ReceiptThermalPrintResult?> printReceipt({
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
    String? logoPath,
  }) async {
    final result = await buildReceiptPdf(
      orderId: orderId,
      buyerName: buyerName,
      orderItemNames: orderItemNames,
      totalAmount: totalAmount,
      paidAmount: paidAmount,
      unpaidAmount: unpaidAmount,
      latestPaidAmount: latestPaidAmount,
      latestPaidDate: latestPaidDate,
      orderListName: orderListName,
      mandoobName: mandoobName,
      paymentHistory: paymentHistory,
      pdfPageFormat: pdfPageFormat,
      isLandScape: isLandScape,
      logoPath: logoPath,
    );

    await _writePdf(result.bytes, orderId);

    if (result.isThermal) {
      return ReceiptThermalPrinter.printPdf(
        pdfBytes: result.bytes,
        pageWidthPt: result.pageFormat.width,
      );
    }

    await Printing.layoutPdf(
      name: 'وصل استلام',
      format: result.pageFormat,
      dynamicLayout: false,
      onLayout: (_) async => result.bytes,
    );
    return null;
  }

  static PdfPageFormat _resolvePageFormat(PdfPageFormat requested, bool isLandscape, bool isThermal) {
    if (isThermal) {
      return PdfPageFormat(
        requested.width,
        _estimateThermalHeight(requested.width),
        marginTop: 2 * PdfPageFormat.mm,
        marginBottom: 4 * PdfPageFormat.mm,
        marginLeft: 1.5 * PdfPageFormat.mm,
        marginRight: 1.5 * PdfPageFormat.mm,
      );
    }

    final sheet = isLandscape ? requested.landscape : requested;
    return sheet.copyWith(
      marginTop: 18,
      marginBottom: 18,
      marginLeft: 22,
      marginRight: 22,
    );
  }

  static double _estimateThermalHeight(double widthPt) {
    final style = _ReceiptStyle.fromPageWidth(widthPt);
    final row = style.bodySize * 1.4 + style.cellPad * 2 + 2;
    final header = style.headerSize * 1.45 + style.titleSize * 1.45 + 18;
    final table = row * 11;
    final history = style.titleSize * 1.45 + 8 + row * 7;
    final footer = style.footerSize * 3 + 20;
    final gaps = style.sectionGap * 3;
    return (header + table + history + footer + gaps + 8 * PdfPageFormat.mm) * 1.2;
  }

  static pw.Widget _buildHeader(
    pw.MemoryImage? logo,
    pw.Font ttf,
    pw.Font ttfBold,
    _ReceiptStyle style,
  ) {
    return pw.Column(
      crossAxisAlignment: pw.CrossAxisAlignment.center,
      children: [
        if (logo != null)
          pw.Padding(
            padding: const pw.EdgeInsets.only(bottom: 8),
            child: pw.SizedBox(
              width: 48,
              height: 48,
              child: pw.Image(logo, fit: pw.BoxFit.contain),
            ),
          ),
        pw.Text(
          'شركة قلعة الضمان',
          style: pw.TextStyle(
            font: ttfBold,
            fontSize: style.headerSize,
            color: PdfColors.black,
          ),
          textAlign: pw.TextAlign.center,
          textDirection: pw.TextDirection.rtl,
        ),
        pw.SizedBox(height: 4),
        pw.Text(
          'وصل استلام',
          style: pw.TextStyle(
            font: ttfBold,
            fontSize: style.titleSize,
            color: PdfColors.black,
          ),
          textAlign: pw.TextAlign.center,
          textDirection: pw.TextDirection.rtl,
        ),
        pw.SizedBox(height: 8),
        pw.Divider(color: PdfColors.black, thickness: 1.2),
      ],
    );
  }

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
    required _ReceiptStyle style,
  }) {
    return pw.Table(
      border: pw.TableBorder.all(
        color: PdfColors.black,
        width: style.isThermal ? 1 : 0.8,
      ),
      defaultVerticalAlignment: pw.TableCellVerticalAlignment.middle,
      columnWidths: {
        0: const pw.FlexColumnWidth(1.7),
        1: const pw.FlexColumnWidth(1.1),
      },
      children: [
        _buildTableRow('رقم الوصل', orderId.toString(), ttf, ttfBold, style),
        _buildTableRow('الزبون', buyerName, ttf, ttfBold, style),
        _buildTableRow('المادة المباعة', orderItemNames, ttf, ttfBold, style),
        _buildTableRow('المبلغ الكلي', _money(numberFormat, totalAmount), ttf, ttfBold, style),
        _buildTableRow('المبلغ الواصل', _money(numberFormat, paidAmount), ttf, ttfBold, style),
        _buildTableRow('المبلغ المتبقي', _money(numberFormat, unpaidAmount), ttf, ttfBold, style),
        _buildTableRow(
          'المبلغ المدفوع',
          _money(numberFormat, latestPaidAmount),
          ttf,
          ttfBold,
          style,
          highlightValue: true,
        ),
        _buildTableRow('تاريخ الدفع', formattedDate, ttf, ttfBold, style),
        _buildTableRow('الوقت', formattedTime, ttf, ttfBold, style),
        _buildTableRow('اسم القائمة', orderListName, ttf, ttfBold, style),
        _buildTableRow(
          'اسم الجابي',
          mandoobName.isEmpty ? 'لا يوجد' : mandoobName,
          ttf,
          ttfBold,
          style,
        ),
      ],
    );
  }

  static String _money(intl.NumberFormat numberFormat, double amount) {
    return '${numberFormat.format(amount)} د.ع';
  }

  static pw.TableRow _buildTableRow(
    String label,
    String value,
    pw.Font ttf,
    pw.Font ttfBold,
    _ReceiptStyle style, {
    bool highlightValue = false,
  }) {
    return pw.TableRow(
      children: [
        pw.Container(
          padding: pw.EdgeInsets.symmetric(horizontal: style.cellPad, vertical: style.cellPad),
          alignment: pw.Alignment.center,
          child: pw.Text(
            value,
            style: pw.TextStyle(
              font: highlightValue || style.isThermal ? ttfBold : ttf,
              fontSize: style.bodySize,
              color: PdfColors.black,
            ),
            textAlign: pw.TextAlign.center,
            textDirection: pw.TextDirection.rtl,
          ),
        ),
        pw.Container(
          padding: pw.EdgeInsets.symmetric(horizontal: style.cellPad, vertical: style.cellPad),
          alignment: pw.Alignment.center,
          decoration: style.isThermal
              ? null
              : const pw.BoxDecoration(
                  color: PdfColors.grey200,
                ),
          child: pw.Text(
            label,
            style: pw.TextStyle(
              font: ttfBold,
              fontSize: style.bodySize,
              color: PdfColors.black,
            ),
            textAlign: pw.TextAlign.center,
            textDirection: pw.TextDirection.rtl,
          ),
        ),
      ],
    );
  }

  static pw.Widget _buildPaymentHistory(
    List<InstallmentPayment> paymentHistory,
    pw.Font ttf,
    pw.Font ttfBold,
    intl.NumberFormat numberFormat,
    _ReceiptStyle style,
  ) {
    return pw.Column(
      crossAxisAlignment: pw.CrossAxisAlignment.stretch,
      children: [
        pw.Text(
          'تسديدات اخر سبعة ايام',
          style: pw.TextStyle(
            font: ttfBold,
            fontSize: style.titleSize,
            color: PdfColors.black,
          ),
          textAlign: pw.TextAlign.right,
          textDirection: pw.TextDirection.rtl,
        ),
        pw.SizedBox(height: 6),
        pw.Table(
          border: pw.TableBorder.all(
            color: PdfColors.black,
            width: style.isThermal ? 1 : 0.8,
          ),
          defaultVerticalAlignment: pw.TableCellVerticalAlignment.middle,
          columnWidths: {
            0: const pw.FlexColumnWidth(1),
            1: const pw.FlexColumnWidth(1.4),
          },
          children: _buildPaymentHistoryRows(
            paymentHistory,
            ttf,
            ttfBold,
            numberFormat,
            style,
          ),
        ),
      ],
    );
  }

  static List<pw.TableRow> _buildPaymentHistoryRows(
    List<InstallmentPayment> paymentHistory,
    pw.Font ttf,
    pw.Font ttfBold,
    intl.NumberFormat numberFormat,
    _ReceiptStyle style,
  ) {
    final now = DateTime.now();
    final last7Days = List.generate(
      7,
      (index) => now.subtract(Duration(days: index)),
    );

    return last7Days.map((date) {
      final payment = paymentHistory.firstWhere(
        (p) => p.date.year == date.year && p.date.month == date.month && p.date.day == date.day,
        orElse: () => InstallmentPayment(
          id: 0,
          orderId: 0,
          customerFullName: '',
          amount: 0,
          date: date,
          lastUpdatedAt: date,
        ),
      );
      final dateStr = intl.DateFormat('yyyy-MM-dd').format(date);
      final hasPayment = payment.amount > 0;
      final amountStr = hasPayment ? _money(numberFormat, payment.amount) : '0 د.ع';

      return pw.TableRow(
        children: [
          pw.Container(
            padding: pw.EdgeInsets.symmetric(horizontal: style.cellPad, vertical: style.cellPad - 1),
            alignment: pw.Alignment.center,
            child: pw.Text(
              amountStr,
              style: pw.TextStyle(
                font: hasPayment || style.isThermal ? ttfBold : ttf,
                fontSize: style.historySize,
                color: PdfColors.black,
              ),
              textAlign: pw.TextAlign.center,
              textDirection: pw.TextDirection.rtl,
            ),
          ),
          pw.Container(
            padding: pw.EdgeInsets.symmetric(horizontal: style.cellPad, vertical: style.cellPad - 1),
            alignment: pw.Alignment.center,
            decoration: style.isThermal
                ? null
                : const pw.BoxDecoration(
                    color: PdfColors.grey100,
                  ),
            child: pw.Text(
              dateStr,
              style: pw.TextStyle(
                font: ttf,
                fontSize: style.historySize,
                color: PdfColors.black,
              ),
              textAlign: pw.TextAlign.center,
            ),
          ),
        ],
      );
    }).toList();
  }

  static pw.Widget _buildFooter(pw.Font ttf, pw.Font ttfBold, _ReceiptStyle style) {
    return pw.Column(
      crossAxisAlignment: pw.CrossAxisAlignment.center,
      children: [
        pw.Divider(color: PdfColors.black, thickness: 1),
        pw.SizedBox(height: 6),
        pw.Text(
          'رقم الشكاوي: 6757 - 07740827170',
          style: pw.TextStyle(
            font: ttfBold,
            fontSize: style.footerSize,
            color: PdfColors.black,
          ),
          textAlign: pw.TextAlign.center,
          textDirection: pw.TextDirection.rtl,
        ),
        pw.SizedBox(height: 4),
        pw.Text(
          'وقت الدوام: من الساعة 9 صباحا الى الساعة 8 مساء',
          style: pw.TextStyle(
            font: ttf,
            fontSize: style.footerSize,
            color: PdfColors.black,
          ),
          textAlign: pw.TextAlign.center,
          textDirection: pw.TextDirection.rtl,
        ),
      ],
    );
  }

  static Future<String> _writePdf(Uint8List bytes, int orderId) async {
    Directory? downloadsDir;

    if (Platform.isAndroid) {
      downloadsDir = await getExternalStorageDirectory();
    } else if (Platform.isIOS) {
      downloadsDir = await getApplicationDocumentsDirectory();
    }

    if (downloadsDir == null) {
      throw Exception('لا يمكن الوصول إلى مجلد التنزيلات');
    }

    final timestamp = DateTime.now().millisecondsSinceEpoch;
    final fileName = 'receipt_${orderId}_$timestamp.pdf';
    final savePath = '${downloadsDir.path}/$fileName';
    await File(savePath).writeAsBytes(bytes);
    return savePath;
  }

  static Future<void> openPdf(String path) async {
    final result = await OpenFile.open(path);
    if (result.type != ResultType.done) {
      throw Exception('لا يمكن فتح الملف: ${result.message}');
    }
  }
}

class _ReceiptStyle {
  const _ReceiptStyle({
    required this.isThermal,
    required this.headerSize,
    required this.titleSize,
    required this.bodySize,
    required this.historySize,
    required this.footerSize,
    required this.cellPad,
    required this.sectionGap,
  });

  final bool isThermal;
  final double headerSize;
  final double titleSize;
  final double bodySize;
  final double historySize;
  final double footerSize;
  final double cellPad;
  final double sectionGap;

  static _ReceiptStyle fromPageWidth(double width) {
    final mm = width / PdfPageFormat.mm;
    if (mm <= 60) {
      return const _ReceiptStyle(
        isThermal: true,
        headerSize: 20,
        titleSize: 14,
        bodySize: 13,
        historySize: 12,
        footerSize: 11,
        cellPad: 5,
        sectionGap: 8,
      );
    }
    if (mm <= 90) {
      return const _ReceiptStyle(
        isThermal: true,
        headerSize: 20,
        titleSize: 14,
        bodySize: 13,
        historySize: 12,
        footerSize: 11,
        cellPad: 6,
        sectionGap: 10,
      );
    }
    return const _ReceiptStyle(
      isThermal: false,
      headerSize: 22,
      titleSize: 16,
      bodySize: 14,
      historySize: 12,
      footerSize: 12,
      cellPad: 8,
      sectionGap: 14,
    );
  }
}
