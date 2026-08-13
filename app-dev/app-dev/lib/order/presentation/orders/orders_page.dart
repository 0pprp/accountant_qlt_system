import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:team/account/mapper.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/custom_app_bar_filter.dart';
import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_calendar.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/ui/widgets/multi_select_expansion_tile.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/debouncer.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';
import 'package:team/home/presentation/main/widgets/mandob_motaba_page_wrapper_scaffold.dart';
import 'package:team/order/domain/order_status.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';
import 'package:team/order/presentation/orders/bloc/orders_bloc.dart';
import 'package:team/order/presentation/orders/widgets/order_info.dart';
import 'package:team/payment/mapper.dart';
import 'package:vector_graphics/vector_graphics.dart';

class OrdersPage extends StatelessWidget {
  final PageController? pageController;
  final ValueNotifier<int>? currentPage;
  final bool isSearching;
  final int? orderListId;
  const OrdersPage({super.key, this.pageController, this.currentPage, this.isSearching = false, this.orderListId});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create:
          (BuildContext context) => OrdersBloc(
            orderRepository: GetIt.I.get<OrderRepository>(),
            paymentMapper: GetIt.I.get<PaymentMapper>(),
          ),
      child: OrdersPageView(
        pageController: pageController,
        currentPage: currentPage,
        isSearching: isSearching,
        orderListId: orderListId,
      ),
    );
  }
}

class OrdersPageView extends StatefulWidget {
  final PageController? pageController;
  final ValueNotifier<int>? currentPage;
  final bool isSearching;
  final int? orderListId;
  const OrdersPageView({
    super.key,
    required this.pageController,
    required this.currentPage,
    required this.isSearching,
    this.orderListId,
  });

  @override
  State<OrdersPageView> createState() => _OrdersPageViewState();
}

class _OrdersPageViewState extends State<OrdersPageView> with AutomaticKeepAliveClientMixin {
  late final _bloc = context.read<OrdersBloc>();
  final _selectedCategory = ValueNotifier<ExpansionOption?>(null);
  late final _selectedExecutionStatuses = ValueNotifier<List<ExecutionStatus>>(
    List.of(_bloc.executionStatuses),
  );
  final _startDate = ValueNotifier<DateTime?>(null);
  final _endDate = ValueNotifier<DateTime?>(null);
  final categoriesExpansionTileController = ExpansionTileController();
  final executionStatusesExpansionTileController = ExpansionTileController();
  final _scrollController = ScrollController();
  final _searchController = TextEditingController();
  final _searchDebouncer = Debouncer(milliseconds: 300);

  @override
  void initState() {
    _bloc.orderListId = widget.orderListId;
    if (!widget.isSearching) {
      if (!GetIt.I.get<AccountMapper>().isMotaba) {
        _bloc.add(InitialOrdersEvent());
      }
    } else {
      _bloc.add(GetRecentSearches());
    }
    super.initState();
  }

  @override
  void dispose() {
    _selectedExecutionStatuses.dispose();
    _startDate.dispose();
    _endDate.dispose();
    _selectedCategory.dispose();
    _scrollController.dispose();
    _searchController.dispose();
    _searchDebouncer.dispose();
    super.dispose();
  }

  void _clearFilters({bool refresh = true}) {
    _startDate.value = null;
    _endDate.value = null;
    _selectedCategory.value = null;
    _selectedExecutionStatuses.value = List.of(OrdersBloc.defaultExecutionStatuses);
    _bloc.selectedOrderItem = null;
    _bloc.startDate = null;
    _bloc.endDate = null;
    _bloc.executionStatuses = List.of(OrdersBloc.defaultExecutionStatuses);
    if (refresh) {
      _bloc.add(InitialOrdersEvent());
    }
  }

