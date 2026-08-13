import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:open_file/open_file.dart';
import 'package:path_provider/path_provider.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/services/request/request.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/custom_app_bar_filter.dart';
import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_calendar.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/ui/widgets/payment_info_card.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/debouncer.dart';
import 'package:team/common/utils/helpers/text_util.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';
import 'package:team/home/presentation/main/widgets/mandob_motaba_page_wrapper_scaffold.dart';
import 'package:team/payment/infrastructure/repositories/payment_repository.dart';
import 'package:team/payment/presentation/payments/bloc/payments_bloc.dart';
import 'package:vector_graphics/vector_graphics.dart';

class PaymentsPage extends StatelessWidget {
  final PageController? pageController;
  final ValueNotifier<int>? currentPage;
  final bool isSearching;
  final int? orderListId;
  const PaymentsPage({super.key, this.pageController, this.currentPage, this.isSearching = false, this.orderListId});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create: (BuildContext context) => PaymentsBloc(paymentRepository: GetIt.I.get<PaymentRepository>()),
      child: PaymentsPageView(
        pageController: pageController,
        currentPage: currentPage,
        isSearching: isSearching,
        orderListId: orderListId,
      ),
    );
  }
}

class PaymentsPageView extends StatefulWidget {
  final PageController? pageController;
  final ValueNotifier<int>? currentPage;
  final bool isSearching;
  final int? orderListId;
  const PaymentsPageView({
    super.key,
    required this.pageController,
    required this.currentPage,
    required this.isSearching,
    this.orderListId,
  });

  @override
  State<PaymentsPageView> createState() => _PaymentsPageViewState();
}

class _PaymentsPageViewState extends State<PaymentsPageView> with AutomaticKeepAliveClientMixin {
  late final _bloc = context.read<PaymentsBloc>();
  final _selectedCategory = ValueNotifier<ExpansionOption?>(null);
  final _scrollController = ScrollController();
  final categoriesExpansionTileController = ExpansionTileController();
  final _startDate = ValueNotifier<DateTime?>(null);
  final _endDate = ValueNotifier<DateTime?>(null);
  final _searchController = TextEditingController();
  final _searchDebouncer = Debouncer(milliseconds: 300);
  final printPageSize = ValueNotifier<ExpansionOption>(ExpansionOption(id: 1, name: 'A4'));
  final printDirectionSize = ValueNotifier<ExpansionOption>(ExpansionOption(id: 0, name: 'عامودي'));
  final printPageSizeExpansionTileController = ExpansionTileController();
  final printDirectionExpansionTileController = ExpansionTileController();

  @override
  void initState() {
    _bloc.orderListId = widget.orderListId;
    _scrollController.addListener(() {
      if (_scrollController.offset >= _scrollController.position.maxScrollExtent - 250) {
        if (!((_bloc.state.event is InitialPaymentsEvent && _bloc.state is LoadingState) ||
            _scrollController.position.userScrollDirection != ScrollDirection.reverse)) {
          final lastUpdatedAt =
              _bloc.searchedInstallmentPayments == null
                  ? _bloc.installmentPayments?.lastOrNull?.lastUpdatedAt
                  : _bloc.searchedInstallmentPayments?.lastOrNull?.lastUpdatedAt;
          if (lastUpdatedAt != null) {
            _bloc.add(InitialPaymentsEvent(lastId: lastUpdatedAt));
          }
        }
      }
    });
    if (!widget.isSearching) {
      if (!GetIt.I.get<AccountMapper>().isMotaba) {
        _bloc.add(InitialPaymentsEvent());
      }
    } else {
      _bloc.add(GetRecentSearches());
    }
    super.initState();
  }

  void _clearFilters({bool refresh = true}) {
    _startDate.value = null;
    _endDate.value = null;
    _selectedCategory.value = null;
    _bloc.startDate = null;
    _bloc.endDate = null;
    if (refresh) {
      _bloc.add(InitialPaymentsEvent(lastId: null));
    }
  }

