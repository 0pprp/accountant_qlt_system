import 'package:carousel_slider/carousel_slider.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_cached_network_image.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/home/presentation/home/bloc/home_bloc.dart';
import 'package:team/injections.dart';
import 'package:team/order/domain/order_list/order_list.dart';
import 'package:vector_graphics/vector_graphics.dart';

class HomeUserPerformance extends StatelessWidget {
  const HomeUserPerformance({super.key});

  @override
  Widget build(BuildContext context) {
    final bloc = context.read<HomeBloc>();
    return SizedBox(
      // Motaba's card carries a lot more info (list, mandoob name, phone),
      // so it needs a taller box. Keep home_page.dart's report-container
      // top margin in sync with this value if you change it again.
      height: bloc.isMotaba ? 372 : 222,
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 24),
            child: DefaultBuilder<HomeBloc>(
              buildWhen: (previous, state) => state.event is GetUserEvent,
              builder: (context, state) {
                return Padding(
                  padding: const EdgeInsets.only(bottom: 16),
                  child: Row(
                    children: [
                      ClipRRect(
                        borderRadius: BorderRadius.circular(48),
                        child: DefaultCachedNetworkImage(
                          imageUrl:
                              bloc.user?.profilePicture == null
                                  ? null
                                  : Injections.baseUrl + bloc.user!.profilePicture!.relativePath,
                          height: 48,
                          width: 48,
                          isAvatar: true,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              bloc.user?.fullName ?? '',
                              style: AppTextStyle.headlineMedium,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            Text(
                              bloc.user?.branches.firstOrNull?.name ?? '',
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: AppTextStyle.bodyMedium.withColor(AppColor.text2),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                );
              },
            ),
          ),
          // Motaba is connected to many order lists, so they get a swipeable
          // carousel of cards (one per list). Mandoob keeps the single
          // static card, unchanged from before.
          Expanded(
            child:
                bloc.isMotaba
                    ? _MotabaOrderListCarousel(bloc: bloc)
                    : Padding(
                      padding: EdgeInsets.symmetric(horizontal: 24),
                      child: _OrderListCard(orderList: bloc.mandoobOrderList!),
                    ),
          ),
        ],
      ),
    );
  }
}

/// Carousel of order-list summary cards for the "motaba" role. Swiping (or
/// using the indicator) changes [HomeBloc.currentOrderListIndex], which also
/// drives the "تقرير القائمة" section lower on the home page.
class _MotabaOrderListCarousel extends StatefulWidget {
  final HomeBloc bloc;
  const _MotabaOrderListCarousel({required this.bloc});

  @override
  State<_MotabaOrderListCarousel> createState() => _MotabaOrderListCarouselState();
}

class _MotabaOrderListCarouselState extends State<_MotabaOrderListCarousel> {
  final CarouselSliderController _controller = CarouselSliderController();

  @override
  Widget build(BuildContext context) {
    final orderLists = widget.bloc.orderLists ?? const <OrderList>[];
    if (orderLists.isEmpty) return const SizedBox.shrink();

    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        CarouselSlider.builder(
          carouselController: _controller,
          itemCount: orderLists.length,
          itemBuilder: (context, index, realIndex) {
            return Padding(
              padding: EdgeInsetsGeometry.only(left: 16),
              child: _MotabaOrderListCard(orderList: orderLists[index]),
            );
          },
          options: CarouselOptions(
            // Fixed (not double.infinity) so it matches _MotabaOrderListCard's
            // own height exactly and we don't depend on however much room
            // the outer Expanded happens to hand back.
            height: _MotabaOrderListCard.cardHeight,
            viewportFraction: 0.85,
            enlargeCenterPage: false,
            enableInfiniteScroll: false,
            padEnds: true,
            onPageChanged: (index, reason) {
              widget.bloc.add(ChangeOrderListEvent(index));
            },
          ),
        ),
        if (orderLists.length > 1) ...[
          const SizedBox(height: 10),
          DefaultBuilder<HomeBloc>(
            buildWhen: (previous, state) => state.event is ChangeOrderListEvent,
            builder: (context, state) {
              return _CarouselIndicator(length: orderLists.length, currentIndex: widget.bloc.currentOrderListIndex);
            },
          ),
        ],
      ],
    );
  }
}

class _CarouselIndicator extends StatelessWidget {
  final int length;
  final int currentIndex;
  const _CarouselIndicator({required this.length, required this.currentIndex});

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: List.generate(length, (index) {
        final isActive = index == currentIndex;
        return AnimatedContainer(
          duration: const Duration(milliseconds: 250),
          curve: Curves.easeOut,
          margin: const EdgeInsets.symmetric(horizontal: 3),
          height: 6,
          width: isActive ? 18 : 6,
          decoration: BoxDecoration(
            color: isActive ? AppColor.primary : AppColor.surface3,
            borderRadius: BorderRadius.circular(3),
          ),
        );
      }),
    );
  }
}

/// Motaba-specific card: smaller progress ring up top next to the customer
/// stats, then the order list name, the mandoob's full name, and their
/// phone number (tap to copy) as pill rows underneath - per the reference
/// design. Kept separate from [_OrderListCard] so the mandoob's existing
/// single-list card is completely untouched.
class _MotabaOrderListCard extends StatelessWidget {
  final OrderList orderList;
  const _MotabaOrderListCard({required this.orderList});

  /// Also used as the CarouselOptions height, so keep them in sync.
  static const double cardHeight = 208;
  static const double _progressSize = 75;

  void _copyPhoneNumber(BuildContext context, String phoneNumber) {
    if (phoneNumber.isEmpty) return;
    Clipboard.setData(ClipboardData(text: phoneNumber));
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(const SnackBar(content: Text('تم نسخ رقم الهاتف'), duration: Duration(seconds: 1)));
  }

