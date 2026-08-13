import 'dart:math';

import 'package:flutter/material.dart';

class DonutChartWidget extends StatelessWidget {
  final double mainSegmentPercentage;
  final double secondarySegmentPercentage;
  final Color mainColor;
  final Color secondaryColor;

  const DonutChartWidget({
    this.mainSegmentPercentage = 72.0, // Default to 72% (430k/600k)
    this.secondarySegmentPercentage = 8.0, // Default to 8% (50k/600k)
    this.mainColor = const Color(0xFF006560),
    this.secondaryColor = Colors.amber,
  });

  @override
  Widget build(BuildContext context) {
    return CustomPaint(
      size: Size(120, 120),
      painter: DonutChartPainter(
        mainSegmentPercentage: mainSegmentPercentage,
        secondarySegmentPercentage: secondarySegmentPercentage,
        mainColor: mainColor,
        secondaryColor: secondaryColor,
      ),
    );
  }
}

class DonutChartPainter extends CustomPainter {
  final double mainSegmentPercentage;
  final double secondarySegmentPercentage;
  final Color mainColor;
  final Color secondaryColor;

  DonutChartPainter({
    required this.mainSegmentPercentage,
    required this.secondarySegmentPercentage,
    required this.mainColor,
    required this.secondaryColor,
  });

  @override
  void paint(Canvas canvas, Size size) {
    final center = Offset(size.width / 2, size.height / 2);
    final radius = min(size.width, size.height) / 2;
    final strokeWidth = radius * 0.25;

    // Background circle
    final backgroundPaint =
        Paint()
          ..color = Colors.grey.shade100
          ..style = PaintingStyle.stroke
          ..strokeWidth = strokeWidth;

    canvas.drawCircle(center, radius - strokeWidth / 2, backgroundPaint);

    // Draw segments
    final rect = Rect.fromCircle(center: center, radius: radius - strokeWidth / 2);

    // Convert percentages to radians
    final mainSweepAngle = 2 * pi * (mainSegmentPercentage / 100);
    final secondarySweepAngle = 2 * pi * (secondarySegmentPercentage / 100);

    // Main segment
    final mainPaint =
        Paint()
          ..color = mainColor
          ..style = PaintingStyle.stroke
          ..strokeWidth = strokeWidth
          ..strokeCap = StrokeCap.round;

    // Define the start and sweep angles
    final mainStartAngle = -pi / 2; // Start from the top

    canvas.drawArc(rect, mainStartAngle, mainSweepAngle, false, mainPaint);

    // Secondary segment
    final secondaryPaint =
        Paint()
          ..color = secondaryColor
          ..style = PaintingStyle.stroke
          ..strokeWidth = strokeWidth
          ..strokeCap = StrokeCap.round;

    // Position secondary segment in the middle of the remaining space
    final remainingAngle = 2 * pi - mainSweepAngle;
    final secondaryStartAngle = mainStartAngle + mainSweepAngle + (remainingAngle - secondarySweepAngle) / 2;

    canvas.drawArc(rect, secondaryStartAngle, secondarySweepAngle, false, secondaryPaint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
