import 'package:curved_navigation_bar/curved_navigation_bar.dart';
import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';

class GlassyBottomBarItem {
  const GlassyBottomBarItem({
    required this.icon,
    required this.label,
  });

  final IconData icon;
  final String label;
}

/// Team-app style curved bottom bar (same motion as app-dev / delegate).
class GlassyBottomBar extends StatelessWidget {
  const GlassyBottomBar({
    super.key,
    required this.items,
    required this.index,
    required this.onTap,
  });

  final List<GlassyBottomBarItem> items;
  final int index;
  final ValueChanged<int> onTap;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        boxShadow: [
          BoxShadow(
            color: const Color(0xFFD3D1D8).withValues(alpha: 0.15),
            blurRadius: 20,
            spreadRadius: 0,
            offset: const Offset(0, -8),
          ),
        ],
      ),
      child: CurvedNavigationBar(
        backgroundColor: Colors.transparent,
        color: AppColors.surface,
        buttonBackgroundColor: AppColors.primary,
        index: index,
        height: 72,
        animationCurve: Curves.easeOutQuad,
        animationDuration: const Duration(milliseconds: 400),
        items: [
          for (var i = 0; i < items.length; i++)
            Column(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                Padding(
                  padding: const EdgeInsets.all(4),
                  child: Icon(
                    items[i].icon,
                    size: i == index ? 30 : 24,
                    color: i == index ? AppColors.onPrimary : AppColors.muted,
                  ),
                ),
                if (i != index)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 2),
                    child: Text(
                      items[i].label,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        fontSize: 11,
                        fontWeight: FontWeight.w600,
                        color: AppColors.muted,
                      ),
                    ),
                  ),
              ],
            ),
        ],
        onTap: onTap,
      ),
    );
  }
}
