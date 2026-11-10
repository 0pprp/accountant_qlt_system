import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/animated_waves.dart';
import 'package:sales_employee_app/core/widgets/brand_logo.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/updates/data/update_models.dart';
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
      final force = update.forceUpdate || update.kind == MobileUpdateKind.mandatory;
      await showCenteredDialog(
        context: context,
        barrierDismissible: !force,
        child: PopScope(
          canPop: !force,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const BrandLogo(height: 56),
              const SizedBox(height: 12),
              Text(
                force ? 'يجب تحديث التطبيق للاستمرار' : 'يتوفر تحديث جديد',
                style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              Text(
                update.message ?? 'يرجى تحديث التطبيق للمتابعة',
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 16),
              if (update.storeUrl != null && update.storeUrl!.isNotEmpty)
                ElevatedButton(
                  onPressed: () => Injection.appVersion.openDownload(update.storeUrl!),
                  child: const Text('تحديث الآن'),
                ),
              if (!force) ...[
                const SizedBox(height: 8),
                TextButton(
                  onPressed: () => Navigator.pop(context),
                  child: const Text('لاحقاً'),
                ),
              ],
            ],
          ),
        ),
      );
      if (force) return;
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
                style: TextStyle(fontSize: 28, fontWeight: FontWeight.w700, color: AppColors.primary),
              ),
              SizedBox(height: 6),
              Text('موظف المبيعات', style: TextStyle(color: AppColors.textSecondary)),
              SizedBox(height: 28),
              CircularProgressIndicator(color: AppColors.primary),
            ],
          ),
        ),
      ),
    );
  }
}
