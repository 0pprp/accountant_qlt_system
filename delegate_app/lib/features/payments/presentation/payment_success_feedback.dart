import 'dart:async';

import 'package:audioplayers/audioplayers.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';

/// Center flash + electronic-payment ding after a successful local save.
class PaymentSuccessFeedback {
  PaymentSuccessFeedback._();

  static bool _playing = false;
  static final AudioPlayer _player = AudioPlayer();

  static Future<void> playLocalSaveSuccess(BuildContext context) async {
    if (_playing) return;
    _playing = true;
    try {
      unawaited(_playDing());
      try {
        await HapticFeedback.mediumImpact();
      } catch (_) {}
      if (!context.mounted) return;
      await _showCenterFlash(context);
    } finally {
      _playing = false;
    }
  }

  static Future<void> _playDing() async {
    try {
      await _player.stop();
      await _player.play(AssetSource('sounds/payment_ding.wav'));
    } catch (_) {
      try {
        await SystemSound.play(SystemSoundType.click);
      } catch (_) {}
    }
  }

  static Future<void> _showCenterFlash(BuildContext context) async {
    final overlay = Overlay.maybeOf(context, rootOverlay: true);
    if (overlay == null) return;

    late OverlayEntry entry;
    entry = OverlayEntry(
      builder: (ctx) => IgnorePointer(
        child: Material(
          color: Colors.black.withValues(alpha: 0.35),
          child: Center(
            child: TweenAnimationBuilder<double>(
              tween: Tween(begin: 0.86, end: 1),
              duration: const Duration(milliseconds: 160),
              curve: Curves.easeOutBack,
              builder: (context, scale, child) => Transform.scale(
                scale: scale,
                child: child,
              ),
              child: Container(
                width: 260,
                padding:
                    const EdgeInsets.symmetric(horizontal: 24, vertical: 26),
                decoration: BoxDecoration(
                  color: AppColors.surface,
                  borderRadius: BorderRadius.circular(22),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.22),
                      blurRadius: 24,
                      offset: const Offset(0, 10),
                    ),
                  ],
                ),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Container(
                      width: 64,
                      height: 64,
                      decoration: BoxDecoration(
                        color: AppColors.primary.withValues(alpha: 0.12),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(
                        Icons.check_circle_rounded,
                        color: AppColors.primary,
                        size: 48,
                      ),
                    ),
                    const SizedBox(height: 14),
                    Text(
                      'تم التسديد بنجاح',
                      textAlign: TextAlign.center,
                      style: AppTextStyles.title.copyWith(
                        color: AppColors.primary,
                        fontSize: 20,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      'حُفظ محلياً وسيُرسل عند توفر الإنترنت',
                      textAlign: TextAlign.center,
                      style: AppTextStyles.muted.copyWith(fontSize: 13),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );

    overlay.insert(entry);
    await Future<void>.delayed(const Duration(milliseconds: 1400));
    entry.remove();
  }
}
