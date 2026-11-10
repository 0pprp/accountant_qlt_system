import '../../sales/presentation/new_sales_request_page.dart';
import 'dart:async';

import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/animated_waves.dart';
import 'package:sales_employee_app/core/widgets/brand_logo.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/core/widgets/glassy_status_card.dart';
import 'package:sales_employee_app/features/catalog/presentation/price_board_page.dart';
import 'package:sales_employee_app/features/catalog/presentation/warehouse_page.dart';
import 'package:sales_employee_app/features/home/presentation/widgets/home_announcements.dart';
import 'package:sales_employee_app/features/home/presentation/widgets/home_drawer.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sale_order.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_requests_bin_page.dart';
import 'package:sales_employee_app/features/sales/presentation/sale_details_page.dart';
import 'package:sales_employee_app/features/shift/presentation/shift_page.dart';
import 'package:sales_employee_app/injection.dart';

class HomeTab extends StatefulWidget {
  const HomeTab({
    super.key,
    required this.displayName,
    required this.onLogout,
    required this.dutyActive,
    required this.onDutyChanged,
  });

  final String displayName;
  final VoidCallback onLogout;
  final bool dutyActive;
  final VoidCallback onDutyChanged;

  @override
  State<HomeTab> createState() => _HomeTabState();
}

class _HomeTabState extends State<HomeTab> {
  final _searchCtrl = TextEditingController();
  List<SalesBinSummary> _bins = const [];
  List<SaleOrder> _searchHits = const [];
  bool _loading = false;
  bool _searchMode = false;
  Timer? _poll;

  static const _announcements = [
    HomeAnnouncement(
      title: 'طلبات البيع لحظية',
      body: 'حالات الطلب تتحدّث تلقائياً عند أي تعديل من لوحة مدير المبيعات.',
    ),
    HomeAnnouncement(
      title: 'إتمام البيع',
      body: 'من طلبات البيع اضغط إتمام البيع وعبّئ الخطوات الخمس للطلب التجريبي.',
    ),
    HomeAnnouncement(
      title: 'طلب مبيع جديد',
      body: 'أرسل طلب بيع جديد للتدقيق في لوحة المبيعات قبل الإسناد.',
    ),
  ];

  static const _binIcons = <String, IconData>{
    'pending': Icons.receipt_long_rounded,
    'ready': Icons.task_alt_rounded,
    'disclosure': Icons.fact_check_rounded,
    'on_hold': Icons.pause_circle_outline,
    'rejected': Icons.cancel_outlined,
    'sold': Icons.verified_rounded,
  };

  static const _binColors = <String, Color>{
    'pending': AppColors.primary,
    'ready': AppColors.success,
    'disclosure': AppColors.info,
    'on_hold': AppColors.warning,
    'rejected': AppColors.danger,
    'sold': AppColors.secondary,
  };

  @override
  void initState() {
    super.initState();
    // SignalR مستقل عن حالة الدوام — مطلوب لانعكاس التحديثات على اللوحة/العدادات
    unawaited(_startRealtime());
    // لا تحمّل العدّادات فوق شاشة الدوام الإلزامي — يقلّل سباق الجلسة على Web
    if (widget.dutyActive) {
      _bootDuty();
    }
  }

