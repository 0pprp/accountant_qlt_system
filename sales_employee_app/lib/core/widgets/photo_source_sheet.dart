import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';

/// يفتح ورقة: كاميرا أولاً ثم معرض الصور (مثل التطبيق القديم).
Future<XFile?> pickImageWithSourceSheet(
  BuildContext context, {
  bool preferCamera = true,
  int imageQuality = 85,
  double? maxWidth = 1600,
}) async {
  final source = await showModalBottomSheet<ImageSource>(
    context: context,
    showDragHandle: true,
    builder: (ctx) {
      return SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 8, 16, 20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'اختر مصدر الصورة',
                textAlign: TextAlign.center,
                style: TextStyle(fontWeight: FontWeight.w800, fontSize: 16),
              ),
              const SizedBox(height: 12),
              if (preferCamera) ...[
                FilledButton.icon(
                  onPressed: () => Navigator.pop(ctx, ImageSource.camera),
                  icon: const Icon(Icons.photo_camera_outlined),
                  label: const Text('فتح الكاميرا'),
                ),
                const SizedBox(height: 8),
                OutlinedButton.icon(
                  onPressed: () => Navigator.pop(ctx, ImageSource.gallery),
                  icon: const Icon(Icons.photo_library_outlined),
                  label: const Text('معرض الصور'),
                ),
              ] else ...[
                OutlinedButton.icon(
                  onPressed: () => Navigator.pop(ctx, ImageSource.gallery),
                  icon: const Icon(Icons.photo_library_outlined),
                  label: const Text('معرض الصور'),
                ),
                const SizedBox(height: 8),
                FilledButton.icon(
                  onPressed: () => Navigator.pop(ctx, ImageSource.camera),
                  icon: const Icon(Icons.photo_camera_outlined),
                  label: const Text('فتح الكاميرا'),
                ),
              ],
              const SizedBox(height: 4),
              TextButton(
                onPressed: () => Navigator.pop(ctx),
                child: const Text('إلغاء', style: TextStyle(color: AppColors.muted)),
              ),
            ],
          ),
        ),
      );
    },
  );
  if (source == null) return null;
  return ImagePicker().pickImage(
    source: source,
    imageQuality: imageQuality,
    maxWidth: maxWidth,
    preferredCameraDevice: CameraDevice.rear,
  );
}
