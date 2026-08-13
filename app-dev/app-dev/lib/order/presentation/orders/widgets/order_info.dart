import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:lottie/lottie.dart';
import 'package:pdf/pdf.dart';
import 'package:team/account/infrastructure/repository/account_repository.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/ui/widgets/show_animated_dialog.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/receipt_pdf_generator_enhanced.dart';
import 'package:team/order/domain/collect_installment/collect_installment.dart';
import 'package:team/order/domain/order/order.dart';
import 'package:team/order/domain/order_status.dart';
import 'package:team/order/presentation/orders/bloc/orders_bloc.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';
import 'package:team/payment/infrastructure/providers/local_payment_provider.dart';
import 'package:vector_graphics/vector_graphics.dart';

import '../../../../common/utils/helpers/text_util.dart';

class OrderInfo extends StatefulWidget {
  final Order order;
  final VoidCallback onTap;
  const OrderInfo({super.key, required this.order, required this.onTap});

  @override
  State<OrderInfo> createState() => _OrderInfoState();
}

class _OrderInfoState extends State<OrderInfo> {
  late final _bloc = context.read<OrdersBloc>();
  late final TextEditingController amountController = TextEditingController(
    text: IraqiDinarInputFormatter().format(widget.order.dailyInstallmentAmount.toString()),
  );
  //print data
  final printPageSize = ValueNotifier<ExpansionOption>(ExpansionOption(id: 1, name: 'A4'));
  final printDirectionSize = ValueNotifier<ExpansionOption>(ExpansionOption(id: 0, name: 'عامودي'));
  final printPageSizeExpansionTileController = ExpansionTileController();
  final printDirectionExpansionTileController = ExpansionTileController();

