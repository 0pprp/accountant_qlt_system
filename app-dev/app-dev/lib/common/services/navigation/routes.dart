import 'package:flutter/material.dart' show GlobalKey, MaterialPageRoute, ModalRoute, NavigatorState;

class Routes {
  static final navigationKey = GlobalKey<NavigatorState>();

  static Future<T?>? toReplacement<T extends Object?, TO extends Object?>(
    String routeName, {
    Object? args,
  }) =>
      navigationKey.currentState?.pushReplacementNamed<T, TO>(
        routeName,
        arguments: args,
      );

  static void popUntil(String routeName) => navigationKey.currentState?.popUntil(ModalRoute.withName(routeName));

  static Future<T?>? to<T extends Object?>(
    String routeName, {
    Object? args,
  }) =>
      navigationKey.currentState?.pushNamed<T>(
        routeName,
        arguments: args,
      );

  static Future<T?>? toRoute<T extends Object?>(
    MaterialPageRoute<T> routeName, {
    Object? args,
  }) =>
      navigationKey.currentState?.push<T>(routeName);

  static goBack<T extends Object?>([T? result]) => navigationKey.currentState?.pop<T>(result);
}
