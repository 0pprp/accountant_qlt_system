import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/app_page_route.dart';
import 'package:committee_officer_application/core/widgets/brand_logo.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/features/field/presentation/field_followups_page.dart';
import 'package:committee_officer_application/features/home/presentation/committee_actions.dart';
import 'package:committee_officer_application/features/home/presentation/tutorial_videos_page.dart';
import 'package:committee_officer_application/features/legal/presentation/legal_followups_page.dart';
import 'package:committee_officer_application/features/sales/presentation/pending_sales_page.dart';
import 'package:committee_officer_application/features/sales/presentation/sales_requests_page.dart';
import 'package:committee_officer_application/features/stats/presentation/list_stats_page.dart';
import 'package:committee_officer_application/features/tasks/presentation/tasks_page.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

class AppDrawer extends StatelessWidget {
  const AppDrawer({
    super.key,
    required this.displayName,
    required this.onLogout,
    this.onShiftEnded,
  });

  final String displayName;
  final VoidCallback onLogout;
  final VoidCallback? onShiftEnded;

  void _open(BuildContext context, Widget page) {
    Navigator.pop(context);
    Navigator.of(context).push(appFadeRoute(page));
  }

  Future<void> _endShift(BuildContext context) async {
    Navigator.pop(context);
    final ok = await showConfirmDialog(
      context,
      title: 'إنهاء الدوام',
      message: 'هل تريد إنهاء دوام اليوم؟',
    );
    if (!ok) return;
    try {
      await Injection.shift.end();
      onShiftEnded?.call();
    } catch (e) {
      if (context.mounted) {
        await showAppMessage(context, title: 'فشل', message: '$e', color: AppColors.danger);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Drawer(
      backgroundColor: Colors.transparent,
      child: ClipRect(
        child: AnimatedWavesBackground(
          intensity: 0.7,
          child: SafeArea(
            child: Column(
              children: [
                Container(
                  width: double.infinity,
                  margin: const EdgeInsets.fromLTRB(12, 12, 12, 8),
                  padding: const EdgeInsets.fromLTRB(18, 18, 18, 16),
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(18),
                    gradient: const LinearGradient(
                      colors: [AppColors.primary, AppColors.primaryDark],
                      begin: Alignment.topRight,
                      end: Alignment.bottomLeft,
                    ),
                    boxShadow: [
                      BoxShadow(
                        color: AppColors.primary.withValues(alpha: 0.25),
                        blurRadius: 16,
                        offset: const Offset(0, 8),
                      ),
                    ],
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const BrandLogo(height: 48, tone: BrandLogoTone.white),
                      const SizedBox(height: 12),
                      Text(displayName, style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w700, fontSize: 16)),
                      const Text('مسؤول اللجنة — قلعة الضمان', style: TextStyle(color: Colors.white70, fontSize: 12)),
                    ],
                  ),
                ),
                Expanded(
                  child: ListView(
                    padding: const EdgeInsets.fromLTRB(10, 4, 10, 10),
                    children: [
                      _item(context, Icons.storefront_outlined, 'المبيعات / تصديق', () {
                        _open(context, const PendingSalesPage());
                      }),
                      _item(context, Icons.request_page_outlined, 'طلبات البيع', () {
                        _open(context, const SalesRequestsPage());
                      }),
                      _item(context, Icons.bar_chart_rounded, 'الإحصائيات', () {
                        _open(context, const ListStatsPage());
                      }),
                      _item(context, Icons.gavel_outlined, 'الزبائن القديمة', () {
                        _open(context, const LegalFollowupsPage());
                      }),
                      _item(context, Icons.hiking_rounded, 'ميدانية', () {
                        _open(context, const FieldFollowupsPage());
                      }),
                      _item(context, Icons.task_alt_rounded, 'المهام', () {
                        _open(context, const TasksPage());
                      }),
                      _item(context, Icons.ondemand_video_outlined, 'فيديو تعليمي', () {
                        _open(context, const TutorialVideosPage());
                      }),
                      _item(context, Icons.note_alt_outlined, 'ملاحظة للمدير', () {
                        Navigator.pop(context);
                        CommitteeActions.sendDirectorNote(context);
                      }),
                      _item(context, Icons.report_gmailerrorred_outlined, 'إرسال شكوى', () {
                        Navigator.pop(context);
                        CommitteeActions.sendComplaint(context);
                      }),
                      _item(context, Icons.schedule_rounded, 'إنهاء الدوام', () => _endShift(context)),
                    ],
                  ),
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.logout, color: AppColors.danger),
                  title: const Text('تسجيل الخروج', style: TextStyle(color: AppColors.danger, fontWeight: FontWeight.w700)),
                  onTap: () async {
                    Navigator.pop(context);
                    final ok = await showConfirmDialog(context, title: 'تسجيل الخروج', message: 'هل تريد الخروج؟');
                    if (!ok) return;
                    await Injection.auth.logout();
                    onLogout();
                  },
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _item(BuildContext context, IconData icon, String title, VoidCallback onTap) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Material(
        color: Colors.white.withValues(alpha: 0.82),
        borderRadius: BorderRadius.circular(14),
        child: InkWell(
          borderRadius: BorderRadius.circular(14),
          onTap: onTap,
          child: ListTile(
            leading: Icon(icon, color: AppColors.primary),
            title: Text(title, style: const TextStyle(fontWeight: FontWeight.w700)),
            trailing: Icon(Icons.chevron_left, color: AppColors.primary.withValues(alpha: 0.45)),
          ),
        ),
      ),
    );
  }
}
