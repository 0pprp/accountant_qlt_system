import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';

class ShopEvaluationResult {
  const ShopEvaluationResult({
    required this.rating,
    this.note,
    required this.shopStockEstimatedValue,
    required this.estimatedDailyRevenue,
  });

  final int rating;
  final String? note;
  final double shopStockEstimatedValue;
  final double estimatedDailyRevenue;
}

/// تقييم المحل قبل أول تسديد لمبيع جديد — نظير مصدر SalesHaider.
Future<ShopEvaluationResult?> showShopEvaluationDialog(
  BuildContext context, {
  String title = 'تقييم المحل — المبيعات الجديدة',
}) {
  return showDialog<ShopEvaluationResult>(
    context: context,
    barrierDismissible: false,
    builder: (ctx) => _ShopEvaluationDialog(title: title),
  );
}

class _ShopEvaluationDialog extends StatefulWidget {
  const _ShopEvaluationDialog({required this.title});

  final String title;

  @override
  State<_ShopEvaluationDialog> createState() => _ShopEvaluationDialogState();
}

class _ShopEvaluationDialogState extends State<_ShopEvaluationDialog> {
  final _note = TextEditingController();
  final _stock = TextEditingController();
  final _daily = TextEditingController();
  int _rating = 7;
  String? _error;

  @override
  void dispose() {
    _note.dispose();
    _stock.dispose();
    _daily.dispose();
    super.dispose();
  }

  double? _parseMoney(String raw) {
    final cleaned = raw.replaceAll(RegExp(r'[^0-9.]'), '');
    if (cleaned.isEmpty) return null;
    return double.tryParse(cleaned);
  }

  void _submit() {
    if (_rating < 7 && _note.text.trim().isEmpty) {
      setState(
        () => _error = 'الملاحظة إجبارية عندما يكون تقييم المحل أقل من 7',
      );
      return;
    }
    final stock = _parseMoney(_stock.text);
    if (stock == null || stock <= 0) {
      setState(() => _error = 'قيمة بضاعة المحل مطلوبة');
      return;
    }
    final daily = _parseMoney(_daily.text);
    if (daily == null || daily <= 0) {
      setState(() => _error = 'واردات المحل اليومية مطلوبة');
      return;
    }
    Navigator.pop(
      context,
      ShopEvaluationResult(
        rating: _rating,
        note: _note.text.trim().isEmpty ? null : _note.text.trim(),
        shopStockEstimatedValue: stock,
        estimatedDailyRevenue: daily,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Text(
          widget.title,
          style: AppTextStyles.title,
          textAlign: TextAlign.right,
        ),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                _rating < 7
                    ? 'التقييم إجباري مع ملاحظة إجبارية'
                    : 'التقييم إجباري مع ملاحظة اختيارية',
                style: AppTextStyles.muted.copyWith(
                  color: _rating < 7 ? AppColors.warning : AppColors.muted,
                ),
                textAlign: TextAlign.right,
              ),
              const SizedBox(height: 8),
              Text(
                'التقييم: $_rating',
                style: AppTextStyles.body.copyWith(fontWeight: FontWeight.w700),
                textAlign: TextAlign.right,
              ),
              Slider(
                value: _rating.toDouble(),
                min: 1,
                max: 10,
                divisions: 9,
                label: '$_rating',
                activeColor: AppColors.primary,
                onChanged: (v) => setState(() {
                  _rating = v.round();
                  _error = null;
                }),
              ),
              TextField(
                controller: _note,
                maxLines: 3,
                style: AppTextStyles.body,
                decoration: InputDecoration(
                  labelText: _rating < 7 ? 'ملاحظة *' : 'ملاحظة (اختياري)',
                  filled: true,
                  fillColor: AppColors.background,
                ),
                onChanged: (_) => setState(() => _error = null),
              ),
              TextField(
                controller: _stock,
                keyboardType: TextInputType.number,
                inputFormatters: [
                  FilteringTextInputFormatter.allow(RegExp(r'[0-9.]')),
                ],
                style: AppTextStyles.body,
                decoration: const InputDecoration(
                  labelText: 'قيمة بضاعة المحل *',
                  filled: true,
                  fillColor: AppColors.background,
                ),
                onChanged: (_) => setState(() => _error = null),
              ),
              TextField(
                controller: _daily,
                keyboardType: TextInputType.number,
                inputFormatters: [
                  FilteringTextInputFormatter.allow(RegExp(r'[0-9.]')),
                ],
                style: AppTextStyles.body,
                decoration: const InputDecoration(
                  labelText: 'واردات المحل اليومية *',
                  filled: true,
                  fillColor: AppColors.background,
                ),
                onChanged: (_) => setState(() => _error = null),
              ),
              if (_error != null) ...[
                const SizedBox(height: 8),
                Text(
                  _error!,
                  style: AppTextStyles.muted.copyWith(color: AppColors.danger),
                  textAlign: TextAlign.right,
                ),
              ],
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text(
              'إلغاء',
              style: AppTextStyles.body.copyWith(color: AppColors.danger),
            ),
          ),
          ElevatedButton(
            onPressed: _submit,
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary),
            child: Text('متابعة', style: AppTextStyles.button),
          ),
        ],
      ),
    );
  }
}