  @override
  Widget build(BuildContext context) {
    final mandoobFullName = orderList.mandobFullName;
    final phoneNumber = orderList.mandobPhoneNumber ?? '';

    return Container(
      height: cardHeight,
      decoration: BoxDecoration(
        color: AppColor.surface2,
        border: Border.all(color: AppColor.stroke, width: 2),
        borderRadius: BorderRadius.circular(24),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Expanded(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _StatRow(
                  dotColor: AppColor.surface4,
                  dotBorderColor: AppColor.text4,
                  label: 'العملاء',
                  value: orderList.totalOrderCount.toString(),
                ),
                const SizedBox(height: 6),
                _StatRow(
                  dotColor: AppColor.primary,
                  dotBorderColor: AppColor.surface1,
                  label: 'العملاء المسددين',
                  value: orderList.todayCollectedOrderCount.toString(),
                ),
                const SizedBox(height: 20),
                Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  spacing: 8,
                  children: [
                    _InfoPill(iconAsset: 'assets/svg/order_lists_small_icon.svg', text: orderList.name),
                    _InfoPill(iconAsset: 'assets/svg/user_small_icon.svg', text: mandoobFullName),
                    GestureDetector(
                      onTap: () => _copyPhoneNumber(context, phoneNumber),
                      child: _InfoPill(
                        iconAsset: 'assets/svg/call_small_icon.svg',
                        text: phoneNumber,
                        isNumber: true,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          SizedBox(width: 20),
          SizedBox(
            width: _progressSize,
            height: _progressSize,
            child: CircularProgressIndicator(
              value:
                  orderList.totalOrderCount == 0 ? 0 : orderList.todayCollectedOrderCount / orderList.totalOrderCount,
              backgroundColor: AppColor.surface4,
              strokeWidth: 13,
              strokeCap: StrokeCap.round,
            ),
          ),
        ],
      ),
    );
  }
}

/// A rounded, single-line "icon + text" row, used for the order list name,
/// mandoob name, and phone number rows on [_MotabaOrderListCard].
class _InfoPill extends StatelessWidget {
  final String iconAsset;
  final String text;
  final bool isNumber;
  const _InfoPill({required this.iconAsset, required this.text, this.isNumber = false});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(color: AppColor.surface1, borderRadius: BorderRadius.circular(16)),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        spacing: 4,
        children: [
          VectorGraphic(loader: AssetBytesLoader(iconAsset)),
          Flexible(
            child: Text(
              text,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style:
                  isNumber
                      ? AppTextStyle.numberSmall.withColor(AppColor.text2)
                      : AppTextStyle.bodySmall.withColor(AppColor.text2),
            ),
          ),
        ],
      ),
    );
  }
}

/// A single order-list summary card: list name, customer/collected counts,
/// and the circular progress ring. Used as the static card for "mandoob"
/// (single list).
class _OrderListCard extends StatelessWidget {
  final OrderList orderList;
  const _OrderListCard({required this.orderList});

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 158,
      decoration: BoxDecoration(
        color: AppColor.surface2,
        border: Border.all(color: AppColor.stroke, width: 2),
        borderRadius: BorderRadius.circular(24),
      ),
      padding: const EdgeInsets.all(24),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    VectorGraphic(loader: AssetBytesLoader('assets/svg/list_icon.svg')),
                    const SizedBox(width: 4),
                    Text('القائمة', style: AppTextStyle.bodySmall.withColor(AppColor.text2)),
                  ],
                ),
                Text(orderList.name, maxLines: 2, overflow: TextOverflow.ellipsis, style: AppTextStyle.bodyLarge),
                Spacer(),
                Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: _StatRow(
                    dotColor: AppColor.surface4,
                    dotBorderColor: AppColor.text4,
                    label: 'العملاء',
                    value: orderList.totalOrderCount.toString(),
                  ),
                ),
                _StatRow(
                  dotColor: AppColor.primary,
                  dotBorderColor: AppColor.surface1,
                  label: 'العملاء المسددين',
                  value: orderList.todayCollectedOrderCount.toString(),
                ),
              ],
            ),
          ),
          SizedBox(width: 20),
          SizedBox(
            width: 100,
            height: 100,
            child: CircularProgressIndicator(
              value:
                  orderList.totalOrderCount == 0 ? 0 : orderList.todayCollectedOrderCount / orderList.totalOrderCount,
              backgroundColor: AppColor.surface4,
              strokeWidth: 16,
              strokeCap: StrokeCap.round,
            ),
          ),
        ],
      ),
    );
  }
}

class _StatRow extends StatelessWidget {
  final Color dotColor;
  final Color dotBorderColor;
  final String label;
  final String value;
  const _StatRow({required this.dotColor, required this.dotBorderColor, required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          width: 10,
          height: 10,
          decoration: ShapeDecoration(
            color: dotColor,
            shape: OvalBorder(
              side: BorderSide(width: 0.30, strokeAlign: BorderSide.strokeAlignOutside, color: dotBorderColor),
            ),
          ),
        ),
        const SizedBox(width: 4),
        Expanded(child: Text(label, overflow: TextOverflow.ellipsis, style: AppTextStyle.labelMedium)),
        Text(
          value,
          textAlign: TextAlign.center,
          style: TextStyle(color: AppColor.text1, fontSize: 12, fontFamily: 'Rubik'),
        ),
        const SizedBox(width: 4),
        Text('عميل', style: AppTextStyle.labelLarge.withColor(AppColor.text2)),
      ],
    );
  }
}
