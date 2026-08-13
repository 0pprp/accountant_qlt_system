import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:lottie/lottie.dart';
import 'package:pdf/pdf.dart';
import 'package:team/account/infrastructure/repository/account_repository.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/ui/widgets/show_animated_dialog.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/receipt_pdf_generator_enhanced.dart';
import 'package:team/common/utils/helpers/text_util.dart';
import 'package:team/order/domain/collect_installment/collect_installment.dart';
import 'package:team/order/domain/order/order.dart';
import 'package:team/order/domain/order_details/order_details.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';
import 'package:team/order/presentation/order_details/bloc/order_details_bloc.dart';
import 'package:team/order/presentation/order_details/widgets/customer_info_view.dart';
import 'package:team/order/presentation/order_details/widgets/order_header_buttons.dart';
import 'package:team/order/presentation/order_details/widgets/sale_details_view.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';
import 'package:team/payment/infrastructure/providers/local_payment_provider.dart';
import 'package:vector_graphics/vector_graphics.dart';

class OrderDetailsPage extends StatelessWidget {
  final Order order;
  final int userId;
  final int? installmentPaymentId;
  final int dailyInstallmentAmount;
  const OrderDetailsPage({
    super.key,
    required this.order,
    required this.userId,
    this.installmentPaymentId,
    required this.dailyInstallmentAmount,
  });

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create:
          (BuildContext context) => OrderDetailsBloc(
            orderRepository: GetIt.I.get<OrderRepository>(),
            accountMapper: GetIt.I.get<AccountMapper>(),
          ),
      child: OrderDetailsPageView(
        order: order,
        userId: userId,
        installmentPaymentId: installmentPaymentId,
        dailyInstallmentAmount: dailyInstallmentAmount,
      ),
    );
  }
}

class OrderDetailsPageView extends StatefulWidget {
  final Order order;
  final int userId;
  final int? installmentPaymentId;
  final int dailyInstallmentAmount;
  const OrderDetailsPageView({
    super.key,
    required this.order,
    required this.userId,
    this.installmentPaymentId,
    required this.dailyInstallmentAmount,
  });

  @override
  State<OrderDetailsPageView> createState() => _OrderDetailsPageViewState();
}

class _OrderDetailsPageViewState extends State<OrderDetailsPageView> {
  late final _bloc = context.read<OrderDetailsBloc>();
  late final TextEditingController amountController = TextEditingController(
    text: IraqiDinarInputFormatter().format(widget.dailyInstallmentAmount.toString()),
  );
  //print data
  final printPageSize = ValueNotifier<ExpansionOption>(ExpansionOption(id: 1, name: 'A4'));
  final printDirectionSize = ValueNotifier<ExpansionOption>(ExpansionOption(id: 0, name: 'عامودي'));
  final printPageSizeExpansionTileController = ExpansionTileController();
  final printDirectionExpansionTileController = ExpansionTileController();

  @override
  void initState() {
    _bloc.add(InitialOrderDetailsEvent(orderId: widget.order.id, userId: widget.userId));
    super.initState();
  }

