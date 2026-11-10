import 'dart:math' as math;

import 'package:flutter/material.dart';

import '../theme/app_colors.dart';

/// Soft teal wave decoration for cards — sits behind content, never covers text.
class BrandWavePainter extends CustomPainter {
  const BrandWavePainter({this.opacity = 0.14});

  final double opacity;

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..style = PaintingStyle.fill
      ..color = AppColors.primary.withValues(alpha: opacity);

    final path = Path()
      ..moveTo(0, size.height * 0.72)
      ..cubicTo(
        size.width * 0.22,
        size.height * 0.55,
        size.width * 0.38,
        size.height * 0.95,
        size.width * 0.55,
        size.height * 0.78,
      )
      ..cubicTo(
        size.width * 0.72,
        size.height * 0.62,
        size.width * 0.88,
        size.height * 0.92,
        size.width,
        size.height * 0.7,
      )
      ..lineTo(size.width, size.height)
      ..lineTo(0, size.height)
      ..close();
    canvas.drawPath(path, paint);

    final paint2 = Paint()
      ..style = PaintingStyle.fill
      ..color = AppColors.secondary.withValues(alpha: opacity * 0.7);
    final path2 = Path()
      ..moveTo(0, size.height * 0.88)
      ..quadraticBezierTo(
        size.width * 0.4,
        size.height * 0.7,
        size.width * 0.7,
        size.height * 0.9,
      )
      ..quadraticBezierTo(
        size.width * 0.88,
        size.height,
        size.width,
        size.height * 0.86,
      )
      ..lineTo(size.width, size.height)
      ..lineTo(0, size.height)
      ..close();
    canvas.drawPath(path2, paint2);

    // Top-right accent arc
    final arc = Paint()
      ..style = PaintingStyle.stroke
      ..strokeWidth = 18
      ..color = AppColors.primary.withValues(alpha: opacity * 0.55)
      ..strokeCap = StrokeCap.round;
    canvas.drawArc(
      Rect.fromCircle(
        center: Offset(size.width + 8, -10),
        radius: 52,
      ),
      math.pi * 0.55,
      math.pi * 0.7,
      false,
      arc,
    );
  }

  @override
  bool shouldRepaint(covariant BrandWavePainter oldDelegate) =>
      oldDelegate.opacity != opacity;
}
