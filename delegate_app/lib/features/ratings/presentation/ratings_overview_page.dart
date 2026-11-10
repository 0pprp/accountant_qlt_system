import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/time/iraq_date.dart';
import '../../../core/widgets/app_dialog.dart';
import '../../../core/widgets/brand_wave_painter.dart';
import '../../../core/widgets/loading_view.dart';
import '../../../injection.dart';
import '../../customers/domain/entities/customer.dart';
import '../domain/services/customer_rating_calculator.dart';

class _RatedCustomer {
  const _RatedCustomer({
    required this.customer,
    required this.score,
    required this.level,
  });

  final Customer customer;
  final int score;
  final String level;
}

class RatingsOverviewPage extends StatefulWidget {
  const RatingsOverviewPage({super.key});

  @override
  State<RatingsOverviewPage> createState() => _RatingsOverviewPageState();
}

class _RatingsOverviewPageState extends State<RatingsOverviewPage> {
  bool _loading = true;
  double _average = 0;
  final Map<String, int> _counts = {
    'ممتاز': 0,
    'جيد': 0,
    'ضعيف': 0,
    'متلكئ': 0,
    'سيء': 0,
  };
  final Map<String, List<_RatedCustomer>> _byLevel = {};
  int _inCohort = 0;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    final asOf = IraqDate.calendarDate();
    final customers = await Injection.getLocalCustomers();
    final scores = <int>[];
    final counts = {
      'ممتاز': 0,
      'جيد': 0,
      'ضعيف': 0,
      'متلكئ': 0,
      'سيء': 0,
    };
    final byLevel = <String, List<_RatedCustomer>>{
      'ممتاز': [],
      'جيد': [],
      'ضعيف': [],
      'متلكئ': [],
      'سيء': [],
    };

    for (final c in customers) {
      final last = CustomerRatingCalculator.parseDate(c.lastPaymentDate);
      final remaining = c.remaining ?? 0;
      if (!CustomerRatingCalculator.inOverallCohort(
        remaining: remaining,
        lastPayment: last,
        asOf: asOf,
      )) {
        continue;
      }
      final score = CustomerRatingCalculator.scoreFor(
        amountTotalSales: c.salePrice ?? 0,
        receiptsTotal: c.receiptTotal ?? 0,
        amountRemaining: remaining,
        dateSaleDevice: CustomerRatingCalculator.parseDate(c.dateSaleDevice),
        lastPaymentDate: last,
        asOf: asOf,
      );
      if (score == null) continue;
      scores.add(score);
      final level = CustomerRatingCalculator.levelName(score);
      counts[level] = (counts[level] ?? 0) + 1;
      byLevel.putIfAbsent(level, () => []).add(
            _RatedCustomer(customer: c, score: score, level: level),
          );
    }

    if (!mounted) return;
    setState(() {
      _average = CustomerRatingCalculator.averageScore(scores);
      _counts
        ..clear()
        ..addAll(counts);
      _byLevel
        ..clear()
        ..addAll(byLevel);
      _inCohort = scores.length;
      _loading = false;
    });
  }

  Future<void> _openLevel(String level) async {
    final list = _byLevel[level] ?? const [];
    final money = NumberFormat('#,##0.##', 'en');
    await AppDialog.show<void>(
      context: context,
      title: 'زبائن — $level',
      child: list.isEmpty
          ? Text(
              'لا يوجد زبائن في هذا المستوى',
              style: AppTextStyles.muted,
              textAlign: TextAlign.center,
            )
          : Column(
              children: [
                for (final r in list)
                  Container(
                    width: double.infinity,
                    margin: const EdgeInsets.only(bottom: 8),
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: AppColors.surfaceSoft,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: AppColors.border),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          r.customer.name,
                          style: AppTextStyles.body.copyWith(
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'الواصل: ${money.format(r.customer.receiptTotal ?? 0)}',
                          style: AppTextStyles.muted,
                        ),
                        Text(
                          'الباقي: ${money.format(r.customer.remaining ?? 0)}',
                          style: AppTextStyles.muted,
                        ),
                        Text(
                          'سعر البيع: ${money.format(r.customer.salePrice ?? 0)}',
                          style: AppTextStyles.muted,
                        ),
                        Text(
                          'التقييم: ${r.score}',
                          style: AppTextStyles.muted.copyWith(
                            color: AppColors.primary,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                      ],
                    ),
                  ),
              ],
            ),
      actions: [
        FilledButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('إغلاق'),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: Text('التقييم', style: AppTextStyles.title)),
      body: _loading
          ? const LoadingView()
          : RefreshIndicator(
              onRefresh: _load,
              child: ListView(
                padding: const EdgeInsets.fromLTRB(16, 16, 16, 32),
                children: [
                  ClipRRect(
                    borderRadius: BorderRadius.circular(16),
                    child: Stack(
                      children: [
                        Positioned.fill(child: Container(color: AppColors.primary)),
                        const Positioned.fill(
                          child: CustomPaint(
                            painter: BrandWavePainter(opacity: 0.2),
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.all(20),
                          child: Column(
                            children: [
                              Text(
                                'متوسط تقييم المندوب',
                                style: AppTextStyles.muted.copyWith(
                                  color: Colors.white.withValues(alpha: 0.9),
                                ),
                              ),
                              const SizedBox(height: 6),
                              Text(
                                CustomerRatingCalculator.formatAverage(
                                  _average,
                                ),
                                style: AppTextStyles.display.copyWith(
                                  color: Colors.white,
                                ),
                              ),
                              Text(
                                'من $_inCohort زبون في العينة',
                                style: AppTextStyles.muted.copyWith(
                                  color: Colors.white.withValues(alpha: 0.85),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                  ..._counts.entries.map(
                    (e) => Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: Material(
                        color: AppColors.surface,
                        borderRadius: BorderRadius.circular(14),
                        child: InkWell(
                          borderRadius: BorderRadius.circular(14),
                          onTap: () => _openLevel(e.key),
                          child: Ink(
                            decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(14),
                              border: Border.all(color: AppColors.border),
                            ),
                            padding: const EdgeInsets.symmetric(
                              horizontal: 16,
                              vertical: 14,
                            ),
                            child: Row(
                              children: [
                                Expanded(
                                  child: Text(e.key, style: AppTextStyles.body),
                                ),
                                Text(
                                  '${e.value}',
                                  style: AppTextStyles.amount,
                                ),
                                const SizedBox(width: 6),
                                Icon(
                                  Icons.chevron_left,
                                  color: AppColors.primary.withValues(
                                    alpha: 0.55,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}
