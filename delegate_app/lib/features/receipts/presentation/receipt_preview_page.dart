import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:printing/printing.dart';

import '../../../core/theme/app_colors.dart';

class ReceiptPreviewPage extends StatelessWidget {
  const ReceiptPreviewPage({
    super.key,
    required this.bytes,
    required this.title,
  });

  final Uint8List bytes;
  final String title;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(title),
        actions: [
          IconButton(
            icon: const Icon(Icons.share_outlined),
            onPressed: () =>
                Printing.sharePdf(bytes: bytes, filename: 'receipt.pdf'),
          ),
          IconButton(
            icon: const Icon(Icons.print_outlined),
            onPressed: () =>
                Printing.layoutPdf(onLayout: (_) async => bytes),
          ),
        ],
      ),
      body: PdfPreview(
        build: (_) async => bytes,
        canChangeOrientation: false,
        canChangePageFormat: false,
      ),
    );
  }
}
