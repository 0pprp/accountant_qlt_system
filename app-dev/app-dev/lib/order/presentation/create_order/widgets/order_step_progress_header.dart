import 'package:flutter/material.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';

class OrderStepProgressHeader extends StatelessWidget {
  final PageController pageController;
  final ValueNotifier pageIndex;
  OrderStepProgressHeader({
    super.key,
    required this.pageController,
    required this.pageIndex,
  });

  final _currentPageTitle = [
    'تفاصيل العميل',
    'مستندات العميل',
    'تفاصيل القسط',
    'مستندات البيع',
    'معلومات البائع',
  ];

  final _nextPageInfo = [
    'الخطوة التالية:مستندات العميل',
    'الخطوة التالية:تفاصيل القسط',
    'الخطوة التالية:مستندات البيع',
    'الخطوة التالية:تفاصيل البيع',
    '',
  ];

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: EdgeInsets.symmetric(horizontal: 24),
      padding: EdgeInsets.only(top: 26, bottom: 12),
      decoration: BoxDecoration(
        border: Border(
          bottom: BorderSide(color: AppColor.stroke, width: 1),
        ),
      ),
      child: ValueListenableBuilder(
        valueListenable: pageIndex,
        builder:
            (context, value, child) => Row(
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  spacing: 10,
                  children: [
                    Text(
                      _currentPageTitle[pageIndex.value],
                      style: AppTextStyle.headlineLarge,
                    ),
                    if (_nextPageInfo[pageIndex.value].isNotEmpty)
                      Text(
                        _nextPageInfo[pageIndex.value],
                        style: AppTextStyle.labelMedium.withColor(AppColor.text2),
                      ),
                  ],
                ),
                Spacer(),
                Stack(
                  alignment: Alignment.center,
                  children: [
                    SizedBox(
                      height: 70,
                      width: 70,
                      child: AnimatedCircularProgressIndicator(
                        currentStep: pageIndex.value + 1,
                        totalSteps: 5,
                      ),
                    ),
                    Text(
                      '${pageIndex.value + 1} من 5',
                      style: AppTextStyle.headlineSmall,
                    ),
                  ],
                ),
              ],
            ),
      ),
    );
  }
}

class AnimatedCircularProgressIndicator extends StatelessWidget {
  final int currentStep;
  final int totalSteps;

  const AnimatedCircularProgressIndicator({
    super.key,
    required this.currentStep,
    required this.totalSteps,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 70,
      width: 70,
      child: TweenAnimationBuilder<double>(
        tween: Tween(begin: 0, end: currentStep / totalSteps),
        duration: const Duration(milliseconds: 400),
        curve: Curves.easeInOut,
        builder:
            (context, value, _) => CircularProgressIndicator(
              value: value,
              strokeWidth: 12,
              strokeCap: StrokeCap.round,
              backgroundColor: AppColor.surface4,
              valueColor: AlwaysStoppedAnimation<Color>(AppColor.primary),
            ),
      ),
    );
  }
}
