import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/features/home/data/announcements_remote.dart';
import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';

class HomeAnnouncementsCarousel extends StatelessWidget {
  const HomeAnnouncementsCarousel({
    super.key,
    required this.state,
    required this.items,
    required this.onRetry,
  });

  final AnnouncementsLoadState state;
  final List<AnnouncementItem> items;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    switch (state) {
      case AnnouncementsLoadState.loading:
        return const Padding(
          padding: EdgeInsets.symmetric(vertical: 12),
          child: Center(child: SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2))),
        );
      case AnnouncementsLoadState.forbidden:
      case AnnouncementsLoadState.error:
        return Container(
          margin: const EdgeInsets.only(bottom: 12),
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: 0.9),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: AppColors.danger.withValues(alpha: 0.35)),
          ),
          child: Row(
            children: [
              Expanded(
                child: Text(
                  state == AnnouncementsLoadState.forbidden
                      ? 'لا صلاحية لعرض الإعلانات.'
                      : 'تعذر تحميل الإعلانات.',
                  style: const TextStyle(color: AppColors.danger, fontSize: 13),
                ),
              ),
              TextButton(onPressed: onRetry, child: const Text('إعادة المحاولة')),
            ],
          ),
        );
      case AnnouncementsLoadState.successEmpty:
        return const SizedBox.shrink();
      case AnnouncementsLoadState.successData:
        return SizedBox(
          height: 110,
          child: PageView.builder(
            itemCount: items.length,
            controller: PageController(viewportFraction: 0.92),
            itemBuilder: (context, i) {
              final a = items[i];
              return Padding(
                padding: const EdgeInsets.only(left: 6),
                child: Material(
                  color: Colors.white.withValues(alpha: 0.95),
                  borderRadius: BorderRadius.circular(14),
                  child: InkWell(
                    borderRadius: BorderRadius.circular(14),
                    onTap: a.linkUrl == null || a.linkUrl!.isEmpty
                        ? null
                        : () async {
                            final uri = Uri.tryParse(a.linkUrl!);
                            if (uri != null) {
                              await launchUrl(uri, mode: LaunchMode.externalApplication);
                            }
                          },
                    child: Padding(
                      padding: const EdgeInsets.all(12),
                      child: Row(
                        children: [
                          if (a.bannerUrl != null && a.bannerUrl!.isNotEmpty)
                            ClipRRect(
                              borderRadius: BorderRadius.circular(8),
                              child: Image.network(
                                a.bannerUrl!,
                                width: 72,
                                height: 72,
                                fit: BoxFit.cover,
                                errorBuilder: (_, __, ___) => const Icon(Icons.campaign_outlined),
                              ),
                            )
                          else
                            const Icon(Icons.campaign_outlined, color: AppColors.primary, size: 36),
                          const SizedBox(width: 10),
                          Expanded(
                            child: Text(
                              a.textBody.isEmpty ? 'إعلان' : a.textBody,
                              maxLines: 3,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13.5),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              );
            },
          ),
        );
    }
  }
}
