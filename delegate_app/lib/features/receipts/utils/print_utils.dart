import 'package:flutter/material.dart';
import 'package:pdf/pdf.dart';
import 'package:printing/printing.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../../core/theme/app_colors.dart';

/// نسخ من PrintUtils في SalesHaider-main/delegate_application.
class PrintUtils {
  static const String _pageWidthKey = 'pref_page_width';
  static const String _pageHeightKey = 'pref_page_height';
  static const String _pageMarginKey = 'pref_page_margin';

  static Future<void> savePageFormat(PdfPageFormat format) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setDouble(_pageWidthKey, format.width);
    await prefs.setDouble(_pageHeightKey, format.height);
    await prefs.setDouble(_pageMarginKey, format.marginBottom);
  }

  static Future<PdfPageFormat> getPageFormat() async {
    final prefs = await SharedPreferences.getInstance();
    final width = prefs.getDouble(_pageWidthKey);
    final height = prefs.getDouble(_pageHeightKey);
    final margin = prefs.getDouble(_pageMarginKey) ?? 20.0;

    if (width != null && height != null) {
      return PdfPageFormat(
        width,
        height,
        marginBottom: margin,
        marginTop: margin,
        marginLeft: margin,
        marginRight: margin,
      );
    }
    return PdfPageFormat.a4;
  }

  static PdfPageFormat ensureFiniteHeight(PdfPageFormat format) {
    if (format.height == double.infinity) {
      return format.copyWith(height: 40 * PdfPageFormat.cm);
    }
    return format;
  }

  static Future<void> previewPdf(
    BuildContext context,
    LayoutCallback build, {
    String title = 'معاينة ومشاركة الوصل',
  }) async {
    final initialFormat = await getPageFormat();
    if (!context.mounted) return;

    await Navigator.of(context, rootNavigator: true).push(
      MaterialPageRoute(
        builder: (context) => Scaffold(
          appBar: AppBar(
            title: Text(title),
            backgroundColor: AppColors.primary,
            foregroundColor: AppColors.onPrimary,
            centerTitle: true,
          ),
          body: PdfPreview(
            build: build,
            allowSharing: true,
            allowPrinting: true,
            initialPageFormat: initialFormat,
            dynamicLayout: false,
            pdfFileName:
                'receipt_${DateTime.now().millisecondsSinceEpoch}.pdf',
            canChangeOrientation: true,
            canChangePageFormat: true,
            pageFormats: const {
              'A4': PdfPageFormat.a4,
              'رول 80 مم': PdfPageFormat.roll80,
              'رول 58 مم': PdfPageFormat.roll57,
              'A5': PdfPageFormat.a5,
            },
            loadingWidget: const Center(child: CircularProgressIndicator()),
            onPageFormatChanged: savePageFormat,
          ),
        ),
      ),
    );
  }
}
