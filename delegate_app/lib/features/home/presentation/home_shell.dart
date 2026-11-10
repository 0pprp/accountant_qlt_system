import 'dart:io';

import 'package:curved_navigation_bar/curved_navigation_bar.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/update/app_update_gate.dart';
import '../../../core/widgets/app_dialog.dart';
import '../../../injection.dart';
import '../../auth/domain/usecases/logout_usecase.dart';
import '../../auth/presentation/login_page.dart';
import '../../customers/presentation/customers_page.dart';
import '../../payments/presentation/today_payments_page.dart';
import '../../ratings/presentation/ratings_overview_page.dart';
import 'account_page.dart';
import 'home_page.dart';
import 'widgets/home_drawer.dart';

class HomeShell extends StatefulWidget {
  const HomeShell({super.key});

  @override
  State<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends State<HomeShell> {
  static const _homeIndex = 1;

  final _scaffoldKey = GlobalKey<ScaffoldState>();
  late final List<Widget> _pages;

  int _index = _homeIndex;
  String _delegateName = 'مندوب';
  String _listName = 'قائمة التحصيل';

  @override
  void initState() {
    super.initState();
    _pages = [
      const CustomersPage(),
      HomePage(onOpenMenu: () => _scaffoldKey.currentState?.openDrawer()),
      const TodayPaymentsPage(),
    ];
    _loadSessionLabels();
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      if (!mounted) return;
      await AppUpdateGate.checkAndPrompt(context, Injection.dioClient);
    });
  }

  Future<void> _loadSessionLabels() async {
    final session = await Injection.authRepository.currentSession();
    if (!mounted || session == null) return;
    setState(() {
      _delegateName = session.delegateName?.trim().isNotEmpty == true
          ? session.delegateName!
          : 'مندوب #${session.delegateId}';
      _listName = session.cityName?.trim().isNotEmpty == true
          ? session.cityName!
          : 'قائمة التحصيل';
    });
  }

  void _goTo(int index) {
    if (_index == index) return;
    setState(() => _index = index);
  }

  Future<void> _logout({bool force = false}) async {
    try {
      await Injection.logoutUseCase(force: force);
      if (!mounted) return;
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute(builder: (_) => const LoginPage()),
        (_) => false,
      );
    } on LogoutBlockedException catch (e) {
      if (!mounted) return;
      final forceQuit = await AppDialog.show<bool>(
        context: context,
        title: 'تسديدات معلّقة',
        child: Text('$e', style: AppTextStyles.body, textAlign: TextAlign.center),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('إلغاء'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('خروج قسري'),
          ),
        ],
      );
      if (forceQuit == true) await _logout(force: true);
    }
  }

  void _openRating() {
    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => const RatingsOverviewPage()),
    );
  }

  void _openAccount() {
    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => const AccountPage()),
    );
  }

  Widget _navIcon(String name, {required bool selected}) {
    final asset = selected
        ? 'assets/svg/${name}_selected_icon.svg'
        : 'assets/svg/${name}_icon.svg';
    return Padding(
      padding: const EdgeInsets.all(4),
      child: SvgPicture.asset(
        asset,
        width: selected ? 28 : 24,
        height: selected ? 28 : 24,
        colorFilter: ColorFilter.mode(
          selected ? Colors.white : AppColors.muted,
          BlendMode.srcIn,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.paddingOf(context).bottom;

    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: const SystemUiOverlayStyle(
        statusBarColor: Colors.transparent,
        statusBarIconBrightness: Brightness.light,
        statusBarBrightness: Brightness.dark,
      ),
      child: Scaffold(
        key: _scaffoldKey,
        backgroundColor: AppColors.background,
        extendBody: true,
        drawer: HomeDrawer(
          delegateName: _delegateName,
          listName: _listName,
          onRating: _openRating,
          onAccount: _openAccount,
          onLogout: () => _logout(),
          appVersion: '1.0.0',
        ),
        // IndexedStack keeps tab state — no reload when switching.
        body: IndexedStack(index: _index, children: _pages),
        bottomNavigationBar: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            DecoratedBox(
              decoration: BoxDecoration(
                boxShadow: [
                  BoxShadow(
                    color: const Color(0xFFD3D1D8).withValues(alpha: 0.18),
                    blurRadius: 20,
                    offset: const Offset(0, -8),
                  ),
                ],
              ),
              child: CurvedNavigationBar(
                backgroundColor: Colors.transparent,
                color: AppColors.surface,
                buttonBackgroundColor: AppColors.primary,
                index: _index,
                height: 68,
                animationCurve: Curves.easeOutQuad,
                animationDuration: const Duration(milliseconds: 400),
                items: [
                  Column(
                    mainAxisAlignment: MainAxisAlignment.end,
                    children: [
                      _navIcon('clients', selected: _index == 0),
                      if (_index != 0)
                        Text(
                          'زبائن',
                          style: AppTextStyles.muted.copyWith(fontSize: 11),
                        ),
                    ],
                  ),
                  Column(
                    mainAxisAlignment: MainAxisAlignment.end,
                    children: [
                      _navIcon('home', selected: _index == 1),
                      if (_index != 1)
                        Text(
                          'الرئيسية',
                          style: AppTextStyles.muted.copyWith(fontSize: 11),
                        ),
                    ],
                  ),
                  Column(
                    mainAxisAlignment: MainAxisAlignment.end,
                    children: [
                      _navIcon('payments', selected: _index == 2),
                      if (_index != 2)
                        Text(
                          'تسديدات',
                          style: AppTextStyles.muted.copyWith(fontSize: 11),
                        ),
                    ],
                  ),
                ],
                onTap: _goTo,
              ),
            ),
            // Fill Android/iOS system nav so the curved bar is never clipped.
            if (!kIsWeb && (Platform.isAndroid || bottomInset > 0))
              ColoredBox(
                color: AppColors.surface,
                child: SizedBox(height: bottomInset),
              ),
          ],
        ),
      ),
    );
  }
}
