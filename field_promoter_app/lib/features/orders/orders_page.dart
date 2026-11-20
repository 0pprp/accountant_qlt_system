import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

import '../../core/formatting/iraq_time.dart';
import '../../core/network/api_errors.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/feedback.dart';
import '../../core/widgets/neu.dart';
import '../../injection.dart';
import '../promoter/models.dart';
import 'order_detail_page.dart';

Color stageColor(String stage) => switch (stage) {
  'finished' => AppColors.success,
  'sold' => const Color(0xFF1565C0),
  'rejected' => AppColors.danger,
  _ => AppColors.primary,
};

class OrdersPage extends StatefulWidget {
  const OrdersPage({super.key, required this.tab, required this.index});
  final ValueListenable<int> tab;
  final int index;

  @override
  State<OrdersPage> createState() => _OrdersPageState();
}

class _OrdersPageState extends State<OrdersPage> {
  OrderTab _current = OrderTab.inProgress;
  final Map<OrderTab, GlobalKey<_OrderListState>> _keys = {
    for (final t in OrderTab.values) t: GlobalKey<_OrderListState>(),
  };
  bool _seen = false;

  @override
  void initState() {
    super.initState();
    widget.tab.addListener(_onTab);
    Injection.store.addListener(_onStore);
  }

  @override
  void dispose() {
    widget.tab.removeListener(_onTab);
    Injection.store.removeListener(_onStore);
    super.dispose();
  }

  void _onStore() {
    if (mounted) setState(() {});
  }

  void _onTab() {
    if (widget.tab.value != widget.index) return;
    unawaited(Injection.store.refreshCounts().then((_) {}, onError: (_) {}));
    if (_seen) {
      _keys[_current]?.currentState?.refreshIfStale();
    }
    if (!_seen) setState(() => _seen = true);
  }

  @override
  Widget build(BuildContext context) {
    if (!_seen) return const SizedBox.shrink();
    final counts = Injection.store.counts;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const Padding(
          padding: EdgeInsets.fromLTRB(18, 16, 18, 12),
          child: PageHeader(title: 'الطلبات', subtitle: 'متابعة طلبات البيع التي أرسلتها'),
        ),
        SizedBox(
          height: 46,
          child: ListView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 18),
            children: [
              for (final t in OrderTab.values)
                Padding(
                  padding: const EdgeInsetsDirectional.only(end: 10, bottom: 6),
                  child: NeuChip(
                    label: '${t.label} (${counts.of(t)})',
                    selected: _current == t,
                    color: stageColor(t.apiName),
                    onTap: () => setState(() => _current = t),
                  ),
                ),
            ],
          ),
        ),
        const SizedBox(height: 6),
        Expanded(
          child: IndexedStack(
            index: _current.index,
            children: [
              for (final t in OrderTab.values)
                _OrderList(key: _keys[t], tab: t, active: _current == t),
            ],
          ),
        ),
      ],
    );
  }
}

class _OrderList extends StatefulWidget {
  const _OrderList({super.key, required this.tab, required this.active});
  final OrderTab tab;
  final bool active;

  @override
  State<_OrderList> createState() => _OrderListState();
}

class _OrderListState extends State<_OrderList> {
  final _items = <PromoterOrder>[];
  final _scroll = ScrollController();
  int _page = 0;
  bool _hasMore = true;
  bool _loading = false;
  String? _error;
  DateTime? _loadedAt;
  bool _started = false;

  @override
  void initState() {
    super.initState();
    _scroll.addListener(_onScroll);
    if (widget.active) _start();
  }

