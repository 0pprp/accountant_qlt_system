import 'widgets/delegate_note_button.dart';

import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/app_page_route.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/core/widgets/ui/data_field.dart';
import 'package:committee_officer_application/core/widgets/ui/entity_card.dart';
import 'package:committee_officer_application/features/home/data/announcements_remote.dart';
import 'package:committee_officer_application/features/home/data/committee_remote.dart';
import 'package:committee_officer_application/features/home/presentation/committee_actions.dart';
import 'package:committee_officer_application/features/home/presentation/tutorial_videos_page.dart';
import 'package:committee_officer_application/features/home/presentation/widgets/home_announcements_carousel.dart';
import 'package:committee_officer_application/features/lists/presentation/list_detail_page.dart';
import 'package:committee_officer_application/features/tasks/presentation/tasks_page.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

class HomeTab extends StatefulWidget {
  const HomeTab({
    super.key,
    required this.displayName,
    required this.openDrawer,
    required this.onShiftEnded,
  });

  final String displayName;
  final VoidCallback openDrawer;
  final VoidCallback onShiftEnded;

  @override
  State<HomeTab> createState() => _HomeTabState();
}

class _HomeTabState extends State<HomeTab> {
  List<CommitteeListCard> _cards = const [];
  bool _loading = true;
  String? _error;
  bool _endingShift = false;
  AnnouncementsLoadState _annState = AnnouncementsLoadState.loading;
  List<AnnouncementItem> _annItems = const [];

  @override
  void initState() {
    super.initState();
    _load();
    _loadAnnouncements();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final cards = await Injection.committee.listCards();
      if (!mounted) return;
      setState(() {
        _cards = cards;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _error = DioClient.mapError(e);
      });
    }
  }

  Future<void> _loadAnnouncements() async {
    setState(() => _annState = AnnouncementsLoadState.loading);
    final res = await Injection.announcements.listFollower();
    if (!mounted) return;
    setState(() {
      _annState = res.state;
      _annItems = res.items;
    });
  }

  Future<void> _endShift() async {
    final ok = await showConfirmDialog(
      context,
      title: 'إنهاء الدوام',
      message: 'هل تريد إنهاء دوام اليوم؟ ستحتاج لبدء دوام جديد للعودة.',
    );
    if (!ok) return;
    setState(() => _endingShift = true);
    try {
      await Injection.shift.end();
      if (!mounted) return;
      widget.onShiftEnded();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'فشل',
        message: DioClient.mapError(e),
        color: AppColors.danger,
      );
    } finally {
      if (mounted) setState(() => _endingShift = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      body: AnimatedWavesBackground(
        child: SafeArea(
          bottom: false,
          child: RefreshIndicator(
            onRefresh: _load,
            child: ListView(
              padding: EdgeInsets.fromLTRB(16, 8, 16, 100 + bottom),
              children: [
                Row(
                  children: [
                    IconButton(
                      onPressed: widget.openDrawer,
                      icon: const Icon(
                        Icons.menu_rounded,
                        color: AppColors.primary,
                      ),
                    ),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            'تطبيق مسؤول اللجنة',
                            style: TextStyle(
                              fontWeight: FontWeight.w800,
                              fontSize: 17,
                            ),
                          ),
                          Text(
                            widget.displayName,
                            style: const TextStyle(
                              color: AppColors.textSecondary,
                              fontSize: 12.5,
                            ),
                          ),
                        ],
                      ),
                    ),
                    TextButton.icon(
                      onPressed: _endingShift ? null : _endShift,
                      icon: const Icon(Icons.logout_rounded, size: 18),
                      label: const Text('إنهاء دوام'),
                    ),
                  ],
                ),
                const SizedBox(height: 10),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    _chip(Icons.task_alt_rounded, 'مهام مسؤول اللجنة', () {
                      Navigator.of(context)
                          .push(appFadeRoute(const TasksPage()));
                    }),
                    _chip(Icons.note_alt_outlined, 'ملاحظة للمدير', () {
                      CommitteeActions.sendDirectorNote(context);
                    }),
                    _chip(
                      Icons.report_gmailerrorred_outlined,
                      'إرسال شكوى',
                      () {
                        CommitteeActions.sendComplaint(context);
                      },
                    ),
                    _chip(Icons.ondemand_video_outlined, 'فيديو تعليمي', () {
                      Navigator.of(context)
                          .push(appFadeRoute(const TutorialVideosPage()));
                    }),
                  ],
                ),
                const SizedBox(height: 12),
                HomeAnnouncementsCarousel(
                  state: _annState,
                  items: _annItems,
                  onRetry: _loadAnnouncements,
                ),
                const SizedBox(height: 8),
                Text(
                  _cards.length <= 1
                      ? 'القوائم المخصصة'
                      : 'القوائم المخصصة (${_cards.length})',
                  style: const TextStyle(
                    fontWeight: FontWeight.w700,
                    fontSize: 16,
                  ),
                ),
                const SizedBox(height: 8),
                if (_loading)
                  const Padding(
                    padding: EdgeInsets.all(40),
                    child: Center(child: CircularProgressIndicator()),
                  )
                else if (_error != null)
                  Padding(
                    padding: const EdgeInsets.all(24),
                    child: Text(
                      _error!,
                      textAlign: TextAlign.center,
                      style: const TextStyle(color: AppColors.danger),
                    ),
                  )
                else if (_cards.isEmpty)
                  const Padding(
                    padding: EdgeInsets.all(32),
                    child: Text(
                      'لا توجد قوائم مخصصة لك حاليًا',
                      textAlign: TextAlign.center,
                    ),
                  )
                else
                  ..._cards.asMap().entries.map((e) => _card(e.value, e.key)),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _chip(IconData icon, String label, VoidCallback onTap) {
    return ActionChip(
      avatar: Icon(icon, size: 18, color: AppColors.primary),
      label: Text(label),
      onPressed: onTap,
      backgroundColor: Colors.white.withValues(alpha: 0.9),
    );
  }

  Widget _card(CommitteeListCard c, int index) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 280 + (index * 40)),
      curve: Curves.easeOutCubic,
      builder: (context, t, child) => Opacity(
        opacity: t,
        child: Transform.translate(
          offset: Offset(0, (1 - t) * 12),
          child: child,
        ),
      ),
      child: Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: EntityCard(
          name: c.listName,
          eyebrow: 'قائمة #${c.listId}',
          statusLabel: c.phantomLabel,
          onTap: () {
            Navigator.of(context).push(
              appFadeRoute(
                ListDetailPage(listId: c.listId, listName: c.listName),
              ),
            );
          },
          fields: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              DataField(label: 'المندوب', value: c.mandobName, requiredField: true),
              DataField(
                label: 'زبائن نشطون',
                value: '${c.activeCustomers}',
                requiredField: true,
              ),
              DataField(
                label: 'درجة الشهر الحالي',
                value: '${c.currentMonthScore}',
                requiredField: true,
              ),
              DataField(
                label: 'درجة الشهر السابق',
                value: '${c.previousMonthScore}',
                requiredField: true,
              ),
            ],
          ),
          actions: DelegateNoteButton(
            listName: c.listName,
            save: (text) => Injection.committee.addDelegateNote(c.listId, text),
          ),
        ),
      ),
    );
  }
}