  @override
  void didUpdateWidget(covariant HomeTab oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.dutyActive && !oldWidget.dutyActive) {
      _bootDuty();
    } else if (!widget.dutyActive) {
      _poll?.cancel();
      // لا تقطع SignalR عند إيقاف الدوام — فقط polling الموقع/الدوام
    }
  }

  @override
  void dispose() {
    _poll?.cancel();
    unawaited(Injection.salesRealtime.stop());
    _searchCtrl.dispose();
    super.dispose();
  }

  Future<void> _bootDuty() async {
    // لا تملأ الشاشة من demo_orders عند التشغيل — العدادات من SalesRequests
    await _loadBins(force: true);
    _poll?.cancel();
    // polling احتياطي للدوام — SignalR يُدار من initState/_startRealtime
    _poll = Timer.periodic(const Duration(seconds: 12), (_) => _loadBins(force: true, silent: true));
    unawaited(_startRealtime());
  }

  Future<void> _startRealtime() async {
    try {
      final token = await Injection.sessionStore.getAccessToken();
      final userId = await Injection.sessionStore.getUserId();
      if (token == null || token.isEmpty) return;
      await Injection.salesRealtime.start(
        accessToken: token,
        sessionGeneration: userId ?? 0,
        handler: (_) {
          if (!mounted) return;
          unawaited(_loadBins(force: true, silent: true));
        },
      );
    } catch (_) {
      // فشل الاتصال لا يفعّل demo ولا يُخفي الخطأ بصمت عن العدادات
    }
  }

  Future<void> _loadBins({bool force = false, bool silent = false}) async {
    if (!silent && mounted) setState(() => _loading = true);
    try {
      // 04C: عدادات طلبات الموظف من SalesRequests — ليس Orders/demo
      final bins = await Injection.salesRemote.getSalesRequestBins(forceRefresh: force);
      final defaults = [
        const SalesBinSummary(key: 'pending', label: 'طلبات البيع', count: 0),
        const SalesBinSummary(key: 'ready', label: 'جاهز للبيع', count: 0),
        const SalesBinSummary(key: 'disclosure', label: 'تم الكشف', count: 0),
        const SalesBinSummary(key: 'on_hold', label: 'معلقة', count: 0),
        const SalesBinSummary(key: 'rejected', label: 'مرفوض', count: 0),
        const SalesBinSummary(key: 'sold', label: 'تم البيع', count: 0),
      ];
      final byKey = {for (final b in bins) b.key.toLowerCase().replaceAll('-', '_'): b};
      final merged = defaults
          .map((d) {
            final hit = byKey[d.key];
            return SalesBinSummary(
              key: d.key,
              label: hit?.label.isNotEmpty == true ? hit!.label : d.label,
              count: hit?.count ?? 0,
            );
          })
          .toList();
      if (!mounted) return;
      setState(() {
        _bins = merged;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      // لا تبديل إلى demo_orders_store عند فشل الشبكة
      if (!silent) {
        await showAppMessage(context, title: 'حالات الطلب', message: DioClient.mapError(e));
      }
    }
  }

  Future<void> _searchAcross() async {
    final q = _searchCtrl.text.trim();
    if (q.isEmpty) {
      setState(() {
        _searchMode = false;
        _searchHits = const [];
      });
      return;
    }
    setState(() {
      _loading = true;
      _searchMode = true;
    });
    try {
      List<SaleOrder> all = const [];
      try {
        all = await Injection.salesRemote.getOrders(forceRefresh: true);
      } catch (_) {}
      final demo = await Injection.demoOrders.all();
      var customerIds = <int>{};
      try {
        final customers = await Injection.customersRemote.search(query: q);
        customerIds = customers.map((c) => c.id).toSet();
      } catch (_) {}
      final merged = [...demo, ...all];
      final filtered = merged.where((o) {
        final name = o.customerName.toLowerCase();
        return name.contains(q.toLowerCase()) ||
            '${o.id}'.contains(q) ||
            (o.customerId != null && customerIds.contains(o.customerId));
      }).toList();
      if (!mounted) return;
      setState(() {
        _searchHits = filtered;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      await showAppMessage(context, title: 'بحث', message: DioClient.mapError(e));
    }
  }

  void _openNewSaleRequest() {
    Navigator.of(context).push(MaterialPageRoute(builder: (_) => const NewSalesRequestPage()));
  }

  @override
  Widget build(BuildContext context) {
    if (!widget.dutyActive) {
      return AnimatedWavesBackground(
        child: SafeArea(
          child: Center(
            child: Padding(
              padding: const EdgeInsets.all(28),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const BrandLogo(height: 96),
                  const SizedBox(height: 18),
                  const Text(
                    'قلعة الضمان',
                    style: TextStyle(fontSize: 26, fontWeight: FontWeight.w700, color: AppColors.primary),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'ابدأ الدوام للمتابعة إلى حالات الطلب',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: AppColors.textSecondary),
                  ),
                  const SizedBox(height: 28),
                  ElevatedButton(
                    onPressed: () async {
                      await Navigator.of(context).push(
                        MaterialPageRoute(
                          builder: (_) => ShiftPage(onDutyChanged: widget.onDutyChanged),
                        ),
                      );
                    },
                    child: const Text('بدء الدوام'),
                  ),
                ],
              ),
            ),
          ),
        ),
      );
    }

    return Scaffold(
      drawer: HomeDrawer(
        displayName: widget.displayName,
        onLogout: widget.onLogout,
        onDutyChanged: widget.onDutyChanged,
      ),
      body: AnimatedWavesBackground(
        intensity: 1.15,
        child: SafeArea(
          child: RefreshIndicator(
            onRefresh: () => _loadBins(force: true),
            child: CustomScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              slivers: [
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
                    child: Column(
                      children: [
                        Row(
                          children: [
                            Builder(
                              builder: (ctx) => IconButton(
                                onPressed: () => Scaffold.of(ctx).openDrawer(),
                                icon: const Icon(Icons.menu_rounded, color: AppColors.primary),
                              ),
                            ),
                            const Expanded(child: Center(child: BrandLogo(height: 52))),
                            IconButton(
                              onPressed: () => Navigator.of(context).push(
                                MaterialPageRoute(
                                  builder: (_) => ShiftPage(onDutyChanged: widget.onDutyChanged),
                                ),
                              ),
                              icon: const Icon(Icons.schedule_rounded, color: AppColors.primary),
                            ),
                          ],
                        ),
                        Text(widget.displayName, style: const TextStyle(color: AppColors.textSecondary)),
                        const SizedBox(height: 12),
                        const HomeAnnouncementsCarousel(items: _announcements),
                        const SizedBox(height: 14),
                        Row(
                          children: [
                            Expanded(
                              child: _shortcut(
                                icon: Icons.inventory_2_rounded,
                                label: 'المخزن',
                                onTap: () => Navigator.of(context).push(
                                  MaterialPageRoute(builder: (_) => const WarehousePage()),
                                ),
                              ),
                            ),
                            const SizedBox(width: 10),
                            Expanded(
                              child: _shortcut(
                                icon: Icons.price_change_rounded,
                                label: 'لوحة الأسعار',
                                onTap: () => Navigator.of(context).push(
                                  MaterialPageRoute(builder: (_) => const PriceBoardPage()),
                                ),
                              ),
                            ),
                            const SizedBox(width: 10),
                            Expanded(
                              child: _shortcut(
                                icon: Icons.add_business_rounded,
                                label: 'طلب مبيع جديد',
                                onTap: _openNewSaleRequest,
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 12),
                        TextField(
                          controller: _searchCtrl,
                          decoration: InputDecoration(
                            hintText: 'بحث بالاسم أو الرقم عبر كل الحالات',
                            prefixIcon: const Icon(Icons.search),
                            suffixIcon: IconButton(
                              onPressed: _searchAcross,
                              icon: const Icon(Icons.arrow_back),
                            ),
                          ),
                          onSubmitted: (_) => _searchAcross(),
                        ),
                        const SizedBox(height: 14),
                        const Align(
                          alignment: Alignment.centerRight,
                          child: Text(
                            'حالات الطلبات',
                            style: TextStyle(fontSize: 17, fontWeight: FontWeight.w700),
                          ),
                        ),
                        const SizedBox(height: 4),
                        const Align(
                          alignment: Alignment.centerRight,
                          child: Text(
                            'تحديث لحظي كل بضع ثوانٍ',
                            style: TextStyle(fontSize: 12, color: AppColors.textSecondary),
                          ),
                        ),
                        const SizedBox(height: 10),
                      ],
                    ),
                  ),
                ),
                if (_loading && _bins.isEmpty)
                  const SliverFillRemaining(
                    hasScrollBody: false,
                    child: Center(child: CircularProgressIndicator()),
                  )
                else if (_searchMode)
                  SliverPadding(
                    padding: const EdgeInsets.fromLTRB(16, 0, 16, 120),
                    sliver: _searchHits.isEmpty
                        ? const SliverToBoxAdapter(
                            child: Padding(
                              padding: EdgeInsets.only(top: 40),
                              child: Center(child: Text('لا نتائج')),
                            ),
                          )
                        : SliverList.builder(
                            itemCount: _searchHits.length,
                            itemBuilder: (_, i) {
                              final o = _searchHits[i];
                              final demo = o.id < 0;
                              return Card(
                                child: ListTile(
                                  title: Text(o.customerName),
                                  subtitle: Text('#${o.id.abs()} • ${o.status ?? '-'}'),
                                  onTap: demo
                                      ? null
                                      : () => Navigator.of(context).push(
                                            MaterialPageRoute(
                                              builder: (_) => SaleDetailsPage(orderId: o.id),
                                            ),
                                          ),
                                ),
                              );
                            },
                          ),
                  )
                else
                  SliverPadding(
                    padding: const EdgeInsets.fromLTRB(16, 0, 16, 120),
                    sliver: SliverGrid(
                      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: 2,
                        mainAxisSpacing: 12,
                        crossAxisSpacing: 12,
                        childAspectRatio: 1.15,
                      ),
                      delegate: SliverChildBuilderDelegate(
                        (context, i) {
                          final b = _bins[i];
                          return GlassyStatusCard(
                            title: b.label,
                            count: b.count,
                            icon: _binIcons[b.key] ?? Icons.inbox_rounded,
                            accent: _binColors[b.key] ?? AppColors.primary,
                            onTap: () async {
                              await Navigator.of(context).push(
                                MaterialPageRoute(
                                  settings: RouteSettings(
                                    name: '/sales-requests/bin/${b.key}',
                                  ),
                                  // 04A/04H: صناديق SalesRequest — ليست Orders
                                  builder: (_) => SalesRequestsBinPage(
                                    uiStatusKey: b.key,
                                    title: b.label,
                                  ),
                                ),
                              );
                              await _loadBins(force: true, silent: true);
                            },
                          );
                        },
                        childCount: _bins.length,
                      ),
                    ),
                  ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _shortcut({
    required IconData icon,
    required String label,
    required VoidCallback onTap,
    String? subtitle,
    bool muted = false,
  }) {
    return Material(
      color: AppColors.surface.withValues(alpha: muted ? 0.7 : 1),
      borderRadius: BorderRadius.circular(14),
      child: InkWell(
        borderRadius: BorderRadius.circular(14),
        onTap: onTap,
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 6),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: AppColors.border),
          ),
          child: Column(
            children: [
              Icon(icon, color: muted ? AppColors.muted : AppColors.primary),
              const SizedBox(height: 6),
              Text(
                label,
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontWeight: FontWeight.w600,
                  fontSize: 11.5,
                  color: muted ? AppColors.muted : AppColors.textPrimary,
                ),
              ),
              if (subtitle != null)
                Text(
                  subtitle,
                  style: const TextStyle(fontSize: 10, color: AppColors.info),
                ),
            ],
          ),
        ),
      ),
    );
  }
}
