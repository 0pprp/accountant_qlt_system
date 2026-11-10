import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';

class AnimatedWavesBackground extends StatefulWidget {
  const AnimatedWavesBackground({
    super.key,
    required this.child,
    this.intensity = 1,
  });

  final Widget child;
  final double intensity;

  @override
  State<AnimatedWavesBackground> createState() => _AnimatedWavesBackgroundState();
}

class _AnimatedWavesBackgroundState extends State<AnimatedWavesBackground>
    with SingleTickerProviderStateMixin {
  late final AnimationController _ctrl;

  @override
  void initState() {
    super.initState();
    _ctrl = AnimationController(vsync: this, duration: const Duration(seconds: 8))
      ..repeat();
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _ctrl,
      builder: (context, child) {
        return DecoratedBox(
          decoration: const BoxDecoration(
            gradient: LinearGradient(
              begin: Alignment.topCenter,
              end: Alignment.bottomCenter,
              colors: [Color(0xFFE7F4F3), AppColors.background],
            ),
          ),
          child: CustomPaint(
            painter: _MultiWavePainter(
              t: _ctrl.value,
              intensity: widget.intensity,
            ),
            child: child,
          ),
        );
      },
      child: widget.child,
    );
  }
}

class _MultiWavePainter extends CustomPainter {
  _MultiWavePainter({required this.t, required this.intensity});

  final double t;
  final double intensity;

  @override
  void paint(Canvas canvas, Size size) {
    void wave({
      required double yFactor,
      required Color color,
      required double amp,
      required double phase,
      required double freq,
    }) {
      final path = Path()..moveTo(0, 0)..lineTo(0, size.height * yFactor);
      for (var x = 0.0; x <= size.width; x += 8) {
        final y = size.height * yFactor +
            math.sin((x / size.width * freq * math.pi * 2) + t * math.pi * 2 + phase) *
                amp *
                intensity;
        path.lineTo(x, y);
      }
      path
        ..lineTo(size.width, 0)
        ..close();
      canvas.drawPath(path, Paint()..color = color);
    }

    wave(
      yFactor: 0.14,
      color: AppColors.primary.withValues(alpha: 0.07),
      amp: 18,
      phase: 0,
      freq: 1.2,
    );
    wave(
      yFactor: 0.22,
      color: AppColors.accent.withValues(alpha: 0.06),
      amp: 14,
      phase: 1.1,
      freq: 1.6,
    );
    wave(
      yFactor: 0.78,
      color: AppColors.primary.withValues(alpha: 0.05),
      amp: 22,
      phase: 2.2,
      freq: 1.1,
    );
    wave(
      yFactor: 0.88,
      color: AppColors.secondary.withValues(alpha: 0.05),
      amp: 16,
      phase: 0.6,
      freq: 1.8,
    );
  }

  @override
  bool shouldRepaint(covariant _MultiWavePainter oldDelegate) =>
      oldDelegate.t != t || oldDelegate.intensity != intensity;
}
