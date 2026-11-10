class DelegateSession {
  const DelegateSession({
    required this.delegateId,
    required this.asyncId,
    required this.apiBase,
    this.delegateName,
    this.cityName,
    this.orderListId,
    this.followerCollectionEnabled = false,
    this.username,
    this.accessToken,
  });

  /// Central user id (string).
  final String delegateId;

  /// Offline queue owner key (= user id).
  final String asyncId;
  final String apiBase;
  final String? delegateName;
  final String? cityName;
  /// معرّف قائمة التحصيل المكلَّفة للمندوب (OrderLists.Id).
  final int? orderListId;
  /// جباية المتابع مفعّلة على قائمة المندوب الحالية — يمنع التحصيل لهذه القائمة فقط.
  final bool followerCollectionEnabled;
  final String? username;
  final String? accessToken;
}
