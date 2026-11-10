import 'dart:io';
import 'dart:math' as math;
import 'dart:typed_data';

import 'package:flutter/services.dart';
import 'package:path_provider/path_provider.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:printing/printing.dart';

class ReceiptThermalPrintResult {
  const ReceiptThermalPrintResult({
    required this.bytes,
    required this.pageFormat,
    required this.rasterWidthPx,
    required this.rasterHeightPx,
    required this.dpi,
    this.pngPath,
  });

  final Uint8List bytes;
  final PdfPageFormat pageFormat;
  final int rasterWidthPx;
  final int rasterHeightPx;
  final double dpi;
  final String? pngPath;
}

class ReceiptThermalPrinter {
  static const MethodChannel _channel = MethodChannel('com.qalaatal_dhaman.team/thermal_print');
  static const double dpi = 203;
  static const int _blackThreshold = 168;
  static const int _printerDots58mm = 384;
  static const int _printerDots80mm = 576;

  static Future<ReceiptThermalPrintResult> rasterize({
    required Uint8List pdfBytes,
    required double pageWidthPt,
  }) async {
    final pages = await Printing.raster(pdfBytes, dpi: dpi).toList();
    if (pages.isEmpty) {
      throw Exception('تعذر تحويل الوصل إلى صورة للطباعة الحرارية');
    }

    final width = pages.first.width;
    if (width <= 0) {
      throw Exception('عرض صورة الوصل غير صالح');
    }

    var totalHeight = 0;
    for (final page in pages) {
      if (page.width != width) {
        throw Exception('عرض صفحات الوصل غير متطابق');
      }
      totalHeight += page.height;
    }
    if (totalHeight <= 0) {
      throw Exception('ارتفاع صورة الوصل غير صالح');
    }

    final merged = _whiteBuffer(width * totalHeight * 4);
    var rowOffset = 0;
    for (final page in pages) {
      _copyPixels(
        dest: merged,
        destOffset: rowOffset * width * 4,
        src: page.pixels,
        count: page.height * width * 4,
      );
      rowOffset += page.height;
    }

    _thresholdToBlackWhite(merged);
    final firstInk = _firstInkRow(merged, width, totalHeight);
    final lastInk = _lastInkRow(merged, width, totalHeight);
    final croppedHeight = (lastInk - firstInk + 12).clamp(1, totalHeight - firstInk);
    final cropped = Uint8List(width * croppedHeight * 4);
    _copyPixels(
      dest: cropped,
      destOffset: 0,
      src: Uint8List.sublistView(merged, firstInk * width * 4),
      count: cropped.length,
    );

    final targetWidth = _printerWidthDots(pageWidthPt);
    final scaled = _scaleToWidth(cropped, width, croppedHeight, targetWidth);
    final png = await PdfRaster(scaled.width, scaled.height, scaled.pixels).toPng();
    final heightPt = pageWidthPt * scaled.height / scaled.width;
    final pageFormat = PdfPageFormat(pageWidthPt, heightPt, marginAll: 0);

    final dir = await getTemporaryDirectory();
    final pngPath = '${dir.path}/receipt_thermal.png';
    await File(pngPath).writeAsBytes(png, flush: true);

    final doc = pw.Document();
    final image = pw.MemoryImage(png);
    doc.addPage(
      pw.Page(
        pageFormat: pageFormat,
        margin: pw.EdgeInsets.zero,
        build: (context) => pw.Image(image, dpi: dpi, fit: pw.BoxFit.fill),
      ),
    );

    return ReceiptThermalPrintResult(
      bytes: await doc.save(),
      pageFormat: pageFormat,
      rasterWidthPx: scaled.width,
      rasterHeightPx: scaled.height,
      dpi: dpi,
      pngPath: pngPath,
    );
  }

