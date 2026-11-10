import 'package:committee_officer_application/core/network/dio_client.dart';

class CommitteeListCard {
  CommitteeListCard({
    required this.listId,
    required this.listName,
    required this.mandobName,
    required this.phantomLabel,
    required this.currentMonthScore,
    required this.previousMonthScore,
    required this.activeCustomers,
    required this.collectionEnabled,
  });

  final int listId;
  final String listName;
  final String mandobName;
  final String phantomLabel;
  final double currentMonthScore;
  final double previousMonthScore;
  final int activeCustomers;
  final bool collectionEnabled;

  factory CommitteeListCard.fromJson(
    Map<String, dynamic> j,
  ) => CommitteeListCard(
    listId: int.tryParse('${j['listId'] ?? j['ListId'] ?? 0}') ?? 0,
    listName: '${j['listName'] ?? j['ListName'] ?? ''}',
    mandobName: '${j['mandobName'] ?? j['MandobName'] ?? ''}',
    phantomLabel: '${j['phantomLabel'] ?? j['PhantomLabel'] ?? ''}',
    currentMonthScore: double.tryParse('${j['currentMonthScore'] ?? 0}') ?? 0,
    previousMonthScore: double.tryParse('${j['previousMonthScore'] ?? 0}') ?? 0,
    activeCustomers: int.tryParse('${j['activeCustomers'] ?? 0}') ?? 0,
    collectionEnabled:
        j['collectionEnabled'] == true || j['CollectionEnabled'] == true,
  );
}

class CommitteeRemote {
  CommitteeRemote(this._dio);
  final DioClient _dio;

  List<Map<String, dynamic>> _list(Object? data) {
    if (data is List) {
      return data
          .whereType<Map>()
          .map((e) => Map<String, dynamic>.from(e))
          .toList();
    }
    if (data is Map) {
      final map = Map<String, dynamic>.from(data);
      final inner = map['data'] ?? map['Data'] ?? map['items'] ?? map['Items'];
      if (inner is List) {
        return inner
            .whereType<Map>()
            .map((e) => Map<String, dynamic>.from(e))
            .toList();
      }
      // عنصر واحد بدون غلاف قائمة
      if (map.containsKey('listId') ||
          map.containsKey('ListId') ||
          map.containsKey('id') ||
          map.containsKey('Id')) {
        return [map];
      }
    }
    return const [];
  }

  Future<void> addDelegateNote(int listId, String noteText) async {
    await _dio.dio.post(
      'api/v1/app/committee/order-lists/$listId/delegate-notes',
      data: {'noteText': noteText.trim()},
    );
  }

  Future<List<CommitteeListCard>> listCards() async {
    final res = await _dio.dio.get('api/v1/app/committee/list-cards');
    return _list(res.data).map(CommitteeListCard.fromJson).toList();
  }

  Future<List<Map<String, dynamic>>> lists() async {
    final res = await _dio.dio.get('api/v1/app/committee/lists');
    return _list(res.data);
  }

  Future<Map<String, dynamic>> daily({
    required int listId,
    required String showType,
    String? date,
  }) async {
    final res = await _dio.dio.get(
      'api/v1/app/committee/daily',
      queryParameters: {
        'listId': listId,
        'showType': showType,
        if (date != null) 'date': date,
      },
    );
    if (res.data is Map) return Map<String, dynamic>.from(res.data as Map);
    return {};
  }

  Future<List<Map<String, dynamic>>> collectionLists() async {
    final res = await _dio.dio.get('api/v1/app/committee/collection-lists');
    return _list(res.data);
  }

  Future<List<Map<String, dynamic>>> declining() async {
    final res = await _dio.dio.get('api/v1/app/committee/declining-customers');
    return _list(res.data);
  }

  Future<List<Map<String, dynamic>>> pendingSales() async {
    final res = await _dio.dio.get('api/v1/app/committee/pending-sales');
    return _list(res.data);
  }

  Future<List<Map<String, dynamic>>> missingDayRatings() async {
    final res = await _dio.dio.get('api/v1/app/committee/day-ratings/missing');
    return _list(res.data);
  }

  Future<void> postDayRating({
    required int listId,
    required int score,
    String? note,
  }) async {
    await _dio.dio.post(
      'api/v1/app/committee/day-ratings',
      data: {'listId': listId, 'score': score, if (note != null) 'note': note},
    );
  }

