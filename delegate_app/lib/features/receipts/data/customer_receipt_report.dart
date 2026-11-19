import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:intl/intl.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;

import '../../customers/domain/entities/customer.dart';
import '../utils/pdf_asset_cache.dart';
import '../utils/print_utils.dart';

/// نسخ من CustomerReceiptReport في SalesHaider — قائمة المسددين.
class CustomerReceiptReport {
  final String delegateName;
  final String receiptName;
  final int numberOfReceipts;
  final double amountReceiptTotal;
  final List<Map<String, dynamic>> receiptData;
  final Map<int, double> installmentByCustomer;

  CustomerReceiptReport({
    required this.delegateName,
    required this.receiptName,
    required this.numberOfReceipts,
    required this.amountReceiptTotal,
    required this.receiptData,
    this.installmentByCustomer = const {},
  });

  factory CustomerReceiptReport.fromToday({
    required String delegateName,
    required String receiptName,
    required List<Map<String, dynamic>> receiptData,
    required List<Customer> customers,
  }) {
    final installmentMap = {
      for (final c in customers) c.customerId: c.dailyInstallment ?? 0.0,
    };
    final total = receiptData.fold<double>(
      0,
      (sum, item) => sum + ((item['Amount'] as num?)?.toDouble() ?? 0),
    );
    return CustomerReceiptReport(
      delegateName: delegateName,
      receiptName: receiptName,
      numberOfReceipts: receiptData.length,
      amountReceiptTotal: total,
      receiptData: receiptData,
      installmentByCustomer: installmentMap,
    );
  }

  String _formatNumber(double number) {
    final formatter = NumberFormat('#,###');
    return formatter.format(number);
  }

  Future<void> printReceipt(BuildContext context) async {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) => const Center(child: CircularProgressIndicator()),
    );

    try {
      await initializeDateFormatting('ar', null);

      final rows = <List<String>>[];
      var totalAmountDaySale = 0.0;

      for (final customer in receiptData) {
        final customerId =
            int.tryParse(customer['CustomerId']?.toString() ?? '') ?? 0;
        final amountDaySales = installmentByCustomer[customerId] ?? 0.0;
        final amount = (customer['Amount'] as num?)?.toDouble() ?? 0.0;

        if (amount > 0) {
          totalAmountDaySale += amountDaySales;
        }
        rows.add([
          customer['CustomerName']?.toString() ?? '',
          '${_formatNumber(amountDaySales)} دع',
          '${_formatNumber(amount)} دع',
        ]);
      }

      if (!context.mounted) return;
      Navigator.of(context).pop();

      await PrintUtils.previewPdf(
        context,
        (format) => _generatePdf(format, rows, totalAmountDaySale),
        title: 'معاينة المسددين',
      );
    } catch (e) {
      if (context.mounted) {
        Navigator.of(context).pop();
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('تعذر تجهيز PDF المسددين: $e')),
        );
      }
    }
  }

  Future<Uint8List> _generatePdf(
    PdfPageFormat format,
    List<List<String>> rows,
    double totalAmountDaySale,
  ) async {
    final pdf = pw.Document();
    final ttf = await PdfAssetCache.getFontRegular();
    final ttfBold = await PdfAssetCache.getFontBold();
    final currentDate = DateFormat.yMMMMd('ar').format(DateTime.now());

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
                  pw.Text('المسددين', style: const pw.TextStyle(fontSize: 14)),
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
                _buildRow('عدد التسديدات', numberOfReceipts.toString()),
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
                    _buildCell('القسط', isHeader: true),
                    _buildCell('المسدد', isHeader: true),
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
                _buildRow(
                  'مجموع القسط',
                  '${_formatNumber(totalAmountDaySale)} دع',
                ),
                _buildRow(
                  'مجموع التسديدات',
                  '${_formatNumber(amountReceiptTotal)} دع',
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
