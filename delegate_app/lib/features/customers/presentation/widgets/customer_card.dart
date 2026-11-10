import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../../core/widgets/brand_wave_painter.dart';
import '../../../payments/presentation/collect_payment_sheet.dart';
import '../../../payments/presentation/payment_success_feedback.dart';
import '../../../receipts/data/receipt_build_helper.dart';
import '../../../receipts/presentation/receipt_preview_page.dart';
import '../../domain/entities/customer.dart';
import '../customer_detail_page.dart';

class CustomerCard extends StatefulWidget {
  const CustomerCard({
    super.key,
    required this.customer,
    this.onChanged,
  });

  final Customer customer;
  final VoidCallback? onChanged;

  @override
  State<CustomerCard> createState() => _CustomerCardState();
}

class _CustomerCardState extends State<CustomerCard> {
  Uint8List? _receiptBytes;
  bool _printing = false;

  Customer get c => widget.customer;

  String _money(double? v) {
    if (v == null) return '—';
    return NumberFormat('#,##0.##', 'en').format(v);
  }

  String _date(String? raw) {
    if (raw == null || raw.trim().isEmpty) return '—';
    final t = raw.trim();
    final dt = DateTime.tryParse(t);
    if (dt == null) return t.split('T').first;
    final local = dt.toLocal();
    return '${local.year}-${local.month.toString().padLeft(2, '0')}-${local.day.toString().padLeft(2, '0')}';
  }

  Future<void> _openReceipt(Uint8List bytes) async {
    await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => ReceiptPreviewPage(
          bytes: bytes,
          title: receiptPreviewTitle(c.name),
        ),
      ),
    );
  }

  Future<void> _openDetails() async {
    await Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => CustomerDetailPage(customer: c)),
    );
    widget.onChanged?.call();
  }

  Future<void> _collect() async {
    final result = await showCollectPaymentSheet(context, c);
    if (result == null || !mounted) return;

    await PaymentSuccessFeedback.playLocalSaveSuccess(context);
    if (!mounted) return;

    try {
      final bytes = await buildCustomerReceiptPdf(
        customer: c,
        amountPaid: result.amount,
      );
      if (!mounted) return;
      setState(() => _receiptBytes = bytes);
      await _openReceipt(bytes);
    } catch (_) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('تم التسديد — تعذر تجهيز الوصل')),
      );
    }
    widget.onChanged?.call();
  }

  Future<void> _print() async {
    final bytes = _receiptBytes;
    if (bytes == null || _printing) return;
    setState(() => _printing = true);
    try {
      await _openReceipt(bytes);
    } finally {
      if (mounted) setState(() => _printing = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final paid = c.receiptTotal;
    final canPrint = _receiptBytes != null;

    return Material(
      color: Colors.transparent,
      child: InkWell(
        borderRadius: BorderRadius.circular(18),
        onTap: _openDetails,
        child: Ink(
          decoration: BoxDecoration(
            color: AppColors.card,
            borderRadius: BorderRadius.circular(18),
            border: Border.all(color: AppColors.primary.withValues(alpha: 0.12)),
            boxShadow: [
              BoxShadow(
                color: AppColors.primary.withValues(alpha: 0.06),
                blurRadius: 14,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(18),
            child: Stack(
              children: [
                Positioned.fill(
                  child: CustomPaint(
                    painter: const BrandWavePainter(opacity: 0.11),
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.fromLTRB(14, 14, 14, 12),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              c.name,
                              style: AppTextStyles.title.copyWith(fontSize: 17),
                            ),
                          ),
                          Icon(
                            Icons.chevron_left,
                            color: AppColors.primary.withValues(alpha: 0.55),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      _row('رقم الهاتف', c.phone ?? '—'),
                      _row('سعر البيع', _money(c.salePrice)),
                      _row('الواصل', _money(paid)),
                      _row('الباقي', _money(c.remaining)),
                      _row('القسط المستحق', _money(c.dailyInstallment)),
                      _row('تاريخ المبيع', _date(c.dateSaleDevice)),
                      _row('آخر تسديد', _date(c.lastPaymentDate)),
                      _row('العنوان', c.address ?? '—'),
                      _row('المحل', c.shopName ?? '—'),
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          Expanded(
                            child: FilledButton.icon(
                              onPressed: _collect,
                              icon: const Icon(Icons.payments_outlined, size: 18),
                              label: const Text('تسديد'),
                              style: FilledButton.styleFrom(
                                minimumSize: const Size.fromHeight(42),
                                padding:
                                    const EdgeInsets.symmetric(horizontal: 8),
                              ),
                            ),
                          ),
                          const SizedBox(width: 10),
                          Expanded(
                            child: OutlinedButton.icon(
                              onPressed: canPrint && !_printing ? _print : null,
                              icon: Icon(
                                Icons.print_outlined,
                                size: 18,
                                color: canPrint
                                    ? AppColors.primary
                                    : AppColors.muted,
                              ),
                              label: Text(
                                'طباعة',
                                style: AppTextStyles.body.copyWith(
                                  color: canPrint
                                      ? AppColors.primary
                                      : AppColors.muted,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                              style: OutlinedButton.styleFrom(
                                minimumSize: const Size.fromHeight(42),
                                padding:
                                    const EdgeInsets.symmetric(horizontal: 8),
                                side: BorderSide(
                                  color: canPrint
                                      ? AppColors.primary
                                      : AppColors.border,
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _row(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 100,
            child: Text(label, style: AppTextStyles.muted.copyWith(fontSize: 13)),
          ),
          Expanded(
            child: Text(
              value,
              style: AppTextStyles.body.copyWith(
                fontWeight: FontWeight.w600,
                fontSize: 13.5,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
