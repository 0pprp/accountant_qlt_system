import 'package:flutter/material.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';

Future<DateTime?> showCommitteeDatePicker(
  BuildContext context, {
  required DateTime initialDate,
  required DateTime lastDate,
  String title = 'اختيار التاريخ',
}) {
  var selected = DateUtils.dateOnly(initialDate);
  return showCenteredDialog<DateTime>(
    context: context,
    builder: (dialogContext) => Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          title,
          style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w800),
        ),
        SizedBox(
          width: 330,
          child: CalendarDatePicker(
            initialDate: selected,
            firstDate: DateTime(2000),
            lastDate: lastDate,
            onDateChanged: (date) => selected = date,
          ),
        ),
        Row(
          children: [
            Expanded(
              child: OutlinedButton(
                onPressed: () => Navigator.pop(dialogContext),
                child: const Text('إلغاء'),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: FilledButton(
                onPressed: () => Navigator.pop(dialogContext, selected),
                child: const Text('تطبيق'),
              ),
            ),
          ],
        ),
      ],
    ),
  );
}
