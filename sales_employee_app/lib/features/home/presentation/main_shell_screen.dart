import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/core/widgets/glassy_bottom_bar.dart';
import 'package:sales_employee_app/features/home/presentation/home_tab.dart';
import 'package:sales_employee_app/features/ratings/presentation/ratings_tab.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_requests_tab.dart';
import 'package:sales_employee_app/features/shift/presentation/shift_page.dart';
import 'package:sales_employee_app/injection.dart';

class MainShellScreen extends StatefulWidget {
  const MainShellScreen({super.key, required this.onLogout});

  final VoidCallback onLogout;

  @override
  State<MainShellScreen> createState() => _MainShellScreenState();
}

class _MainShellScreenState extends State<MainShellScreen> with WidgetsBindingObserver {
  /// Visual RTL: right=طلبات البيع(0), center=الرئيسية(1), left=تقييمات(2)
  static const _homeIndex = 1;

  late final PageController _pageController;
  final _isAnimating = ValueNotifier<bool>(false);

  int _index = _homeIndex;
  /// Bumped only when a tap is rejected so CurvedNavigationBar snaps back.
  int _navReset = 0;
  String _name = 'موظف المبيعات';
  bool _dutyActive = false;
  bool _ready = false;
  bool _forcingDuty = false;

  static const _items = [
    GlassyBottomBarItem(icon: Icons.receipt_long_rounded, label: 'طلبات البيع'),
    GlassyBottomBarItem(icon: Icons.home_rounded, label: 'الرئيسية'),
    GlassyBottomBarItem(icon: Icons.star_rate_rounded, label: 'تقييمات زبائني'),
  ];

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _pageController = PageController(initialPage: _homeIndex);
    _bootstrap();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _isAnimating.dispose();
    _pageController.dispose();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      _refreshDuty();
    }
  }

  Future<void> _bootstrap() async {
    final n = await Injection.sessionStore.getFullName();
    final duty = await Injection.dutySession.isActive();
    if (!mounted) return;
    setState(() {
      if (n != null && n.isNotEmpty) _name = n;
      _dutyActive = duty;
      _ready = true;
    });
    await _syncTracker(duty);
    if (!duty) await _forceDutyGate();
  }

  Future<void> _syncTracker(bool duty) async {
    if (duty) {
      await Injection.dutyTracker.start();
    } else {
      await Injection.dutyTracker.stop();
    }
  }

  Future<void> _refreshDuty() async {
    final duty = await Injection.dutySession.isActive();
    if (!mounted) return;
    final was = _dutyActive;
    setState(() => _dutyActive = duty);
    await _syncTracker(duty);
    if (!duty && was) {
      // Ended duty or crossed midnight → lock app until start again.
      await _forceDutyGate();
    } else if (!duty) {
      await _forceDutyGate();
    }
  }

  Future<void> _forceDutyGate() async {
    if (!mounted || _forcingDuty) return;
    if (await Injection.dutySession.isActive()) return;
    _forcingDuty = true;
    try {
      while (mounted && !await Injection.dutySession.isActive()) {
        if (!mounted) return;
        await Navigator.of(context).push(
          MaterialPageRoute(
            fullscreenDialog: true,
            builder: (_) => ShiftPage(
              mandatory: true,
              onDutyChanged: () async {
                await _refreshDuty();
              },
            ),
          ),
        );
        if (!mounted) return;
        await _refreshDuty();
        if (_dutyActive) break;
      }
    } finally {
      _forcingDuty = false;
    }
  }

  Future<void> _onTab(int i) async {
    if (i == _index) return;

    if (!_dutyActive) {
      setState(() => _navReset++);
      await showAppMessage(
        context,
        title: 'الدوام مطلوب',
        message: 'ابدأ الدوام أولاً — كل يوم جديد بعد منتصف الليل يتطلب بدء دوام جديد',
        icon: Icons.lock_clock,
        color: AppColors.warning,
      );
      if (!mounted) return;
      await _forceDutyGate();
      return;
    }

    _isAnimating.value = true;
    setState(() => _index = i);
    await _pageController.animateToPage(
      i,
      duration: const Duration(milliseconds: 500),
      curve: Curves.easeOut,
    );
    _isAnimating.value = false;
  }

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.paddingOf(context).bottom;
    if (!_ready) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    final pages = [
      // «طلبات البيع» = SalesRequest inbox — ليس OrdersTab (سجلات البيع في الدرج).
      const SalesRequestsTab(),
      HomeTab(
        displayName: _name,
        onLogout: widget.onLogout,
        dutyActive: _dutyActive,
        onDutyChanged: _refreshDuty,
      ),
      const RatingsTab(),
    ];

    return Scaffold(
      backgroundColor: AppColors.background,
      extendBody: true,
      body: PageView(
        controller: _pageController,
        physics: const NeverScrollableScrollPhysics(),
        onPageChanged: (value) {
          if (!_isAnimating.value && value != _index) {
            setState(() => _index = value);
          }
        },
        children: pages,
      ),
      bottomNavigationBar: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          GlassyBottomBar(
            key: ValueKey(_navReset),
            items: _items,
            index: _index,
            onTap: _onTab,
          ),
          if (!kIsWeb && (Platform.isAndroid || bottomInset > 0))
            ColoredBox(
              color: AppColors.surface,
              child: SizedBox(height: bottomInset),
            ),
        ],
      ),
    );
  }
}
