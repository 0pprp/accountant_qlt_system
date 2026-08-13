import 'dart:ui';

import 'package:flutter/material.dart';

Future<T?> showAnimatedDialog<T>({
  required BuildContext context,
  required Widget child,
  bool barrierDismissible = true,
  Color? barrierColor,
  String? barrierLabel,
  bool useSafeArea = true,
  bool useRootNavigator = true,
  RouteSettings? routeSettings,
  Offset? anchorPoint,
  TraversalEdgeBehavior? traversalEdgeBehavior,
}) {
  return showGeneralDialog(
    context: context,
    barrierDismissible: barrierDismissible,
    useRootNavigator: useRootNavigator,
    barrierColor: barrierColor ?? Color(0xffC6C6C6).withValues(alpha: 0.5),
    barrierLabel: 'Dialog',
    pageBuilder: (context, animation, secondaryAnimation) => child,
    transitionDuration: const Duration(milliseconds: 600),
    transitionBuilder:
        (context, animation, secondaryAnimation, child) => FadeTransition(
          opacity: animation,
          child: SlideTransition(
            position: Tween<Offset>(
              begin: const Offset(0, 0.2),
              end: const Offset(0, 0),
            ).chain(CurveTween(curve: ElasticOutCurve(0.7))).animate(animation),
            child: BackdropFilter(filter: ImageFilter.blur(sigmaX: 2, sigmaY: 2), child: child),
          ),
        ),
  );
}
