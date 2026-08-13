import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:intl/intl.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/ui/widgets/snackbar.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/customer/mapper.dart';
import 'package:team/order/domain/order/order.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';
import 'package:team/order/presentation/create_order/bloc/create_order_bloc.dart';
import 'package:team/order/presentation/create_order/widgets/order_step_progress_header.dart';

import 'widgets/customer_details_step_view.dart';
import 'widgets/customer_documents_step_view.dart';
import 'widgets/installment_details_step_view.dart';
import 'widgets/order_documents_step_view.dart';
import 'widgets/seller_details_step_view.dart';

class CreateOrderPage extends StatelessWidget {
  final Order? order;
  final int? currentOrderListId;
  const CreateOrderPage({super.key, this.order, this.currentOrderListId});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create:
          (BuildContext context) => CreateOrderBloc(
            orderRepository: GetIt.I.get<OrderRepository>(),
            accountMapper: GetIt.I.get<AccountMapper>(),
            customerMapper: GetIt.I.get<CustomerMapper>(),
            order: order,
            defaultOrderListId: currentOrderListId,
          ),
      child: OrderDetailsPageView(order: order),
    );
  }
}

class OrderDetailsPageView extends StatefulWidget {
  final Order? order;
  const OrderDetailsPageView({super.key, this.order});

  @override
  State<OrderDetailsPageView> createState() => _OrderDetailsPageViewState();
}

class _OrderDetailsPageViewState extends State<OrderDetailsPageView> {
  late final _bloc = context.read<CreateOrderBloc>();
  late final _pageController = PageController(initialPage: widget.order != null ? 2 : 0);
  late final _pageIndex = ValueNotifier(widget.order != null ? 2 : 0);

  final _fullNameController = TextEditingController();
  final _motherNameController = TextEditingController();
  final _nationalCodeController = TextEditingController();
  final _birthDateController = TextEditingController();
  final _birthDate = ValueNotifier<DateTime?>(null);
  final _businessNameController = TextEditingController();
  final _whatsAppPhoneNumberController = TextEditingController();
  final _phoneNumberController = TextEditingController();
  final _businessAddressController = TextEditingController();
  final _businessNearestKnownLocationController = TextEditingController();

  final _orderListTextController = TextEditingController();
  final _sellerAddressController = TextEditingController();
  final _sellerNameController = TextEditingController();
  final _sellDateTimeController = TextEditingController();
  final _sellDateTime = ValueNotifier<DateTime?>(null);

  @override
  void initState() {
    _bloc.add(InitialCreateOrderEvent());
    super.initState();
  }

  @override
  void dispose() {
    _fullNameController.dispose();
    _motherNameController.dispose();
    _nationalCodeController.dispose();
    _birthDateController.dispose();
    _birthDate.dispose();
    _businessNameController.dispose();
    _whatsAppPhoneNumberController.dispose();
    _phoneNumberController.dispose();
    _businessAddressController.dispose();
    _businessNearestKnownLocationController.dispose();
    _orderListTextController.dispose();
    _sellerAddressController.dispose();
    _sellerNameController.dispose();
    _sellDateTimeController.dispose();
    _sellDateTime.dispose();
    _pageController.dispose();
    _pageIndex.dispose();
    super.dispose();
  }

  void _onPopInvokedWithResult(didPop, result) {
    if (didPop) return;
    if (_pageController.page == 0) {
      Navigator.pop(context);
    }
    _pageController.previousPage(duration: Duration(milliseconds: 300), curve: Curves.easeInOut);
  }

  void _moveToNextPage() {
    _pageController.nextPage(duration: Duration(milliseconds: 300), curve: Curves.easeInOut);
  }

