import 'package:carousel_slider/carousel_slider.dart';
import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';

class HomeAnnouncement {
  const HomeAnnouncement({
    required this.title,
    this.body,
  });

  final String title;
  final String? body;
}

class HomeAnnouncementsCarousel extends StatefulWidget {
  const HomeAnnouncementsCarousel({super.key, required this.items});

  final List<HomeAnnouncement> items;

  @override
  State<HomeAnnouncementsCarousel> createState() => _HomeAnnouncementsCarouselState();
}

class _HomeAnnouncementsCarouselState extends State<HomeAnnouncementsCarousel> {
  int _index = 0;

  @override
  Widget build(BuildContext context) {
    if (widget.items.isEmpty) return const SizedBox.shrink();
    return Column(
      children: [
        CarouselSlider.builder(
          itemCount: widget.items.length,
          itemBuilder: (context, i, _) {
            final a = widget.items[i];
            return Container(
              width: double.infinity,
              margin: const EdgeInsets.symmetric(horizontal: 4),
              padding: const EdgeInsets.fromLTRB(18, 16, 18, 14),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(18),
                gradient: LinearGradient(
                  colors: [
                    AppColors.primary.withValues(alpha: 0.12),
                    AppColors.surfaceSoft,
                  ],
                ),
                border: Border.all(color: AppColors.surfaceMuted),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    a.title,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w700,
                      color: AppColors.primaryDark,
                    ),
                  ),
                  if (a.body != null) ...[
                    const SizedBox(height: 8),
                    Expanded(
                      child: Text(
                        a.body!,
                        maxLines: 3,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(color: AppColors.muted, height: 1.35),
                      ),
                    ),
                  ],
                ],
              ),
            );
          },
          options: CarouselOptions(
            height: 132,
            autoPlay: widget.items.length > 1,
            autoPlayInterval: const Duration(seconds: 5),
            enlargeCenterPage: true,
            viewportFraction: 0.94,
            onPageChanged: (i, _) => setState(() => _index = i),
          ),
        ),
        if (widget.items.length > 1) ...[
          const SizedBox(height: 10),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: List.generate(widget.items.length, (i) {
              final active = i == _index;
              return AnimatedContainer(
                duration: const Duration(milliseconds: 220),
                margin: const EdgeInsets.symmetric(horizontal: 3),
                width: active ? 18 : 6,
                height: 6,
                decoration: BoxDecoration(
                  color: active ? AppColors.primary : AppColors.surfaceMuted,
                  borderRadius: BorderRadius.circular(8),
                ),
              );
            }),
          ),
        ],
      ],
    );
  }
}
