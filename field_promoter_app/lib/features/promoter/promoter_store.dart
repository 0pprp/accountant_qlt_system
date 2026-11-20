import 'dart:async';

import 'package:flutter/foundation.dart';

import '../shift/shift_tracker.dart';
import 'models.dart';
import 'promoter_api.dart';

class PromoterStore extends ChangeNotifier {
  PromoterStore(this._api, this._tracker) {
    _tracker.onServerStopped = () => unawaited(refreshMe(force: true));
  }

  static const meTtl = Duration(seconds: 30);
  static const countsTtl = Duration(seconds: 20);

  final PromoterApi _api;
  final ShiftTracker _tracker;

  PromoterMe? _me;
  DateTime? _meAt;
  Future<PromoterMe>? _meFlight;

  OrderCounts _counts = OrderCounts.zero;
  DateTime? _countsAt;
  Future<OrderCounts>? _countsFlight;

  PromoterMe? get me => _me;
  OrderCounts get counts => _counts;
  bool get isWorking => _me?.isWorking ?? false;

  Future<PromoterMe> refreshMe({bool force = false}) {
    final cached = _me;
    final at = _meAt;
    if (!force && cached != null && at != null && DateTime.now().difference(at) < meTtl) {
      return Future.value(cached);
    }
    return _meFlight ??= _loadMe().whenComplete(() => _meFlight = null);
  }

  Future<PromoterMe> _loadMe() async {
    final me = await _api.me();
    _me = me;
    _meAt = DateTime.now();
    notifyListeners();
    unawaited(_tracker.syncWithServer(me.isWorking));
    return me;
  }

  Future<OrderCounts> refreshCounts({bool force = false}) {
    final at = _countsAt;
    if (!force && at != null && DateTime.now().difference(at) < countsTtl) {
      return Future.value(_counts);
    }
    return _countsFlight ??= _loadCounts().whenComplete(() => _countsFlight = null);
  }

  Future<OrderCounts> _loadCounts() async {
    final c = await _api.orderCounts();
    _counts = c;
    _countsAt = DateTime.now();
    notifyListeners();
    return c;
  }

  void invalidate() {
    _meAt = null;
    _countsAt = null;
  }

  void applyShift(ShiftState state) {
    final m = _me;
    if (m == null) return;
    _me = PromoterMe(
      id: m.id,
      fullName: m.fullName,
      username: m.username,
      branches: m.branches,
      isWorking: state.isWorking,
      shiftStartedAt: state.isWorking ? (state.startedAt ?? m.shiftStartedAt) : null,
      todayPins: m.todayPins,
      todayOrders: m.todayOrders,
      todayWorkMinutes: m.todayWorkMinutes,
      targetMinutes: m.targetMinutes,
      monthOrders: m.monthOrders,
      monthSold: m.monthSold,
      supportPhone: m.supportPhone,
      supportWhatsapp: m.supportWhatsapp,
      products: m.products,
    );
    notifyListeners();
  }

  Future<void> afterActivity() async {
    invalidate();
    try {
      await Future.wait([refreshMe(force: true), refreshCounts(force: true)]);
    } catch (_) {}
  }

  void reset() {
    _me = null;
    _meAt = null;
    _counts = OrderCounts.zero;
    _countsAt = null;
    notifyListeners();
  }
}
