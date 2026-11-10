import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

/// أكشنات النظام (نفس بزنس المتابع) عبر AppArea committee — بدون تغيير تصميم الشاشات.
abstract final class CommitteeActions {
  static Future<void> sendDirectorNote(BuildContext context) async {
    final text = await showCenteredTextPrompt(
      context,
      title: 'ملاحظة إلى المدير',
      hint: 'اكتب الملاحظة…',
      maxLines: 5,
    );
    if (text == null || text.isEmpty) return;
    try {
      await Injection.committee.directorNote(text);
      if (!context.mounted) return;
      await showAppMessage(context, title: 'تم', message: 'أُرسلت الملاحظة إلى المدير.');
    } catch (e) {
      if (!context.mounted) return;
      await showAppMessage(
        context,
        title: 'فشل',
        message: DioClient.mapError(e),
        color: AppColors.danger,
      );
    }
  }

  static Future<void> sendComplaint(BuildContext context) async {
    final text = await showCenteredTextPrompt(
      context,
      title: 'إرسال شكوى',
      hint: 'اكتب نص الشكوى…',
      maxLines: 5,
    );
    if (text == null || text.isEmpty) return;
    try {
      await Injection.committee.complaint(text);
      if (!context.mounted) return;
      await showAppMessage(context, title: 'تم', message: 'أُرسلت الشكوى.');
    } catch (e) {
      if (!context.mounted) return;
      await showAppMessage(
        context,
        title: 'فشل',
        message: DioClient.mapError(e),
        color: AppColors.danger,
      );
    }
  }
}
