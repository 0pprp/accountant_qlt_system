import 'dart:typed_data';

import '../../../injection.dart';
import '../../customers/domain/entities/customer.dart';
import 'receipt_pdf_builder.dart';

/// Builds a full Arabic receipt PDF from customer + paid amount.
Future<Uint8List> buildCustomerReceiptPdf({
  required Customer customer,
  required double amountPaid,
}) async {
  final session = await Injection.authRepository.currentSession();
  final count = customer.countReceipt ?? 0;
  final receiptNo = (count + 1).toString();

  return ReceiptPdfBuilder().build(
    customerName: customer.name,
    amountPaid: amountPaid,
    receiptNumber: receiptNo,
    itemName: customer.shopName,
    totalSales: customer.salePrice,
    paidTotal: customer.receiptTotal,
    remaining: customer.remaining,
    listName: session?.cityName,
    collectorName: session?.delegateName ?? session?.username,
  );
}

String receiptPreviewTitle(String customerName) => 'وصل الزبون $customerName';