  static Future<ReceiptThermalPrintResult> printPdf({
    required Uint8List pdfBytes,
    required double pageWidthPt,
  }) async {
    final result = await rasterize(pdfBytes: pdfBytes, pageWidthPt: pageWidthPt);
    final pngPath = result.pngPath;
    if (pngPath == null) {
      throw Exception('تعذر تجهيز صورة الوصل للطباعة');
    }
    try {
      await _channel.invokeMethod('printPng', {'path': pngPath});
    } catch (_) {
      await Printing.layoutPdf(
        name: 'وصل استلام',
        format: result.pageFormat,
        dynamicLayout: false,
        onLayout: (_) async => result.bytes,
      );
    }
    return result;
  }

  static int _printerWidthDots(double pageWidthPt) {
    final mm = pageWidthPt / PdfPageFormat.mm;
    return mm <= 70 ? _printerDots58mm : _printerDots80mm;
  }

  static Uint8List _whiteBuffer(int length) {
    final buffer = Uint8List(length);
    for (var i = 0; i < length; i += 4) {
      buffer[i] = 255;
      buffer[i + 1] = 255;
      buffer[i + 2] = 255;
      buffer[i + 3] = 255;
    }
    return buffer;
  }

  static void _copyPixels({
    required Uint8List dest,
    required int destOffset,
    required Uint8List src,
    required int count,
  }) {
    if (destOffset < 0 || destOffset >= dest.length || src.isEmpty || count <= 0) {
      return;
    }
    final copyCount = math.min(count, math.min(src.length, dest.length - destOffset));
    if (copyCount <= 0) {
      return;
    }
    dest.setRange(destOffset, destOffset + copyCount, src);
  }

  static void _thresholdToBlackWhite(Uint8List rgba) {
    for (var i = 0; i < rgba.length; i += 4) {
      final alpha = rgba[i + 3];
      final luminance = 0.299 * rgba[i] + 0.587 * rgba[i + 1] + 0.114 * rgba[i + 2];
      final value = (alpha < 128 || luminance >= _blackThreshold) ? 255 : 0;
      rgba[i] = value;
      rgba[i + 1] = value;
      rgba[i + 2] = value;
      rgba[i + 3] = 255;
    }
  }

  static int _firstInkRow(Uint8List rgba, int width, int height) {
    for (var y = 0; y < height; y++) {
      if (_rowHasInk(rgba, width, y)) {
        return math.max(y - 4, 0);
      }
    }
    return 0;
  }

  static int _lastInkRow(Uint8List rgba, int width, int height) {
    for (var y = height - 1; y >= 0; y--) {
      if (_rowHasInk(rgba, width, y)) {
        return math.min(y + 4, height - 1);
      }
    }
    return math.max(height - 1, 0);
  }

  static bool _rowHasInk(Uint8List rgba, int width, int y) {
    final start = y * width * 4;
    if (start + width * 4 > rgba.length) {
      return false;
    }
    for (var x = 0; x < width; x++) {
      if (rgba[start + x * 4] < 250) {
        return true;
      }
    }
    return false;
  }

  static ({int width, int height, Uint8List pixels}) _scaleToWidth(
    Uint8List src,
    int srcW,
    int srcH,
    int dstW,
  ) {
    if (srcW <= 0 || srcH <= 0 || dstW <= 0) {
      return (width: srcW, height: srcH, pixels: src);
    }
    if (srcW == dstW) {
      return (width: srcW, height: srcH, pixels: src);
    }
    final dstH = math.max(1, (srcH * dstW / srcW).round());
    final dst = Uint8List(dstW * dstH * 4);
    for (var y = 0; y < dstH; y++) {
      final srcY = math.min(srcH - 1, (y * srcH / dstH).floor());
      for (var x = 0; x < dstW; x++) {
        final srcX = math.min(srcW - 1, (x * srcW / dstW).floor());
        final si = (srcY * srcW + srcX) * 4;
        final di = (y * dstW + x) * 4;
        dst[di] = src[si];
        dst[di + 1] = src[si + 1];
        dst[di + 2] = src[si + 2];
        dst[di + 3] = 255;
      }
    }
    return (width: dstW, height: dstH, pixels: dst);
  }
}
