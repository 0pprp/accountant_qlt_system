import 'package:dotted_border/dotted_border.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/show_animated_dialog.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/number_extentions.dart';
import 'package:team/order/domain/order_item_details/order_item_details.dart';
import 'package:team/order/presentation/create_order/bloc/create_order_bloc.dart';
import 'package:vector_graphics/vector_graphics.dart';

import 'add_sale_dialog.dart';

class InstallmentDetailsStepView extends StatefulWidget {
  const InstallmentDetailsStepView({super.key});

  @override
  State<InstallmentDetailsStepView> createState() => _InstallmentDetailsStepViewState();
}

class _InstallmentDetailsStepViewState extends State<InstallmentDetailsStepView> with SingleTickerProviderStateMixin {
  late final _bloc = context.read<CreateOrderBloc>();
  late final _tabController = TabController(length: 2, vsync: this);

  // Add-sale form controllers
  final _itemNameController = TextEditingController();
  final _categoryController = TextEditingController();
  final _quantityController = TextEditingController();
  final _sellPriceController = TextEditingController();
  final _buyPriceController = TextEditingController();
  final _dailyInstallmentController = TextEditingController();
  final _downPaymentController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _bloc.add(GetCustomersPaginatedEvent());
  }

  @override
  void dispose() {
    _tabController.dispose();
    _itemNameController.dispose();
    _categoryController.dispose();
    _quantityController.dispose();
    _sellPriceController.dispose();
    _buyPriceController.dispose();
    _dailyInstallmentController.dispose();
    _downPaymentController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(right: 24, left: 24, top: 24, bottom: 24),
      child: Column(
        children: [
          Container(
            padding: const EdgeInsets.all(0),
            decoration: BoxDecoration(
              color: AppColor.surface1,
              borderRadius: BorderRadius.circular(12),
            ),
            child: TabBar(
              controller: _tabController,
              indicator: BoxDecoration(
                color: AppColor.primary,
                borderRadius: BorderRadius.circular(12),
              ),
              indicatorSize: TabBarIndicatorSize.tab,
              indicatorPadding: const EdgeInsets.symmetric(horizontal: 4),
              dividerColor: Colors.transparent,
              overlayColor: const WidgetStatePropertyAll(Colors.transparent),
              splashBorderRadius: BorderRadius.circular(12),
              labelPadding: EdgeInsets.zero,
              tabs: [
                Tab(
                  height: 40,
                  child: _SideTab(
                    label: 'مبيع من المخزن',
                    icon: 'in_storage_purchase_icon.svg',
                    controller: _tabController,
                    index: 0,
                  ),
                ),
                Tab(
                  height: 40,
                  child: _SideTab(
                    label: 'مبيع خارجي',
                    icon: 'out_storage_purchase_icon.svg',
                    controller: _tabController,
                    index: 1,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 8),
          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                OrderItemsListView(isInStorage: true),
                OrderItemsListView(isInStorage: false),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class OrderItemsListView extends StatelessWidget {
  const OrderItemsListView({
    super.key,
    required bool isInStorage,
  }) : _isInStorage = isInStorage;

  final bool _isInStorage;

  @override
  Widget build(BuildContext context) {
    final bloc = context.read<CreateOrderBloc>();
    return DefaultBuilder<CreateOrderBloc>(
      buildWhen:
          (previous, state) =>
              state.event is DeleteOrderItemEvent ||
              state.event is AddOrderItemEvent ||
              state.event is EditOrderItemEvent,
      builder:
          (context, state) => ListView(
            children: [
              Padding(
                padding: const EdgeInsets.only(top: 16, bottom: 16),
                child: Text(
                  'المبيع:',
                  style: AppTextStyle.headlineMedium.withColor(AppColor.text2),
                ),
              ),
              for (
                int index = 0;
                index < (_isInStorage ? bloc.inStorageOrderItems : bloc.outStorageOrderItems).length;
                index++
              )
                Builder(
                  builder: (context) {
                    final orderItem = (_isInStorage ? bloc.inStorageOrderItems : bloc.outStorageOrderItems)[index];
                    return OrderSummaryView(index: index, orderItem: orderItem);
                  },
                ),
              DottedBorder(
                options: RoundedRectDottedBorderOptions(
                  radius: const Radius.circular(24),
                  dashPattern: const [6, 6],
                  strokeWidth: 1.5,
                  color: AppColor.primary,
                  padding: EdgeInsets.zero,
                ),
                childOnTop: false,
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(24),
                  child: Material(
                    color: AppColor.surface4,
                    child: InkWell(
                      onTap: () {
                        showAnimatedDialog(
                          context: context,
                          child: Center(
                            child: Padding(
                              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 24),
                              child: DefaultBlocProvider.value(
                                value: bloc,
                                child: AddSaleDialog(isInStorage: _isInStorage),
                              ),
                            ),
                          ),
                        );
                      },
                      child: SizedBox(
                        height: 72,
                        child: Center(
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            spacing: 4,
                            children: [
                              VectorGraphic(
                                loader: AssetBytesLoader(
                                  'assets/svg/add_outlines_circle_icon.svg',
                                ),
                              ),
                              Text(
                                'أضافة مبيع',
                                style: AppTextStyle.headlineMedium.withColor(AppColor.primary),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
    );
  }
}

class OrderSummaryView extends StatelessWidget {
  const OrderSummaryView({
    super.key,
    required this.index,
    required this.orderItem,
  });

  final int index;
  final OrderItemDetails orderItem;

  @override
  Widget build(BuildContext context) {
    final bloc = context.read<CreateOrderBloc>();
    return Container(
      margin: EdgeInsets.only(bottom: 16),
      padding: EdgeInsets.symmetric(horizontal: 24, vertical: 16),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColor.stroke),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'المبيع ${index + 1}',
            style: AppTextStyle.bodyMedium.withColor(AppColor.text2),
          ),
          Padding(
            padding: const EdgeInsets.only(top: 6, bottom: 16),
            child: Row(
              children: [
                Expanded(
                  child: Container(
                    padding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
                    decoration: BoxDecoration(
                      color: AppColor.surface1,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: AppColor.stroke, width: 1.5),
                    ),
                    child: Text(
                      orderItem.productName ?? '',
                      style: AppTextStyle.bodySmall.withColor(AppColor.text1),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                ),
              ],
            ),
          ),
          Row(
            spacing: 16,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  spacing: 6,
                  children: [
                    Text(
                      'الكمية',
                      style: AppTextStyle.bodyMedium.withColor(AppColor.text2),
                    ),
                    Row(
                      children: [
                        Expanded(
                          child: Container(
                            padding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
                            decoration: BoxDecoration(
                              color: AppColor.surface1,
                              borderRadius: BorderRadius.circular(12),
                              border: Border.all(color: AppColor.stroke, width: 1.5),
                            ),
                            child: Center(
                              child: Text(
                                orderItem.quantity.toString(),
                                style: AppTextStyle.numberMedium.withColor(AppColor.text1),
                                overflow: TextOverflow.ellipsis,
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              Expanded(
                flex: 3,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  spacing: 6,
                  children: [
                    Text(
                      'السعر الكلي',
                      style: AppTextStyle.bodyMedium.withColor(AppColor.text2),
                    ),
                    Row(
                      children: [
                        Expanded(
                          child: Container(
                            padding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
                            decoration: BoxDecoration(
                              color: AppColor.surface1,
                              borderRadius: BorderRadius.circular(12),
                              border: Border.all(color: AppColor.stroke, width: 1.5),
                            ),
                            child: Row(
                              children: [
                                Text(
                                  orderItem.sellAmount.toIraqiDinarString(),
                                  style: AppTextStyle.numberMedium.withColor(AppColor.text1),
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ],
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
          Padding(
            padding: const EdgeInsets.only(top: 24),
            child: Row(
              spacing: 16,
              children: [
                Expanded(
                  child: FilledButton(
                    onPressed: () {
                      showAnimatedDialog(
                        context: context,
                        child: Center(
                          child: Padding(
                            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 24),
                            child: DefaultBlocProvider.value(
                              value: bloc,
                              child: AddSaleDialog(
                                orderItem: orderItem,
                                isInStorage: orderItem.productId != null,
                              ),
                            ),
                          ),
                        ),
                      );
                    },
                    child: Text('عرض التفاصيل'),
                  ),
                ),
                FilledButton(
                  onPressed: () {
                    bloc.add(DeleteOrderItemEvent(isInStorage: orderItem.productId != null, index: index));
                  },
                  style: FilledButton.styleFrom(
                    backgroundColor: AppColor.white,
                    padding: EdgeInsets.zero,
                    foregroundColor: AppColor.error,
                  ),
                  child: VectorGraphic(loader: AssetBytesLoader('assets/svg/delete_icon.svg')),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

// ── Tab widget ────────────────────────────────────────────────────────────────

class _SideTab extends StatelessWidget {
  final String label;
  final String icon;
  final TabController controller;
  final int index;

  const _SideTab({
    required this.label,
    required this.icon,
    required this.controller,
    required this.index,
  });

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: controller.animation!,
      builder: (context, child) {
        final isSelected = controller.index == index;
        return Row(
          mainAxisAlignment: MainAxisAlignment.center,
          spacing: 6,
          children: [
            VectorGraphic(
              loader: AssetBytesLoader('assets/svg/$icon'),
              colorFilter: ColorFilter.mode(
                isSelected ? AppColor.white : AppColor.text2,
                BlendMode.srcIn,
              ),
            ),
            Text(
              label,
              style: AppTextStyle.bodySmall.withColor(
                isSelected ? AppColor.white : AppColor.text2,
              ),
            ),
          ],
        );
      },
    );
  }
}
