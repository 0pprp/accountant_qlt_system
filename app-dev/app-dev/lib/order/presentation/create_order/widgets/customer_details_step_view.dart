import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';
import 'package:team/account/domain/user_info/user_info.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/order/presentation/create_order/bloc/create_order_bloc.dart';
import 'package:team/order/presentation/create_order/widgets/customer_birth_date_input.dart';

class CustomerDetailsStepView extends StatefulWidget {
  const CustomerDetailsStepView({
    super.key,
    required this.fullNameController,
    required this.motherNameController,
    required this.nationalCodeController,
    required this.birthDateController,
    required this.birthDate,
    required this.businessNameController,
    required this.whatsAppPhoneNumberController,
    required this.phoneNumberController,
    required this.businessAddressController,
    required this.businessNearestKnownLocationController,
  });

  final TextEditingController fullNameController;
  final TextEditingController motherNameController;
  final TextEditingController nationalCodeController;
  final TextEditingController birthDateController;
  final ValueNotifier<DateTime?> birthDate;
  final TextEditingController businessNameController;
  final TextEditingController whatsAppPhoneNumberController;
  final TextEditingController phoneNumberController;
  final TextEditingController businessAddressController;
  final TextEditingController businessNearestKnownLocationController;

  @override
  State<CustomerDetailsStepView> createState() => _CustomerDetailsStepViewState();
}

class _CustomerDetailsStepViewState extends State<CustomerDetailsStepView> {
  late final _bloc = context.read<CreateOrderBloc>();
  final FocusNode fullNameFocusNode = FocusNode();
  final MenuController _menuController = MenuController();

  Timer? _searchDebounce;

  // sentinel for the "load more" entry — no real customer has id -1
  static final UserInfo _loadMoreEntry = UserInfo(
    id: -1,
    fullName: 'تحميل المزيد...',
    motherName: '',
    businessName: '',
    createdAt: DateTime.fromMillisecondsSinceEpoch(0),
  );

  @override
  void initState() {
    super.initState();
    fullNameFocusNode.addListener(_onFocusChange);
    widget.fullNameController.addListener(_onSearchTextChanged);

    widget.fullNameController.text = _bloc.selectedCustomer?.fullName ?? '';
    widget.motherNameController.text = _bloc.selectedCustomer?.motherName ?? '';
    widget.nationalCodeController.text = _bloc.selectedCustomer?.nationalCode ?? '';
    widget.birthDate.value = _bloc.selectedCustomer?.birthDate;
    if (_bloc.selectedCustomer?.birthDate != null) {
      widget.birthDateController.text = DateFormat('yyyy-MM-dd').format(_bloc.selectedCustomer!.birthDate);
    }
    widget.businessNameController.text = _bloc.selectedCustomer?.business.name ?? '';
    widget.whatsAppPhoneNumberController.text = _bloc.selectedCustomer?.whatsAppPhoneNumber ?? '';
    widget.phoneNumberController.text = _bloc.selectedCustomer?.phoneNumber ?? '';
    widget.businessAddressController.text = _bloc.selectedCustomer?.business.address ?? '';
    widget.businessNearestKnownLocationController.text = _bloc.selectedCustomer?.business.nearestKnownLocation ?? '';

    // load page 1 immediately — don't rely on focus, tapping the trailing arrow
    // opens the menu without reliably focusing the field first
    if (_bloc.customers.isEmpty) {
      _bloc.add(GetCustomersPaginatedEvent());
    }
  }

  @override
  void dispose() {
    _searchDebounce?.cancel();
    widget.fullNameController.removeListener(_onSearchTextChanged);
    fullNameFocusNode.removeListener(_onFocusChange);
    fullNameFocusNode.dispose();
    super.dispose();
  }

  // fallback retry in case the initial load above came back empty/errored
  void _onFocusChange() {
    if (fullNameFocusNode.hasFocus && _bloc.customers.isEmpty && !_bloc.isFetchingCustomers) {
      _bloc.add(GetCustomersPaginatedEvent());
    }
  }

