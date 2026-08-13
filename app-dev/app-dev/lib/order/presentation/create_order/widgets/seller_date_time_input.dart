import 'package:calendar_date_picker2/calendar_date_picker2.dart';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/theme/app_theme.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';

class SellerDateTimeInput extends StatelessWidget {
  const SellerDateTimeInput({
    super.key,
    required this.dateTimeController,
    required this.sellDateTime,
    this.title = 'تاريخ و وقت الانشاء',
    this.hint = 'تاريخ و وقت الانشاء',
  });

  final TextEditingController dateTimeController;
  final ValueNotifier<DateTime?> sellDateTime;
  final String title;
  final String hint;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        spacing: 6,
        children: [
          Text(title, style: AppTextStyle.bodyMedium.withColor(AppColor.text2)),
          Theme(
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
            child: GestureDetector(
              onTap: () => _pickSellDateTime(context),
              child: DefaultTextField(
                textEditingController: dateTimeController,
                hint: hint,
                filled: true,
                textDirection: TextDirection.ltr,
                textInputAction: TextInputAction.next,
                enabled: false,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _pickSellDateTime(BuildContext context) async {
    final initial = sellDateTime.value ?? DateTime.now();

    final pickedDate = await showCalendarDatePicker2Dialog(
      context: context,
      config: CalendarDatePicker2WithActionButtonsConfig(
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
        // creation date can't be in the future
        lastDate: DateTime.now(),
        currentDate: initial,
        openedFromDialog: true,
        useAbbrLabelForMonthModePicker: true,
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
                    ),
                    child: Center(
                      child: Text(
                        MaterialLocalizations.of(context).formatDecimal(date.day),
                        style: textStyle,
                      ),
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
        calendarType: CalendarDatePicker2Type.single,
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
      borderRadius: BorderRadius.circular(12),
      dialogSize: Size(
        MediaQuery.sizeOf(context).width - 48,
        MediaQuery.sizeOf(context).height / 2,
      ),
      builder: (context, child) {
        return Localizations(
          locale: Locale('en'),
          delegates: [
            GlobalCupertinoLocalizations.delegate,
            GlobalMaterialLocalizations.delegate,
            GlobalWidgetsLocalizations.delegate,
          ],
          child: child,
        );
      },
    );

    final selectedDate = pickedDate?.firstOrNull;
    if (selectedDate == null) return;

    final pickedTime = await showTimePicker(
      context: context,
      initialTime: TimeOfDay.fromDateTime(initial),
      builder: (context, child) {
        return Localizations(
          locale: Locale('en'),
          delegates: [
            GlobalCupertinoLocalizations.delegate,
            GlobalMaterialLocalizations.delegate,
            GlobalWidgetsLocalizations.delegate,
          ],
          child: Theme(
            data: AppTheme.lightTheme.copyWith(
              colorScheme: AppTheme.lightTheme.colorScheme.copyWith(primary: AppColor.primary),
            ),
            child: child!,
          ),
        );
      },
    );
    if (pickedTime == null) return;

    final combined = DateTime(
      selectedDate.year,
      selectedDate.month,
      selectedDate.day,
      pickedTime.hour,
      pickedTime.minute,
    );

    sellDateTime.value = combined;
    dateTimeController.text = _formatSellDateTime(combined);
  }

  String _formatSellDateTime(DateTime date) {
    final hour12 = date.hour % 12 == 0 ? 12 : date.hour % 12;
    final minute = date.minute.toString().padLeft(2, '0');
    final period = date.hour < 12 ? 'صباحا' : 'مساءا';
    return '${date.year} / ${date.month} / ${date.day}  -  $hour12:$minute $period';
  }
}