  @override
  void didUpdateWidget(covariant _OrderList oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.active && !_started) _start();
  }

  @override
  void dispose() {
    _scroll.dispose();
    super.dispose();
  }

  void _start() {
    _started = true;
    unawaited(reload());
  }

  void refreshIfStale() {
    final at = _loadedAt;
    if (at == null || DateTime.now().difference(at) > const Duration(seconds: 10)) {
      unawaited(reload());
    }
  }

  void _onScroll() {
    if (_scroll.position.extentAfter < 300) unawaited(_more());
  }

  Future<void> reload() async {
    _page = 0;
    _hasMore = true;
    _error = null;
    await _more(reset: true);
    unawaited(Injection.store.refreshCounts(force: true).then((_) {}, onError: (_) {}));
  }

  Future<void> _more({bool reset = false}) async {
    if (_loading || (!_hasMore && !reset)) return;
    setState(() => _loading = true);
    try {
      final next = _page + 1;
      final res = await Injection.api.orders(widget.tab, next);
      if (!mounted) return;
      setState(() {
        if (reset) _items.clear();
        _items.addAll(res.items);
        _page = next;
        _hasMore = res.hasMore;
        _error = null;
        _loadedAt = DateTime.now();
      });
    } catch (e) {
      if (mounted) setState(() => _error = ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _open(PromoterOrder o) async {
    final changed = await Navigator.of(context).push<bool>(
      MaterialPageRoute(builder: (_) => OrderDetailPage(orderId: o.id, initial: o)),
    );
    if (changed == true) unawaited(reload());
  }

  @override
  Widget build(BuildContext context) {
    if (_items.isEmpty) {
      if (_loading || !_started) return const Center(child: CircularProgressIndicator());
      if (_error != null) {
        return StateView(
          icon: Icons.cloud_off_rounded,
          message: _error!,
          actionLabel: 'إعادة المحاولة',
          onAction: reload,
        );
      }
      return RefreshIndicator(
        onRefresh: reload,
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          children: const [
            SizedBox(height: 120),
            StateView(icon: Icons.inbox_outlined, message: 'لا توجد طلبات هنا'),
          ],
        ),
      );
    }
    return RefreshIndicator(
      onRefresh: reload,
      child: ListView.separated(
        controller: _scroll,
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(18, 8, 18, 24),
        itemCount: _items.length + 1,
        separatorBuilder: (_, _) => const SizedBox(height: 14),
        itemBuilder: (context, i) {
          if (i == _items.length) {
            if (_loading) {
              return const Padding(
                padding: EdgeInsets.all(12),
                child: Center(child: CircularProgressIndicator()),
              );
            }
            if (_error != null) {
              return TextButton(onPressed: _more, child: Text('$_error — إعادة المحاولة'));
            }
            return const SizedBox(height: 8);
          }
          return OrderCard(order: _items[i], onTap: () => _open(_items[i]));
        },
      ),
    );
  }
}

class OrderCard extends StatelessWidget {
  const OrderCard({super.key, required this.order, required this.onTap});
  final PromoterOrder order;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final color = stageColor(order.stage);
    return NeuCard(
      onTap: onTap,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  order.customerName,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16),
                ),
              ),
              const SizedBox(width: 8),
              Flexible(child: StatusPill(order.statusLabel, color: color)),
            ],
          ),
          const SizedBox(height: 6),
          Row(
            children: [
              const Icon(Icons.inventory_2_outlined, size: 16, color: AppColors.muted),
              const SizedBox(width: 4),
              Expanded(
                child: Text(
                  order.product,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(color: AppColors.muted),
                ),
              ),
              Text(
                '#${order.id} · ${IraqTime.smart(order.createdAt)}',
                textDirection: TextDirection.rtl,
                style: const TextStyle(color: AppColors.muted, fontSize: 12),
              ),
            ],
          ),
          if (order.sale != null) ...[
            const SizedBox(height: 10),
            ClipRRect(
              borderRadius: BorderRadius.circular(6),
              child: LinearProgressIndicator(
                value: order.sale!.percent / 100,
                minHeight: 6,
                backgroundColor: AppColors.primarySoft,
                color: color,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              'تسديد المبيعة ${order.sale!.percent}%',
              style: const TextStyle(fontSize: 12, color: AppColors.muted),
            ),
          ],
        ],
      ),
    );
  }
}
