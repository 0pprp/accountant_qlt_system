import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:team/account/domain/admin_user/admin_user.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/home/domain/app_version_response/app_version_response.dart';
import 'package:team/home/infrastructure/repositories/home_repository.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';
import 'package:team/order/domain/order_list/order_list.dart';
import 'package:team/order/mapper.dart';

part 'home_event.dart';
part 'home_state.dart';

class HomeBloc extends DefaultBloc {
  final HomeRepository homeRepository;
  final AccountMapper accountMapper;
  final OrderMapper customerMapper;
  HomeBloc({required this.homeRepository, required this.accountMapper, required this.customerMapper})
    : super(HomeInitial()) {
    safeOn<InitialHomeEvent>(_initialHome);
    safeOn<UpdateDataEvent>(_updateData, onCatch: _updateDataOnCatch);
    safeOn<GetUserEvent>(_getUserEvent);
    safeOn<CheckPendingCollects>(_checkPendingCollects);
    safeOn<CheckForUpdates>(_checkForUpdates);
    safeOn<ChangeOrderListEvent>(_changeOrderList);
  }

  OrderList? mandoobOrderList;
  List<OrderList>? orderLists;
  AdminUser? user;
  bool hasPendingCollects = false;

  /// Index of the order list currently selected in the motaba home carousel.
  int currentOrderListIndex = 0;

  /// Direction of the last carousel page change: 1 when moving to a later
  /// list (next), -1 when moving to an earlier one (previous). Drives which
  /// way the "تقرير القائمة" section slides in/out.
  int slideDirection = 1;

  final isMotaba = GetIt.I.get<AccountMapper>().isMotaba;

  /// The order list whose report should currently be displayed: the single
  /// assigned list for a mandoob, or whichever list is selected in the
  /// motaba carousel.
  OrderList? get currentOrderList =>
      isMotaba
          ? (orderLists != null && orderLists!.isNotEmpty ? orderLists![currentOrderListIndex] : null)
          : mandoobOrderList;

  void _initialHome(InitialHomeEvent event, Emitter emit) async {
    if (user == null) {
      add(GetUserEvent());
    }
    add(CheckPendingCollects());
    orderLists = await homeRepository.getOrderLists();
    if (!isMotaba) {
      mandoobOrderList = orderLists!.first;
    }
    //check if new version is available
    add(CheckForUpdates());
  }

  void _getUserEvent(GetUserEvent event, Emitter emit) async {
    user = await accountMapper.getCurrentAdminUser();
  }

  void _updateData(UpdateDataEvent event, Emitter emit) async {
    final offlineCollects = customerMapper.getOfflineCollects();
    await customerMapper.collectOfflineInstallments(offlineCollects);
    await customerMapper.saveOfflineCollects([]);
    GetIt.I.get<MainBloc>().add(OrderPaymentEvent());
    add(CheckPendingCollects());
  }

  void _checkPendingCollects(CheckPendingCollects event, Emitter emit) async {
    final offlineCollects = customerMapper.getOfflineCollects();
    hasPendingCollects = offlineCollects.isNotEmpty;
  }

  Future<void> _updateDataOnCatch(UpdateDataEvent event, Emitter emit) async {
    emit(ChangeDialogTextState(false));
  }

  Future<AppVersionResponse> _checkForUpdates(CheckForUpdates event, emit) async {
    final appVersionResponse = await homeRepository.checkForUpdates();
    return appVersionResponse;
  }

  void _changeOrderList(ChangeOrderListEvent event, Emitter emit) {
    slideDirection = event.index >= currentOrderListIndex ? -1 : 1;
    currentOrderListIndex = event.index;
  }
}
