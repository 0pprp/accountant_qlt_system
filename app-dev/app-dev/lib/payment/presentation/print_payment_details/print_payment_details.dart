// import 'dart:io';
//
// import 'package:flutter/material.dart';
// import 'package:flutter_bloc/flutter_bloc.dart';
// import 'package:get_it/get_it.dart';
// import 'package:open_file/open_file.dart';
// import 'package:path_provider/path_provider.dart';
// import 'package:permission_handler/permission_handler.dart';
// import 'package:team/common/services/request/request.dart';
// import 'package:team/common/ui/theme/app_color.dart';
// import 'package:team/common/ui/theme/app_text_styles.dart';
// import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
// import 'package:team/common/ui/widgets/default_appbar.dart';
// import 'package:team/common/ui/widgets/default_error_widget.dart';
// import 'package:team/common/ui/widgets/default_loading_widget.dart';
// import 'package:team/common/utils/bloc/default_bloc.dart';
// import 'package:team/order/domain/order_list_item/order_list_item.dart';
// import 'package:team/payment/infrastructure/repositories/payment_repository.dart';
// import 'package:vector_graphics/vector_graphics.dart';
//
// import 'bloc/print_payment_details_bloc.dart';
//
// class PrintPaymentDetailsPage extends StatelessWidget {
//   final PrintDetails printDetails;
//   const PrintPaymentDetailsPage({super.key, required this.printDetails});
//
//   @override
//   Widget build(BuildContext context) {
//     return DefaultBlocProvider(
//       create: (BuildContext context) => PrintPaymentDetailsBloc(paymentRepository: GetIt.I.get<PaymentRepository>()),
//       child: PrintPaymentDetailsPageView(printDetails: printDetails),
//     );
//   }
// }
//
// class PrintPaymentDetailsPageView extends StatefulWidget {
//   final PrintDetails printDetails;
//   const PrintPaymentDetailsPageView({super.key, required this.printDetails});
//
//   @override
//   State<PrintPaymentDetailsPageView> createState() => _PrintPaymentDetailsPageViewState();
// }
//
// class _PrintPaymentDetailsPageViewState extends State<PrintPaymentDetailsPageView> {
//   late final _bloc = context.read<PrintPaymentDetailsBloc>();
//
//   final printPageSize = ValueNotifier<OrderListItem>(OrderListItem(id: 1, name: 'A4'));
//   final printDirectionSize = ValueNotifier<OrderListItem>(OrderListItem(id: 0, name: 'عامودي'));
//   final printPageSizeExpansionTileController = ExpansionTileController();
//   final printDirectionExpansionTileController = ExpansionTileController();
//   @override
//   void initState() {
//     _bloc.add(InitialPrintPaymentDetailsEvent());
//     super.initState();
//   }
//
//   Future<void> _downloadPdf() async {
//     // Request storage permission for Android
//     if (Platform.isAndroid) {
//       var status = await Permission.storage.status;
//       if (!status.isGranted) {
//         status = await Permission.storage.request();
//         if (!status.isGranted) {
//           throw Exception('Storage permission denied');
//         }
//       }
//     }
//
//     // Get the appropriate downloads directory
//     Directory? downloadsDir;
//
//     if (Platform.isAndroid) {
//       // For Android, use external storage downloads directory
//       downloadsDir = Directory('/storage/emulated/0/Download');
//       if (!await downloadsDir.exists()) {
//         downloadsDir = await getExternalStorageDirectory();
//       }
//     } else if (Platform.isIOS) {
//       // For iOS, use the documents directory (Files app accessible)
//       downloadsDir = await getApplicationDocumentsDirectory();
//     }
//
//     if (downloadsDir == null) {
//       throw Exception('Could not access downloads directory');
//     }
//
//     // Create filename with timestamp to avoid conflicts
//     final timestamp = DateTime.now().millisecondsSinceEpoch;
//     final fileName =
//         '${widget.printDetails.printType == PrintType.orderPayments ? 'order_payments' : 'daily_report'}_${widget.printDetails.printType.name}_$timestamp.pdf';
//     final savePath = '${downloadsDir.path}/$fileName';
//
//     // Download the file
//
//     if (widget.printDetails.orderId == null) {
//       await GetIt.I.get<Request>().download(
//         'api/v1/app/InstallmentPayments/daily-pdf-report?status=${widget.printDetails.printType.index}&size=${printPageSize.value.id}&Orientation=${printDirectionSize.value.id}',
//         savePath,
//       );
//     } else {
//       await GetIt.I.get<Request>().download(
//         'api/v1/app/Orders/${widget.printDetails.orderId}/installment-payments/pdf-report?size=${printPageSize.value.id}&Orientation=${printDirectionSize.value.id}',
//         savePath,
//       );
//     }
//
//     // Open the downloaded file
//     final result = await OpenFile.open(savePath);
//
//     if (result.type != ResultType.done) {
//       throw Exception('Could not open the downloaded file: ${result.message}');
//     }
//   }
//
//   Future<void> _generatePdf() async {
//     try {
//       // Show loading indicator
//       showDialog(
//         context: context,
//         barrierDismissible: false,
//         builder: (context) => const Center(child: CircularProgressIndicator()),
//       );
//
//       // Generate PDF
//       final pdf = await _downloadPdf();
//
//       // Hide loading indicator
//       Navigator.of(context).pop();
//     } catch (e) {
//       // Hide loading indicator if still showing
//       if (Navigator.canPop(context)) {
//         Navigator.of(context).pop();
//       }
//
//       // Show error message
//       ScaffoldMessenger.of(
//         context,
//       ).showSnackBar(SnackBar(content: Text('حدث خطأ أثناء إنشاء ملف PDF: $e'), backgroundColor: Colors.red));
//     }
//   }
//
//   void _showPrintSettingsBottomSheet() {
//     showModalBottomSheet(
//       context: context,
//       isScrollControlled: true,
//       backgroundColor: Colors.white,
//       builder: (context) {
//         return Padding(
//           padding: const EdgeInsets.all(24),
//           child: Column(
//             crossAxisAlignment: CrossAxisAlignment.start,
//             mainAxisSize: MainAxisSize.min,
//             children: [
//               Center(
//                 child: Container(
//                   width: 100,
//                   height: 4,
//                   decoration: BoxDecoration(color: AppColor.text2, borderRadius: BorderRadius.circular(4)),
//                 ),
//               ),
//               SizedBox(height: 32),
//               Padding(
//                 padding: const EdgeInsets.only(bottom: 8),
//                 child: Text('حجم الصفحة', style: AppTextStyle.headlineSmall.withColor(AppColor.text2)),
//               ),
//               Padding(
//                 padding: const EdgeInsets.only(bottom: 24),
//                 child: DefaultExpansionTile(
//                   valueNotifier: printPageSize,
//                   isNumber: true,
//                   onExpanded: () {
//                     printDirectionExpansionTileController.collapse();
//                   },
//                   controller: printPageSizeExpansionTileController,
//                   children: [
//                     OrderListItem(id: 0, name: 'A3'),
//                     OrderListItem(id: 1, name: 'A4'),
//                     OrderListItem(id: 2, name: 'A5'),
//                     OrderListItem(id: 3, name: 'Letter'),
//                   ],
//                 ),
//               ),
//               Padding(
//                 padding: const EdgeInsets.only(bottom: 8),
//                 child: Text('أتجاه الصفحة', style: AppTextStyle.headlineSmall.withColor(AppColor.text2)),
//               ),
//               Padding(
//                 padding: const EdgeInsets.only(bottom: 24),
//                 child: DefaultExpansionTile(
//                   valueNotifier: printDirectionSize,
//                   controller: printDirectionExpansionTileController,
//                   children: [OrderListItem(id: 0, name: 'عامودي'), OrderListItem(id: 1, name: 'أفقي')],
//                   onExpanded: () {
//                     printPageSizeExpansionTileController.collapse();
//                   },
//                 ),
//               ),
//               Row(
//                 children: [Expanded(child: FilledButton(onPressed: () => Navigator.pop(context), child: Text('حفظ')))],
//               ),
//             ],
//           ),
//         );
//       },
//     );
//   }
//
//   @override
//   Widget build(BuildContext context) {
//     return Scaffold(
//       appBar: DefaultAppBar(
//         title: widget.printDetails.printType == PrintType.orderPayments ? 'تسديدات العميل' : null,
//         customTitle:
//             widget.printDetails.printType == PrintType.orderPayments
//                 ? null
//                 : Container(
//                   padding: EdgeInsets.symmetric(horizontal: 8, vertical: 6),
//                   margin: EdgeInsets.symmetric(horizontal: 2),
//                   decoration: BoxDecoration(
//                     color: AppColor.surface3,
//                     borderRadius: BorderRadius.all(Radius.circular(8)),
//                   ),
//                   child: Text(
//                     widget.printDetails.printType == PrintType.todayNotPaid ? 'غير المسددين' : 'المسددين',
//                     style: AppTextStyle.headlineSmall.copyWith(fontWeight: FontWeight.w400),
//                     overflow: TextOverflow.ellipsis,
//                   ),
//                 ),
//         firstIcon: SizedBox(width: 48),
//       ),
//       bottomNavigationBar: ColoredBox(
//         color: AppColor.surface1,
//         child: Padding(
//           padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 40),
//           child: Row(
//             children: [
//               Expanded(
//                 child: FilledButton(
//                   onPressed: _generatePdf, // Updated to call PDF export function
//                   child: Text('تصدير'),
//                 ),
//               ),
//               SizedBox(width: 24),
//               FilledButton(
//                 onPressed: _showPrintSettingsBottomSheet,
//                 style: FilledButton.styleFrom(
//                   backgroundColor: Colors.white,
//                   foregroundColor: AppColor.primary,
//                   elevation: 4,
//                   shadowColor: Color(0xffd3d3d3).withValues(alpha: 0.3),
//                 ),
//                 child: VectorGraphic(loader: AssetBytesLoader('assets/svg/settings_icon.svg')),
//               ),
//             ],
//           ),
//         ),
//       ),
//       body: DefaultBuilder<PrintPaymentDetailsBloc>(
//         buildWhen: (previous, state) => state.event is InitialPrintPaymentDetailsEvent,
//         builder: (context, state) {
//           if (state is ErrorState) {
//             return DefaultErrorWidget(error: state.error, onRetry: () => _bloc.add(InitialPrintPaymentDetailsEvent()));
//           }
//           if (state is ResponseState) {
//             return Center(child: VectorGraphic(loader: AssetBytesLoader('assets/svg/pdf.svg'), width: 210));
//             // return Padding(
//             //   padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 24),
//             //   child: Container(
//             //     decoration: BoxDecoration(
//             //       color: Colors.white,
//             //       borderRadius: BorderRadius.circular(24),
//             //       boxShadow: [
//             //         BoxShadow(
//             //           color: Color(0xffD3D1D8).withValues(alpha: 0.3),
//             //           offset: Offset(5, 10),
//             //           blurRadius: 20,
//             //           spreadRadius: 0,
//             //         ),
//             //       ],
//             //     ),
//             // child: ListView.builder(
//             //   padding: EdgeInsets.symmetric(horizontal: 22, vertical: 24),
//             //   physics: ScrollPhysics(),
//             //   itemCount: 100,
//             //   shrinkWrap: true,
//             //   itemBuilder: (context, index) {
//             //     return Padding(
//             //       padding: const EdgeInsets.only(bottom: 8),
//             //       child: Container(
//             //         padding: EdgeInsets.symmetric(vertical: 6),
//             //         decoration: BoxDecoration(color: AppColor.surface4, borderRadius: BorderRadius.circular(16)),
//             //         child: Row(
//             //           mainAxisAlignment: MainAxisAlignment.spaceEvenly,
//             //           children: [
//             //             Text('تسديد الأول', style: AppTextStyle.labelLarge.withColor(AppColor.text2)),
//             //             Text('2024 / 1 / 10', style: AppTextStyle.numberSmall.withColor(AppColor.text2)),
//             //             Text(
//             //               '${TextUtil.addComma((index + 100000).toString())} د.ع',
//             //               style: AppTextStyle.labelLarge.withColor(AppColor.text2),
//             //             ),
//             //           ],
//             //         ),
//             //       ),
//             //     );
//             //   },
//             // ),
//             // ),
//             // );
//           }
//           return const DefaultLoadingWidget();
//         },
//       ),
//     );
//   }
// }
//
// class PrintDetails {
//   final PrintType printType;
//   final int? orderId;
//
//   PrintDetails({required this.printType, this.orderId});
// }
//
