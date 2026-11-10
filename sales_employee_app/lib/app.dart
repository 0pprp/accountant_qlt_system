import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:sales_employee_app/core/theme/app_theme.dart';
import 'package:sales_employee_app/features/auth/presentation/login_page.dart';
import 'package:sales_employee_app/features/home/presentation/main_shell_screen.dart';
import 'package:sales_employee_app/features/updates/presentation/splash_page.dart';

enum _AppGate { splash, login, home }

class SalesEmployeeApp extends StatefulWidget {
  const SalesEmployeeApp({super.key});

  @override
  State<SalesEmployeeApp> createState() => _SalesEmployeeAppState();
}

class _SalesEmployeeAppState extends State<SalesEmployeeApp> {
  _AppGate _gate = _AppGate.splash;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'موظف المبيعات',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light(),
      locale: const Locale('ar'),
      supportedLocales: const [Locale('ar'), Locale('en')],
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      builder: (context, child) {
        return Directionality(
          textDirection: TextDirection.rtl,
          child: child ?? const SizedBox.shrink(),
        );
      },
      home: switch (_gate) {
        _AppGate.splash => SplashPage(
            onReady: ({required bool loggedIn}) {
              setState(() => _gate = loggedIn ? _AppGate.home : _AppGate.login);
            },
          ),
        _AppGate.login => LoginPage(
            onLoggedIn: () => setState(() => _gate = _AppGate.home),
          ),
        _AppGate.home => MainShellScreen(
            onLogout: () => setState(() => _gate = _AppGate.login),
          ),
      },
    );
  }
}
