import 'dart:async';

import 'package:flutter/material.dart';

import '../../core/theme/app_theme.dart';
import '../../core/update/app_update_gate.dart';
import '../../injection.dart';
import '../orders/new_order_page.dart';
import '../orders/orders_page.dart';
import '../pins/pins_page.dart';
import '../profile/profile_page.dart';
import '../shift/shift_page.dart';

abstract final class ShellTabs {
  static const newOrder = 0;
  static const pins = 1;
  static const orders = 2;
  static const shift = 3;
  static const profile = 4;
}

class MainShell extends StatefulWidget {
  const MainShell({super.key, required this.onLogout});
  final Future<void> Function() onLogout;

  @override
  State<MainShell> createState() => _MainShellState();
}

class _MainShellState extends State<MainShell> with WidgetsBindingObserver {
  final ValueNotifier<int> _tab = ValueNotifier(ShellTabs.newOrder);

  static const _items = [
    (Icons.add_shopping_cart_rounded, 'طلب بيع'),
    (Icons.storefront_rounded, 'المحلات'),
    (Icons.receipt_long_rounded, 'الطلبات'),
    (Icons.timer_outlined, 'الدوام'),
    (Icons.person_rounded, 'حسابي'),
  ];

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) unawaited(AppUpdateGate.checkAndPrompt(context, Injection.client));
    });
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _tab.dispose();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      unawaited(Injection.store.refreshMe().then((_) {}, onError: (_) {}));
      unawaited(Injection.tracker.flush().then((_) {}, onError: (_) {}));
    }
  }

  void _go(int index) => _tab.value = index;

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return ValueListenableBuilder<int>(
      valueListenable: _tab,
      builder: (context, index, _) => Scaffold(
        body: SafeArea(
          bottom: false,
          child: IndexedStack(
            index: index,
            children: [
              NewOrderPage(onOpenOrders: () => _go(ShellTabs.orders)),
              PinsPage(tab: _tab, index: ShellTabs.pins),
              OrdersPage(tab: _tab, index: ShellTabs.orders),
              ShiftPage(tab: _tab, index: ShellTabs.shift),
              ProfilePage(onLogout: widget.onLogout),
            ],
          ),
        ),
        bottomNavigationBar: Container(
          padding: EdgeInsets.fromLTRB(10, 8, 10, 8 + bottom),
          decoration: BoxDecoration(
            color: AppColors.surface,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
            boxShadow: [
              BoxShadow(
                color: AppColors.shadowDark.withValues(alpha: 0.45),
                blurRadius: 16,
                offset: const Offset(0, -4),
              ),
            ],
          ),
          child: Row(
            children: [
              for (var i = 0; i < _items.length; i++)
                Expanded(
                  child: _TabButton(
                    icon: _items[i].$1,
                    label: _items[i].$2,
                    selected: i == index,
                    onTap: () => _go(i),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }
}

class _TabButton extends StatelessWidget {
  const _TabButton({
    required this.icon,
    required this.label,
    required this.selected,
    required this.onTap,
  });

  final IconData icon;
  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final color = selected ? AppColors.primary : AppColors.muted;
    return Semantics(
      selected: selected,
      button: true,
      label: label,
      child: GestureDetector(
        behavior: HitTestBehavior.opaque,
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 3),
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 180),
            padding: const EdgeInsets.symmetric(vertical: 7),
            decoration: selected
                ? BoxDecoration(
                    color: AppColors.primarySoft,
                    borderRadius: BorderRadius.circular(16),
                  )
                : const BoxDecoration(),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(icon, color: color, size: 24),
                const SizedBox(height: 2),
                Text(
                  label,
                  maxLines: 1,
                  overflow: TextOverflow.fade,
                  softWrap: false,
                  style: TextStyle(
                    color: color,
                    fontSize: 12,
                    fontWeight: selected ? FontWeight.w700 : FontWeight.w500,
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
