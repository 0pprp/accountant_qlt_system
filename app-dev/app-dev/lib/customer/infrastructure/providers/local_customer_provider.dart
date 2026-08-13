import 'dart:convert';

import 'package:hive_ce/hive.dart';
import 'package:team/account/domain/user_info/user_info.dart';

class LocalCustomerProvider {
  late final Box _box;

  final _customerBoxKey = 'customerBoxKey';
  final _recentCustomerSearchesKey = 'recentCustomerSearchesKey';

  Future<void> ensureInitialized() async {
    _box = await Hive.openBox(_customerBoxKey);
  }

  Future<void> saveRecentSearches(List<UserInfo> recentSearches) {
    final jsonList = recentSearches.map((user) => user.toJson()).toList();
    return _box.put(_recentCustomerSearchesKey, jsonEncode(jsonList));
  }

  List<UserInfo> getRecentSearches() {
    final data = _box.get(_recentCustomerSearchesKey);

    if (data == null) return [];

    final List<dynamic> jsonList = jsonDecode(data);
    return jsonList.map((json) => UserInfo.fromJson(json)).toList();
  }

  Future clear() => _box.clear();
}