  void _onSearchTextChanged() {
    _searchDebounce?.cancel();

    final text = widget.fullNameController.text;

    // DropdownMenu writes the tapped entry's label into the field before onSelected runs —
    // this is the "load more" sentinel's own label, not something the user typed, ignore it
    if (text == _loadMoreEntry.fullName) return;

    if (text == (_bloc.selectedCustomerInfo?.fullName ?? '')) return;

    if (_bloc.selectedCustomerInfo != null) {
      _bloc.selectedCustomerInfo = null; // text no longer matches what was picked — selection is stale
    }

    _searchDebounce = Timer(const Duration(milliseconds: 350), () {
      final currentText = widget.fullNameController.text;
      final selected = _bloc.selectedCustomerInfo;
      if (selected != null && selected.fullName == currentText) return; // a selection landed in the meantime
      _bloc.add(GetCustomersPaginatedEvent(searchText: currentText.isEmpty ? null : currentText));
    });
  }

  void _selectCustomer(UserInfo customer) {
    _bloc.selectedCustomerInfo = customer;
    _bloc.selectedCustomer = null;
    widget.fullNameController.text = customer.fullName;

    _menuController.close();
    fullNameFocusNode.unfocus();
    if (customer.id == -2) {
      _clearInputs();
      return;
    }
    _bloc.add(GetCustomerDetailsEvent(customer.id));
  }

  void _clearInputs() {
    widget.motherNameController.clear();
    widget.nationalCodeController.clear();
    widget.birthDate.value = null;
    widget.birthDateController.clear();
    widget.businessNameController.clear();
    widget.whatsAppPhoneNumberController.clear();
    widget.phoneNumberController.clear();
    widget.businessAddressController.clear();
    widget.businessNearestKnownLocationController.clear();
  }

