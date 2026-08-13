import 'package:flutter/cupertino.dart';
import 'package:flutter/foundation.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/payment/domain/installment_payment/installment_payment.dart';
import 'package:team/payment/domain/installment_payments_summary/installment_payments_summary.dart';
import 'package:team/payment/infrastructure/repositories/payment_repository.dart';

part 'payments_event.dart';
part 'payments_state.dart';

class PaymentsBloc extends DefaultBloc {
  final PaymentRepository paymentRepository;
  PaymentsBloc({required this.paymentRepository}) : super(PaymentsInitialState()) {
    safeOn<InitialPaymentsEvent>(_initialPayments, onCatch: (event, emit) async => isFetchingInitialPayments = false);
    safeOn<GetRecentSearches>(_getRecentSearches);
    safeOn<AddRecentSearch>(_addRecentSearches);
    safeOn<ClearRecentSearches>(_clearRecentSearches);
  }

  InstallmentPaymentsSummary? summary;
  bool isFetchingInitialPayments = false;
  List<InstallmentPayment>? installmentPayments;
  List<InstallmentPayment>? searchedInstallmentPayments;
  List<InstallmentPayment>? recentSearches;
  DateTime? startDate;
  DateTime? endDate;
  String? searchText;
  int? orderListId;

  void _initialPayments(InitialPaymentsEvent event, emit) async {
    if (event.orderListId != null) {
      orderListId = event.orderListId;
    }
    if (!isFetchingInitialPayments) {
      isFetchingInitialPayments = true;

      //getSummaryIfNull
      if (summary == null) {
        try {
          summary = await paymentRepository.getInstallmentPaymentsSummary(
            orderListId: orderListId,
            startDate: startDate,
            endDate: endDate,
            searchText: searchText,
          );
        } catch (e) {
          debugPrint(e.toString());
        }
      }
      //getPayments
      final response = await paymentRepository.getInstallmentPayments(
        orderListId: orderListId,
        lastId: event.lastId,
        startDate: startDate,
        endDate: endDate,
        searchText: searchText,
      );
      if (event.lastId == null) {
        if (searchText == null) {
          if (summary == null) {
            final totalAmount = response.fold<double>(0.0, (previousValue, element) => previousValue + element.amount);
            summary = InstallmentPaymentsSummary(totalAmount: totalAmount, totalCount: response.length);
          }
          installmentPayments = response;
        } else {
          searchedInstallmentPayments = response;
        }
      } else {
        if (searchText == null) {
          installmentPayments?.addAll(response);
        } else {
          searchedInstallmentPayments?.addAll(response);
        }
      }
      isFetchingInitialPayments = false;
    }
  }

  void _clearRecentSearches(ClearRecentSearches event, emit) async {
    await paymentRepository.saveRecentSearches([]);
    add(GetRecentSearches());
  }

  void _getRecentSearches(GetRecentSearches event, emit) async {
    searchedInstallmentPayments = null;
    searchText = null;
    recentSearches = paymentRepository.getRecentSearches();
    recentSearches =
        recentSearches!.where((element) {
          if (orderListId == null) {
            return true;
          }
          return element.orderListId == orderListId;
        }).toList();
  }

  void _addRecentSearches(AddRecentSearch event, emit) async {
    final recentSearchesList = List<InstallmentPayment>.from(recentSearches ?? []);

    // Remove any existing item with the same ID
    recentSearchesList.removeWhere((item) => item.id == event.payment.id);

    // Insert the new/updated item at the beginning
    recentSearchesList.insert(0, event.payment);

    // Save the updated list
    await paymentRepository.saveRecentSearches(recentSearchesList);
  }
}
