import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';
import 'package:team/home/presentation/main/widgets/order_list_info.dart';
import 'package:team/order/domain/order_list/order_list.dart';

class MandobMotabaPageWrapperScaffold extends StatefulWidget {
  final Widget Function(int? orderListId) builder;
  final Widget Function(VoidCallback goBackToGrid, int? orderListId)? appBarBuilder;
  final void Function(int orderListId)? onOrderListSelected;
  final int? listId;
  final bool showBody;

  const MandobMotabaPageWrapperScaffold({
    super.key,
    required this.builder,
    this.appBarBuilder,
    this.onOrderListSelected,
    this.listId,
    this.showBody = false,
  });

  @override
  State<MandobMotabaPageWrapperScaffold> createState() => _MandobMotabaPageWrapperScaffoldState();
}

class _MandobMotabaPageWrapperScaffoldState extends State<MandobMotabaPageWrapperScaffold> {
  late final _bloc = context.read<MainBloc>();
  late final bool _isMotaba = GetIt.I.get<AccountMapper>().isMotaba;

  int? _selectedOrderListId;
  bool _hasSelected = false;

  @override
  void initState() {
    super.initState();
    if (_isMotaba && widget.listId == null) _bloc.add(GetOrderLists());
  }

  void _onOrderListTapped(OrderList order) {
    final id = order.id;
    widget.onOrderListSelected?.call(id);
    setState(() {
      _selectedOrderListId = id;
      _hasSelected = true;
    });
  }

  void _goBackToGrid() {
    setState(() {
      _selectedOrderListId = null;
      _hasSelected = false;
      // if (_isMotaba) _bloc.add(GetOrderLists());
    });
  }

  Widget _buildBody() {
    if (widget.listId != null) {
      return widget.builder(widget.listId);
    }

    if (widget.showBody) {
      return widget.builder(_selectedOrderListId);
    }

    if (!_isMotaba || _hasSelected) {
      return widget.builder(_selectedOrderListId);
    }

    return DefaultBuilder<MainBloc>(
      buildWhen: (_, state) => state.event is GetOrderLists,
      builder: (context, state) {
        final lists = _bloc.orderLists;
        if (lists == null) return const DefaultLoadingWidget();

        return _OrderListGrid(orderLists: lists, onTap: _onOrderListTapped);
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: true,
      onPopInvokedWithResult: (didPop, _) {
        if (_hasSelected) _goBackToGrid();
      },
      child: Scaffold(
        appBar:
            widget.appBarBuilder != null
                ? PreferredSize(
                  preferredSize: const Size.fromHeight(kToolbarHeight),
                  child: widget.appBarBuilder!(
                    _goBackToGrid,
                    widget.listId ?? _selectedOrderListId,
                  ),
                )
                : null,
        body: _buildBody(),
      ),
    );
  }
}

class _OrderListGrid extends StatelessWidget {
  final List<OrderList> orderLists;
  final void Function(OrderList order) onTap;

  _OrderListGrid({required this.orderLists, required this.onTap});

  //total of totalUnpaidInstallmentAmount of each orderList
  late final remainingAmount = orderLists.fold<double>(
    0,
    (previousValue, element) => previousValue + element.totalUnpaidInstallmentAmount,
  );

  @override
  Widget build(BuildContext context) {
    return Animate(
      effects: [
        SlideEffect(
          begin: const Offset(0, 1.2),
          end: Offset.zero,
          duration: const Duration(milliseconds: 1500),
          curve: ElasticOutCurve(1.5),
        ),
        FadeEffect(
          begin: 0,
          end: 1,
          duration: const Duration(milliseconds: 1500),
          curve: Curves.easeOut,
        ),
      ],
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 24),
        child: Column(
          children: [
            //المبلغ المتبقي
            // ReportRow(
            //   "الباقي",
            //   "مجموع الأموال المتبقية",
            //   TextUtil.addComma(remainingAmount.toString()),
            // ),
            Expanded(
              child: RefreshIndicator(
                onRefresh: () async {
                  context.read<MainBloc>().add(GetOrderLists());
                },
                color: AppColor.primary,
                backgroundColor: Colors.white,
                child: GridView.builder(
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    mainAxisExtent: 284,
                    crossAxisSpacing: 16,
                    mainAxisSpacing: 16,
                  ),
                  itemCount: orderLists.length,
                  itemBuilder: (context, index) {
                    final order = orderLists[index];
                    return OrderListInfo(
                      orderList: order,
                      onTap: () => onTap(order),
                    );
                  },
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
