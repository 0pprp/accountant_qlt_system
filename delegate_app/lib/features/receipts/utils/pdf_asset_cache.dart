import 'package:flutter/services.dart';
import 'package:pdf/widgets.dart' as pw;

/// نفس فكرة PdfAssetCache في SalesHaider — مع خطوط ElMessiri المتوفرة هنا.
class PdfAssetCache {
  static Uint8List? _fontRegularBytes;
  static Uint8List? _fontBoldBytes;
  static pw.Font? _fontRegular;
  static pw.Font? _fontBold;

  static Future<Uint8List> getFontRegularBytes() async {
    if (_fontRegularBytes != null) return _fontRegularBytes!;
    final data = await rootBundle.load('assets/fonts/ElMessiri-Regular.ttf');
    _fontRegularBytes = Uint8List.fromList(
      data.buffer.asUint8List(data.offsetInBytes, data.lengthInBytes),
    );
    return _fontRegularBytes!;
  }

  static Future<Uint8List> getFontBoldBytes() async {
    if (_fontBoldBytes != null) return _fontBoldBytes!;
    final data = await rootBundle.load('assets/fonts/ElMessiri-Bold.ttf');
    _fontBoldBytes = Uint8List.fromList(
      data.buffer.asUint8List(data.offsetInBytes, data.lengthInBytes),
    );
    return _fontBoldBytes!;
  }

  static Future<pw.Font> getFontRegular() async {
    if (_fontRegular != null) return _fontRegular!;
    final bytes = await getFontRegularBytes();
    _fontRegular = pw.Font.ttf(ByteData.sublistView(bytes));
    return _fontRegular!;
  }

  static Future<pw.Font> getFontBold() async {
    if (_fontBold != null) return _fontBold!;
    final bytes = await getFontBoldBytes();
    _fontBold = pw.Font.ttf(ByteData.sublistView(bytes));
    return _fontBold!;
  }
}