  void _checkFirstStep() {
    if (_bloc.selectedCustomerInfo == null) {
      showToast('الرجاء اختيار عميل او اضافة عميل جديد', type: ToastType.error);
      return;
    }
    if (_motherNameController.text.trim().isEmpty ||
        _nationalCodeController.text.trim().isEmpty ||
        _birthDate.value == null ||
        _businessNameController.text.trim().isEmpty ||
        _whatsAppPhoneNumberController.text.trim().isEmpty ||
        _phoneNumberController.text.trim().isEmpty ||
        _businessAddressController.text.trim().isEmpty ||
        _businessNearestKnownLocationController.text.trim().isEmpty) {
      showToast('الرجاء ملئ جميع الحقول', type: ToastType.error);
      return; // ← fixed
    }

    final customer = User(
      id: _bloc.selectedCustomerInfo!.id,
      fullName: _fullNameController.text,
      motherName: _motherNameController.text,
      nationalCode: _nationalCodeController.text,
      birthDate: _birthDate.value!,
      phoneNumber: _phoneNumberController.text,
      whatsAppPhoneNumber: _whatsAppPhoneNumberController.text,
      business: BusinessInformation(
        name: _businessNameController.text,
        address: _businessAddressController.text,
        nearestKnownLocation: _businessNearestKnownLocationController.text,
      ),
      branch: _bloc.selectedCustomer?.branch ?? BranchInformation(id: -2, name: ''),
      attachments: _bloc.selectedCustomer?.attachments ?? [],
    );

    if (_bloc.selectedCustomerInfo!.id == -2 && _bloc.selectedCustomer == null) {
      _bloc.add(
        CreateCustomerEvent(
          customer,
        ),
      );
      return;
    }
    if (customer != _bloc.selectedCustomer) {
      _bloc.add(EditCustomerEvent(customer));
      return;
    }

    if (_bloc.selectedCustomer != null) {
      _moveToNextPage();
    }
  }

  void _checkSecondStep() {
    final attachments = _bloc.selectedCustomer!.attachments;

    final hasNationalCard = attachments.any((e) => e.type == AttachmentType.nationalCard && e.relativePath != null);
    final hasResidenceCard = attachments.any((e) => e.type == AttachmentType.residenceCard && e.relativePath != null);
    final hasRationCard = attachments.any((e) => e.type == AttachmentType.rationCard && e.relativePath != null);
    final hasPersonalPicture = attachments.any(
      (e) => e.type == AttachmentType.personalPicture && e.relativePath != null,
    );

    if (!hasNationalCard || !hasResidenceCard || !hasRationCard || !hasPersonalPicture) {
      showToast('الرجاء رفع جميع المستندات المطلوبة', type: ToastType.error);
      return;
    }

    _moveToNextPage();
  }

  void _checkThirdStep() {
    if (_bloc.inStorageOrderItems.isEmpty && _bloc.outStorageOrderItems.isEmpty) {
      showToast('الرجاء اضافة منتج واحد على الأقل', type: ToastType.error);
      return;
    }
    _bloc.add(CreateOrderEvent());
  }

  void _checkForthStep() {
    final attachments = _bloc.orderAttachments;

    final hasPurchaseReceipt = attachments.any(
      (e) => e.type == AttachmentType.purchaseReceipt && e.relativePath != null,
    );
    final hasTrustReceipt = attachments.any((e) => e.type == AttachmentType.trustReceipt && e.relativePath != null);
    final hasSaleContract = attachments.any((e) => e.type == AttachmentType.saleContract && e.relativePath != null);

    if (!hasPurchaseReceipt || !hasTrustReceipt || !hasSaleContract) {
      showToast('الرجاء رفع جميع المستندات المطلوبة', type: ToastType.error);
      return;
    }

    _bloc.add(StepFourCompleted());
  }

  void _checkFifthStep() {
    if (_sellerAddressController.text.trim().isEmpty || _sellDateTime.value == null) {
      showToast('الرجاء ملئ جميع الحقول', type: ToastType.error);
      return;
    }

    _bloc.add(
      UpdateOrderWithSellerInfo(
        saleAddress: _sellerAddressController.text.trim(),
        saleDateTime: _sellDateTime.value!,
      ),
    );
  }

  void _listener(BuildContext context, DefaultState state) {
    if (state.event is GetCustomerDetailsEvent && state is ResponseState && _bloc.selectedCustomer != null) {
      final customer = _bloc.selectedCustomer!;
      _motherNameController.text = customer.motherName;
      _nationalCodeController.text = customer.nationalCode;
      _birthDate.value = customer.birthDate;
      _birthDateController.text = DateFormat('yyyy-MM-dd').format(customer.birthDate);
      _businessNameController.text = customer.business.name;
      _whatsAppPhoneNumberController.text = customer.whatsAppPhoneNumber;
      _phoneNumberController.text = customer.phoneNumber;
      _businessAddressController.text = customer.business.address;
      _businessNearestKnownLocationController.text = customer.business.nearestKnownLocation;
    }
    if (state.event is CreateCustomerEvent && state is ResponseState && _bloc.selectedCustomer != null) {
      _moveToNextPage();
    }
    if (state.event is EditCustomerEvent && state is ResponseState && _bloc.selectedCustomer != null) {
      _moveToNextPage();
    }
    if (state.event is CreateOrderEvent && state is ResponseState) {
      _moveToNextPage();
    }
    if (state.event is StepFourCompleted && state is ResponseState) {
      _moveToNextPage();
    }
    if (state.event is UpdateOrderWithSellerInfo && state is ResponseState) {
      Navigator.pop(context);
    }
    if (state.event is GetOrderLists && state is ResponseState) {
      _orderListTextController.text = _bloc.selectedOrderList?.name ?? '';
      _sellerNameController.text = _bloc.sellerFullName ?? '';
    }
  }

