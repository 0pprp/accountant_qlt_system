import 'package:flutter/material.dart';
import 'package:qalaat_in_app_update/qalaat_in_app_update.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/animated_waves.dart';
import 'package:sales_employee_app/core/widgets/brand_logo.dart';
import 'package:sales_employee_app/injection.dart';

class SplashPage extends StatefulWidget {
  const SplashPage({
    super.key,
    required this.onReady,
  });

  final void Function({required bool loggedIn}) onReady;

  @override
  State<SplashPage> createState() => _SplashPageState();
}

class _SplashPageState extends State<SplashPage> {
  @override
  void initState() {
    super.initState();
    _boot();
  }

  Future<void> _boot() async {
    await Future<void>.delayed(const Duration(milliseconds: 900));
    final update = await Injection.appVersion.check();
    if (update != null && mounted) {
      await ForceUpdateScreen.present(
        context,
        info: update,
        primaryColor: AppColors.primary,
        foregroundColor: AppColors.foreground,
        backgroundColor: AppColors.background,
        surfaceColor: AppColors.surface,
        logo: const BrandLogo(height: 64),
        supportLine: (update.releaseNotes?.isNotEmpty ?? false)
            ? update.releaseNotes
            : 'تحسينات استقرار وأداء لإكمال طلبات البيع بسلاسة.',
      );
      if (update.mandatory) return;
    }
    final loggedIn = await Injection.authRepository.hasSession();
    if (!mounted) return;
    widget.onReady(loggedIn: loggedIn);
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: AnimatedWavesBackground(
        intensity: 1.2,
        child: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              BrandLogo(height: 110),
              SizedBox(height: 16),
              Text(
                'قلعة الضمان',
                style: TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.w700,
                  color: AppColors.primary,
                ),
              ),
              SizedBox(height: 6),
              Text(
                'موظف المبيعات',
                style: TextStyle(color: AppColors.textSecondary),
              ),
              SizedBox(height: 28),
              CircularProgressIndicator(color: AppColors.primary),
            ],
          ),
        ),
      ),
    );
  }
}