  void _showPaymentResultLoadingDialog() {
    _bloc.add(
      SendUserPayment(
        collectInstallment: CollectInstallment(
          orderId: widget.order.id,
          date: DateTime.now().toUtc(),
          amount: int.parse(amountController.text.replaceAll(',', '').replaceAll('د.ع', '')),
        ),
        order: widget.order,
      ),
    );
    AssetLottie('assets/images/dialog_success_animated.json').load().then((value) => debugPrint('success loaded'));
    AssetLottie('assets/images/dialog_error_animated.json').load().then((value) => debugPrint('error loaded'));
    showAnimatedDialog(
      context: context,
      child: Dialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24.0)),
        backgroundColor: Colors.transparent,
        child: Container(
          width: 250,
          height: 250,
          decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(24)),
          child: Column(
            children: [
              DefaultBuilder(
                bloc: _bloc,
                buildWhen: (previous, state) => state.event is SendUserPayment,
                builder: (context, state) {
                  return Lottie.asset(
                    'assets/images/dialog_${state.event is SendUserPayment && state is LoadingState
                        ? 'loading'
                        : state.event is SendUserPayment && state is ErrorState
                        ? 'error'
                        : 'success'}_animated.json',
                    height: 183,
                    key: Key(state.toString()),
                    repeat: state is LoadingState,
                  );
                },
              ),
              DefaultBuilder(
                bloc: _bloc,
                buildWhen: (previous, state) => state.event is SendUserPayment,
                builder: (context, state) {
                  return Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        state.event is SendUserPayment && state is ResponseState
                            ? 'التسديد ناجح'
                            : state.event is SendUserPayment && state is ErrorState
                            ? 'فشل التسديد'
                            : 'جاري التسديد  ....',
                        style: AppTextStyle.titleLarge,
                      ),
                      SizedBox(height: 2),
                      if (state is! LoadingState)
                        Text(
                          state.event is SendUserPayment && state is ResponseState
                              ? 'تم تسديد قسط العميل بنجاح'
                              : 'حدثت مشكلة اثناء عملية التسديد',
                          style: AppTextStyle.labelMedium,
                        ),
                    ],
                  );
                },
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _showPaymentDialog() {
    String? errorText;
    amountController.text = IraqiDinarInputFormatter().format(widget.dailyInstallmentAmount.toString());

    showAnimatedDialog(
      context: context,
      child: StatefulBuilder(
        builder: (context, setState) {
          // Calculate remaining balance
          final remainingBalance = widget.order.sellAmount - widget.order.paidAmount;

          // Parse the current amount from the controller
          final currentAmount =
              int.tryParse(amountController.text.replaceAll(',', '').replaceAll('د.ع', '').trim()) ?? 0;

          // Check if amount is valid (not exceeding remaining balance and divisible by 1000)
          final isValidAmount = currentAmount > 0 && currentAmount <= remainingBalance && currentAmount % 1000 == 0;

          return Dialog(
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24.0)),
            insetPadding: EdgeInsets.symmetric(horizontal: 24),
            backgroundColor: Colors.transparent,
            child: Container(
              height: 280,
              padding: EdgeInsets.all(32),
              decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(24)),
              child: Column(
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text('التسديد', style: AppTextStyle.headlineMedium.withColor(AppColor.text2)),
                      VectorGraphic(loader: AssetBytesLoader('assets/svg/plus_green_icon.svg')),
                    ],
                  ),
                  Padding(
                    padding: const EdgeInsets.only(top: 32, bottom: 24),
                    child: DefaultTextField(
                      textEditingController: amountController,
                      textDirection: TextDirection.rtl,
                      fillColor: AppColor.surface1,
                      filled: true,
                      textAlign: TextAlign.center,
                      keyboardType: TextInputType.number,
                      inputFormatters: [FilteringTextInputFormatter.digitsOnly, IraqiDinarInputFormatter()],
                      errorText: errorText,
                      onChange: (value) {
                        setState(() {
                          errorText = null;
                          final amount = int.tryParse(value.replaceAll(',', '').replaceAll('د.ع', '').trim()) ?? 0;
                          if (amount > remainingBalance) {
                            errorText =
                                'المبلغ يتجاوز الرصيد المتبقي (${IraqiDinarInputFormatter().format(remainingBalance.toString())})';
                          } else if (amount > 0 && amount % 1000 != 0) {
                            errorText = 'المبلغ يجب أن يكون قابلاً للقسمة على 1000';
                          }
                        });
                      },
                      onTap: () {
                        //select all
                        amountController.selection = TextSelection(
                          baseOffset: 0,
                          extentOffset: amountController.text.length,
                        );
                      },
                      textStyle: TextStyle(
                        fontFamily: 'Rubik',
                        fontSize: 16,
                        fontWeight: FontWeight.w500,
                        color: AppColor.text2,
                      ),
                      contentPadding: EdgeInsets.zero,
                    ),
                  ),
                  Spacer(),
                  Row(
                    children: [
                      Expanded(
                        flex: 2,
                        child: FilledButton(
                          style: FilledButton.styleFrom(backgroundColor: AppColor.success),
                          onPressed:
                              isValidAmount
                                  ? () {
                                    if (amountController.text.trim().isEmpty ||
                                        amountController.text.replaceAll(RegExp(r'[^\d]'), '').isEmpty) {
                                      setState(() {
                                        errorText = 'هذا الحقل مطلوب';
                                      });
                                    } else if (currentAmount > remainingBalance) {
                                      setState(() {
                                        errorText = 'المبلغ يتجاوز الرصيد المتبقي';
                                      });
                                    } else if (currentAmount % 1000 != 0) {
                                      setState(() {
                                        errorText = 'المبلغ يجب أن يكون قابلاً للقسمة على 1000';
                                      });
                                    } else {
                                      setState(() {
                                        errorText = null;
                                      });
                                      Navigator.pop(context, amountController.text);
                                      _showPaymentResultLoadingDialog();
                                    }
                                  }
                                  : null, // Disable button when amount is invalid
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Text('أضافة'),
                              SizedBox(width: 8),
                              VectorGraphic(loader: AssetBytesLoader('assets/svg/plus_outlined_icon.svg')),
                            ],
                          ),
                        ),
                      ),
                      SizedBox(width: 24),
                      FilledButton(
                        style: FilledButton.styleFrom(
                          backgroundColor: AppColor.surface2,
                          elevation: 7,
                          shadowColor: Color(0xffD3D3D3).withValues(alpha: 0.3),
                        ),
                        onPressed: () => Navigator.pop(context),
                        child: Text('ألغاء', style: AppTextStyle.headlineMedium.withColor(AppColor.success)),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  void _showPrintSettingsBottomSheet(final int orderId) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.white,
      builder: (context) {
        return Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisSize: MainAxisSize.min,
            children: [
              Center(
                child: Container(
                  width: 100,
                  height: 4,
                  decoration: BoxDecoration(color: AppColor.text2, borderRadius: BorderRadius.circular(4)),
                ),
              ),
              SizedBox(height: 16),

              Padding(
                padding: const EdgeInsets.only(bottom: 16),
                child: Text('وصل ازبون', style: AppTextStyle.headlineMedium),
              ),
              Padding(
                padding: const EdgeInsets.only(bottom: 8),
                child: Text('حجم الصفحة', style: AppTextStyle.headlineSmall.withColor(AppColor.text2)),
              ),
              Padding(
                padding: const EdgeInsets.only(bottom: 24),
                child: DefaultExpansionTile(
                  valueNotifier: printPageSize,
                  isNumber: true,
                  onExpanded: () {
                    printDirectionExpansionTileController.collapse();
                  },
                  controller: printPageSizeExpansionTileController,
                  children: [
                    ExpansionOption(id: 0, name: 'A3'),
                    ExpansionOption(id: 1, name: 'A4'),
                    ExpansionOption(id: 2, name: 'A5'),
                    ExpansionOption(id: 3, name: 'Letter'),
                  ],
                ),
              ),
              Padding(
                padding: const EdgeInsets.only(bottom: 8),
                child: Text('أتجاه الصفحة', style: AppTextStyle.headlineSmall.withColor(AppColor.text2)),
              ),
              Padding(
                padding: const EdgeInsets.only(bottom: 24),
                child: DefaultExpansionTile(
                  valueNotifier: printDirectionSize,
                  controller: printDirectionExpansionTileController,
                  children: [ExpansionOption(id: 0, name: 'عامودي'), ExpansionOption(id: 1, name: 'أفقي')],
                  onExpanded: () {
                    printPageSizeExpansionTileController.collapse();
                  },
                ),
              ),
              Row(
                children: [
                  Expanded(
                    child: FilledButton(onPressed: () => _generatePdf(orderId), child: Text('تصدير')),
                  ),
                  SizedBox(width: 16),
                  FilledButton(
                    style: FilledButton.styleFrom(
                      backgroundColor: AppColor.surface2,
                      elevation: 7,
                      shadowColor: Color(0xffD3D3D3).withValues(alpha: 0.3),
                    ),
                    onPressed: () {
                      Navigator.pop(context, true);
                    },
                    child: Text('ألغاء', style: AppTextStyle.headlineMedium.withColor(AppColor.primary)),
                  ),
                ],
              ),
            ],
          ),
        );
      },
    );
  }

  Future<void> _generatePdf(final int orderId) async {
    Navigator.pop(context);
    try {
      // Show loading indicator
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (context) => const Center(child: CircularProgressIndicator()),
      );

      //get pdf data
      //last 7 days data
      final List<InstallmentPayment> last7DaysPayments = GetIt.I
          .get<LocalPaymentProvider>()
          .getInstallmentPaymentsByOrderId(
            widget.order.id,
          );
      final latestPayment = last7DaysPayments.firstOrNull;
      final mandoob = GetIt.I.get<AccountRepository>();

      final orderId = widget.order.id;
      final buyerName = widget.order.customer.fullName ?? '';
      final orderItemNames = widget.order.orderItems.map((e) => e.productName).join(' , ') ?? '';
      final totalAmount = widget.order.sellAmount.toDouble();
      final paidAmount = widget.order.paidAmount.toDouble();
      final unpaidAmount = totalAmount - paidAmount;
      final latestPaidAmount = latestPayment?.amount ?? 0.0;
      final latestPaidDate = latestPayment?.date ?? DateTime.now();
      final orderListName = mandoob.user?.orderList?.name ?? '';
      final mandoobName = mandoob.user?.fullName ?? '';

      // Generate PDF
      final pdfPath = await ReceiptPdfGenerator.generateReceipt(
        orderId: orderId,
        buyerName: buyerName,
        orderItemNames: orderItemNames,
        totalAmount: totalAmount,
        paidAmount: paidAmount,
        unpaidAmount: unpaidAmount,
        latestPaidAmount: latestPaidAmount,
        latestPaidDate: latestPaidDate,
        orderListName: orderListName,
        mandoobName: mandoobName,
        paymentHistory: last7DaysPayments,
        pdfPageFormat: _getPageFormat(),
        isLandScape: _isLandscape(),
        logoPath: 'assets/images/logo.png', // Optional
      );

      // Open the PDF
      await ReceiptPdfGenerator.openPdf(pdfPath);

      // Hide loading indicator
      Navigator.of(context).pop();
    } catch (e) {
      // Hide loading indicator if still showing
      if (Navigator.canPop(context)) {
        Navigator.of(context).pop();
      }

      // Show error message
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text('حدث خطأ أثناء إنشاء ملف PDF: $e'), backgroundColor: Colors.red));
    }
  }

  PdfPageFormat _getPageFormat() {
    switch (printPageSize.value.name) {
      case 'A3':
        return PdfPageFormat.a3;
      case 'A5':
        return PdfPageFormat.a5;
      case 'Letter':
        return PdfPageFormat.letter;
      default:
        return PdfPageFormat.a4;
    }
  }

  // Check if page should be in landscape orientation
  bool _isLandscape() {
    return printDirectionSize.value.name == 'أفقي';
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: DefaultAppBar(
        title: 'المبيعات',
        firstIcon: IconButton(
          onPressed: () {
            _showPrintSettingsBottomSheet(widget.order.id);
          },
          icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/printer_icon.svg')),
        ),
      ),
      bottomNavigationBar: DefaultBuilder<OrderDetailsBloc>(
        buildWhen: (previous, state) => state.event is SendUserPayment || state.event is GetOrderDetailsEvent,
        builder:
            (context, state) => Padding(
              padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 40),
              child: Row(
                children: [
                  Expanded(
                    child: FilledButton(
                      onPressed:
                          widget.installmentPaymentId == null &&
                                  !_bloc.isTodayCollected &&
                                  widget.order.approvalStatus == ApprovalStatus.approved
                              ? _showPaymentDialog
                              : null,
                      child: Text('تسديد'),
                    ),
                  ),
                  SizedBox(width: 24),
                  FilledButton(
                    onPressed: () {},
                    style: FilledButton.styleFrom(
                      backgroundColor: Colors.white,
                      foregroundColor: AppColor.primary,
                      elevation: 4,
                      shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.3),
                    ),
                    child: VectorGraphic(loader: AssetBytesLoader('assets/svg/notes_icon.svg')),
                  ),
                ],
              ),
            ),
      ),
      body: ListView(
        padding: EdgeInsets.symmetric(horizontal: 22, vertical: 24),
        children: [
          OrderHeaderButtons(),
          SizedBox(height: 16),
          DefaultBuilder<OrderDetailsBloc>(
            buildWhen: (previous, state) => state.event is GetUserEvent,
            builder: (context, state) {
              if (state is ErrorState) return SizedBox();
              return CustomerInfoView(user: _bloc.user);
            },
          ),

          DefaultBuilder<OrderDetailsBloc>(
            buildWhen: (previous, state) => state.event is GetOrderDetailsEvent,
            builder: (context, state) {
              if (state is ErrorState) return SizedBox();
              return SalesDetailsView(orderDetails: _bloc.orderDetails);
            },
          ),
        ],
      ),
    );
  }
}
