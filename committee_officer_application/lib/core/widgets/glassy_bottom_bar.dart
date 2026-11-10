import 'dart:ui';

import 'package:flutter/material.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';

class GlassyBottomBarItem {
  const GlassyBottomBarItem({required this.icon, required this.label});
  final IconData icon;
  final String label;
}

class GlassyBottomBar extends StatelessWidget {
  const GlassyBottomBar({
    super.key,
    required this.items,
    required this.index,
    required this.onTap,
  }) : assert(items.length > 1),
       assert(index >= 0 && index < items.length);

  final List<GlassyBottomBarItem> items;
  final int index;
  final ValueChanged<int> onTap;

  @override
  Widget build(BuildContext context) {
    final duration = MediaQuery.disableAnimationsOf(context)
        ? Duration.zero
        : const Duration(milliseconds: 380);
    final rtl = Directionality.of(context) == TextDirection.rtl;
    return SizedBox(
      height: 100,
      child: LayoutBuilder(
        builder: (context, constraints) {
          final cell = constraints.maxWidth / items.length;
          final visualIndex = rtl ? items.length - 1 - index : index;
          return Stack(
            children: [
              Positioned.fill(
                top: 24,
                child: ClipRRect(
                  borderRadius: const BorderRadius.vertical(
                    top: Radius.circular(26),
                  ),
                  child: BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 18, sigmaY: 18),
                    child: DecoratedBox(
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: .88),
                        border: Border.all(
                          color: Colors.white.withValues(alpha: .7),
                        ),
                      ),
                    ),
                  ),
                ),
              ),
              AnimatedPositioned(
                duration: duration,
                curve: Curves.easeOutCubic,
                top: 2,
                left: cell * visualIndex + (cell - 58) / 2,
                width: 58,
                height: 58,
                child: IgnorePointer(
                  child: DecoratedBox(
                    key: const ValueKey('floating-tab-circle'),
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      color: AppColors.primary,
                      border: Border.all(color: Colors.white, width: 3),
                      boxShadow: [
                        BoxShadow(
                          color: AppColors.primary.withValues(alpha: .24),
                          blurRadius: 14,
                          offset: const Offset(0, 5),
                        ),
                      ],
                    ),
                    child: AnimatedSwitcher(
                      duration: duration,
                      child: Icon(
                        items[index].icon,
                        key: ValueKey(index),
                        color: AppColors.onPrimary,
                        size: 27,
                      ),
                    ),
                  ),
                ),
              ),
              Positioned.fill(
                child: Row(
                  children: [
                    for (var i = 0; i < items.length; i++)
                      Expanded(
                        child: Semantics(
                          selected: i == index,
                          button: true,
                          label: items[i].label,
                          child: Material(
                            color: Colors.transparent,
                            child: InkWell(
                              key: ValueKey('bottom-tab-$i'),
                              excludeFromSemantics: true,
                              borderRadius: BorderRadius.circular(24),
                              onTap: () => onTap(i),
                              child: ExcludeSemantics(
                                child: Column(
                                  children: [
                                    const SizedBox(height: 34),
                                    AnimatedOpacity(
                                      opacity: i == index ? 0 : 1,
                                      duration: duration,
                                      child: Icon(
                                        items[i].icon,
                                        color: AppColors.primary.withValues(
                                          alpha: .65,
                                        ),
                                        size: 25,
                                      ),
                                    ),
                                    const SizedBox(height: 9),
                                    Padding(
                                      padding: const EdgeInsets.symmetric(
                                        horizontal: 3,
                                      ),
                                      child: Text(
                                        items[i].label,
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                        style: TextStyle(
                                          fontSize: 11,
                                          fontWeight: i == index
                                              ? FontWeight.w800
                                              : FontWeight.w600,
                                          color: AppColors.primary,
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ),
                          ),
                        ),
                      ),
                  ],
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}
