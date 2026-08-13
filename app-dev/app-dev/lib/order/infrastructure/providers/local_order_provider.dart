import 'dart:convert';

import 'package:hive_ce/hive.dart';
import 'package:team/order/domain/collect_installment/collect_installment.dart';
import 'package:team/order/domain/order/order.dart';

class LocalOrderProvider {
  late final Box _box;

  final _orderBoxKey = 'orderBoxKey';
  final _recentOrderSearchesKey = 'recentOrderSearchesKey';
  final _offlineCollectsKey = 'offlineCollectsKey';
  final _mandoobOrdersKey = 'mandoobOrdersKey';
  final _mandoobOrdersTimestampKey = 'mandoobOrdersTimestampKey';

  Future<void> ensureInitialized() async {
    _box = await Hive.openBox(_orderBoxKey);
  }

  Future<void> saveRecentSearches(List<Order> recentSearches) {
    final jsonList = recentSearches.map((order) => order.toJson()).toList();
    return _box.put(_recentOrderSearchesKey, jsonEncode(jsonList));
  }

  List<Order> getRecentSearches() {
    final data = _box.get(_recentOrderSearchesKey);

    if (data == null) return [];

    final List<dynamic> jsonList = jsonDecode(data);
    return jsonList.map((json) => Order.fromJson(json)).toList();
  }

  Future<void> saveOfflineCollect(List<CollectInstallment> collectInstallments) {
    final jsonList = collectInstallments.map((order) => order.toJson()).toList();
    return _box.put(_offlineCollectsKey, jsonEncode(jsonList));
  }

  List<CollectInstallment> getOfflineCollects() {
    final data = _box.get(_offlineCollectsKey);

    if (data == null) return [];

    final List<dynamic> jsonList = jsonDecode(data);
    return jsonList.map((json) => CollectInstallment.fromJson(json)).toList();
  }

  // New methods for caching Mandoob orders with timestamp
  Future<void> saveMandoobOrders(List<Order> orders) async {
    final jsonList = orders.map((order) => order.toJson()).toList();
    await _box.put(_mandoobOrdersKey, jsonEncode(jsonList));
    await _box.put(_mandoobOrdersTimestampKey, DateTime.now().millisecondsSinceEpoch);
  }

  List<Order>? getCachedMandoobOrders() {
    final data = _box.get(_mandoobOrdersKey);
    if (data == null) return null;

    final List<dynamic> jsonList = jsonDecode(data);
    return jsonList.map((json) => Order.fromJson(json)).toList();
  }

  bool isMandoobOrdersCacheValid() {
    final timestamp = _box.get(_mandoobOrdersTimestampKey);
    if (timestamp == null) return false;

    final cacheDate = DateTime.fromMillisecondsSinceEpoch(timestamp);
    final today = DateTime.now();

    // Check if cache is from today (same day)
    return cacheDate.year == today.year && cacheDate.month == today.month && cacheDate.day == today.day;
  }

  Future clear() => _box.clear();
}
