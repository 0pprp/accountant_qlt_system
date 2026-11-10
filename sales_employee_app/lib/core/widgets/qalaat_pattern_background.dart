import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';

class QalaatPatternBackground extends StatelessWidget {
  const QalaatPatternBackground({super.key, required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [Color(0xFFE8F3F2), AppColors.background],
        ),
      ),
      child: CustomPaint(
        painter: _WavePainter(),
        child: child,
      ),
    );
  }
}

class _WavePainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = AppColors.primary.withValues(alpha: 0.05)
      ..style = PaintingStyle.fill;
    final path = Path()
      ..moveTo(0, size.height * 0.18)
      ..quadraticBezierTo(size.width * 0.25, size.height * 0.12, size.width * 0.5, size.height * 0.18)
      ..quadraticBezierTo(size.width * 0.75, size.height * 0.24, size.width, size.height * 0.16)
      ..lineTo(size.width, 0)
      ..lineTo(0, 0)
      ..close();
    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
