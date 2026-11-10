import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../sales/presentation/submitted_sales_requests_page.dart';
import '../tutorial_videos_page.dart';
import '../../../notes/presentation/send_manager_note_page.dart';

class HomeDrawer extends StatelessWidget {
  const HomeDrawer({
    super.key,
    required this.delegateName,
    required this.listName,
    required this.onRating,
    required this.onAccount,
    required this.onLogout,
    this.appVersion = '1.0.0',
  });

  final String delegateName;
  final String listName;
  final VoidCallback onRating;
  final VoidCallback onAccount;
  final VoidCallback onLogout;
  final String appVersion;

  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: SafeArea(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Container(
              padding: const EdgeInsets.all(20),
              color: AppColors.primary,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  SvgPicture.asset(
                    'assets/svg/logo_white.svg',
                    height: 36,
                    colorFilter: const ColorFilter.mode(
                      Colors.white,
                      BlendMode.srcIn,
                    ),
                  ),
                  const SizedBox(height: 16),
                  Text(
                    delegateName,
                    style: AppTextStyles.title.copyWith(color: Colors.white),
                  ),
                  Text(
                    listName,
                    style: AppTextStyles.muted.copyWith(
                      color: Colors.white.withValues(alpha: 0.85),
                    ),
                  ),
                ],
              ),
            ),
            Expanded(
              child: ListView(
                padding: EdgeInsets.zero,
                children: [
                  ListTile(
                    leading: const Icon(
                      Icons.star_outline,
                      color: AppColors.primary,
                    ),
                    title: Text('التقييم', style: AppTextStyles.body),
                    onTap: () {
                      Navigator.pop(context);
                      onRating();
                    },
                  ),
                  ListTile(
                    leading: const Icon(
                      Icons.person_outline,
                      color: AppColors.primary,
                    ),
                    title: Text('الحساب', style: AppTextStyles.body),
                    onTap: () {
                      Navigator.pop(context);
                      onAccount();
                    },
                  ),
                  ListTile(
                    leading: const Icon(
                      Icons.receipt_long_outlined,
                      color: AppColors.muted,
                    ),
                    title: Text('طلبات البيع', style: AppTextStyles.body),
                    onTap: () {
                      Navigator.pop(context);
                      Navigator.of(context).push(
                        MaterialPageRoute(
                          builder: (_) => const SubmittedSalesRequestsPage(),
                        ),
                      );
                    },
                  ),
                  ListTile(
                    leading: const Icon(
                      Icons.ondemand_video_outlined,
                      color: AppColors.primary,
                    ),
                    title: Text('فيديوهات تعليمية', style: AppTextStyles.body),
                    onTap: () {
                      Navigator.pop(context);
                      Navigator.of(context).push(
                        MaterialPageRoute(
                          builder: (_) => const TutorialVideosPage(),
                        ),
                      );
                    },
                  ),
                  ListTile(
                    leading: const Icon(
                      Icons.note_alt_outlined,
                      color: AppColors.primary,
                    ),
                    title: Text(
                      'ملاحظة إلى الإدارة',
                      style: AppTextStyles.body,
                    ),
                    subtitle: Text(
                      'صندوق ملاحظات التطبيقات',
                      style: AppTextStyles.muted,
                    ),
                    onTap: () {
                      Navigator.pop(context);
                      Navigator.of(context).push(
                        MaterialPageRoute(
                          builder: (_) => const SendManagerNotePage(),
                        ),
                      );
                    },
                  ),
                ],
              ),
            ),
            ListTile(
              leading: const Icon(Icons.logout, color: AppColors.danger),
              title: Text(
                'تسجيل الخروج',
                style: AppTextStyles.body.copyWith(color: AppColors.danger),
              ),
              onTap: () {
                Navigator.pop(context);
                onLogout();
              },
            ),
            Padding(
              padding: const EdgeInsets.only(bottom: 16),
              child: Text(
                'الإصدار $appVersion',
                textAlign: TextAlign.center,
                style: AppTextStyles.muted,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