  Future<void> complaint(String body, {String? subject}) async {
    await _dio.dio.post(
      'api/v1/app/committee/complaints',
      data: {'body': body, if (subject != null) 'subject': subject},
    );
  }

  Future<void> directorNote(String body, {String? subject}) async {
    await _dio.dio.post(
      'api/v1/app/committee/director-notes',
      data: {'body': body, if (subject != null) 'subject': subject},
    );
  }

  /// نشرة مهام اللجنة المفعّلة — JWT + CommitteeTask.AppRead.
  Future<List<Map<String, dynamic>>> tasks() async {
    final res = await _dio.dio.get('api/v1/app/committee/tasks');
    return _list(res.data);
  }

  Future<void> authenticatePendingSale(
    int orderId, {
    required int rating,
    String? note,
    required double shopStockEstimatedValue,
    required double estimatedDailyRevenue,
  }) async {
    await _dio.dio.post(
      'api/v1/app/committee/pending-sales/$orderId/authenticate',
      data: {
        'rating': rating,
        'note': note,
        'shopStockEstimatedValue': shopStockEstimatedValue,
        'estimatedDailyRevenue': estimatedDailyRevenue,
      },
    );
  }

  Future<List<Map<String, dynamic>>> fieldFollowups() async {
    final res = await _dio.dio.get('api/v1/app/committee/field-followups');
    return _list(res.data);
  }

  Future<Map<String, dynamic>> addFieldFollowup({
    required int listId,
    String? listName,
    required int customerId,
    String? customerName,
    String? phoneNumber,
  }) async {
    final res = await _dio.dio.post(
      'api/v1/app/committee/field-followups',
      data: {
        'listId': listId,
        'listName': listName,
        'customerId': customerId,
        'customerName': customerName,
        'phoneNumber': phoneNumber,
      },
    );
    if (res.data is Map) return Map<String, dynamic>.from(res.data as Map);
    return {};
  }

  Future<Map<String, dynamic>> completeFieldFollowup(int id, String note) async {
    final res = await _dio.dio.post(
      'api/v1/app/committee/field-followups/$id/complete',
      data: {'note': note.trim()},
    );
    if (res.data is Map) return Map<String, dynamic>.from(res.data as Map);
    return {};
  }

  Future<Map<String, dynamic>> recordCompleteFieldFollowup({
    required int listId,
    String? listName,
    required int customerId,
    String? customerName,
    String? phoneNumber,
    required String note,
  }) async {
    final res = await _dio.dio.post(
      'api/v1/app/committee/field-followups/record-complete',
      data: {
        'listId': listId,
        'listName': listName,
        'customerId': customerId,
        'customerName': customerName,
        'phoneNumber': phoneNumber,
        'note': note.trim(),
      },
    );
    if (res.data is Map) return Map<String, dynamic>.from(res.data as Map);
    return {};
  }

  Future<Map<int, String>> fieldFollowupLatestNotes(int listId) async {
    final res = await _dio.dio.get(
      'api/v1/app/committee/field-followups/latest-notes',
      queryParameters: {'listId': listId},
    );
    final map = <int, String>{};
    for (final row in _list(res.data)) {
      final id = int.tryParse('${row['customerId'] ?? row['CustomerId'] ?? 0}') ?? 0;
      final note = '${row['note'] ?? row['Note'] ?? ''}'.trim();
      if (id > 0 && note.isNotEmpty) map[id] = note;
    }
    return map;
  }

  Future<List<Map<String, dynamic>>> listStats() async {
    final res = await _dio.dio.get('api/v1/app/committee/list-stats');
    if (res.data is Map) {
      final m = Map<String, dynamic>.from(res.data as Map);
      final items = m['items'] ?? m['Items'];
      if (items is List) {
        return items
            .whereType<Map>()
            .map((e) => Map<String, dynamic>.from(e))
            .toList();
      }
    }
    return _list(res.data);
  }

  Future<void> addCustomerNote({
    required int customerId,
    required int listId,
    required String noteText,
  }) async {
    await _dio.dio.post(
      'api/v1/app/committee/customers/$customerId/notes',
      data: {'listId': listId, 'noteText': noteText.trim()},
    );
  }
}