  void _loadMoreCustomers() {
    final customers = _bloc.customers;
    widget.fullNameController.text = _bloc.selectedCustomerInfo?.fullName ?? '';
    _bloc.add(
      GetCustomersPaginatedEvent(
        lastDateTime: customers.isNotEmpty ? customers.last.createdAt : null,
        searchText: _bloc.currentSearchText,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.only(right: 24, left: 24, top: 24, bottom: 24),
      children: [
        Padding(
          padding: const EdgeInsets.only(bottom: 16),
          child: Text('بيانات العميل:', style: AppTextStyle.headlineMedium),
        ),
        Padding(
          padding: const EdgeInsets.only(bottom: 16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            spacing: 6,
            children: [
              Text('الأسم الثلاثي', style: AppTextStyle.bodyMedium.withColor(AppColor.text2)),
              DefaultBuilder<CreateOrderBloc>(
                buildWhen: (previous, state) => state.event is GetCustomersPaginatedEvent,
                builder: (context, state) {
                  final bloc = context.read<CreateOrderBloc>();
                  final customers = bloc.customers;
                  final selected = bloc.selectedCustomerInfo;

                  return Theme(
                    data: Theme.of(context).copyWith(
                      iconButtonTheme: IconButtonThemeData(),
                    ),
                    child: DropdownMenu<UserInfo?>(
                      controller: widget.fullNameController,
                      focusNode: fullNameFocusNode,
                      menuController: _menuController,
                      expandedInsets: EdgeInsets.zero,
                      enableFilter: false, // searching is server-side via searchText
                      enableSearch: false,
                      requestFocusOnTap: true,
                      closeBehavior: DropdownMenuCloseBehavior.none, // closed manually so "load more" can stay open
                      textInputAction: TextInputAction.next,
                      textStyle: AppTextStyle.bodyMedium,
                      hintText: 'الأسم الكامل الخاص بالعميل',
                      inputDecorationTheme: InputDecorationTheme(
                        filled: true,
                        fillColor: AppColor.surface1,
                        hintStyle: TextStyle(color: AppColor.text4, fontSize: 10, fontWeight: FontWeight.w500),
                        focusedBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                          borderSide: BorderSide(width: 1.5, color: AppColor.primary),
                        ),
                        enabledBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                          borderSide: BorderSide(
                            width: 1.5,
                            color: widget.fullNameController.text.isEmpty ? AppColor.stroke : AppColor.text2,
                          ),
                        ),
                      ),
                      alignmentOffset: Offset(0, 8),
                      dropdownMenuEntries: [
                        ...customers.map((customer) {
                          final isSelected = selected?.id == customer.id;
                          return DropdownMenuEntry<UserInfo>(
                            value: customer,
                            label: customer.fullName,
                            trailingIcon: isSelected ? Icon(Icons.check, size: 18, color: AppColor.primary) : null,
                            style:
                                isSelected
                                    ? MenuItemButton.styleFrom(
                                      backgroundColor: AppColor.primary.withValues(alpha: 0.08),
                                    )
                                    : null,
                          );
                        }),
                        if ((bloc.hasMoreCustomers && customers.isNotEmpty && customers.length > 19) ||
                            bloc.isFetchingCustomers)
                          DropdownMenuEntry<UserInfo>(
                            value: _loadMoreEntry,
                            label: _loadMoreEntry.fullName,
                            enabled: !bloc.isFetchingCustomers,
                            leadingIcon:
                                bloc.isFetchingCustomers
                                    ? const SizedBox(
                                      width: 16,
                                      height: 16,
                                      child: CircularProgressIndicator(
                                        strokeWidth: 2,
                                        color: AppColor.primary,
                                      ),
                                    )
                                    : const Icon(Icons.expand_more),
                          ),
                        if (!bloc.isFetchingCustomers && customers.isEmpty)
                          DropdownMenuEntry<UserInfo>(
                            value: UserInfo(
                              id: -2,
                              fullName: widget.fullNameController.text,
                              motherName: '',
                              businessName: '',
                              createdAt: DateTime.fromMillisecondsSinceEpoch(0),
                            ),
                            label: 'إضافة عميل جديد',
                            leadingIcon: const Icon(Icons.person_add_alt_1_outlined),
                          ),
                        //its a new user so show an add icon
                        // VectorGraphic(loader: AssetBytesLoader('assets/svg/circle_add_icon.svg')),
                      ],
                      onSelected: (customer) {
                        if (customer == null) return;
                        if (customer.id == _loadMoreEntry.id) {
                          _loadMoreCustomers();
                          return;
                        }
                        _selectCustomer(customer);
                      },
                    ),
                  );
                },
              ),
            ],
          ),
        ),
        CustomerDetailsInput(controller: widget.motherNameController, title: 'أسم الأم', hint: ' أسم والدة العميل'),
        CustomerDetailsInput(
          controller: widget.nationalCodeController,
          title: 'رقم الهوية',
          hint: 'رقم الهوية الخاص بالعميل',
          isNumberOnly: true,
        ),
        CustomerBirthDateInput(birthDateController: widget.birthDateController, birthDate: widget.birthDate),
        Container(color: AppColor.stroke, height: 2),
        Padding(
          padding: const EdgeInsets.only(top: 16, bottom: 16),
          child: Text('تفاصيل المتجر:', style: AppTextStyle.headlineMedium),
        ),
        CustomerDetailsInput(
          controller: widget.businessNameController,
          title: 'أسم المتجر',
          hint: 'أسم المحل التجاري الخاص بالعميل',
        ),
        CustomerDetailsInput(
          controller: widget.whatsAppPhoneNumberController,
          title: 'رقم الهاتف (وتساب)',
          hint: 'رقم الهاتف الخاص بالعميل',
          isNumberOnly: true,
          maxLength: 11,
        ),
        CustomerDetailsInput(
          controller: widget.phoneNumberController,
          title: 'رقم الهاتف ثانوي',
          hint: 'رقم الهاتف الثانوي الخاص بالعميل',
          isNumberOnly: true,
          maxLength: 11,
        ),
        CustomerDetailsInput(
          controller: widget.businessAddressController,
          title: 'عنوان المتجر',
          hint: 'عنوان المحل التجاري الخاص بالعميل',
        ),
        CustomerDetailsInput(
          controller: widget.businessNearestKnownLocationController,
          title: 'أقرب نقطة دالة',
          hint: 'أقرب نقطة دالة لعنوان العميل',
        ),
        SizedBox(height: 60),
      ],
    );
  }
}

class CustomerDetailsInput extends StatelessWidget {
  const CustomerDetailsInput({
    super.key,
    required this.controller,
    required this.title,
    required this.hint,
    this.isNumberOnly = false,
    this.maxLength,
  });

  final TextEditingController controller;
  final String title;
  final String hint;
  final bool isNumberOnly;
  final int? maxLength;

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
            filled: true,
            keyboardType: isNumberOnly ? TextInputType.number : null,
            inputFormatters: [
              if (isNumberOnly) FilteringTextInputFormatter.digitsOnly,
              if (maxLength != null) LengthLimitingTextInputFormatter(maxLength),
            ],
            textInputAction: TextInputAction.next,
          ),
        ],
      ),
    );
  }
}
