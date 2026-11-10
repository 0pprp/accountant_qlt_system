import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/brand_logo.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/core/widgets/coming_soon_page.dart';
import 'package:sales_employee_app/features/catalog/presentation/price_board_page.dart';
import 'package:sales_employee_app/features/catalog/presentation/warehouse_page.dart';
import 'package:sales_employee_app/features/sales/presentation/orders_tab.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_nav.dart';
import 'package:sales_employee_app/features/sales/presentation/submitted_sales_requests_page.dart';
import 'package:sales_employee_app/features/shift/presentation/shift_page.dart';
import 'package:sales_employee_app/features/support/presentation/send_manager_note_page.dart';
import 'package:sales_employee_app/features/support/presentation/submit_complaint_page.dart';
import 'package:sales_employee_app/features/support/presentation/tutorial_videos_page.dart';
import 'package:sales_employee_app/injection.dart';

class HomeDrawer extends StatefulWidget {
  const HomeDrawer({
    super.key,
    required this.displayName,
    required this.onLogout,
    required this.onDutyChanged,
  });

  final String displayName;
  final VoidCallback onLogout;
  final VoidCallback onDutyChanged;

  @override
  State<HomeDrawer> createState() => _HomeDrawerState();
}

class _HomeDrawerState extends State<HomeDrawer> with SingleTickerProviderStateMixin {
  late final AnimationController _wave;

  @override
  void initState() {
    super.initState();
    _wave = AnimationController(vsync: this, duration: const Duration(seconds: 6))..repeat();
  }

  @override
  void dispose() {
    _wave.dispose();
    super.dispose();
  }

