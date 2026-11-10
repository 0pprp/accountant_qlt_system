import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../theme/app_colors.dart';

class DataField extends StatelessWidget {
  const DataField({
    super.key,
    required this.label,
    this.value,
    this.isPhone = false,
    this.requiredField = false,
    this.emptyLabel = 'غير مسجل',
  });

  final String label;
  final String? value;
  final bool isPhone;
  final bool requiredField;
  final String emptyLabel;

  @override
  Widget build(BuildContext context) {
    final text = value?.trim() ?? '';
    if (!requiredField && text.isEmpty) return const SizedBox.shrink();
    final display = text.isEmpty ? emptyLabel : text;

    return Padding(
      padding: const EdgeInsets.only(top: 8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label,
            style: const TextStyle(
              color: AppColors.textSecondary,
              fontSize: 14,
              height: 1.4,
            ),
          ),
          const SizedBox(height: 4),
          if (isPhone && text.isNotEmpty)
            Wrap(
              spacing: 8,
              crossAxisAlignment: WrapCrossAlignment.center,
              children: [
                SelectableText(
                  display,
                  textDirection: TextDirection.ltr,
                  style: const TextStyle(
                    color: AppColors.primary,
                    fontSize: 18,
                    fontWeight: FontWeight.w600,
                    height: 1.5,
                  ),
                ),
                IconButton(
                  tooltip: 'اتصال',
                  constraints: const BoxConstraints(minWidth: 44, minHeight: 44),
                  onPressed: () => launchUrl(Uri(scheme: 'tel', path: text)),
                  icon: const Icon(Icons.call_rounded, color: AppColors.primary),
                ),
                IconButton(
                  tooltip: 'نسخ',
                  constraints: const BoxConstraints(minWidth: 44, minHeight: 44),
                  onPressed: () async {
                    await Clipboard.setData(ClipboardData(text: text));
                    if (context.mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('تم نسخ الرقم')),
                      );
                    }
                  },
                  icon: const Icon(Icons.copy_rounded, color: AppColors.muted),
                ),
              ],
            )
          else
            SelectableText(
              display,
              style: const TextStyle(
                color: AppColors.foreground,
                fontSize: 16,
                height: 1.6,
                fontWeight: FontWeight.w600,
              ),
            ),
        ],
      ),
    );
  }
}
