import 'package:flutter/material.dart';

import 'core/theme/app_theme.dart';
import 'core/widgets/loading_view.dart';
import 'features/auth/presentation/login_page.dart';
import 'features/home/presentation/home_shell.dart';
import 'injection.dart';

class DelegateApp extends StatelessWidget {
  const DelegateApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'مندوب التحصيل',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      locale: const Locale('ar'),
      builder: (context, child) {
        return Directionality(
          textDirection: TextDirection.rtl,
          child: child ?? const SizedBox.shrink(),
        );
      },
      home: FutureBuilder(
        future: Injection.authRepository.currentSession(),
        builder: (context, snap) {
          if (snap.connectionState != ConnectionState.done) {
            return const Scaffold(body: LoadingView());
          }
          if (snap.data != null) return const HomeShell();
          return const LoginPage();
        },
      ),
    );
  }
}
