import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/update/app_update_gate.dart';
import 'package:committee_officer_application/core/widgets/glassy_bottom_bar.dart';
import 'package:committee_officer_application/features/collection/presentation/collection_tab.dart';
import 'package:committee_officer_application/features/declining/presentation/declining_tab.dart';
import 'package:committee_officer_application/features/home/presentation/home_tab.dart';
import 'package:committee_officer_application/features/home/presentation/widgets/app_drawer.dart';
import 'package:committee_officer_application/features/payments/presentation/payments_tab.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

class MainShellScreen extends StatefulWidget {
  const MainShellScreen({
    super.key,
    required this.onLogout,
    required this.onShiftEnded,
  });

  final VoidCallback onLogout;
  final VoidCallback onShiftEnded;

  @override
  State<MainShellScreen> createState() => _MainShellScreenState();
}

class _MainShellScreenState extends State<MainShellScreen> {
  static const _homeIndex = 0;
  late final PageController _pageController;
  final _scaffoldKey = GlobalKey<ScaffoldState>();
  final _isAnimating = ValueNotifier(false);
  int _index = _homeIndex;
  int _transition = 0;
  String _name = 'مسؤول اللجنة';

  static const _items = [
    GlassyBottomBarItem(icon: Icons.home_rounded, label: 'الرئيسية'),
    GlassyBottomBarItem(icon: Icons.payments_outlined, label: 'الجباية'),
    GlassyBottomBarItem(icon: Icons.receipt_long_rounded, label: 'التسديدات'),
    GlassyBottomBarItem(icon: Icons.trending_down_rounded, label: 'المتدنية'),
  ];

  @override
  void initState() {
    super.initState();
    _pageController = PageController(initialPage: _homeIndex);
    _boot();
  }

  Future<void> _boot() async {
    await AppUpdateGate.checkAndPrompt(context, Injection.dio);
    if (!mounted) return;
    final n = await Injection.session.getFullName();
    if (!mounted) return;
    if (n != null && n.isNotEmpty) setState(() => _name = n);
  }

  @override
  void dispose() {
    _isAnimating.dispose();
    _pageController.dispose();
    super.dispose();
  }

  Future<void> _onTab(int i) async {
    if (i == _index || !_pageController.hasClients) return;
    final transition = ++_transition;
    _isAnimating.value = true;
    setState(() => _index = i);
    try {
      if (MediaQuery.disableAnimationsOf(context)) {
        _pageController.jumpToPage(i);
      } else {
        await _pageController.animateToPage(
          i,
          duration: const Duration(milliseconds: 380),
          curve: Curves.easeOutCubic,
        );
      }
    } finally {
      if (mounted && transition == _transition) _isAnimating.value = false;
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.paddingOf(context).bottom;
    final pages = [
      HomeTab(
        displayName: _name,
        openDrawer: () => _scaffoldKey.currentState?.openDrawer(),
        onShiftEnded: widget.onShiftEnded,
      ),
      const CollectionTab(),
      const PaymentsTab(),
      const DecliningTab(),
    ];

    return Scaffold(
      key: _scaffoldKey,
      backgroundColor: AppColors.background,
      extendBody: false,
      drawer: AppDrawer(
        displayName: _name,
        onLogout: widget.onLogout,
        onShiftEnded: widget.onShiftEnded,
      ),
      body: PageView(
        controller: _pageController,
        physics: const NeverScrollableScrollPhysics(),
        onPageChanged: (v) {
          if (!_isAnimating.value && v != _index) setState(() => _index = v);
        },
        children: pages,
      ),
      bottomNavigationBar: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          GlassyBottomBar(items: _items, index: _index, onTap: _onTab),
          if (bottomInset > 0)
            ColoredBox(
              color: AppColors.surface,
              child: SizedBox(height: bottomInset),
            ),
        ],
      ),
    );
  }
}
