import 'package:committee_officer_application/core/theme/app_theme.dart';
import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/brand_logo.dart';
import 'package:committee_officer_application/features/auth/presentation/login_page.dart';
import 'package:committee_officer_application/features/home/presentation/main_shell_screen.dart';
import 'package:committee_officer_application/features/shift/presentation/shift_gate_page.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

enum _Gate { splash, login, shift, home, error }

class CommitteeOfficerApp extends StatefulWidget {
  const CommitteeOfficerApp({super.key});

  @override
  State<CommitteeOfficerApp> createState() => _CommitteeOfficerAppState();
}

class _CommitteeOfficerAppState extends State<CommitteeOfficerApp> {
  _Gate _gate = _Gate.splash;
  String _bootError = '';

  @override
  void initState() {
    super.initState();
    _boot();
  }

  Future<void> _boot() async {
    if (mounted) setState(() => _gate = _Gate.splash);
    try {
      await Injection.init();
      if (!await Injection.session.hasSession()) {
        if (mounted) setState(() => _gate = _Gate.login);
        return;
      }
      await Injection.auth.verifyRole();
      final duty = await Injection.shift.currentActive();
      if (mounted) setState(() => _gate = duty ? _Gate.home : _Gate.shift);
    } catch (error) {
      if (mounted) {
        setState(() {
          _bootError = DioClient.mapError(error);
          _gate = _Gate.error;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'تطبيق مسؤول اللجنة',
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
        _Gate.splash => const _Splash(),
        _Gate.error => Scaffold(
          body: SafeArea(
            child: Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(_bootError, textAlign: TextAlign.center),
                    const SizedBox(height: 16),
                    FilledButton(
                      onPressed: _boot,
                      child: const Text('إعادة المحاولة'),
                    ),
                    TextButton(
                      onPressed: () async {
                        await Injection.session.clear();
                        if (mounted) setState(() => _gate = _Gate.login);
                      },
                      child: const Text('العودة لتسجيل الدخول'),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
        _Gate.login => LoginPage(onLoggedIn: _boot),
        _Gate.shift => ShiftGatePage(
          onStarted: () => setState(() => _gate = _Gate.home),
        ),
        _Gate.home => MainShellScreen(
          onLogout: () => setState(() => _gate = _Gate.login),
          onShiftEnded: () => setState(() => _gate = _Gate.shift),
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
      body: AnimatedWavesBackground(
        child: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              BrandLogo(height: 96),
              SizedBox(height: 16),
              Text(
                'تطبيق مسؤول اللجنة',
                style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
              ),
              SizedBox(height: 8),
              Text(
                'جاري التحقق من الجلسة والدوام...',
                style: TextStyle(color: Color(0xFF5A6B6A)),
              ),
              SizedBox(height: 20),
              CircularProgressIndicator(),
            ],
          ),
        ),
      ),
    );
  }
}