  void _openComingSoon(String title, {String? message}) {
    Navigator.pop(context);
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => ComingSoonPage(
          title: title,
          message: message ?? 'قريباً يُحدَّد مسار «$title».',
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Drawer(
      backgroundColor: const Color(0xFFF3FAF9),
      child: Column(
        children: [
          AnimatedBuilder(
            animation: _wave,
            builder: (context, _) {
              return SizedBox(
                width: double.infinity,
                height: 188,
                child: CustomPaint(
                  painter: _DrawerWavePainter(t: _wave.value),
                  child: SafeArea(
                    bottom: false,
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(20, 18, 20, 16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const BrandLogo(height: 52, tone: BrandLogoTone.white),
                          const Spacer(),
                          Text(
                            widget.displayName,
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 17,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                          const SizedBox(height: 2),
                          const Text(
                            'موظف المبيعات — قلعة الضمان',
                            style: TextStyle(color: Colors.white70, fontSize: 12.5),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              );
            },
          ),
          Expanded(
            child: ListView(
              padding: const EdgeInsets.fromLTRB(10, 10, 10, 8),
              children: [
                _item(
                  icon: Icons.schedule_rounded,
                  title: 'الدوام',
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => ShiftPage(onDutyChanged: widget.onDutyChanged),
                      ),
                    );
                  },
                ),
                _item(
                  icon: Icons.receipt_long_rounded,
                  title: 'طلبات البيع',
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        settings: const RouteSettings(name: '/sales-requests/inbox'),
                        builder: (_) => SalesNav.salesRequestsInbox(),
                      ),
                    );
                  },
                ),
                _item(
                  icon: Icons.shopping_bag_outlined,
                  title: 'سجلات البيع',
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        settings: const RouteSettings(name: '/orders'),
                        builder: (_) => const OrdersTab(),
                      ),
                    );
                  },
                ),
                _item(
                  icon: Icons.inventory_2_rounded,
                  title: 'المخزن',
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.of(context).push(
                      MaterialPageRoute(builder: (_) => const WarehousePage()),
                    );
                  },
                ),
                _item(
                  icon: Icons.price_change_rounded,
                  title: 'لوحة الأسعار',
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.of(context).push(
                      MaterialPageRoute(builder: (_) => const PriceBoardPage()),
                    );
                  },
                ),
                _item(
                  icon: Icons.archive_outlined,
                  title: 'الأرشيف',
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.of(context).push(
                      MaterialPageRoute(builder: (_) => const SubmittedSalesRequestsPage()),
                    );
                  },
                ),
                const SizedBox(height: 8),
                const Padding(
                  padding: EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  child: Text(
                    'المزيد',
                    style: TextStyle(
                      color: AppColors.muted,
                      fontWeight: FontWeight.w700,
                      fontSize: 12,
                    ),
                  ),
                ),
                _item(
                  icon: Icons.ondemand_video_rounded,
                  title: 'فيديوهات تعليمية',
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.of(context).push(
                      MaterialPageRoute(builder: (_) => const TutorialVideosPage()),
                    );
                  },
                ),
                _item(
                  icon: Icons.report_gmailerrorred_rounded,
                  title: 'شكوى',
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.of(context).push(
                      MaterialPageRoute(builder: (_) => const SubmitComplaintPage()),
                    );
                  },
                ),
                _item(
                  icon: Icons.sticky_note_2_outlined,
                  title: 'ملاحظة المدير',
                  onTap: () {
                    Navigator.pop(context);
                    Navigator.of(context).push(
                      MaterialPageRoute(builder: (_) => const SendManagerNotePage()),
                    );
                  },
                ),
              ],
            ),
          ),
          const Divider(height: 1),
          Padding(
            padding: const EdgeInsets.fromLTRB(10, 4, 10, 12),
            child: _item(
              icon: Icons.logout_rounded,
              title: 'تسجيل الخروج',
              danger: true,
              onTap: () async {
                Navigator.pop(context);
                final ok = await showConfirmDialog(
                  context,
                  title: 'تسجيل الخروج',
                  message: 'هل تريد الخروج من الحساب؟',
                );
                if (!ok) return;
                await Injection.authRepository.logout();
                widget.onLogout();
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _item({
    required IconData icon,
    required String title,
    required VoidCallback onTap,
    String? badge,
    bool danger = false,
  }) {
    final color = danger ? AppColors.danger : AppColors.primary;
    return Padding(
      padding: const EdgeInsets.only(bottom: 6),
      child: Material(
        color: Colors.white.withValues(alpha: 0.86),
        borderRadius: BorderRadius.circular(14),
        child: InkWell(
          borderRadius: BorderRadius.circular(14),
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 12),
            child: Row(
              children: [
                Container(
                  width: 36,
                  height: 36,
                  decoration: BoxDecoration(
                    color: color.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Icon(icon, color: color, size: 20),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    title,
                    style: TextStyle(
                      fontWeight: FontWeight.w600,
                      color: danger ? AppColors.danger : AppColors.textPrimary,
                    ),
                  ),
                ),
                if (badge != null)
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                    decoration: BoxDecoration(
                      color: AppColors.info.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(99),
                    ),
                    child: Text(
                      badge,
                      style: const TextStyle(fontSize: 10.5, color: AppColors.info, fontWeight: FontWeight.w700),
                    ),
                  ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _DrawerWavePainter extends CustomPainter {
  _DrawerWavePainter({required this.t});
  final double t;

  @override
  void paint(Canvas canvas, Size size) {
    final rect = Offset.zero & size;
    final bg = Paint()
      ..shader = const LinearGradient(
        begin: Alignment.topRight,
        end: Alignment.bottomLeft,
        colors: [AppColors.primary, AppColors.primaryDark, Color(0xFF003D39)],
      ).createShader(rect);
    canvas.drawRect(rect, bg);

    void wave(double amp, double yBase, Color color, double phase) {
      final path = Path()..moveTo(0, yBase);
      for (double x = 0; x <= size.width; x += 4) {
        final y = yBase +
            math.sin((x / size.width * math.pi * 2) + (t * math.pi * 2) + phase) * amp;
        path.lineTo(x, y);
      }
      path
        ..lineTo(size.width, size.height)
        ..lineTo(0, size.height)
        ..close();
      canvas.drawPath(path, Paint()..color = color);
    }

    wave(10, size.height * 0.55, Colors.white.withValues(alpha: 0.08), 0);
    wave(14, size.height * 0.68, Colors.white.withValues(alpha: 0.12), 1.2);
    wave(8, size.height * 0.82, Colors.white.withValues(alpha: 0.16), 2.1);
  }

  @override
  bool shouldRepaint(covariant _DrawerWavePainter oldDelegate) => oldDelegate.t != t;
}
