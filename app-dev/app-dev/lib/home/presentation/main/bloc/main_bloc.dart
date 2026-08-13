import 'package:get_it/get_it.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/home/mapper.dart';
import 'package:team/order/domain/order_list/order_list.dart';

part 'main_event.dart';
part 'main_state.dart';

class MainBloc extends DefaultBloc {
  List<OrderList>? orderLists;
  MainBloc() : super(MainInitial()) {
    on<UserLogedIn>((event, emit) => emit(UserLogedInState()));
    on<UpdateProfileData>((event, emit) => emit(UpdatedProfileData()));
    on<OrderPaymentEvent>((event, emit) {
      emit(OrderPayment());
      add(GetOrderLists());
    });
    on<OrderCreateEvent>((event, emit) => emit(OrderCreated()));
    safeOn<GetOrderLists>((event, emit) async {
      orderLists = await GetIt.I.get<HomeMapper>().getOrderLists();
    });
  }
}
