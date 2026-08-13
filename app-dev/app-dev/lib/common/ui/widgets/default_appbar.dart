import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:vector_graphics/vector_graphics.dart';

class DefaultAppBar extends StatelessWidget implements PreferredSize {
  final String? title;
  final Widget? customTitle;
  final Widget? secondIcon;
  final bool isBackToRoot;
  final Widget? firstIcon;
  final bool showQrButton;
  final VoidCallback? onBackPressed;
  final bool showIcons;
  final bool showLines;
  final Color? backgroundColor;
  const DefaultAppBar({
    super.key,
    this.title,
    this.isBackToRoot = false,
    this.secondIcon,
    this.firstIcon,
    this.onBackPressed,
    this.customTitle,
    this.backgroundColor,
    this.showIcons = true,
    this.showQrButton = false,
    this.showLines = false,
  });

  @override
  Widget build(BuildContext context) {
    return Stack(
      clipBehavior: Clip.none,
      children: [
        Positioned.fill(child: ColoredBox(color: backgroundColor ?? AppColor.surface1)),
        if (showLines)
          Positioned(
            top: 0,
            left: 0,
            child: Animate(
              effects: [
                ScaleEffect(duration: 700.ms, begin: Offset(1.3, 1.3), end: Offset(1, 1), curve: Curves.easeOutQuad),
              ],
              child: VectorGraphic(
                loader: AssetBytesLoader('assets/svg/lines_top_left.svg'),
                colorFilter: ColorFilter.mode(AppColor.primary.withValues(alpha: 0.5), BlendMode.srcIn),
              ),
            ),
          ),
        Padding(
          padding: EdgeInsets.only(top: 16 + MediaQuery.paddingOf(context).top, right: 24, left: 24),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            // crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              if (showIcons)
                if (firstIcon == null)
                  IconButton(
                    onPressed: () => Navigator.pushNamed(context, RouteNames.notifications),
                    icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/notification_icon.svg')),
                  )
                else
                  firstIcon!
              else
                SizedBox(height: 40),
              if (customTitle != null) customTitle!,
              if (title == null && customTitle == null)
                VectorGraphic(loader: AssetBytesLoader('assets/svg/logo_green.svg')),
              if (title != null) Text(title!, style: AppTextStyle.headlineMedium),
              if (showIcons)
                if (secondIcon == null)
                  IconButton(
                    onPressed: () {
                      if (onBackPressed != null) {
                        onBackPressed!();
                      } else {
                        Navigator.pop(context);
                      }
                    },
                    icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/back_icon.svg')),
                  )
                else
                  secondIcon!
              else
                Container(height: 40),
            ],
          ),
        ),
      ],
    );
  }

  @override
  Widget get child => const SizedBox();

  @override
  // TODO: implement preferredSize
  Size get preferredSize => const Size.fromHeight(70);
}