  void _showOrderFiltersBottomSheet() {
    _selectedExecutionStatuses.value = List.of(_bloc.executionStatuses);

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.white,
      builder: (context) {
        return DraggableScrollableSheet(
          initialChildSize: 0.8,
          maxChildSize: 0.9,
          expand: false,
          snap: false,
          minChildSize: 0.6,
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
                  Text('حالة التنفيذ', style: AppTextStyle.headlineSmall.withColor(AppColor.text2)),
                  Padding(
                    padding: const EdgeInsets.only(top: 8, bottom: 24),
                    child: ExecutionStatusMultiSelectTile(
                      selectedValues: _selectedExecutionStatuses,
                      controller: executionStatusesExpansionTileController,
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.only(top: 8),
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
                            _bloc.executionStatuses = List.of(_selectedExecutionStatuses.value);
                            _bloc.add(InitialOrdersEvent());
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
      executionStatusesExpansionTileController.collapse();
      if (value == true) {
        _clearFilters();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return DefaultListener(
      bloc: GetIt.I.get<MainBloc>(),
      listener: (context, state) {
        if (state is OrderPayment || state is OrderCreated) {
          _bloc.add(InitialOrdersEvent());
        }
      },
      child: MandobMotabaPageWrapperScaffold(
        listId: widget.orderListId,
        showBody: widget.isSearching,
        appBarBuilder:
            (goBackToGrid, orderListId) => PreferredSize(
              preferredSize: Size.fromHeight(70),
              child: DefaultBuilder<OrdersBloc>(
                buildWhen: (previous, state) => state.event is InitialOrdersEvent,
                builder: (context, state) {
                  return DefaultAppBar(
                    title:
                        _bloc.selectedOrderItem?.name == null &&
                                _bloc.startDate == null &&
                                _bloc.endDate == null &&
                                !_bloc.hasNonDefaultExecutionStatuses
                            ? 'المبيعات'
                            : null,
                    customTitle:
                        _bloc.selectedOrderItem?.name != null ||
                                _bloc.startDate != null ||
                                _bloc.endDate != null ||
                                _bloc.hasNonDefaultExecutionStatuses
                            ? CustomAppBarFilter(
                              selectedOrderItem: _bloc.selectedOrderItem,
                              startDate: _bloc.startDate,
                              endDate: _bloc.endDate,
                              executionStatuses: _bloc.hasNonDefaultExecutionStatuses ? _bloc.executionStatuses : null,
                            )
                            : null,
                    firstIcon: Row(
                      mainAxisSize: MainAxisSize.min,
                      spacing: 8,
                      children: [
                        if (GetIt.I.get<AccountMapper>().permissions?.order?.canCreate ?? false)
                          IconButton(
                            onPressed: () async {
                              await Navigator.pushNamed(
                                context,
                                RouteNames.createOrder,
                                arguments: {'order': null, 'currentOrderListId': orderListId},
                              );
                              if (orderListId != null || !GetIt.I.get<AccountMapper>().isMotaba) {
                                _bloc.add(InitialOrdersEvent());
                              } else {
                                if (context.mounted) {
                                  context.read<MainBloc>().add(GetOrderLists());
                                }
                              }
                            },
                            icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/add_circle_icon.svg')),
                          ),
                        // if (orderListId != null)
                        widget.isSearching
                            ? SizedBox(width: 0)
                            : IconButton(
                              onPressed:
                                  () => Navigator.pushNamed(context, RouteNames.ordersSearch, arguments: orderListId),
                              icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/search_icon.svg')),
                            ),
                      ],
                    ),
                    onBackPressed: () {
                      if (widget.pageController == null) {
                        Navigator.pop(context);
                        return;
                      }
                      _clearFilters(refresh: false);
                      if (_bloc.orderListId != null) {
                        _bloc.orderListId = null;
                        _bloc.orders = null;
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
          _bloc.add(InitialOrdersEvent(orderListId: orderListId));
        },
        builder:
            (orderListId) => DefaultBuilder<OrdersBloc>(
              buildWhen:
                  (previous, state) =>
                      (!widget.isSearching && state.event is InitialOrdersEvent) || (state.event is GetRecentSearches),
              builder: (context, state) {
                if (state is ErrorState) {
                  return DefaultErrorWidget(error: state.error, onRetry: () => _bloc.add(InitialOrdersEvent()));
                }
                if (state is ResponseState || _bloc.orders != null) {
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
                                        _bloc.add(InitialOrdersEvent());
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
                        DefaultBuilder<OrdersBloc>(
                          buildWhen: (previous, state) => widget.isSearching,
                          builder: (context, state) {
                            if (_bloc.searchedOrders != null) return SizedBox();
                            return Padding(
                              padding: const EdgeInsets.only(top: 16, right: 32, left: 30),
                              child: Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: [
                                  Text(
                                    widget.isSearching ? 'البحث مؤخرا' : 'المضاف مؤخرا',
                                    style: AppTextStyle.headlineMedium.withColor(AppColor.text2),
                                  ),
                                  if (!widget.isSearching)
                                    Row(
                                      children: [
                                        IconButton(
                                          onPressed: _showOrderFiltersBottomSheet,
                                          icon: VectorGraphic(
                                            loader: AssetBytesLoader('assets/svg/filter_icon.svg'),
                                            width: 16,
                                            height: 16,
                                          ),
                                          constraints: BoxConstraints(maxHeight: 32, maxWidth: 32),
                                        ),
                                        DefaultBuilder<OrdersBloc>(
                                          builder: (context, state) {
                                            if (_bloc.selectedOrderItem?.name != null ||
                                                _bloc.startDate != null ||
                                                _bloc.endDate != null ||
                                                _bloc.hasNonDefaultExecutionStatuses) {
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
                        DefaultBuilder<OrdersBloc>(
                          buildWhen: (previous, state) => widget.isSearching || state.event is InitialOrdersEvent,
                          builder: (context, state) {
                            return Expanded(
                              child: Column(
                                children: [
                                  if (widget.isSearching &&
                                      _bloc.recentSearches != null &&
                                      _bloc.searchedOrders == null &&
                                      (state.event is! InitialOrdersEvent && state is! LoadingState))
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
                                              return OrderInfo(
                                                order: _bloc.recentSearches![index],
                                                onTap:
                                                    () => Navigator.pushNamed(
                                                      context,
                                                      RouteNames.ordersDetails,
                                                      arguments: {
                                                        'order': _bloc.recentSearches![index],
                                                        'userId': _bloc.recentSearches![index].customer.id,
                                                        'installmentPaymentId':
                                                            _bloc.recentSearches![index].installmentPaymentId,
                                                        'dailyInstallmentAmount':
                                                            _bloc.recentSearches![index].dailyInstallmentAmount,
                                                      },
                                                    ),
                                              );
                                            },
                                          ),
                                        );
                                      },
                                    ),
                                  if (!widget.isSearching && _bloc.orders != null)
                                    Builder(
                                      builder: (context) {
                                        if (_bloc.orders?.isEmpty ?? false) {
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
                                              _bloc.add(InitialOrdersEvent());
                                            },
                                            color: AppColor.primary,
                                            backgroundColor: Colors.white,
                                            child: ListView.builder(
                                              physics: AlwaysScrollableScrollPhysics(),
                                              controller: _scrollController,
                                              padding: EdgeInsets.only(right: 24, left: 24, bottom: 100),
                                              itemCount: _bloc.orders!.length + 1,
                                              itemBuilder: (context, index) {
                                                if (index == _bloc.orders!.length) {
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
                                                return OrderInfo(
                                                  order: _bloc.orders![index],
                                                  onTap:
                                                      () => Navigator.pushNamed(
                                                        context,
                                                        RouteNames.ordersDetails,
                                                        arguments: {
                                                          'order': _bloc.orders![index],
                                                          'userId': _bloc.orders![index].customer.id,
                                                          'installmentPaymentId':
                                                              _bloc.orders![index].installmentPaymentId,
                                                          'dailyInstallmentAmount':
                                                              _bloc.orders![index].dailyInstallmentAmount,
                                                        },
                                                      ),
                                                );
                                              },
                                            ),
                                          ),
                                        );
                                      },
                                    ),
                                  if (_bloc.searchedOrders != null ||
                                      (state is LoadingState && state.event is InitialOrdersEvent))
                                    Builder(
                                      builder: (context) {
                                        if (state is LoadingState && _bloc.searchedOrders == null) {
                                          return DefaultLoadingWidget();
                                        }
                                        if (_bloc.searchedOrders?.isEmpty ?? false) {
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
                                            itemCount: _bloc.searchedOrders!.length + 1,
                                            itemBuilder: (context, index) {
                                              if (index == _bloc.searchedOrders!.length) {
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
                                              return OrderInfo(
                                                order: _bloc.searchedOrders![index],
                                                onTap: () {
                                                  if (widget.isSearching) {
                                                    _bloc.add(AddRecentSearch(_bloc.searchedOrders![index]));
                                                  }
                                                  Navigator.pushNamed(
                                                    context,
                                                    RouteNames.ordersDetails,
                                                    arguments: {
                                                      'order': _bloc.searchedOrders![index],
                                                      'userId': _bloc.searchedOrders![index].customer.id,
                                                      'installmentPaymentId':
                                                          _bloc.searchedOrders![index].installmentPaymentId,
                                                      'dailyInstallmentAmount':
                                                          _bloc.searchedOrders![index].dailyInstallmentAmount,
                                                    },
                                                  );
                                                },
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
