import 'dart:io';
import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:open_file/open_file.dart';
import 'package:path_provider/path_provider.dart';
import 'package:team/common/services/request/request.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/number_extentions.dart';
import 'package:team/payment/infrastructure/repositories/payment_repository.dart';
import 'package:vector_graphics/vector_graphics.dart';

import 'bloc/order_payment_details_bloc.dart';

class OrderPaymentDetailsPage extends StatelessWidget {
  final int id;
  const OrderPaymentDetailsPage({super.key, required this.id});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create: (BuildContext context) => OrderPaymentDetailsBloc(paymentRepository: GetIt.I.get<PaymentRepository>()),
      child: OrderPaymentDetailsPageView(id: id),
    );
  }
}

class OrderPaymentDetailsPageView extends StatefulWidget {
  final int id;
  const OrderPaymentDetailsPageView({super.key, required this.id});

  @override
  State<OrderPaymentDetailsPageView> createState() => _OrderPaymentDetailsPageViewState();
}

class _OrderPaymentDetailsPageViewState extends State<OrderPaymentDetailsPageView> {
  late final _bloc = context.read<OrderPaymentDetailsBloc>();
  final printPageSize = ValueNotifier<ExpansionOption>(ExpansionOption(id: 1, name: 'A4'));
  final printDirectionSize = ValueNotifier<ExpansionOption>(ExpansionOption(id: 0, name: 'عامودي'));
  final printPageSizeExpansionTileController = ExpansionTileController();
  final printDirectionExpansionTileController = ExpansionTileController();

  Future<void> _downloadPdf() async {
    // Get the appropriate downloads directory
    Directory? downloadsDir;

    if (Platform.isAndroid) {
      // App-specific external storage — no runtime permission needed
      downloadsDir = await getExternalStorageDirectory();
    } else if (Platform.isIOS) {
      // For iOS, use the documents directory (Files app accessible)
      downloadsDir = await getApplicationDocumentsDirectory();
    }

    if (downloadsDir == null) {
      throw Exception('Could not access downloads directory');
    }

    // Create filename with timestamp to avoid conflicts
    final timestamp = DateTime.now().millisecondsSinceEpoch;
    final fileName = 'orderPayments_$timestamp.pdf';
    final savePath = '${downloadsDir.path}/$fileName';

    // Download the file
    await GetIt.I.get<Request>().download(
      'api/v1/app/Orders/${widget.id}/installment-payments/pdf-report?size=${printPageSize.value.id}&Orientation=${printDirectionSize.value.id}',
      savePath,
    );

    // Open the downloaded file
    final result = await OpenFile.open(savePath);

    if (result.type != ResultType.done) {
      throw Exception('Could not open the downloaded file: ${result.message}');
    }
  }