  Future<void> _showPaymentResultLoadingDialog(BuildContext context) async {
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
    await showAnimatedDialog(
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
    await Future.delayed(Duration(milliseconds: 600));
    _showPrintSettingsBottomSheet(widget.order.id);
  }

  void _showPaymentDialog(BuildContext context) async {
    String? errorText;
    amountController.text = IraqiDinarInputFormatter().format(widget.order.dailyInstallmentAmount.toString());

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
                      contentPadding: EdgeInsets.zero,
                      inputFormatters: [FilteringTextInputFormatter.digitsOnly, IraqiDinarInputFormatter()],
                      errorText: errorText,
                      textStyle: TextStyle(
                        fontFamily: 'Rubik',
                        fontSize: 16,
                        fontWeight: FontWeight.w500,
                        color: AppColor.text2,
                      ),
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
                                      _showPaymentResultLoadingDialog(context);
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

  String _getArabicDayName(DateTime date) {
    const arabicDays = [
      'الاثنين', // Monday
      'الثلاثاء', // Tuesday
      'الاربعاء', // Wednesday
      'الخميس', // Thursday
      'الجمعة', // Friday
      'السبت', // Saturday
      'الاحد', // Sunday
    ];
    // DateTime.weekday returns 1 for Monday, 7 for Sunday
    return arabicDays[date.weekday - 1];
  }

  Future<void> _openEditOrder() async {
    await Navigator.pushNamed(
      context,
      RouteNames.createOrder,
      arguments: {'order': widget.order, 'currentOrderListId': widget.order.orderListId},
    );
    _bloc.add(InitialOrdersEvent());
  }

  Future<void> _confirmAndDeleteOrder() async {
    final confirmed = await showAnimatedDialog<bool>(
      context: context,
      child: Dialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text('حذف الطلب', style: AppTextStyle.headlineMedium),
              const SizedBox(height: 12),
              Text(
                'هل أنت متأكد من حذف هذا الطلب؟',
                style: AppTextStyle.bodyMedium.withColor(AppColor.text2),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 24),
              Row(
                children: [
                  Expanded(
                    child: FilledButton(
                      style: FilledButton.styleFrom(backgroundColor: AppColor.error),
                      onPressed: () => Navigator.pop(context, true),
                      child: const Text('يمسح'),
                    ),
                  ),
                  const SizedBox(width: 16),
                  FilledButton(
                    style: FilledButton.styleFrom(backgroundColor: AppColor.surface1),
                    onPressed: () => Navigator.pop(context, false),
                    child: Text('ألغاء', style: AppTextStyle.headlineMedium.withColor(AppColor.text2)),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );

    if (confirmed == true && mounted) {
      _bloc.add(DeleteOrderEvent(orderId: widget.order.id));
    }
  }

  Widget? _buildStatusBadge() {
    final order = widget.order;

    // Draft: show incomplete badge; hide pending approval while still drafting.
    if (order.isDraft) {
      return _StatusBadge(
        label: 'لم يكتمل',
        backgroundColor: AppColor.blue.withValues(alpha: 0.08),
        textColor: AppColor.blue,
      );
    }

    // Completed orders show approval outcome (pending / rejected / approved).
    return switch (order.approvalStatus) {
      ApprovalStatus.pending => _StatusBadge(
        label: ApprovalStatus.pending.arabicLabel,
        backgroundColor: AppColor.yellow.withValues(alpha: 0.2),
        textColor: AppColor.text1,
      ),
      ApprovalStatus.approved || ApprovalStatus.rejected || null => null,
    };
  }

  Widget _buildActionButtons() {
    if (widget.order.showEditDeleteActions) {
      return Row(
        spacing: 16,
        children: [
          Expanded(
            child: FilledButton(
              onPressed: _openEditOrder,
              child: const Text('تعديل'),
            ),
          ),
          FilledButton(
            style: FilledButton.styleFrom(
              backgroundColor: AppColor.surface2,
              foregroundColor: AppColor.error,
              // side: const BorderSide(color: AppColor.stroke, width: 2),
            ),
            onPressed: _confirmAndDeleteOrder,
            child: const Text('يمسح'),
          ),
        ],
      );
    }

    return Row(
      spacing: 16,
      children: [
        Expanded(
          child: FilledButton(
            onPressed:
                widget.order.installmentPaymentId != null ||
                        (widget.order.isCollectedOffline ?? false) ||
                        widget.order.approvalStatus != ApprovalStatus.approved
                    ? null
                    : () {
                      _showPaymentDialog(context);
                    },
            child: const Text('تسديد'),
          ),
        ),
        FilledButton(
          style: FilledButton.styleFrom(
            backgroundColor: AppColor.surface2,
            foregroundColor: AppColor.text1,
          ),
          onPressed: () async {
            if (GetIt.I.get<AccountMapper>().permissions?.order?.canUpdate ?? false) {
              await _openEditOrder();
            } else {
              widget.onTap();
            }
          },
          child: const Text('تفاصيل'),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    final statusBadge = _buildStatusBadge();

    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Material(
        borderRadius: BorderRadius.circular(16),
        elevation: 0,
        color: AppColor.surface2,
        child: InkWell(
          onTap: widget.onTap,
          borderRadius: BorderRadius.circular(16),
          child: Container(
            padding: EdgeInsets.symmetric(vertical: 16, horizontal: 16),
            decoration: BoxDecoration(
              border: Border.all(color: AppColor.stroke, width: 2),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Column(
              children: [
                Column(
                  children: [
                    Padding(
                      padding: const EdgeInsets.only(bottom: 4),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Expanded(
                            child: Column(
                              spacing: 4,
                              children: [
                                Row(
                                  children: [
                                    Flexible(
                                      child: Text(
                                        widget.order.customer.fullName,
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                        style: AppTextStyle.headlineMedium,
                                      ),
                                    ),
                                    if (widget.order.installmentPaymentId != null ||
                                        (widget.order.isCollectedOffline ?? false))
                                      Padding(
                                        padding: const EdgeInsets.only(right: 6, bottom: 1, left: 6),
                                        child: VectorGraphic(
                                          loader: AssetBytesLoader('assets/svg/checkmark_icon.svg'),
                                          colorFilter:
                                              widget.order.isCollectedOffline ?? false
                                                  ? ColorFilter.mode(AppColor.yellow, BlendMode.srcIn)
                                                  : null,
                                        ),
                                      ),
                                  ],
                                ),
                                if (widget.order.creationAddress != null)
                                  Row(
                                    children: [
                                      Expanded(
                                        child: Text(
                                          widget.order.creationAddress!,
                                          overflow: TextOverflow.ellipsis,
                                          textDirection: TextDirection.rtl,
                                          style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                                        ),
                                      ),
                                    ],
                                  ),
                              ],
                            ),
                          ),
                          Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              if (statusBadge != null) ...[
                                statusBadge,
                                const SizedBox(width: 8),
                              ],
                              Container(
                                decoration: BoxDecoration(
                                  color: AppColor.surface1,
                                  borderRadius: BorderRadius.circular(11),
                                ),
                                padding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                                child: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    Text(
                                      widget.order.orderItems.length.toString(),
                                      style: AppTextStyle.numberSmall,
                                    ),
                                    Text(
                                      ' عنصر',
                                      style: AppTextStyle.bodySmall,
                                    ),
                                  ],
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                    Padding(
                      padding: const EdgeInsets.only(bottom: 6),
                      child: Row(
                        children: [
                          Expanded(
                            child: Align(
                              alignment: Alignment.centerRight,
                              child: Text(
                                widget.order.customer.phoneNumber,
                                overflow: TextOverflow.ellipsis,
                                textDirection: TextDirection.ltr,
                                style: AppTextStyle.numberSmall.withColor(AppColor.text2),
                              ),
                            ),
                          ),
                          Row(
                            children: [
                              Text(
                                '${_getArabicDayName(widget.order.createdAt)} - ',
                                overflow: TextOverflow.ellipsis,
                                style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                              ),
                              Text(
                                DateFormat('y/M/d').format(widget.order.createdAt),
                                overflow: TextOverflow.ellipsis,
                                textDirection: TextDirection.ltr,
                                style: AppTextStyle.numberSmall.withColor(AppColor.text2),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
                Container(height: 1, color: AppColor.stroke),
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  child: Column(
                    children: [
                      // Header Row
                      Padding(
                        padding: const EdgeInsets.only(bottom: 12),
                        child: Row(
                          children: [
                            Expanded(
                              flex: 2,
                              child: Text(
                                'العنصر',
                                style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                                textAlign: TextAlign.right,
                              ),
                            ),
                            Expanded(
                              child: Text(
                                'الكمية',
                                style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                                textAlign: TextAlign.center,
                              ),
                            ),
                            Expanded(
                              child: Text(
                                'السعر',
                                style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                                textAlign: TextAlign.center,
                              ),
                            ),
                          ],
                        ),
                      ),
                      // Items List
                      ...widget.order.orderItems.map((item) {
                        return Padding(
                          padding: const EdgeInsets.only(bottom: 8),
                          child: Row(
                            children: [
                              Expanded(
                                flex: 2,
                                child: Text(
                                  item.productName,
                                  style: AppTextStyle.bodyMedium,
                                  textAlign: TextAlign.right,
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ),
                              Expanded(
                                child: Text(
                                  item.quantity.toString(),
                                  style: AppTextStyle.numberSmall,
                                  textAlign: TextAlign.center,
                                ),
                              ),
                              Expanded(
                                child: Text(
                                  '${TextUtil.addComma(item.sellAmount.toString())} د.ع',
                                  style: AppTextStyle.numberSmall,
                                  textAlign: TextAlign.left,
                                  textDirection: TextDirection.rtl,
                                ),
                              ),
                            ],
                          ),
                        );
                      }),
                    ],
                  ),
                ),
                Container(height: 1, color: AppColor.stroke),
                Padding(
                  padding: EdgeInsets.only(bottom: 20, top: 10),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text('المبلغ الكلي', style: AppTextStyle.headlineSmall),
                      Text(
                        '${TextUtil.addComma(widget.order.sellAmount.toString())} د.ع',
                        style: AppTextStyle.numberMedium,
                      ),
                    ],
                  ),
                ),
                _buildActionButtons(),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _StatusBadge extends StatelessWidget {
  final String label;
  final Color backgroundColor;
  final Color textColor;

  const _StatusBadge({
    required this.label,
    required this.backgroundColor,
    required this.textColor,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(11),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      child: Text(
        label,
        style: AppTextStyle.bodySmall.withColor(textColor),
      ),
    );
  }
}
