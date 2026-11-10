import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/brand_logo.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

class ShiftGatePage extends StatefulWidget {
  const ShiftGatePage({super.key, required this.onStarted});

  final VoidCallback onStarted;

  @override
  State<ShiftGatePage> createState() => _ShiftGatePageState();
}

class _ShiftGatePageState extends State<ShiftGatePage> {
  bool _busy = false;

  Future<void> _start() async {
    setState(() => _busy = true);
    try {
      await Injection.shift.start();
      if (!mounted) return;
      widget.onStarted();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(context, title: 'بدء الدوام', message: DioClient.mapError(e), color: AppColors.danger);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return PopScope(
      canPop: false,
      child: Scaffold(
        body: AnimatedWavesBackground(
          child: SafeArea(
            child: Padding(
              padding: EdgeInsets.fromLTRB(24, 20, 24, 20 + bottom),
              child: Column(
                children: [
                  const BrandLogo(height: 78),
                  const SizedBox(height: 16),
                  const Text(
                    'بدء الدوام إلزامي',
                    style: TextStyle(fontSize: 22, fontWeight: FontWeight.w700, color: AppColors.primary),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'لا يمكن فتح مساحة عمل مسؤول اللجنة بدون دوام نشط. سيبدأ تسجيل الموقع بعد البدء.',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: AppColors.textSecondary),
                  ),
                  const Spacer(),
                  Icon(Icons.schedule_rounded, size: 72, color: AppColors.primary.withValues(alpha: 0.85)),
                  const Spacer(),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: _busy ? null : _start,
                      child: _busy
                          ? const SizedBox(
                              width: 22,
                              height: 22,
                              child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                            )
                          : const Text('بدء الدوام'),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
