import 'package:flutter/material.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
import 'package:team/order/domain/order_status.dart';

import '../theme/app_color.dart' show AppColor;

class CustomAppBarFilter extends StatelessWidget {
  const CustomAppBarFilter({
    super.key,
    this.selectedOrderItem,
    this.endDate,
    this.startDate,
    this.executionStatuses,
  });

  final ExpansionOption? selectedOrderItem;
  final DateTime? startDate;
  final DateTime? endDate;
  final List<ExecutionStatus>? executionStatuses;

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: SizedBox(
        height: 30,
        child: Center(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 0),
            child: SizedBox(
              height: 30,
              child: ListView(
                shrinkWrap: true,
                scrollDirection: Axis.horizontal,
                children: [
                  if (executionStatuses != null && executionStatuses!.isNotEmpty)
                    Container(
                      padding: EdgeInsets.symmetric(horizontal: 8, vertical: 6),
                      margin: EdgeInsets.symmetric(horizontal: 2),
                      decoration: BoxDecoration(
                        color: AppColor.surface3,
                        borderRadius: BorderRadius.all(Radius.circular(8)),
                      ),
                      child: Text(
                        executionStatuses!.map((status) => status.arabicLabel).join('، '),
                        style: AppTextStyle.headlineSmall.copyWith(fontWeight: FontWeight.w400),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                  if (selectedOrderItem != null)
                    Container(
                      padding: EdgeInsets.symmetric(horizontal: 8, vertical: 6),
                      margin: EdgeInsets.symmetric(horizontal: 2),
                      decoration: BoxDecoration(
                        color: AppColor.surface3,
                        borderRadius: BorderRadius.all(Radius.circular(8)),
                      ),
                      child: Text(
                        selectedOrderItem?.name ?? '',
                        style: AppTextStyle.headlineSmall.copyWith(fontWeight: FontWeight.w400),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                  if (startDate != null || endDate != null)
                    Container(
                      padding: EdgeInsets.symmetric(horizontal: 8, vertical: 6),
                      margin: EdgeInsets.symmetric(horizontal: 2),
                      decoration: BoxDecoration(
                        color: AppColor.surface3,
                        borderRadius: BorderRadius.all(Radius.circular(8)),
                      ),
                      child: Text(
                        '${startDate == null ? '' : DateFormat('d MMM yyyy').format(startDate!)}${endDate == null ? '' : ' - ${DateFormat('d MMM yyyy').format(endDate!)}'}',
                        textDirection: TextDirection.ltr,
                        textAlign: TextAlign.center,
                        overflow: TextOverflow.ellipsis,
                        style: AppTextStyle.numberSmall,
                      ),
                    ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