  Future<void> _generatePdf() async {
    try {
      Navigator.pop(context);
      // Show loading indicator
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (context) => const Center(child: CircularProgressIndicator()),
      );

      // Generate PDF
      final pdf = await _downloadPdf();

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

  void _showPrintSettingsBottomSheet() {
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
              SizedBox(height: 32),
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
                  Expanded(child: FilledButton(onPressed: () => _generatePdf(), child: Text('تصدير'))),
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

  @override
  void initState() {
    _bloc.add(InitialOrderPaymentDetailsEvent(widget.id));
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: DefaultAppBar(
        title: 'تسديدات العميل',
        firstIcon: IconButton(
          onPressed: () => _showPrintSettingsBottomSheet(),
          icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/printer_icon.svg')),
        ),
      ),
      body: DefaultBuilder<OrderPaymentDetailsBloc>(
        buildWhen: (previous, state) => state.event is InitialOrderPaymentDetailsEvent,
        builder: (context, state) {
          if (state is ErrorState) {
            return DefaultErrorWidget(
              error: state.error,
              onRetry: () => _bloc.add(InitialOrderPaymentDetailsEvent(widget.id)),
            );
          }
          if (_bloc.ordersInstallment != null) {
            return Padding(
              padding: EdgeInsets.symmetric(horizontal: 22, vertical: 24),
              child: Material(
                borderRadius: BorderRadius.circular(16),
                elevation: 0,
                color: AppColor.surface2,
                child: InkWell(
                  // onTap: () => Navigator.pushNamed(context, RouteNames.customerDetails),
                  borderRadius: BorderRadius.circular(16),
                  child: Container(
                    padding: EdgeInsets.symmetric(vertical: 10, horizontal: 16),
                    decoration: BoxDecoration(
                      border: Border.all(color: AppColor.stroke, width: 2),
                      borderRadius: BorderRadius.circular(16),
                    ),
                    child: Column(
                      children: [
                        Row(
                          crossAxisAlignment: CrossAxisAlignment.center,
                          children: [
                            Expanded(
                              child: Column(
                                children: [
                                  Padding(
                                    padding: const EdgeInsets.only(bottom: 12, left: 16),
                                    child: Row(
                                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                      children: [
                                        Expanded(
                                          child: Text(
                                            _bloc.ordersInstallment!.customerFullName,
                                            style: AppTextStyle.headlineMedium,
                                          ),
                                        ),
                                        Text(
                                          _bloc.ordersInstallment!.sellAmount.toIraqiDinarString(),
                                          style: TextStyle(
                                            color: AppColor.text1,
                                            fontWeight: FontWeight.w400,
                                            fontSize: 14,
                                            fontFamily: 'Rubik',
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                  Padding(
                                    padding: const EdgeInsets.only(bottom: 4),
                                    child: Column(
                                      children: [
                                        Padding(
                                          padding: const EdgeInsets.only(right: 8),
                                          child: Row(
                                            children: [
                                              VectorGraphic(loader: AssetBytesLoader('assets/svg/clock_icon.svg')),
                                              Expanded(
                                                child: Align(
                                                  alignment: Alignment.centerRight,
                                                  child: Padding(
                                                    padding: const EdgeInsets.only(right: 4),
                                                    child: Text(
                                                      DateFormat(
                                                        'y / M / d',
                                                      ).format(_bloc.ordersInstallment!.createdAt),
                                                      overflow: TextOverflow.ellipsis,
                                                      textDirection: TextDirection.ltr,
                                                      style: TextStyle(
                                                        color: AppColor.text2,
                                                        fontWeight: FontWeight.w300,
                                                        fontSize: 12,
                                                        fontFamily: 'Rubik',
                                                      ),
                                                    ),
                                                  ),
                                                ),
                                              ),
                                              // SizedBox(width: 26),
                                              // VectorGraphic(loader: AssetBytesLoader('assets/svg/mobile_icon.svg')),
                                              // Flexible(
                                              //   child: Padding(
                                              //     padding: const EdgeInsets.only(right: 4, left: 16),
                                              //     child: Text(
                                              //       _bloc.ordersInstallment!.productName,
                                              //       overflow: TextOverflow.ellipsis,
                                              //       textDirection: TextDirection.ltr,
                                              //       style: TextStyle(
                                              //         color: AppColor.text2,
                                              //         fontWeight: FontWeight.w300,
                                              //         fontSize: 12,
                                              //         fontFamily: 'Rubik',
                                              //       ),
                                              //     ),
                                              //   ),
                                              // ),
                                            ],
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            Transform.rotate(
                              angle: -90 / 180 * pi,
                              child: VectorGraphic(
                                loader: AssetBytesLoader('assets/svg/back_icon.svg'),
                                colorFilter: ColorFilter.mode(Color(0xff808189), BlendMode.srcIn),
                              ),
                            ),
                          ],
                        ),
                        SizedBox(height: 24),
                        Expanded(
                          child: ListView.builder(
                            physics: ScrollPhysics(),
                            itemCount: _bloc.ordersInstallment!.installmentPayments.length,
                            shrinkWrap: true,
                            itemBuilder: (context, index) {
                              return Padding(
                                padding: const EdgeInsets.only(bottom: 8),
                                child: Container(
                                  padding: EdgeInsets.symmetric(vertical: 6),
                                  decoration: BoxDecoration(
                                    color:
                                        _bloc.ordersInstallment!.installmentPayments[index].hasPayment
                                            ? AppColor.surface3
                                            : null,
                                    borderRadius: BorderRadius.circular(16),
                                  ),
                                  child: Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                                    children: [
                                      //الأول
                                      Text(
                                        'تسديد ${_bloc.ordersInstallment!.installmentPayments.length - index}',
                                        style: AppTextStyle.labelLarge.withColor(AppColor.text2),
                                      ),
                                      Text(
                                        DateFormat(
                                          'd / M / y',
                                        ).format(_bloc.ordersInstallment!.installmentPayments[index].date),
                                        style: AppTextStyle.numberSmall.withColor(AppColor.text2),
                                      ),
                                      Text(
                                        _bloc.ordersInstallment!.installmentPayments[index].amount == null
                                            ? 'لا يوجد'
                                            : _bloc.ordersInstallment!.installmentPayments[index].amount!
                                                .toIraqiDinarString(),
                                        style: AppTextStyle.labelLarge.withColor(AppColor.text2),
                                      ),
                                    ],
                                  ),
                                ),
                              );
                            },
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            );
          }
          return const DefaultLoadingWidget();
        },
      ),
    );
  }
}
