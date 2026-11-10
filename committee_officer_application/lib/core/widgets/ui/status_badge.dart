import 'package:flutter/material.dart';

enum StatusBadgeTone { neutral, warning, danger, success, info }

class StatusBadge extends StatelessWidget {
  const StatusBadge({
    super.key,
    required this.label,
    this.tone = StatusBadgeTone.neutral,
  });

  final String label;
  final StatusBadgeTone tone;

  @override
  Widget build(BuildContext context) {
    final colors = switch (tone) {
      StatusBadgeTone.warning => (const Color(0xFFFFF3D9), const Color(0xFF79531A)),
      StatusBadgeTone.danger => (const Color(0xFFFCEBEA), const Color(0xFF963D35)),
      StatusBadgeTone.success => (const Color(0xFFE5F6EA), const Color(0xFF1B5E20)),
      StatusBadgeTone.info => (const Color(0xFFE7F1FB), const Color(0xFF1565C0)),
      StatusBadgeTone.neutral => (const Color(0xFFE9F3F0), const Color(0xFF145C4C)),
    };
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
      decoration: BoxDecoration(
        color: colors.$1,
        borderRadius: BorderRadius.circular(9),
      ),
      child: Text(
        label,
        style: TextStyle(
          color: colors.$2,
          fontSize: 14,
          fontWeight: FontWeight.w600,
          height: 1.4,
        ),
      ),
    );
  }
}
