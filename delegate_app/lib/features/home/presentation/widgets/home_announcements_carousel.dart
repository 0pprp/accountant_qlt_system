import 'package:carousel_slider/carousel_slider.dart';
import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../domain/entities/delegate_home_snapshot.dart';
import '../../domain/entities/home_announcement.dart';

class HomeAnnouncementsCarousel extends StatefulWidget {
  const HomeAnnouncementsCarousel({
    super.key,
    required this.items,
    required this.state,
    this.errorMessage,
    this.onRetry,
  });

  final List<HomeAnnouncement> items;
  final AnnouncementsLoadState state;
  final String? errorMessage;
  final Future<void> Function()? onRetry;

  @override
  State<HomeAnnouncementsCarousel> createState() =>
      _HomeAnnouncementsCarouselState();
}

class _HomeAnnouncementsCarouselState extends State<HomeAnnouncementsCarousel> {
  int _index = 0;
  bool _retrying = false;

  Future<void> _retry() async {
    final fn = widget.onRetry;
    if (fn == null || _retrying) return;
    setState(() => _retrying = true);
    try {
      await fn();
    } finally {
      if (mounted) setState(() => _retrying = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    switch (widget.state) {
      case AnnouncementsLoadState.loading:
        return const SizedBox(
          height: 80,
          child: Center(child: CircularProgressIndicator()),
        );
      case AnnouncementsLoadState.forbidden:
      case AnnouncementsLoadState.error:
        return _FailureCard(
          message: widget.errorMessage ??
              (widget.state == AnnouncementsLoadState.forbidden
                  ? 'لا صلاحية لعرض الإعلانات.'
                  : 'تعذر تحميل الإعلانات.'),
          retrying: _retrying,
          onRetry: widget.onRetry == null ? null : _retry,
        );
      case AnnouncementsLoadState.successEmpty:
        return const SizedBox.shrink();
      case AnnouncementsLoadState.successData:
        break;
    }

    final items = widget.items;
    if (items.isEmpty) return const SizedBox.shrink();

    const cardHeight = 148.0;

    return Column(
      children: [
        CarouselSlider.builder(
          itemCount: items.length,
          itemBuilder: (context, i, _) {
            final a = items[i];
            return Container(
              width: double.infinity,
              margin: const EdgeInsets.symmetric(horizontal: 4),
              padding: const EdgeInsets.fromLTRB(18, 16, 18, 14),
              decoration: BoxDecoration(
                color: AppColors.surfaceSoft,
                borderRadius: BorderRadius.circular(18),
                border: Border.all(color: AppColors.surfaceMuted),
                image: a.imageUrl != null && a.imageUrl!.isNotEmpty
                    ? DecorationImage(
                        image: NetworkImage(a.imageUrl!),
                        fit: BoxFit.cover,
                        colorFilter: ColorFilter.mode(
                          Colors.black.withValues(alpha: 0.35),
                          BlendMode.darken,
                        ),
                      )
                    : null,
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    a.title,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: AppTextStyles.title.copyWith(
                      fontSize: 17,
                      color: a.imageUrl != null
                          ? Colors.white
                          : AppColors.foreground,
                    ),
                  ),
                  if (a.body != null) ...[
                    const SizedBox(height: 8),
                    Expanded(
                      child: Text(
                        a.body!,
                        maxLines: 4,
                        overflow: TextOverflow.ellipsis,
                        style: AppTextStyles.body.copyWith(
                          color: a.imageUrl != null
                              ? Colors.white.withValues(alpha: 0.92)
                              : AppColors.muted,
                          height: 1.35,
                        ),
                      ),
                    ),
                  ],
                ],
              ),
            );
          },
          options: CarouselOptions(
            height: cardHeight,
            viewportFraction: 1,
            enableInfiniteScroll: items.length > 1,
            autoPlay: items.length > 1,
            autoPlayInterval: const Duration(seconds: 3),
            onPageChanged: (i, _) => setState(() => _index = i),
          ),
        ),
        if (items.length > 1) ...[
          const SizedBox(height: 8),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: List.generate(
              items.length,
              (i) => Container(
                width: 7,
                height: 7,
                margin: const EdgeInsets.symmetric(horizontal: 3),
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: i == _index ? AppColors.primary : AppColors.surfaceMuted,
                ),
              ),
            ),
          ),
        ],
      ],
    );
  }
}

class _FailureCard extends StatelessWidget {
  const _FailureCard({
    required this.message,
    required this.retrying,
    this.onRetry,
  });

  final String message;
  final bool retrying;
  final Future<void> Function()? onRetry;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surfaceSoft,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: AppColors.surfaceMuted),
      ),
      child: Column(
        children: [
          Text(message, textAlign: TextAlign.center, style: AppTextStyles.body),
          if (onRetry != null) ...[
            const SizedBox(height: 10),
            OutlinedButton(
              onPressed: retrying ? null : () => onRetry!(),
              child: Text(retrying ? 'جاري إعادة المحاولة…' : 'إعادة المحاولة'),
            ),
          ],
        ],
      ),
    );
  }
}
