import 'package:calendar_date_picker2/calendar_date_picker2.dart';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/theme/app_theme.dart';

class DefaultCalendar extends StatelessWidget {
  final ValueNotifier<DateTime?> startDate;
  final ValueNotifier<DateTime?> endDate;
  final Function? onValueChanged;
  const DefaultCalendar({super.key, required this.startDate, required this.endDate, this.onValueChanged});

  @override
  Widget build(BuildContext context) {
    return Localizations(
      locale: Locale('en'),
      delegates: [
        GlobalCupertinoLocalizations.delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ],
      child: Theme(
        data: AppTheme.lightTheme.copyWith(
          colorScheme: AppTheme.lightTheme.colorScheme.copyWith(outlineVariant: AppColor.surface1),
          textTheme: AppTheme.lightTheme.textTheme.apply(fontFamily: 'Rubik'),
          iconButtonTheme: IconButtonThemeData(
            style: IconButton.styleFrom(
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              backgroundColor: AppColor.surface1,
              elevation: 0,
              shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.3),
            ),
          ),
        ),
        child: Container(
          height: 350,
          padding: EdgeInsets.only(right: 16, left: 16, top: 24),
          decoration: BoxDecoration(
            color: AppColor.surface1,
            borderRadius: BorderRadius.circular(24),
            border: Border.all(color: AppColor.stroke, width: 2),
          ),
          child: CalendarDatePicker2(
            config: CalendarDatePicker2Config(
              yearTextStyle: AppTextStyle.numberMedium,
              dayTextStyle: AppTextStyle.numberMedium,
              monthTextStyle: AppTextStyle.numberMedium,
              selectedRangeHighlightColor: Colors.transparent,
              controlsTextStyle: AppTextStyle.numberMedium,
              selectedRangeDayTextStyle: AppTextStyle.numberMedium.withColor(AppColor.primary),
              weekdayLabelTextStyle: AppTextStyle.headlineMedium.withColor(AppColor.text2),
              animateToDisplayedMonthDate: true,
              centerAlignModePicker: true,
              dayBorderRadius: BorderRadius.circular(8),
              hideMonthPickerDividers: true,
              hideYearPickerDividers: true,
              hideScrollViewTopHeaderDivider: true,
              hideScrollViewMonthWeekHeader: true,
              hideScrollViewTopHeader: true,
              modePickersGap: 0,
              weekdayLabels: ['Sa', 'Su', 'Mo', 'Tu', 'We', 'Th', 'Fr'],
              selectedRangeDecorationPredicate: ({
                required dayToBuild,
                required decoration,
                required isEndDate,
                required isStartDate,
              }) {
                return BoxDecoration(
                  color: AppColor.surface3,
                  borderRadius: BorderRadius.circular(8),
                  shape: BoxShape.rectangle,
                );
              },
              dayBuilder: ({required date, decoration, isDisabled, isSelected, isToday, textStyle}) {
                return Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Expanded(
                      child: AspectRatio(
                        aspectRatio: 1,
                        child: Container(
                          decoration: decoration?.copyWith(
                            borderRadius: BorderRadius.circular(8),
                            shape: BoxShape.rectangle,
                            // color: (isSelected ?? false) ? AppColor.primary : Colors.white,
                          ),
                          child: Center(
                            child: Text(MaterialLocalizations.of(context).formatDecimal(date.day), style: textStyle),
                          ),
                        ),
                      ),
                    ),
                  ],
                );
              },
              monthBuilder: ({decoration, isCurrentMonth, isDisabled, isSelected, required month, textStyle}) {
                return Center(
                  child: Container(
                    decoration: decoration?.copyWith(color: (isSelected ?? false) ? AppColor.primary : null),
                    height: 36,
                    width: 72,
                    child: Center(
                      child: Semantics(
                        selected: isSelected,
                        button: true,
                        child: Text(
                          getLocaleShortMonthFormat(Locale('en')).format(DateTime(2025, month)),
                          style: textStyle?.copyWith(color: (isSelected ?? false) ? AppColor.white : null),
                        ),
                      ),
                    ),
                  ),
                );
              },
              calendarType: CalendarDatePicker2Type.range,
              yearBuilder: ({decoration, isCurrentYear, isDisabled, isSelected, textStyle, required year}) {
                return Center(
                  child: Container(
                    decoration: decoration?.copyWith(color: (isSelected ?? false) ? AppColor.primary : null),
                    height: 36,
                    width: 72,
                    child: Center(
                      child: Semantics(
                        selected: isSelected,
                        button: true,
                        child: Text(
                          year.toString(),
                          style: textStyle?.copyWith(color: (isSelected ?? false) ? AppColor.white : null),
                        ),
                      ),
                    ),
                  ),
                );
              },
            ),
            value: [],
            onValueChanged: (value) {
              if (value.isNotEmpty) {
                startDate.value = value.first;
              }
              if (value.length > 1) {
                endDate.value = value[1];
              } else {
                endDate.value = null;
              }
              onValueChanged?.call();
            },
          ),
        ),
      ),
    );
  }
}
