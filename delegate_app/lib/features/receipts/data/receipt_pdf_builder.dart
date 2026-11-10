import 'package:flutter/services.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:intl/intl.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;

/// Thermal receipt PDF matching SalesHaider delegate layout (Arabic fonts).
class ReceiptPdfBuilder {
  static const _complaintsPhone = '6757-07717696008';
  static const _weekLabels = <String>[
    'اليوم',
    'أمس',
    'قبل يومين',
    'قبل 3 أيام',
    'قبل 4 أيام',
    'قبل 5 أيام',
    'قبل 6 أيام',
  ];

  static pw.Font? _fontRegular;
  static pw.Font? _fontBold;

  Future<pw.Font> _regular() async {
    if (_fontRegular != null) return _fontRegular!;
    final data = await rootBundle.load('assets/fonts/ElMessiri-Regular.ttf');
    _fontRegular = pw.Font.ttf(data);
    return _fontRegular!;
  }

  Future<pw.Font> _bold() async {
    if (_fontBold != null) return _fontBold!;
    final data = await rootBundle.load('assets/fonts/ElMessiri-Bold.ttf');
    _fontBold = pw.Font.ttf(data);
    return _fontBold!;
  }

  String _money(double n) {
    final formatter = NumberFormat('#,###', 'en');
    return '${formatter.format(n.round())} دع';
  }

  Future<Uint8List> build({
    required String customerName,
    required double amountPaid,
    String? receiptNumber,
    String? itemName,
    double? totalSales,
    double? paidTotal,
    double? remaining,
    String? listName,
    String? collectorName,
    String? complaintsPhone,
    List<double>? weekAmounts,
  }) async {
    await initializeDateFormatting('ar', null);
    final ttf = await _regular();
    final ttfBold = await _bold();

    final now = DateTime.now();
    final dateStr = DateFormat.yMMMMd('ar').format(now);
    final timeStr = DateFormat.jm('en').format(now);

    final sales = totalSales ?? 0;
    final paidBefore = paidTotal ?? 0;
    final paidAfter = paidBefore + amountPaid;
    final remainAfter = remaining != null
        ? (remaining - amountPaid).clamp(0, double.infinity).toDouble()
        : (sales > 0 ? (sales - paidAfter).clamp(0, double.infinity).toDouble() : 0.0);

    final week = List<double>.generate(7, (i) {
      if (weekAmounts != null && i < weekAmounts.length) {
        return weekAmounts[i];
      }
      return i == 0 ? amountPaid : 0.0;
    });
    if (week[0] < amountPaid) week[0] = amountPaid;

    final doc = pw.Document();
    doc.addPage(
      pw.Page(
        // Finite height — infinite roll breaks PdfPreview on some devices.
        pageFormat: PdfPageFormat(
          80 * PdfPageFormat.mm,
          220 * PdfPageFormat.mm,
          marginAll: 8,
        ),
        theme: pw.ThemeData.withFont(base: ttf, bold: ttfBold),
        build: (context) {
          return pw.Directionality(
            textDirection: pw.TextDirection.rtl,
            child: pw.Column(
              crossAxisAlignment: pw.CrossAxisAlignment.stretch,
              children: [
                pw.Text(
                  'شركة قلعة الضمان',
                  textAlign: pw.TextAlign.center,
                  style: pw.TextStyle(
                    fontSize: 16,
                    fontWeight: pw.FontWeight.bold,
                  ),
                ),
                pw.Text(
                  'وصل استلام',
                  textAlign: pw.TextAlign.center,
                  style: const pw.TextStyle(fontSize: 13),
                ),
                pw.SizedBox(height: 8),
                pw.Table(
                  border: pw.TableBorder.all(width: 0.6),
                  columnWidths: const {
                    0: pw.FlexColumnWidth(2),
                    1: pw.FlexColumnWidth(1.1),
                  },
                  children: [
                    _row('رقم الوصل', receiptNumber?.trim().isNotEmpty == true
                        ? receiptNumber!.trim()
                        : '—'),
                    _row('الزبون', customerName),
                    _row(
                      'المادة المباعة',
                      (itemName == null || itemName.trim().isEmpty)
                          ? '—'
                          : itemName.trim(),
                    ),
                    _row('المبلغ الكلي', _money(sales)),
                    _row('المبلغ الواصل', _money(paidAfter)),
                    _row('المبلغ المتبقي', _money(remainAfter)),
                    _row('المبلغ المدفوع', _money(amountPaid)),
                    _row('تاريخ الدفع', dateStr),
                    _row('الوقت', timeStr),
                    _row(
                      'اسم القائمة',
                      (listName == null || listName.trim().isEmpty)
                          ? '—'
                          : listName.trim(),
                    ),
                    _row(
                      'اسم الجابي',
                      (collectorName == null || collectorName.trim().isEmpty)
                          ? '—'
                          : collectorName.trim(),
                    ),
                  ],
                ),
                pw.SizedBox(height: 8),
                pw.Align(
                  alignment: pw.Alignment.centerRight,
                  child: pw.Text(
                    'تسديدات آخر سبعة أيام',
                    style: pw.TextStyle(
                      fontSize: 11,
                      fontWeight: pw.FontWeight.bold,
                    ),
                  ),
                ),
                pw.SizedBox(height: 4),
                pw.Table(
                  border: pw.TableBorder.all(width: 0.6),
                  columnWidths: const {
                    0: pw.FlexColumnWidth(2),
                    1: pw.FlexColumnWidth(1.1),
                  },
                  children: List.generate(
                    7,
                    (i) => _row(_weekLabels[i], _money(week[i])),
                  ),
                ),
                pw.SizedBox(height: 12),
                pw.Text(
                  'رقم الشكاوى: ${complaintsPhone ?? _complaintsPhone}',
                  textAlign: pw.TextAlign.center,
                  style: const pw.TextStyle(fontSize: 9),
                ),
                pw.Text(
                  'وقت الدوام: من الساعة 9 صباحاً الى الساعة 8 مساءً',
                  textAlign: pw.TextAlign.center,
                  style: const pw.TextStyle(fontSize: 9),
                ),
              ],
            ),
          );
        },
      ),
    );
    return doc.save();
  }

  pw.TableRow _row(String label, String value) {
    return pw.TableRow(
      children: [
        pw.Container(
          padding: const pw.EdgeInsets.symmetric(horizontal: 4, vertical: 3),
          alignment: pw.Alignment.center,
          child: pw.Text(
            value,
            textAlign: pw.TextAlign.center,
            style: const pw.TextStyle(fontSize: 10),
          ),
        ),
        pw.Container(
          color: PdfColors.grey300,
          padding: const pw.EdgeInsets.symmetric(horizontal: 4, vertical: 3),
          alignment: pw.Alignment.center,
          child: pw.Text(
            label,
            textAlign: pw.TextAlign.center,
            style: pw.TextStyle(
              fontSize: 10,
              fontWeight: pw.FontWeight.bold,
            ),
          ),
        ),
      ],
    );
  }
}
