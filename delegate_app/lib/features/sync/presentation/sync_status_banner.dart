import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../injection.dart';

class SyncStatusBanner extends StatefulWidget {
  const SyncStatusBanner({super.key});

  @override
  State<SyncStatusBanner> createState() => _SyncStatusBannerState();
}

class _SyncStatusBannerState extends State<SyncStatusBanner> {
  int _pending = 0;
  bool _syncing = false;

  @override
  void initState() {
    super.initState();
    _reload();
  }

  Future<void> _reload() async {
    final n = await Injection.paymentsRepository.countPending();
    if (mounted) setState(() => _pending = n);
  }

  Future<void> _send() async {
    setState(() => _syncing = true);
    try {
      final n = await Injection.paymentSyncService.syncNow();
      if (!mounted) return;
      final detail = Injection.paymentsRepository.lastSyncError ??
          Injection.paymentSyncService.lastError;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            n > 0
                ? 'تم إرسال $n تسديد'
                : (detail != null && detail.isNotEmpty
                    ? detail
                    : 'تعذر الإرسال — افتح التسديدات لمعرفة السبب'),
          ),
        ),
      );
      await _reload();
    } finally {
      if (mounted) setState(() => _syncing = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_pending <= 0) {
      return const SizedBox.shrink();
    }
    return Material(
      color: AppColors.card,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
        child: Row(
          children: [
            const Icon(Icons.cloud_upload_outlined, color: AppColors.primary),
            const SizedBox(width: 8),
            Expanded(
              child: Text(
                'تسديدات معلّقة: $_pending',
                style: AppTextStyles.body,
              ),
            ),
            TextButton(
              onPressed: _syncing ? null : _send,
              child: _syncing
                  ? const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: AppColors.primary,
                      ),
                    )
                  : Text(
                      'إرسال الآن',
                      style: AppTextStyles.body.copyWith(
                        color: AppColors.primary,
                      ),
                    ),
            ),
          ],
        ),
      ),
    );
  }
}