  void _showPaymentFiltersBottomSheet() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.white,
      builder: (context) {
        return DraggableScrollableSheet(
          initialChildSize: 0.7,
          maxChildSize: 0.9,
          expand: false,
          snap: false,
          minChildSize: 0.4,
          builder:
              (context, scrollController) => ListView(
                controller: scrollController,
                shrinkWrap: true,
                padding: EdgeInsets.all(24),
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
                    padding: const EdgeInsets.only(top: 32),
                    child: Text('التاريخ', style: AppTextStyle.headlineSmall.withColor(AppColor.text2)),
                  ),
                  Padding(
                    padding: const EdgeInsets.only(bottom: 16, top: 8),
                    child: ValueListenableBuilder(
                      valueListenable: _endDate,
                      builder:
                          (context, end, child) => ValueListenableBuilder(
                            valueListenable: _startDate,
                            builder:
                                (context, start, child) =>
                                    start == null
                                        ? SizedBox()
                                        : ListTile(
                                          shape: const ContinuousRectangleBorder(
                                            borderRadius: BorderRadius.all(Radius.circular(16)),
                                            side: BorderSide(color: AppColor.stroke, width: 2),
                                          ),
                                          title: Text(
                                            '${DateFormat('d MMM yyyy').format(start)}${end == null ? '' : ' - ${DateFormat('d MMM yyyy').format(end)}'}',
                                            textDirection: TextDirection.ltr,
                                            textAlign: TextAlign.center,
                                            style: AppTextStyle.numberLarge,
                                          ),
                                        ),
                          ),
                    ),
                  ),
                  DefaultCalendar(
                    startDate: _startDate,
                    endDate: _endDate,
                    onValueChanged: () {
                      _bloc.startDate = _startDate.value;
                      _bloc.endDate = _endDate.value ?? _startDate.value;
                    },
                  ),
                  SizedBox(height: 32),
                  Row(
                    children: [
                      Expanded(
                        child: FilledButton(
                          onPressed: () {
                            _bloc.add(InitialPaymentsEvent(lastId: null));
                            Navigator.pop(context);
                          },
                          child: Text('تحديد'),
                        ),
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
    ).then((value) {
      if (value == true) {
        _clearFilters();
      }
    });
  }

  Future<void> _downloadPdf(bool showPaidInstallments) async {
    // Get app-specific directory — no permission needed on any Android version
    Directory downloadsDir;

    if (Platform.isAndroid) {
      downloadsDir = await getExternalStorageDirectory() ?? await getApplicationDocumentsDirectory();
    } else if (Platform.isIOS) {
      downloadsDir = await getApplicationDocumentsDirectory();
    } else {
      throw Exception('Unsupported platform');
    }

    // Create filename with timestamp to avoid conflicts
    final timestamp = DateTime.now().millisecondsSinceEpoch;
    final fileName = 'daily_report_${showPaidInstallments ? 'paid' : 'not_paid'}_$timestamp.pdf';
    final savePath = '${downloadsDir.path}/$fileName';

    // Download the file
    await GetIt.I.get<Request>().download(
      'api/v1/app/InstallmentPayments/daily-pdf-report?showPaidInstallments=$showPaidInstallments&size=${printPageSize.value.id}&Orientation=${printDirectionSize.value.id}',
      savePath,
    );

    // Open the downloaded file
    final result = await OpenFile.open(savePath);

    if (result.type != ResultType.done) {
      throw Exception('Could not open the downloaded file: ${result.message}');
    }
  }

  Future<void> _generatePdf(bool showPaidInstallments) async {
    Navigator.pop(context);
    try {
      // Show loading indicator
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (context) => const Center(child: CircularProgressIndicator()),
      );

      // Generate PDF
      final pdf = await _downloadPdf(showPaidInstallments);

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

  void _showPrintSettingsBottomSheet(bool showPaidInstallments) {
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
                  Expanded(
                    child: FilledButton(onPressed: () => _generatePdf(showPaidInstallments), child: Text('تصدير')),
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

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return DefaultListener(
      bloc: GetIt.I.get<MainBloc>(),
      listener: (context, state) {
        if (state is OrderPayment || state is OrderCreated) {
          _bloc.add(InitialPaymentsEvent());
        }
      },
      child: MandobMotabaPageWrapperScaffold(
        listId: widget.orderListId,
        showBody: widget.isSearching,
        appBarBuilder:
            (goBackToGrid, orderListId) => PreferredSize(
              preferredSize: Size.fromHeight(70),
              child: DefaultBuilder(
                bloc: _bloc,
                buildWhen: (previous, state) => state.event is InitialPaymentsEvent,
                builder: (context, state) {
                  return DefaultAppBar(
                    title: _bloc.startDate == null && _bloc.endDate == null ? 'تسديدات' : null,
                    customTitle:
                        _bloc.startDate != null || _bloc.endDate != null
                            ? CustomAppBarFilter(
                              startDate: _bloc.startDate,
                              endDate: _bloc.endDate,
                            )
                            : null,
                    firstIcon:
                        widget.isSearching
                            ? SizedBox(width: 48)
                            : IconButton(
                              onPressed:
                                  () => Navigator.pushNamed(context, RouteNames.paymentsSearch, arguments: orderListId),
                              icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/search_icon.svg')),
                            ),
                    onBackPressed: () {
                      if (widget.pageController == null) {
                        Navigator.maybePop(context);
                        return;
                      }
                      _clearFilters(refresh: false);
                      if (_bloc.orderListId != null) {
                        _bloc.orderListId = null;
                        _bloc.installmentPayments = null;
                        _bloc.summary = null;
                        goBackToGrid();
                        return;
                      }
                      widget.pageController?.animateToPage(
                        2,
                        duration: Duration(milliseconds: 500),
                        curve: Curves.easeOut,
                      );
                      widget.currentPage?.value = 2;
                    },
                  );
                },
              ),
            ),
        onOrderListSelected: (orderListId) {
          _bloc.add(InitialPaymentsEvent(orderListId: orderListId));
        },
        builder:
            (orderListId) => DefaultBuilder<PaymentsBloc>(
              buildWhen:
                  (previous, state) =>
                      (!widget.isSearching && state.event is InitialPaymentsEvent) ||
                      (state.event is GetRecentSearches),
              builder: (context, state) {
                if (state is ErrorState) {
                  return DefaultErrorWidget(error: state.error, onRetry: () => _bloc.add(InitialPaymentsEvent()));
                }
                if (state is ResponseState || _bloc.installmentPayments != null) {
                  return Animate(
                    effects: [
                      SlideEffect(
                        begin: Offset(0, 1.2),
                        duration: Duration(milliseconds: 1500),
                        end: Offset(0, 0),
                        curve: ElasticOutCurve(1.5),
                      ),
                      FadeEffect(begin: 0, end: 1, duration: Duration(milliseconds: 1500), curve: Curves.easeOut),
                    ],
                    child: Column(
                      children: [
                        if (_bloc.summary != null && !widget.isSearching)
                          Container(
                            margin: EdgeInsets.only(left: 24, right: 24, top: 24),
                            decoration: BoxDecoration(borderRadius: BorderRadius.circular(16)),
                            child: ClipRRect(
                              borderRadius: BorderRadius.circular(16),
                              child: Stack(
                                alignment: Alignment.center,
                                children: [
                                  Positioned.fill(
                                    child: VectorGraphic(
                                      loader: AssetBytesLoader('assets/svg/row_background.svg'),
                                      fit: BoxFit.cover,
                                    ),
                                  ),
                                  Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      Container(
                                        margin: EdgeInsets.only(right: 16, top: 16, bottom: 16),
                                        padding: EdgeInsets.symmetric(vertical: 10, horizontal: 16),
                                        decoration: BoxDecoration(
                                          color: AppColor.surface2,
                                          borderRadius: BorderRadius.circular(16),
                                        ),
                                        child: Row(
                                          children: [
                                            Text(
                                              TextUtil.addComma(_bloc.summary!.totalCount.toString()),
                                              style: AppTextStyle.numberLarge,
                                            ),
                                            Padding(
                                              padding: const EdgeInsets.only(right: 8, top: 2),
                                              child: Text(
                                                'تسديد',
                                                style: AppTextStyle.bodyLarge.withColor(AppColor.text2),
                                              ),
                                            ),
                                          ],
                                        ),
                                      ),
                                      Expanded(
                                        child: Container(
                                          margin: EdgeInsets.only(left: 16, right: 8),
                                          padding: EdgeInsets.symmetric(vertical: 10),
                                          decoration: BoxDecoration(
                                            color: AppColor.surface2,
                                            borderRadius: BorderRadius.circular(16),
                                          ),
                                          child: Center(
                                            child: Row(
                                              mainAxisSize: MainAxisSize.min,
                                              children: [
                                                Text(
                                                  TextUtil.addComma(_bloc.summary!.totalAmount.toString()),
                                                  style: AppTextStyle.numberLarge,
                                                ),
                                                Padding(
                                                  padding: const EdgeInsets.only(right: 4, top: 2),
                                                  child: Text(
                                                    'د.ع',
                                                    style: AppTextStyle.bodyLarge.withColor(AppColor.text2),
                                                  ),
                                                ),
                                              ],
                                            ),
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),
                                ],
                              ),
                            ),
                          ),
                        if (!widget.isSearching)
                          Padding(
                            padding: const EdgeInsets.only(top: 16, right: 24, left: 24),
                            child: Row(
                              children: [
                                Expanded(
                                  child: FilledButton(
                                    onPressed: () => _showPrintSettingsBottomSheet(true),
                                    style: FilledButton.styleFrom(
                                      backgroundColor: AppColor.primary,
                                      foregroundColor: AppColor.white,
                                      padding: EdgeInsets.zero,
                                    ),
                                    child: Stack(
                                      alignment: Alignment.center,
                                      fit: StackFit.expand,
                                      children: [
                                        ClipRRect(
                                          borderRadius: BorderRadius.circular(16),
                                          child: Transform.flip(
                                            flipX: true,
                                            child: VectorGraphic(
                                              loader: AssetBytesLoader('assets/svg/button_background.svg'),
                                              fit: BoxFit.fill,
                                              colorFilter: ColorFilter.mode(Colors.white, BlendMode.srcIn),
                                            ),
                                          ),
                                        ),
                                        Center(
                                          child: Text(
                                            'المسددين',
                                            style: AppTextStyle.bodySmall.withColor(AppColor.white),
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                ),
                                SizedBox(width: 24),
                                Expanded(
                                  child: FilledButton(
                                    onPressed: () => _showPrintSettingsBottomSheet(false),
                                    style: FilledButton.styleFrom(
                                      backgroundColor: AppColor.primary,
                                      foregroundColor: AppColor.white,
                                      padding: EdgeInsets.zero,
                                    ),
                                    child: Stack(
                                      alignment: Alignment.center,
                                      fit: StackFit.expand,
                                      children: [
                                        ClipRRect(
                                          borderRadius: BorderRadius.circular(16),
                                          child: Transform.flip(
                                            flipX: true,
                                            child: VectorGraphic(
                                              loader: AssetBytesLoader('assets/svg/button_background.svg'),
                                              fit: BoxFit.fill,
                                              colorFilter: ColorFilter.mode(Colors.white, BlendMode.srcIn),
                                            ),
                                          ),
                                        ),
                                        Center(
                                          child: Text(
                                            'غير المسددين',
                                            style: AppTextStyle.bodySmall.withColor(AppColor.white),
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        if (widget.isSearching)
                          Padding(
                            padding: const EdgeInsets.only(top: 24, right: 24, left: 24),
                            child: ValueListenableBuilder(
                              valueListenable: _searchController,
                              builder:
                                  (context, value, child) => DefaultTextField(
                                    textDirection: TextDirection.rtl,
                                    textEditingController: _searchController,
                                    textInputAction: TextInputAction.done,
                                    fillColor: AppColor.surface2,
                                    filled: true,
                                    hint: 'البحث',
                                    onChange: (text) {
                                      if (text.trim().isEmpty) {
                                        _bloc.add(GetRecentSearches());
                                        return;
                                      }
                                      _searchDebouncer.run(() {
                                        _bloc.searchText = text.trim();
                                        _bloc.add(InitialPaymentsEvent(lastId: null));
                                      });
                                    },
                                    suffixIcon:
                                        value.text.trim().isNotEmpty
                                            ? GestureDetector(
                                              onTap: () {
                                                _searchController.clear();

                                                _bloc.add(GetRecentSearches());
                                              },
                                              child: Icon(Icons.close_rounded, size: 24),
                                            )
                                            : SizedBox(
                                              width: 24,
                                              height: 24,
                                              child: Center(
                                                child: VectorGraphic(
                                                  loader: AssetBytesLoader('assets/svg/search_outlined_icon.svg'),
                                                ),
                                              ),
                                            ),
                                  ),
                            ),
                          ),
                        DefaultBuilder<PaymentsBloc>(
                          buildWhen: (previous, state) => widget.isSearching,
                          builder: (context, state) {
                            if (_bloc.searchedInstallmentPayments != null) return SizedBox();
                            return Padding(
                              padding: const EdgeInsets.only(top: 16, right: 32, left: 30),
                              child: Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: [
                                  Text(
                                    widget.isSearching ? 'البحث مؤخرا' : 'المسددة مؤخرا',
                                    style: AppTextStyle.headlineMedium.withColor(AppColor.text2),
                                  ),

                                  if (!widget.isSearching)
                                    Row(
                                      children: [
                                        IconButton(
                                          onPressed: _showPaymentFiltersBottomSheet,
                                          icon: VectorGraphic(
                                            loader: AssetBytesLoader('assets/svg/filter_icon.svg'),
                                            width: 16,
                                            height: 16,
                                          ),
                                          constraints: BoxConstraints(maxHeight: 32, maxWidth: 32),
                                        ),
                                        DefaultBuilder<PaymentsBloc>(
                                          builder: (context, state) {
                                            if (_bloc.startDate != null || _bloc.endDate != null) {
                                              return Padding(
                                                padding: const EdgeInsets.only(right: 8),
                                                child: IconButton(
                                                  onPressed: _clearFilters,
                                                  icon: Icon(Icons.close_rounded, size: 16),
                                                  constraints: BoxConstraints(maxHeight: 32, maxWidth: 32),
                                                ),
                                              );
                                            }
                                            return SizedBox();
                                          },
                                        ),
                                      ],
                                    ),
                                  if (widget.isSearching && (_bloc.recentSearches?.isNotEmpty ?? true))
                                    IconButton(
                                      onPressed: () => _bloc.add(ClearRecentSearches()),
                                      iconSize: 16,
                                      icon: Icon(Icons.close_rounded, size: 16, color: AppColor.text1),
                                      constraints: BoxConstraints(maxHeight: 32, maxWidth: 32),
                                    ),
                                ],
                              ),
                            );
                          },
                        ),
                        SizedBox(height: 16),
                        if (widget.isSearching && _bloc.recentSearches == null)
                          Padding(
                            padding: const EdgeInsets.only(top: 32),
                            child: Text(
                              'القائمة فارغة حالياً!',
                              style: AppTextStyle.bodyMedium.copyWith(color: AppColor.text2),
                            ),
                          ),
                        DefaultBuilder<PaymentsBloc>(
                          buildWhen: (previous, state) => widget.isSearching || state.event is InitialPaymentsEvent,
                          builder: (context, state) {
                            return Expanded(
                              child: Column(
                                children: [
                                  if (widget.isSearching &&
                                      _bloc.recentSearches != null &&
                                      _bloc.searchedInstallmentPayments == null &&
                                      (state.event is! InitialPaymentsEvent && state is! LoadingState))
                                    Builder(
                                      builder: (context) {
                                        if (_bloc.recentSearches?.isEmpty ?? false) {
                                          return Padding(
                                            padding: const EdgeInsets.only(top: 32),
                                            child: Text(
                                              'القائمة فارغة حالياً!',
                                              style: AppTextStyle.bodyMedium.copyWith(color: AppColor.text2),
                                            ),
                                          );
                                        }
                                        return Expanded(
                                          child: ListView.builder(
                                            padding: EdgeInsets.only(right: 24, left: 24, bottom: 100),
                                            itemCount: _bloc.recentSearches!.length,
                                            itemBuilder: (context, index) {
                                              return Padding(
                                                padding: const EdgeInsets.only(bottom: 8),
                                                child: PaymentInfoCard(
                                                  installmentPayment: _bloc.recentSearches![index],
                                                  onTap: () {
                                                    Navigator.pushNamed(
                                                      context,
                                                      RouteNames.customerPaymentDetails,
                                                      arguments: _bloc.recentSearches![index].orderId,
                                                    );
                                                  },
                                                ),
                                              );
                                            },
                                          ),
                                        );
                                      },
                                    ),
                                  if (!widget.isSearching && _bloc.installmentPayments != null)
                                    Builder(
                                      builder: (context) {
                                        if (_bloc.installmentPayments?.isEmpty ?? false) {
                                          return Padding(
                                            padding: const EdgeInsets.only(top: 32),
                                            child: Text(
                                              'القائمة فارغة حالياً!',
                                              style: AppTextStyle.bodyMedium.copyWith(color: AppColor.text2),
                                            ),
                                          );
                                        }
                                        return Expanded(
                                          child: RefreshIndicator(
                                            onRefresh: () async {
                                              setState(() {
                                                _bloc.add(InitialPaymentsEvent());
                                              });
                                            },
                                            displacement: 20,
                                            color: AppColor.primary,
                                            backgroundColor: Colors.white,
                                            child: ListView.builder(
                                              controller: _scrollController,
                                              physics: AlwaysScrollableScrollPhysics(),
                                              padding: EdgeInsets.only(right: 24, left: 24, bottom: 100),
                                              itemCount: _bloc.installmentPayments!.length + 1,
                                              itemBuilder: (context, index) {
                                                if (index == _bloc.installmentPayments!.length) {
                                                  if (state is LoadingState) {
                                                    return DefaultLoadingWidget(
                                                      color: AppColor.primary,
                                                      height: 50,
                                                      width: 50,
                                                    );
                                                  }
                                                  if (state is ErrorState) {
                                                    return DefaultErrorWidget(error: state.error);
                                                  }
                                                  return SizedBox();
                                                }
                                                return Padding(
                                                  padding: const EdgeInsets.only(bottom: 8),
                                                  child: PaymentInfoCard(
                                                    installmentPayment: _bloc.installmentPayments![index],
                                                    onTap: () {
                                                      Navigator.pushNamed(
                                                        context,
                                                        RouteNames.customerPaymentDetails,
                                                        arguments: _bloc.installmentPayments![index].orderId,
                                                      );
                                                    },
                                                  ),
                                                );
                                              },
                                            ),
                                          ),
                                        );
                                      },
                                    ),
                                  if (_bloc.searchedInstallmentPayments != null ||
                                      (state is LoadingState && state.event is InitialPaymentsEvent))
                                    Builder(
                                      builder: (context) {
                                        if (state is LoadingState && _bloc.searchedInstallmentPayments == null) {
                                          return DefaultLoadingWidget();
                                        }
                                        if (_bloc.searchedInstallmentPayments?.isEmpty ?? false) {
                                          return Padding(
                                            padding: const EdgeInsets.only(top: 32),
                                            child: Text(
                                              'القائمة فارغة حالياً!',
                                              style: AppTextStyle.bodyMedium.copyWith(color: AppColor.text2),
                                            ),
                                          );
                                        }

                                        return Expanded(
                                          child: ListView.builder(
                                            controller: _scrollController,
                                            padding: EdgeInsets.only(right: 24, left: 24, bottom: 100),
                                            itemCount: _bloc.searchedInstallmentPayments!.length + 1,
                                            itemBuilder: (context, index) {
                                              if (index == _bloc.searchedInstallmentPayments!.length) {
                                                if (state is LoadingState) {
                                                  return DefaultLoadingWidget(
                                                    color: AppColor.primary,
                                                    height: 50,
                                                    width: 50,
                                                  );
                                                }
                                                if (state is ErrorState) {
                                                  return DefaultErrorWidget(error: state.error);
                                                }
                                                return SizedBox();
                                              }
                                              return Padding(
                                                padding: const EdgeInsets.only(bottom: 8),
                                                child: PaymentInfoCard(
                                                  installmentPayment: _bloc.searchedInstallmentPayments![index],
                                                  onTap: () {
                                                    if (widget.isSearching) {
                                                      _bloc.add(
                                                        AddRecentSearch(_bloc.searchedInstallmentPayments![index]),
                                                      );
                                                    }
                                                    Navigator.pushNamed(
                                                      context,
                                                      RouteNames.customerPaymentDetails,
                                                      arguments: _bloc.searchedInstallmentPayments![index].orderId,
                                                    );
                                                  },
                                                ),
                                              );
                                            },
                                          ),
                                        );
                                      },
                                    ),
                                ],
                              ),
                            );
                          },
                        ),
                      ],
                    ),
                  );
                }
                return const DefaultLoadingWidget();
              },
            ),
      ),
    );
  }

  @override
  bool get wantKeepAlive => true;
}
