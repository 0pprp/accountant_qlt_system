import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import 'config/app_env.dart';
import 'core/network/api_errors.dart';
import 'core/theme/app_theme.dart';
import 'core/widgets/brand.dart';
import 'core/widgets/neu.dart';
import 'features/auth/login_page.dart';
import 'features/shell/main_shell.dart';
import 'injection.dart';

enum _Gate { splash, login, home, error }

class FieldPromoterApp extends StatefulWidget {
  const FieldPromoterApp({super.key});

  @override
  State<FieldPromoterApp> createState() => _FieldPromoterAppState();
}

class _FieldPromoterAppState extends State<FieldPromoterApp> {
  final _messengerKey = GlobalKey<ScaffoldMessengerState>();
  _Gate _gate = _Gate.splash;
  String _error = '';
  String? _loginNotice;

  @override
  void initState() {
    super.initState();
    Injection.client.onSessionExpired = _onSessionExpired;
    _boot();
  }

  Future<void> _onSessionExpired() async {
    if (_gate == _Gate.login) return;
    await Injection.tracker.stop(clearQueue: true);
    await Injection.session.clear();
    Injection.store.reset();
    if (!mounted) return;
    setState(() {
      _loginNotice = ApiErrors.sessionExpired;
      _gate = _Gate.login;
    });
  }

  Future<void> _boot() async {
    setState(() => _gate = _Gate.splash);
    try {
      if (!await Injection.session.hasSession()) {
        if (mounted) setState(() => _gate = _Gate.login);
        return;
      }
      await Injection.store.refreshMe();
      if (mounted) setState(() => _gate = _Gate.home);
    } catch (e) {
      final err = ApiErrors.from(e);
      if (!mounted) return;
      if (err.statusCode == 401 || err.statusCode == 403) {
        await Injection.session.clear();
        if (!mounted) return;
        setState(() {
          _loginNotice = err.message;
          _gate = _Gate.login;
        });
        return;
      }
      setState(() {
        _error = err.message;
        _gate = _Gate.error;
      });
    }
  }

  Future<void> _logout() async {
    await Injection.signOut();
    if (!mounted) return;
    setState(() {
      _loginNotice = null;
      _gate = _Gate.login;
    });
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: AppEnv.appTitle,
      debugShowCheckedModeBanner: false,
      scaffoldMessengerKey: _messengerKey,
      theme: AppTheme.light(),
      locale: const Locale('ar'),
      supportedLocales: const [Locale('ar')],
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      builder: (context, child) => Directionality(
        textDirection: TextDirection.rtl,
        child: child ?? const SizedBox.shrink(),
      ),
      home: switch (_gate) {
        _Gate.splash => const _Splash(),
        _Gate.login => LoginPage(
          notice: _loginNotice,
          onLoggedIn: () {
            _loginNotice = null;
            _boot();
          },
        ),
        _Gate.home => MainShell(onLogout: _logout),
        _Gate.error => _BootError(
          message: _error,
          onRetry: _boot,
          onLogin: () async {
            await Injection.signOut();
            if (mounted) setState(() => _gate = _Gate.login);
          },
        ),
      },
    );
  }
}

class _Splash extends StatelessWidget {
  const _Splash();

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            LogoTile(size: 96),
            SizedBox(height: 18),
            Text(
              AppEnv.appTitle,
              style: TextStyle(fontSize: 22, fontWeight: FontWeight.w700),
            ),
            Text('قلعة الضمان', style: TextStyle(color: AppColors.muted)),
            SizedBox(height: 24),
            CircularProgressIndicator(),
          ],
        ),
      ),
    );
  }
}

class _BootError extends StatelessWidget {
  const _BootError({required this.message, required this.onRetry, required this.onLogin});
  final String message;
  final VoidCallback onRetry;
  final VoidCallback onLogin;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.wifi_off_rounded, size: 56, color: AppColors.muted),
                const SizedBox(height: 14),
                Text(
                  message,
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontSize: 16),
                ),
                const SizedBox(height: 22),
                NeuButton(label: 'إعادة المحاولة', onPressed: onRetry),
                const SizedBox(height: 12),
                TextButton(
                  onPressed: onLogin,
                  child: const Text('العودة لتسجيل الدخول'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
