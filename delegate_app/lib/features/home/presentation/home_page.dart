import '../../sales/presentation/new_sales_request_page.dart';
import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/loading_view.dart';
import '../../../injection.dart';
import '../../sync/presentation/sync_status_banner.dart';
import '../domain/entities/delegate_home_snapshot.dart';
import 'tutorial_videos_page.dart';
import 'widgets/home_announcements_carousel.dart';
import 'widgets/home_header_panel.dart';
import 'widgets/home_week_rating_card.dart';

class HomePage extends StatefulWidget {
  const HomePage({
    super.key,
    required this.onOpenMenu,
  });

  final VoidCallback onOpenMenu;

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> with WidgetsBindingObserver {
  DelegateHomeSnapshot? _snap;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _bootstrap();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    // Soft refresh only — never flip the whole page into a loading spinner.
    if (state == AppLifecycleState.resumed && _snap != null) {
      _reload();
    }
  }

  Future<void> _bootstrap() async {
    setState(() => _loading = true);
    try {
      // Paint from local cache first — do not block the UI on remote refresh.
      await _reload();
    } finally {
      if (mounted) setState(() => _loading = false);
    }
    // Background refresh once; keep current UI visible.
    try {
      await Injection.refreshCustomers(force: false);
      if (mounted) await _reload();
    } catch (_) {}
  }

  Future<void> _reload() async {
    final snap = await Injection.getHomeSnapshot();
    if (mounted) setState(() => _snap = snap);
  }

  void _openNewSaleRequest() {
    Navigator.of(context).push(MaterialPageRoute(builder: (_) => const NewSalesRequestPage()));
  }

  @override
  Widget build(BuildContext context) {
    final snap = _snap;
    return Column(
      children: [
        if (snap != null)
          HomeHeaderPanel(
            delegateName: snap.delegateName,
            listName: snap.listName,
            rating: snap.currentRating,
            avatarUrl: snap.avatarUrl,
            onOpenMenu: widget.onOpenMenu,
          )
        else
          Container(
            height: 100,
            color: AppColors.primary,
            alignment: Alignment.center,
            child: const CircularProgressIndicator(color: Colors.white),
          ),
        const SyncStatusBanner(),
        Expanded(
          child: _loading && snap == null
              ? const LoadingView()
              : RefreshIndicator(
                  color: AppColors.primary,
                  onRefresh: () async {
                    await Injection.refreshCustomers(force: true);
                    await Injection.paymentSyncService.syncNow();
                    await _reload();
                  },
                  child: ListView(
                    padding: EdgeInsets.fromLTRB(
                      16,
                      16,
                      16,
                      120 + MediaQuery.paddingOf(context).bottom,
                    ),
                    children: [
                      if (snap != null) ...[
                        HomeAnnouncementsCarousel(
                          items: snap.announcements,
                          state: snap.announcementsState,
                          errorMessage: snap.announcementsError,
                          onRetry: _reload,
                        ),
                        const SizedBox(height: 16),
                        HomeWeekRatingCard(
                          previous: snap.previousWeekRating,
                          current: snap.currentRating,
                          trend: snap.trend,
                          installmentSum: snap.installmentSum,
                          yesterdayIncome: snap.yesterdayIncome,
                        ),
                        const SizedBox(height: 16),
                        FilledButton.icon(
                          onPressed: _openNewSaleRequest,
                          icon: const Icon(Icons.add_shopping_cart_outlined),
                          label: const Text('طلب مبيع جديد'),
                        ),
                        const SizedBox(height: 10),
                        OutlinedButton.icon(
                          onPressed: () {
                            Navigator.of(context).push(
                              MaterialPageRoute(
                                builder: (_) => const TutorialVideosPage(),
                              ),
                            );
                          },
                          icon: const Icon(Icons.play_circle_outline),
                          label: const Text('مشاهدة الفيديو التعليمي'),
                        ),
                      ],
                    ],
                  ),
                ),
        ),
      ],
    );
  }
}