  @override
  Widget build(BuildContext context) {
    return DefaultListener<CreateOrderBloc>(
      listener: _listener,
      child: PopScope(
        canPop: false,
        onPopInvokedWithResult: _onPopInvokedWithResult,
        child: Scaffold(
          backgroundColor: AppColor.surface2,
          appBar: DefaultAppBar(
            title: 'أضافة مبيع',
            firstIcon: SizedBox(width: 40),
            onBackPressed: () => Navigator.maybePop(context),
            backgroundColor: AppColor.surface2,
          ),
          extendBody: true,
          bottomNavigationBar: Padding(
            padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 44),
            child: Row(
              children: [
                Expanded(
                  child: DefaultBuilder<CreateOrderBloc>(
                    buildWhen:
                        (previous, state) =>
                            state.event is CreateCustomerEvent ||
                            state.event is CreateOrderEvent ||
                            state.event is StepFourCompleted ||
                            state.event is UpdateOrderWithSellerInfo,
                    builder: (context, state) {
                      final isLoading = state is LoadingState;
                      return FilledButton(
                        onPressed:
                            isLoading
                                ? null
                                : () {
                                  if (_pageIndex.value == 0) {
                                    _checkFirstStep();
                                  }
                                  if (_pageIndex.value == 1) {
                                    _checkSecondStep();
                                  }
                                  if (_pageIndex.value == 2) {
                                    _checkThirdStep();
                                  }
                                  if (_pageIndex.value == 3) {
                                    _checkForthStep();
                                  }
                                  if (_pageIndex.value == 4) {
                                    _checkFifthStep();
                                  }
                                },
                        child: isLoading ? DefaultLoadingWidget() : Text('متابعة'),
                      );
                    },
                  ),
                ),
              ],
            ),
          ),
          body: Column(
            children: [
              OrderStepProgressHeader(
                pageController: _pageController,
                pageIndex: _pageIndex,
              ),
              Expanded(
                child: DefaultBuilder<CreateOrderBloc>(
                  buildWhen: (previous, state) => state.event is InitialCreateOrderEvent,
                  builder: (context, state) {
                    if (widget.order == null || _bloc.orderDetails != null) {
                      return PageView(
                        controller: _pageController,
                        onPageChanged: (value) => _pageIndex.value = value,
                        physics: NeverScrollableScrollPhysics(),
                        children: [
                          CustomerDetailsStepView(
                            fullNameController: _fullNameController,
                            motherNameController: _motherNameController,
                            nationalCodeController: _nationalCodeController,
                            birthDateController: _birthDateController,
                            birthDate: _birthDate,
                            businessNameController: _businessNameController,
                            whatsAppPhoneNumberController: _whatsAppPhoneNumberController,
                            phoneNumberController: _phoneNumberController,
                            businessAddressController: _businessAddressController,
                            businessNearestKnownLocationController: _businessNearestKnownLocationController,
                          ),
                          CustomerDocumentsStepView(),
                          InstallmentDetailsStepView(),
                          OrderDocumentsStepView(),
                          SellerDetailsStepView(
                            orderListTextController: _orderListTextController,
                            sellerAddressController: _sellerAddressController,
                            sellerNameController: _sellerNameController,
                            sellDateTimeController: _sellDateTimeController,
                            sellDateTime: _sellDateTime,
                          ),
                        ],
                      );
                    }
                    if (state is ErrorState) {
                      return DefaultErrorWidget(
                        error: state.error,
                        onRetry: () => _bloc.add(InitialCreateOrderEvent()),
                      );
                    }
                    return DefaultLoadingWidget();
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
