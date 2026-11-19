import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:intl/intl.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;

import '../../customers/domain/entities/customer.dart';
import '../utils/pdf_asset_cache.dart';
import '../utils/print_utils.dart';

/// نسخ من CustomerNoReceiptReport في SalesHaider — قائمة الغير مسددين.
class CustomerNoReceiptReport {
  final String delegateName;
  final String receiptName;
  final List<Customer> customerData;

  CustomerNoReceiptReport({
    required this.delegateName,
    required this.receiptName,
    required this.customerData,
  });

  String _formatNumber(double number) {
    final formatter = NumberFormat('#,###');
    return formatter.format(number);
  }

  static bool isContinuous(Customer c, {DateTime? now}) {
    final reference = now ?? DateTime.now();
    var lastRecent = false;
    var saleRecent = false;

    final last = c.lastPaymentDate;
    if (last != null && last.trim().isNotEmpty) {
      try {
        lastRecent =
            reference.difference(DateTime.parse(last.trim())).inDays < 365;
      } catch (_) {}
    }

    final sale = c.dateSaleDevice;
    if (sale != null && sale.trim().isNotEmpty) {
      try {
        saleRecent =
            reference.difference(DateTime.parse(sale.trim())).inDays < 365;
      } catch (_) {}
    }

    return lastRecent || saleRecent;
  }

  static String numberOfDayPayment(Customer c, {DateTime? now}) {
    final reference = now ?? DateTime.now();
    DateTime? anchor;
    final last = c.lastPaymentDate?.trim();
    if (last != null && last.isNotEmpty) {
      anchor = DateTime.tryParse(last);
    }
    if (anchor == null) {
      final sale = c.dateSaleDevice?.trim();
      if (sale != null && sale.isNotEmpty) {
        anchor = DateTime.tryParse(sale);
      }
    }
    if (anchor == null) return '0';
    return reference.difference(anchor).inDays.toString();
  }

  Future<void> printReport(BuildContext context) async {
    await initializeDateFormatting('ar', null);
    if (!context.mounted) return;
    await PrintUtils.previewPdf(
      context,
      _generatePdf,
      title: 'معاينة الغير مسددين',
    );
  }

  Future<Uint8List> _generatePdf(PdfPageFormat format) async {
    final pdf = pw.Document();
    final ttf = await PdfAssetCache.getFontRegular();
    final ttfBold = await PdfAssetCache.getFontBold();
    final currentDate = DateFormat.yMMMMd('ar').format(DateTime.now());

    final rows = <List<String>>[];
    var totalDaySales = 0.0;

    for (final customer in customerData) {
      if (customer.isLegal) continue;
      final amountDaySales = customer.dailyInstallment ?? 0.0;
      totalDaySales += amountDaySales;
      rows.add([
        customer.name,
        numberOfDayPayment(customer),
        '${_formatNumber(amountDaySales)} دع',
      ]);
    }

    pdf.addPage(
      pw.MultiPage(
        pageFormat: PrintUtils.ensureFiniteHeight(format),
        margin: const pw.EdgeInsets.all(20),
        theme: pw.ThemeData.withFont(base: ttf, bold: ttfBold),
        textDirection: pw.TextDirection.rtl,
        build: (pw.Context context) {
          return [
            pw.Row(
              mainAxisAlignment: pw.MainAxisAlignment.center,
              children: [
                pw.Column(children: [
                  pw.Text(
                    'شركة قلعة الضمان',
                    style: pw.TextStyle(
                      fontSize: 16,
                      fontWeight: pw.FontWeight.bold,
                    ),
                  ),
                  pw.Text(
                    'الغير مسددين',
                    style: const pw.TextStyle(fontSize: 14),
                  ),
                ]),
              ],
            ),
            pw.SizedBox(height: 10),
            pw.Table(
              border: pw.TableBorder.all(),
              columnWidths: {
                0: const pw.FlexColumnWidth(2),
                1: const pw.FlexColumnWidth(1),
              },
              children: [
                _buildRow('اسم القائمة', delegateName),
                _buildRow('اسم الجابي', receiptName),
                _buildRow('عدد التسديدات', '0'),
                _buildRow('التاريخ', currentDate),
              ],
            ),
            pw.SizedBox(height: 15),
            pw.Table(
              border: pw.TableBorder.all(),
              children: [
                pw.TableRow(
                  decoration: const pw.BoxDecoration(color: PdfColors.grey200),
                  children: [
                    _buildCell('اسم الزبون', isHeader: true),
                    _buildCell('عدد الأيام', isHeader: true),
                    _buildCell('القسط', isHeader: true),
                  ],
                ),
                ...rows.map(
                  (row) => pw.TableRow(children: [
                    _buildCell(row[0]),
                    _buildCell(row[1]),
                    _buildCell(row[2]),
                  ]),
                ),
              ],
            ),
            pw.SizedBox(height: 15),
            pw.Table(
              border: pw.TableBorder.all(),
              columnWidths: {
                0: const pw.FlexColumnWidth(2),
                1: const pw.FlexColumnWidth(1),
              },
              children: [
                _buildRow('عدد العملاء', rows.length.toString()),
                _buildRow(
                  'مجموع القسط',
                  '${_formatNumber(totalDaySales)} دع',
                ),
              ],
            ),
          ];
        },
      ),
    );

    return pdf.save();
  }

  pw.TableRow _buildRow(String label, String value) {
    return pw.TableRow(children: [
      _buildCell(value),
      _buildCell(label, isHeader: true),
    ]);
  }

  pw.Widget _buildCell(String text, {bool isHeader = false}) {
    return pw.Container(
      color: isHeader ? PdfColors.grey200 : null,
      padding: const pw.EdgeInsets.all(5),
      alignment: pw.Alignment.center,
      child: pw.Text(
        text,
        textAlign: pw.TextAlign.center,
        style: isHeader
            ? pw.TextStyle(fontWeight: pw.FontWeight.bold, fontSize: 10)
            : const pw.TextStyle(fontSize: 10),
      ),
    );
  }
}
