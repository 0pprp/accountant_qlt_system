import 'package:flutter/cupertino.dart';
import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/time_util.dart';
import 'package:team/order/domain/collect_installment/collect_installment.dart';
import 'package:team/order/domain/order/order.dart';
import 'package:team/order/domain/order_status.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';
import 'package:team/payment/mapper.dart';

part 'orders_event.dart';
part 'orders_state.dart';

class OrdersBloc extends DefaultBloc {
  final OrderRepository orderRepository;
  final PaymentMapper paymentMapper;
  OrdersBloc({required this.orderRepository, required this.paymentMapper}) : super(OrdersInitialState()) {
    safeOn<InitialOrdersEvent>(
      _initialOrders,
      onCatch: (event, emit) async => isFetchingInitialOrders = false,
    );
    safeOn<SendUserPayment>(_sendUserPayment);
    safeOn<AddRecentSearch>(_addRecentSearches);
    safeOn<GetRecentSearches>(_getRecentSearches);
    safeOn<ClearRecentSearches>(_clearRecentSearches);
    safeOn<DeleteOrderEvent>(_deleteOrder);
  }

  static const List<ExecutionStatus> defaultExecutionStatuses = [
    ExecutionStatus.notStarted,
    ExecutionStatus.inProgress,
  ];

  List<Order>? orders;
  List<Order>? searchedOrders;
  List<Order>? recentSearches;
  bool isFetchingInitialOrders = false;
  ExpansionOption? selectedOrderItem;
  DateTime? startDate;
  DateTime? endDate;
  String? searchText;
  int? orderListId;
  List<ExecutionStatus> executionStatuses = List.of(defaultExecutionStatuses);

  bool get hasNonDefaultExecutionStatuses {
    if (executionStatuses.length != defaultExecutionStatuses.length) return true;
    return !defaultExecutionStatuses.every(executionStatuses.contains);
  }

  void _initialOrders(InitialOrdersEvent event, emit) async {
    if (event.orderListId != null) {
      orderListId = event.orderListId;
    }
    if (!isFetchingInitialOrders) {
      isFetchingInitialOrders = true;
      List<Order> response = await orderRepository.getMandoobOrders(
        orderListId: orderListId,
        startDate: startDate,
        endDate: endDate,
        searchText: searchText,
        executionStatuses: executionStatuses,
      );
      try {
        final offlineCollects = orderRepository.getOfflineCollects();
        response =
            response
                .map(
                  (e) => e.copyWith(
                    isCollectedOffline: offlineCollects.any(
                      (element) =>
                          element.orderId == e.id && (element.date?.isSameDay(DateTime.now().toUtc()) ?? false),
                    ),
                  ),
                )
                .toList();
      } catch (e) {
        debugPrint(e.toString());
      }

      if (searchText == null) {
        orders = response;
      } else {
        searchedOrders = response;
      }

      isFetchingInitialOrders = false;
    }
  }

  void _sendUserPayment(SendUserPayment event, emit) async {
    await orderRepository.collectInstallment(event.collectInstallment, event.order);
  }

  void _clearRecentSearches(ClearRecentSearches event, emit) async {
    await orderRepository.saveRecentSearches([]);
    add(GetRecentSearches());
  }

  void _getRecentSearches(GetRecentSearches event, emit) async {
    searchedOrders = null;
    searchText = null;
    recentSearches = orderRepository.getRecentSearches();
    recentSearches =
        recentSearches!.where((element) {
          if (orderListId == null) {
            return true;
          }
          return element.orderListId == orderListId;
        }).toList();
  }

  void _addRecentSearches(AddRecentSearch event, emit) async {
    final recentSearchesList = List<Order>.from(recentSearches ?? []);

    // Remove any existing item with the same ID
    recentSearchesList.removeWhere((item) => item.id == event.order.id);

    // Insert the new/updated item at the beginning
    recentSearchesList.insert(0, event.order);

    // Save the updated list
    await orderRepository.saveRecentSearches(recentSearchesList);
  }

  void _deleteOrder(DeleteOrderEvent event, emit) async {
    await orderRepository.deleteOrder(event.orderId);
    orders = orders?.where((order) => order.id != event.orderId).toList();
    searchedOrders = searchedOrders?.where((order) => order.id != event.orderId).toList();
    add(InitialOrdersEvent());
  }
}
