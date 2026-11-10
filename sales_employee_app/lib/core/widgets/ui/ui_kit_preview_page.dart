import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

import '../../theme/app_colors.dart';
import 'action_row.dart';
import 'data_field.dart';
import 'data_state.dart';
import 'entity_card.dart';
import 'status_badge.dart';

/// Debug/widget preview only — not a production navigation target.
class UiKitPreviewPage extends StatelessWidget {
  const UiKitPreviewPage({super.key});

  @override
  Widget build(BuildContext context) {
    assert(kDebugMode, 'UiKitPreviewPage is debug-only');
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(
          title: const Text('معاينة مكوّنات العرض'),
          backgroundColor: AppColors.primary,
          foregroundColor: AppColors.onPrimary,
        ),
        body: SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 48),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
            const Text(
              'الحاوية تمرّر البيانات؛ المكوّن لا يعرف endpoint أو صلاحية.',
              style: TextStyle(fontSize: 16, height: 1.6),
            ),
            const SizedBox(height: 16),
            MediaQuery(
              data: MediaQuery.of(context).copyWith(textScaler: const TextScaler.linear(1.3)),
              child: EntityCard(
                eyebrow: 'طلب #42',
                name: 'اسم طويل جداً للزبون للتحقق من الالتفاف دون قص',
                statusLabel: 'قيد المتابعة',
                statusTone: StatusBadgeTone.warning,
                identityExtra: const DataField(
                  label: 'الهاتف',
                  value: '07801234567',
                  isPhone: true,
                  requiredField: true,
                ),
                fields: const Column(
                  children: [
                    DataField(
                      label: 'العنوان',
                      value: 'بغداد / وصف طويل جداً يجب أن يظهر كاملاً على الشاشة',
                      requiredField: true,
                    ),
                    DataField(
                      label: 'ملاحظة',
                      value: 'سطر1\nسطر2\nسطر3',
                      requiredField: true,
                    ),
                  ],
                ),
                actions: ActionRow(
                  primaryLabel: 'إجراء أساسي',
                  secondary: [('ثانوي ظاهر', () {})],
                  onPrimary: () {},
                ),
              ),
            ),
            const SizedBox(height: 16),
            const DataStateView(kind: DataStateKind.loading, title: 'تحميل'),
            const SizedBox(height: 8),
            const DataStateView(kind: DataStateKind.empty, title: 'فارغ', message: 'ليست صفراً وهمياً'),
            const SizedBox(height: 8),
            DataStateView(
              kind: DataStateKind.error,
              title: 'فشل',
              message: 'حالة خطأ مستقلة',
              onRetry: () {},
            ),
            const SizedBox(height: 8),
            const DataStateView(
              kind: DataStateKind.forbidden,
              title: 'ممنوع',
              message: 'لا تُعرض البيانات ثم تُخفى بـCSS',
            ),
            ],
          ),
        ),
      ),
    );
  }
}
