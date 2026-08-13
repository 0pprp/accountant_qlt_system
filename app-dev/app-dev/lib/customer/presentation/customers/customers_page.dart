import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/custom_app_bar_filter.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/debouncer.dart';
import 'package:team/customer/infrastructure/repositories/customer_repository.dart';
import 'package:team/customer/presentation/customers/bloc/customers_bloc.dart';
import 'package:team/customer/presentation/customers/widgets/customer_info.dart';
import 'package:team/payment/mapper.dart';
import 'package:vector_graphics/vector_graphics.dart';

class CustomersPage extends StatelessWidget {
  final PageController? pageController;
  final ValueNotifier<int>? currentPage;
  final bool isSearching;
  const CustomersPage({super.key, this.pageController, this.currentPage, this.isSearching = false});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create:
          (BuildContext context) => CustomersBloc(
            customerRepository: GetIt.I.get<CustomerRepository>(),
            paymentMapper: GetIt.I.get<PaymentMapper>(),
          ),
      child: CustomersPageView(pageController: pageController, currentPage: currentPage, isSearching: isSearching),
    );
  }
}

class CustomersPageView extends StatefulWidget {
  final PageController? pageController;
  final ValueNotifier<int>? currentPage;
  final bool isSearching;
  const CustomersPageView({
    super.key,
    required this.pageController,
    required this.currentPage,
    required this.isSearching,
  });

  @override
  State<CustomersPageView> createState() => _CustomersPageViewState();
}

class _CustomersPageViewState extends State<CustomersPageView> with AutomaticKeepAliveClientMixin {
  late final _bloc = context.read<CustomersBloc>();
  final _scrollController = ScrollController();
  final _searchController = TextEditingController();
  final _searchDebouncer = Debouncer(milliseconds: 300);

  @override
  void initState() {
    if (!widget.isSearching) {
      _bloc.add(InitialCustomersEvent());
    } else {
      _bloc.add(GetRecentSearches());
    }
    super.initState();
  }

