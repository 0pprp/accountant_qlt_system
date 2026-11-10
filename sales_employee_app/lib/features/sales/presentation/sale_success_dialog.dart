import 'package:audioplayers/audioplayers.dart';
import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';

Future<void> showSaleSuccessDialog(BuildContext context, {required int orderId}) async {
  try {
    final player = AudioPlayer();
    await player.play(AssetSource('sounds/sale_success.wav'));
  } catch (_) {}

  if (!context.mounted) return;
  await showCenteredDialog(
    context: context,
    barrierDismissible: false,
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        const Icon(Icons.check_circle_rounded, color: AppColors.success, size: 64),
        const SizedBox(height: 12),
        const Text(
          'تم إتمام البيع',
          style: TextStyle(fontSize: 20, fontWeight: FontWeight.w700),
        ),
        const SizedBox(height: 8),
        Text(
          'رقم البيع: $orderId\nسيظهر في سجلات البيع المكتملة',
          textAlign: TextAlign.center,
          style: const TextStyle(color: AppColors.textSecondary),
        ),
        const SizedBox(height: 18),
        SizedBox(
          width: double.infinity,
          child: ElevatedButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('حسناً'),
          ),
        ),
      ],
    ),
  );
}
