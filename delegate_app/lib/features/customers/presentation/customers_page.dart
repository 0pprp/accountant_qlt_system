import 'package:flutter/material.dart';

import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/empty_state.dart';
import '../../../core/widgets/loading_view.dart';
import '../../../injection.dart';
import '../../sync/presentation/sync_status_banner.dart';
import '../domain/entities/customer.dart';
import 'widgets/customer_card.dart';

class CustomersPage extends StatefulWidget {
  const CustomersPage({super.key});

  @override
  State<CustomersPage> createState() => _CustomersPageState();
}

class _CustomersPageState extends State<CustomersPage> {
  final _search = TextEditingController();
  List<Customer> _items = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load(initialRefresh: true);
  }

  @override
  void dispose() {
    _search.dispose();
    super.dispose();
  }

  Future<void> _load({bool initialRefresh = false}) async {
    setState(() {
      _error = null;
      if (_items.isEmpty) _loading = true;
    });
    try {
      if (initialRefresh) {
        final cached = await Injection.getLocalCustomers();
        if (cached.isNotEmpty && mounted) {
          setState(() {
            _items = cached;
            _loading = false;
          });
        }
        if (cached.isEmpty) {
          await Injection.refreshCustomers(force: true);
        } else {
          // ignore: unawaited_futures
          Injection.refreshCustomers(force: false).then((_) async {
            if (!mounted) return;
            final list =
                await Injection.getLocalCustomers(query: _search.text);
            if (mounted) setState(() => _items = list);
          });
        }
      }
      final list = await Injection.getLocalCustomers(query: _search.text);
      if (!mounted) return;
      setState(() => _items = list);
    } catch (e) {
      if (!mounted) return;
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      bottom: false,
      child: Column(
        children: [
          const SyncStatusBanner(),
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
            child: TextField(
              controller: _search,
              style: AppTextStyles.body,
              decoration: InputDecoration(
                hintText: 'بحث بالاسم أو الهاتف أو الرقم',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: IconButton(
                  icon: const Icon(Icons.refresh),
                  onPressed: () => _load(initialRefresh: true),
                ),
              ),
              onChanged: (_) => _load(),
            ),
          ),
          Expanded(
            child: _loading && _items.isEmpty
                ? const LoadingView()
                : _error != null && _items.isEmpty
                    ? EmptyState(
                        message: _error!,
                        icon: Icons.error_outline,
                      )
                    : _items.isEmpty
                        ? const EmptyState(
                            message: 'لا يوجد زبائن في الكاش المحلي',
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
                                itemCount: _items.length,
                                separatorBuilder: (_, _) =>
                                    const SizedBox(height: 12),
                                itemBuilder: (context, i) {
                                  final c = _items[i];
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
