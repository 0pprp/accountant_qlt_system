import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/empty_state.dart';
import '../../../core/widgets/loading_view.dart';
import '../../../injection.dart';
import '../../payments/data/delegate_business_date_service.dart';
import '../domain/entities/customer.dart';
import 'widgets/customer_card.dart';

class CustomersPage extends StatefulWidget {
  const CustomersPage({super.key});

  @override
  State<CustomersPage> createState() => _CustomersPageState();
}

class _CustomersPageState extends State<CustomersPage> {
  final _search = TextEditingController();
  List<Customer> _all = [];
  bool _loading = true;
  String? _error;
  CustomerListFilter _filter = CustomerListFilter.continuous;

  @override
  void initState() {
    super.initState();
    DelegateBusinessDateService.dayRevision.addListener(_onDayRollover);
    _load(initialRefresh: true);
  }

  void _onDayRollover() {
    if (!mounted) return;
    _load(initialRefresh: true);
  }

  @override
  void dispose() {
    DelegateBusinessDateService.dayRevision.removeListener(_onDayRollover);
    _search.dispose();
    super.dispose();
  }

  List<Customer> get _items {
    final q = _search.text.trim();
    return _all.where((c) {
      if (!_filter.matches(c)) return false;
      if (q.isEmpty) return true;
      final hay = '${c.name} ${c.phone ?? ''} ${c.shopName ?? ''} ${c.customerId}'
          .toLowerCase();
      return hay.contains(q.toLowerCase());
    }).toList();
  }

  Future<void> _load({bool initialRefresh = false}) async {
    setState(() {
      _error = null;
      if (_all.isEmpty) _loading = true;
    });
    try {
      if (initialRefresh) {
        final cached = await Injection.getLocalCustomers();
        if (cached.isNotEmpty && mounted) {
          setState(() {
            _all = cached;
            _loading = false;
          });
        }
        if (cached.isEmpty) {
          await Injection.refreshCustomers(force: true);
        } else {
          // ignore: unawaited_futures
          Injection.refreshCustomers(force: false).then((_) async {
            if (!mounted) return;
            final list = await Injection.getLocalCustomers();
            if (mounted) setState(() => _all = list);
          });
        }
      }
      final list = await Injection.getLocalCustomers();
      if (!mounted) return;
      setState(() => _all = list);
    } catch (e) {
      if (!mounted) return;
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final items = _items;
    return SafeArea(
      bottom: false,
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _search,
                    style: AppTextStyles.body,
                    decoration: const InputDecoration(
                      hintText: 'بحث بالاسم أو الهاتف أو الرقم',
                      prefixIcon: Icon(Icons.search),
                      isDense: true,
                    ),
                    onChanged: (_) => setState(() {}),
                  ),
                ),
                const SizedBox(width: 8),
                PopupMenuButton<CustomerListFilter>(
                  tooltip: 'فلتر العملاء',
                  initialValue: _filter,
                  onSelected: (v) => setState(() => _filter = v),
                  itemBuilder: (context) => [
                    for (final f in CustomerListFilter.values)
                      CheckedPopupMenuItem(
                        value: f,
                        checked: _filter == f,
                        child: Text(f.labelAr),
                      ),
                  ],
                  child: Container(
                    height: 48,
                    padding: const EdgeInsets.symmetric(horizontal: 12),
                    decoration: BoxDecoration(
                      color: AppColors.card,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(
                        color: AppColors.primary.withValues(alpha: 0.25),
                      ),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.filter_list_rounded, size: 20),
                        const SizedBox(width: 6),
                        Text(
                          _filter.labelAr,
                          style: AppTextStyles.body.copyWith(
                            fontWeight: FontWeight.w700,
                            fontSize: 13,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                IconButton(
                  tooltip: 'تحديث',
                  icon: const Icon(Icons.refresh),
                  onPressed: () => _load(initialRefresh: true),
                ),
              ],
            ),
          ),
          Expanded(
            child: _loading && _all.isEmpty
                ? const LoadingView()
                : _error != null && _all.isEmpty
                    ? EmptyState(
                        message: _error!,
                        icon: Icons.error_outline,
                      )
                    : items.isEmpty
                        ? EmptyState(
                            message: _all.isEmpty
                                ? 'لا يوجد زبائن في الكاش المحلي'
                                : 'لا زبائن ضمن فلتر «${_filter.labelAr}»',
                          )
                        : Builder(
                            builder: (context) {
                              final bottomPad = 130.0 +
                                  MediaQuery.paddingOf(context).bottom;
                              return ListView.separated(
                                physics:
                                    const AlwaysScrollableScrollPhysics(),
                                padding: EdgeInsets.fromLTRB(
                                  16,
                                  8,
                                  16,
                                  bottomPad,
                                ),
                                itemCount: items.length,
                                separatorBuilder: (_, _) =>
                                    const SizedBox(height: 12),
                                itemBuilder: (context, i) {
                                  final c = items[i];
                                  return CustomerCard(
                                    key: ValueKey(c.customerId),
                                    customer: c,
                                    onChanged: () => _load(),
                                  );
                                },
                              );
                            },
                          ),
          ),
        ],
      ),
    );
  }
}