  void _clearFilters() {
    _bloc.selectedOrderItem = null;
    _bloc.startDate = null;
    _bloc.endDate = null;
    _bloc.add(InitialCustomersEvent());
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return Scaffold(
      appBar: PreferredSize(
        preferredSize: Size.fromHeight(70),
        child: DefaultBuilder<CustomersBloc>(
          buildWhen: (previous, state) => state.event is InitialCustomersEvent,
          builder: (context, state) {
            return DefaultAppBar(
              title:
                  _bloc.selectedOrderItem?.name == null && _bloc.startDate == null && _bloc.endDate == null
                      ? 'العملاء'
                      : null,
              customTitle:
                  _bloc.selectedOrderItem?.name != null || _bloc.startDate != null || _bloc.endDate != null
                      ? CustomAppBarFilter(
                        selectedOrderItem: _bloc.selectedOrderItem,
                        startDate: _bloc.startDate,
                        endDate: _bloc.endDate,
                      )
                      : null,
              firstIcon:
                  widget.isSearching
                      ? SizedBox(width: 48)
                      : IconButton(
                        onPressed: () => Navigator.pushNamed(context, RouteNames.customersSearch),
                        icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/search_icon.svg')),
                      ),
              onBackPressed: () {
                widget.pageController?.animateToPage(
                  2,
                  duration: Duration(milliseconds: 500),
                  curve: Curves.easeOut,
                );
                widget.currentPage?.value = 2;
                if (widget.pageController == null) {
                  Navigator.maybePop(context);
                }
              },
            );
          },
        ),
      ),
      body: DefaultBuilder<CustomersBloc>(
        buildWhen:
            (previous, state) =>
                (!widget.isSearching && state.event is InitialCustomersEvent) || (state.event is GetRecentSearches),
        builder: (context, state) {
          if (state is ErrorState) {
            return DefaultErrorWidget(error: state.error, onRetry: () => _bloc.add(InitialCustomersEvent()));
          }
          if (state is ResponseState || _bloc.customers != null) {
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
                                  _bloc.add(InitialCustomersEvent());
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
                  DefaultBuilder<CustomersBloc>(
                    buildWhen: (previous, state) => widget.isSearching,
                    builder: (context, state) {
                      if (_bloc.searchedCustomers != null) return SizedBox();
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
                                  // IconButton(
                                  //   onPressed: _showCustomerFiltersBottomSheet,
                                  //   icon: VectorGraphic(
                                  //     loader: AssetBytesLoader('assets/svg/filter_icon.svg'),
                                  //     width: 16,
                                  //     height: 16,
                                  //   ),
                                  //   constraints: BoxConstraints(maxHeight: 32, maxWidth: 32),
                                  // ),
                                  DefaultBuilder<CustomersBloc>(
                                    builder: (context, state) {
                                      if (_bloc.selectedOrderItem?.name != null ||
                                          _bloc.startDate != null ||
                                          _bloc.endDate != null) {
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
                  DefaultBuilder<CustomersBloc>(
                    buildWhen: (previous, state) => widget.isSearching || state.event is InitialCustomersEvent,
                    builder: (context, state) {
                      return Expanded(
                        child: Column(
                          children: [
                            if (widget.isSearching &&
                                _bloc.recentSearches != null &&
                                _bloc.searchedCustomers == null &&
                                (state.event is! InitialCustomersEvent && state is! LoadingState))
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
                                        return CustomerInfo(
                                          customer: _bloc.recentSearches![index],
                                          onTap:
                                              () => Navigator.pushNamed(
                                                context,
                                                RouteNames.customerDetails,
                                                arguments: _bloc.recentSearches![index].id,
                                              ),
                                        );
                                      },
                                    ),
                                  );
                                },
                              ),
                            if (!widget.isSearching && _bloc.customers != null)
                              Builder(
                                builder: (context) {
                                  if (_bloc.customers?.isEmpty ?? false) {
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
                                        _bloc.add(InitialCustomersEvent());
                                      },
                                      color: AppColor.primary,
                                      backgroundColor: Colors.white,
                                      child: ListView.builder(
                                        physics: AlwaysScrollableScrollPhysics(),
                                        controller: _scrollController,
                                        padding: EdgeInsets.only(right: 24, left: 24, bottom: 100),
                                        itemCount: _bloc.customers!.length + 1,
                                        itemBuilder: (context, index) {
                                          if (index == _bloc.customers!.length) {
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
                                          return CustomerInfo(
                                            customer: _bloc.customers![index],
                                            onTap:
                                                () => Navigator.pushNamed(
                                                  context,
                                                  RouteNames.customerDetails,
                                                  arguments: _bloc.customers![index].id,
                                                ),
                                          );
                                        },
                                      ),
                                    ),
                                  );
                                },
                              ),
                            if (_bloc.searchedCustomers != null ||
                                (state is LoadingState && state.event is InitialCustomersEvent))
                              Builder(
                                builder: (context) {
                                  if (state is LoadingState && _bloc.searchedCustomers == null) {
                                    return DefaultLoadingWidget();
                                  }
                                  if (_bloc.searchedCustomers?.isEmpty ?? false) {
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
                                      itemCount: _bloc.searchedCustomers!.length + 1,
                                      itemBuilder: (context, index) {
                                        if (index == _bloc.searchedCustomers!.length) {
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
                                        return CustomerInfo(
                                          customer: _bloc.searchedCustomers![index],
                                          onTap: () {
                                            if (widget.isSearching) {
                                              _bloc.add(AddRecentSearch(_bloc.searchedCustomers![index]));
                                            }
                                            Navigator.pushNamed(
                                              context,
                                              RouteNames.customerDetails,
                                              arguments: _bloc.searchedCustomers![index].id,
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
    );
  }

  @override
  bool get wantKeepAlive => true;
}
