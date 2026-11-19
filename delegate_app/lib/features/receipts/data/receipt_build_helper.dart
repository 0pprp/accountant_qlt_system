import 'package:flutter/material.dart';

import '../../../injection.dart';
import '../../customers/domain/entities/customer.dart';
import 'receipt_week_amounts.dart';
import 'report_receipt.dart';

const _complaintsPhone = '6757-07717696008';

/// يبني ويُظهر وصل الاستلام بنفس منطق SalesHaider (ReportReceipt + PrintUtils).
Future<void> openCustomerReceiptPdf({
  required BuildContext context,
  required Customer customer,
  required double amountPaid,
  bool alreadyAppliedToTotals = false,
}) async {
  final session = await Injection.authRepository.currentSession();
  final listRaw = session?.cityName?.trim() ?? '';
  final listName = listRaw.isEmpty ? '—' : listRaw;
  final collectorRaw =
      (session?.delegateName ?? session?.username)?.trim() ?? '';
  final collector = collectorRaw.isEmpty ? '—' : collectorRaw;

  final paidBefore = customer.receiptTotal ?? 0;
  final remainBefore = customer.remaining ?? 0;
  final receiptsTotal =
      alreadyAppliedToTotals ? paidBefore : paidBefore + amountPaid;
  final amountRemaining = alreadyAppliedToTotals
      ? remainBefore
      : (remainBefore - amountPaid).clamp(0, double.infinity).toDouble();

  final weekLabels = ReceiptWeekAmounts.ensureDayLabels(null);
  final fromCache = ReceiptWeekAmounts.ensureAmounts(customer.weekAmounts);
  final weekAmounts = ReceiptWeekAmounts.guaranteeTodayShowsPayment(
    weekAmounts: fromCache,
    amountPush: amountPaid,
  );

  final printedNo = ReceiptWeekAmounts.printedReceiptNumber(
    (customer.countReceipt ?? 0).toString(),
  );

  final shop = customer.shopName?.trim() ?? '';
  final report = ReportReceipt(
    customerName: customer.name,
    receiptName: collector,
    delegateName: listName,
    itemsNames: shop.isEmpty ? '—' : shop,
    amountTotalSales: customer.salePrice ?? 0,
    receiptsTotal: receiptsTotal,
    amountRemaining: amountRemaining,
    amountPush: amountPaid,
    phoneNumberCompany: _complaintsPhone,
    lastSevenDays: weekLabels,
    lastSevenAmounts: weekAmounts,
    countReceiptDevice: printedNo,
    receiptNumberAlreadyPrinted: true,
  );

  if (!context.mounted) return;
  await report.printReceipt(context);
}

String receiptPreviewTitle(String customerName) => 'وصل الزبون $customerName';
