import 'dart:io';

import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/photo_source_sheet.dart';

class SalePhotoSlot extends StatelessWidget {
  const SalePhotoSlot({
    super.key,
    required this.label,
    required this.required,
    this.file,
    this.onChanged,
    this.preferCamera = true,
  });

  final String label;
  final bool required;
  final XFile? file;
  final ValueChanged<XFile?>? onChanged;
  final bool preferCamera;

  bool get _has => file != null;

  Future<void> _pick(BuildContext context) async {
    final f = await pickImageWithSourceSheet(context, preferCamera: preferCamera);
    if (f != null) onChanged?.call(f);
  }

  @override
  Widget build(BuildContext context) {
    final title = required ? '$label *' : '$label (اختياري)';
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(title, style: const TextStyle(fontWeight: FontWeight.w700)),
          const SizedBox(height: 8),
          if (_has) ...[
            ClipRRect(
              borderRadius: BorderRadius.circular(12),
              child: Image.file(
                File(file!.path),
                height: 140,
                fit: BoxFit.cover,
                errorBuilder: (_, _, _) => Container(
                  height: 80,
                  alignment: Alignment.center,
                  color: AppColors.surface,
                  child: Text(file!.name, textAlign: TextAlign.center),
                ),
              ),
            ),
            const SizedBox(height: 8),
          ],
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              OutlinedButton.icon(
                onPressed: onChanged == null ? null : () => _pick(context),
                icon: const Icon(Icons.photo_camera_outlined, size: 18),
                label: Text(_has ? 'إعادة اختيار' : 'كاميرا / معرض'),
              ),
              if (_has)
                TextButton.icon(
                  onPressed: onChanged == null ? null : () => onChanged!(null),
                  icon: const Icon(Icons.delete_outline, size: 18),
                  label: const Text('حذف'),
                ),
            ],
          ),
        ],
      ),
    );
  }
}
