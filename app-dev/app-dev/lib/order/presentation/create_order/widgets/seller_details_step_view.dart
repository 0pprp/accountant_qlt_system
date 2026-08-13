import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/order/domain/order_list_info/order_list_info.dart';
import 'package:team/order/presentation/create_order/bloc/create_order_bloc.dart';
import 'package:team/order/presentation/create_order/widgets/seller_date_time_input.dart';

class SellerDetailsStepView extends StatefulWidget {
  const SellerDetailsStepView({
    super.key,
    required this.orderListTextController,
    required this.sellerAddressController,
    required this.sellerNameController,
    required this.sellDateTimeController,
    required this.sellDateTime,
  });

  final TextEditingController orderListTextController;
  final TextEditingController sellerAddressController;
  final TextEditingController sellerNameController;
  final TextEditingController sellDateTimeController;
  final ValueNotifier<DateTime?> sellDateTime;

  @override
  State<SellerDetailsStepView> createState() => _SellerDetailsStepViewState();
}

class _SellerDetailsStepViewState extends State<SellerDetailsStepView> {
  late final _bloc = context.read<CreateOrderBloc>();

  final bool _isMotaba = GetIt.I.get<AccountMapper>().isMotaba;

  final FocusNode _orderListFocusNode = FocusNode();
  final MenuController _orderListMenuController = MenuController();

  DateTime _combineDateAndTime(DateTime? date, TimeOfDay? time) {
    final baseDate = date ?? DateTime.now();

    if (time == null) {
      return baseDate;
    }

    return DateTime(
      baseDate.year,
      baseDate.month,
      baseDate.day,
      time.hour,
      time.minute,
    );
  }

  String _formatSellDateTime(DateTime date) {
    final hour12 = date.hour % 12 == 0 ? 12 : date.hour % 12;
    final minute = date.minute.toString().padLeft(2, '0');
    final period = date.hour < 12 ? 'صباحا' : 'مساءا';
    // Note: Added padding to month and day for consistency (e.g., 05 instead of 5)
    final month = date.month.toString().padLeft(2, '0');
    final day = date.day.toString().padLeft(2, '0');
    return '${date.year} / $month / $day  -  $hour12:$minute $period';
  }

  void _selectOrderList(OrderListInfo orderList) {
    _bloc.selectedOrderList = orderList;
    widget.orderListTextController.text = orderList.name;
    widget.sellerNameController.text = orderList.mandobFullName ?? '';
    _orderListMenuController.close();
    _orderListFocusNode.unfocus();
    setState(() {});
  }

  @override
  void initState() {
    super.initState();
    widget.orderListTextController.text = _bloc.selectedOrderList?.name ?? '';
    widget.sellerNameController.text = _bloc.sellerFullName ?? '';
    widget.sellerAddressController.text = _bloc.orderDetails?.creationAddress ?? '';
    final saleDate = _bloc.orderDetails?.saleDate;
    final saleTime = _bloc.orderDetails?.saleTime;
    final combinedDateTime = _combineDateAndTime(saleDate, saleTime);
    widget.sellDateTime.value = combinedDateTime;
    widget.sellDateTimeController.text = _formatSellDateTime(combinedDateTime);
  }

  @override
  void dispose() {
    _orderListFocusNode.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.only(right: 24, left: 24, top: 24, bottom: 24),
      children: [
        if (_isMotaba)
          DefaultBuilder<CreateOrderBloc>(
            buildWhen: (_, state) => state.event is GetOrderLists,
            builder: (context, state) {
              final orderLists = _bloc.orderLists ?? [];
              return Padding(
                padding: const EdgeInsets.only(bottom: 16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  spacing: 6,
                  children: [
                    Text('القائمة', style: AppTextStyle.bodyMedium.withColor(AppColor.text2)),
                    Theme(
                      data: Theme.of(context).copyWith(iconButtonTheme: IconButtonThemeData()),
                      child: DropdownMenu<OrderListInfo?>(
                        controller: widget.orderListTextController,
                        focusNode: _orderListFocusNode,
                        menuController: _orderListMenuController,
                        expandedInsets: EdgeInsets.zero,
                        enableFilter: false,
                        enableSearch: false,
                        requestFocusOnTap: true,
                        closeBehavior: DropdownMenuCloseBehavior.none,
                        textInputAction: TextInputAction.next,
                        textStyle: AppTextStyle.bodyMedium,
                        hintText: 'اختر القائمة',
                        inputDecorationTheme: InputDecorationTheme(
                          filled: true,
                          fillColor: AppColor.surface1,
                          hintStyle: TextStyle(
                            color: AppColor.text4,
                            fontSize: 10,
                            fontWeight: FontWeight.w500,
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12),
                            borderSide: BorderSide(width: 1.5, color: AppColor.primary),
                          ),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12),
                            borderSide: BorderSide(
                              width: 1.5,
                              color: widget.orderListTextController.text.isEmpty ? AppColor.stroke : AppColor.text2,
                            ),
                          ),
                        ),
                        alignmentOffset: const Offset(0, 8),
                        dropdownMenuEntries:
                            orderLists.map((orderList) {
                              final isSelected = _bloc.selectedOrderList?.id == orderList.id;
                              return DropdownMenuEntry<OrderListInfo>(
                                value: orderList,
                                label: orderList.name,
                                trailingIcon: isSelected ? Icon(Icons.check, size: 18, color: AppColor.primary) : null,
                                style:
                                    isSelected
                                        ? MenuItemButton.styleFrom(
                                          backgroundColor: AppColor.primary.withValues(alpha: 0.08),
                                        )
                                        : null,
                              );
                            }).toList(),
                        onSelected: (orderList) {
                          if (orderList == null) return;
                          _selectOrderList(orderList);
                        },
                      ),
                    ),
                  ],
                ),
              );
            },
          )
        else
          SellerDetailsInput(
            controller: widget.orderListTextController,
            title: 'القائمة',
            hint: 'القائمة',
            isDisabled: true,
          ),
        SellerDetailsInput(
          controller: widget.sellerAddressController,
          title: 'عنوان البيع',
          hint: 'يحدد العنوان عن طريق الخرائط',
        ),
        SellerDetailsInput(
          controller: widget.sellerNameController,
          title: 'المستخدم',
          hint: 'المستخدم',
          isDisabled: true,
        ),
        SellerDateTimeInput(
          dateTimeController: widget.sellDateTimeController,
          sellDateTime: widget.sellDateTime,
          title: 'تاريخ و وقت الانشاء',
        ),
      ],
    );
  }
}

class SellerDetailsInput extends StatelessWidget {
  const SellerDetailsInput({
    super.key,
    required this.controller,
    required this.title,
    required this.hint,
    this.isDisabled = false,
    this.isNumberOnly = false,
    this.textDirection,
  });

  final TextEditingController controller;
  final String title;
  final String hint;
  final bool isDisabled;
  final bool isNumberOnly;
  final TextDirection? textDirection;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        spacing: 6,
        children: [
          Text(title, style: AppTextStyle.bodyMedium.withColor(AppColor.text2)),
          DefaultTextField(
            textEditingController: controller,
            hint: hint,
            enabled: !isDisabled,
            filled: true,
            keyboardType: isNumberOnly ? TextInputType.number : null,
            inputFormatters: isNumberOnly ? [FilteringTextInputFormatter.digitsOnly] : null,
            textInputAction: TextInputAction.next,
            textDirection: textDirection,
          ),
        ],
      ),
    );
  }
}
