import 'package:dio/dio.dart';
import 'package:sales_employee_app/core/cache/app_cache.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/network/json_map.dart';
import 'package:sales_employee_app/features/sales/domain/entities/order_list_lite.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sale_order.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sales_request_item.dart';
import 'package:sales_employee_app/features/sales/domain/sales_request_tab_map.dart';

class SalesRemoteDataSource {
  SalesRemoteDataSource(this._dio, {AppCache? cache}) : _cache = cache;
  final DioClient _dio;
  final AppCache? _cache;

  Future<List<SaleOrder>> getOrders({bool forceRefresh = false}) async {
    const key = 'orders:all';
    if (!forceRefresh && _cache != null) {
      final cached = await _cache.getJson(key);
      if (cached is List) {
        return JsonMap.asMapList(cached).map(SaleOrder.fromJson).toList();
      }
    }
    final res = await _dio.dio.get('api/v1/app/Orders');
    final orders = _mapOrders(res.data);
    await _cache?.putJson(
      key,
      orders
          .map(
            (o) => {
              'id': o.id,
              'customerName': o.customerName,
              'status': o.status,
              'total': o.total,
              'createdAt': o.createdAt,
              'customerId': o.customerId,
            },
          )
          .toList(),
      ttl: const Duration(seconds: 45),
    );
    return orders;
  }

  Future<SaleOrder?> getOrder(int id) async {
    final res = await _dio.dio.get('api/v1/app/Orders/$id');
    if (res.data is Map) {
      return SaleOrder.fromJson(Map<String, dynamic>.from(res.data as Map));
    }
    return null;
  }

  Future<List<SalesBinSummary>> getBins({bool forceRefresh = false}) async {
    const key = 'orders:bins';
    if (!forceRefresh && _cache != null) {
      final cached = await _cache.getJson(key);
      if (cached is List) {
        return JsonMap.asMapList(cached)
            .map(
              (m) => SalesBinSummary(
                key: JsonMap.str(m, ['key']),
                label: JsonMap.str(m, ['label']),
                count: JsonMap.asInt(m, ['count']) ?? 0,
              ),
            )
            .toList();
      }
    }

    List<SalesBinSummary> bins;
    try {
      final res = await _dio.dio.get('api/v1/app/SalesWorkflow/bins');
      final list = _extractList(res.data);
      bins = list.map((m) {
        final key = JsonMap.str(m, ['key', 'Key', 'status', 'Status']);
        final labelRaw = JsonMap.str(m, ['label', 'Label', 'title', 'Title'], fallback: key);
        return SalesBinSummary(
          key: key,
          label: _friendlyLabel(key, labelRaw),
          count: JsonMap.asInt(m, ['count', 'Count']) ?? 0,
        );
      }).toList();
    } catch (_) {
      try {
        final orders = await getOrders(forceRefresh: forceRefresh);
        bins = _fallbackBins(orders);
      } catch (_) {
        bins = _emptyBins();
      }
    }

    await _cache?.putJson(
      key,
      bins.map((b) => {'key': b.key, 'label': b.label, 'count': b.count}).toList(),
      ttl: const Duration(seconds: 20),
    );
    return bins;
  }

  /// طلبات الموظف من SalesRequests حسب تبويب الدورة (ليس Order.WorkflowStatus).
  Future<List<SalesRequestItem>> getSalesRequestsByTab(
    String uiOrApiTab, {
    bool forceRefresh = false,
  }) async {
    final tab = SalesRequestTabMap.apiTabForUiKey(uiOrApiTab);
    final cacheKey = 'sales-requests:tab:$tab';
    if (!forceRefresh && _cache != null) {
      final cached = await _cache.getJson(cacheKey);
      if (cached is List) {
        return JsonMap.asMapList(cached).map(SalesRequestItem.fromJson).toList();
      }
    }

    final res = await _dio.dio.get(
      'api/v1/app/SalesRequests',
      queryParameters: {'tab': tab},
    );
    final items = _extractList(res.data).map(SalesRequestItem.fromJson).toList();
    await _cache?.putJson(
      cacheKey,
      items
          .map(
            (e) => {
              'requestId': e.requestId,
              'customerName': e.customerName,
              'customerPhone': e.customerPhone,
              'status': e.status,
              'statusLabel': e.statusLabel,
              'tabKey': e.tabKey,
              'linkedOrderId': e.linkedOrderId,
              'isReturned': e.isReturned,
              'lastStatusNote': e.lastStatusNote,
            },
          )
          .toList(),
      ttl: const Duration(seconds: 20),
    );
    return items;
  }

  Future<SalesRequestItem?> getSalesRequest(int requestId) async {
    final res = await _dio.dio.get('api/v1/app/SalesRequests/$requestId');
    if (res.data is Map) {
      return SalesRequestItem.fromJson(Map<String, dynamic>.from(res.data as Map));
    }
    return null;
  }

  Future<void> viewSalesRequest(int requestId) async {
    await _dio.dio.post('api/v1/app/SalesRequests/$requestId/view');
    await _cache?.invalidatePrefix('sales-requests:');
  }

  Future<void> prepareSalesRequest(int requestId, {String? note}) async {
    await _dio.dio.post(
      'api/v1/app/SalesRequests/$requestId/prepare',
      data: {'note': note},
    );
    await _cache?.invalidatePrefix('sales-requests:');
  }

  Future<void> pendSalesRequest(int requestId, {required String note}) async {
    await _dio.dio.post(
      'api/v1/app/SalesRequests/$requestId/pend',
      data: {'note': note},
    );
    await _cache?.invalidatePrefix('sales-requests:');
  }

  Future<void> rejectSalesRequest(int requestId, {required String note}) async {
    await _dio.dio.post(
      'api/v1/app/SalesRequests/$requestId/reject',
      data: {'note': note},
    );
    await _cache?.invalidatePrefix('sales-requests:');
  }

  Future<void> inspectSalesRequest(int requestId) async {
    await _dio.dio.post('api/v1/app/SalesRequests/$requestId/inspect');
    await _cache?.invalidatePrefix('sales-requests:');
  }

  Future<SalesRequestItem?> linkDraft({
    required int requestId,
    required int orderId,
  }) async {
    final res = await _dio.dio.post(
      'api/v1/app/SalesRequests/$requestId/link-draft',
      data: {'orderId': orderId},
    );
    await _cache?.invalidatePrefix('sales-requests:');
    if (res.data is Map) {
      return SalesRequestItem.fromJson(Map<String, dynamic>.from(res.data as Map));
    }
    return null;
  }

  /// يضمن LinkedOrderId من السيرفر بدون تكرار البيع.
  Future<SalesRequestItem?> ensureDraft(int requestId) async {
    final res = await _dio.dio.post('api/v1/app/SalesRequests/$requestId/ensure-draft');
    await _cache?.invalidatePrefix('sales-requests:');
    if (res.data is Map) {
      return SalesRequestItem.fromJson(Map<String, dynamic>.from(res.data as Map));
    }
    return null;
  }

  /// عقد جديد: زملاء مؤهلون لنقل الاسم.
  Future<List<({int id, String name})>> transferPeers(int requestId, {String? search}) async {
    final res = await _dio.dio.get(
      'api/v1/app/SalesRequests/$requestId/transfer-peers',
      queryParameters: {
        if (search != null && search.trim().isNotEmpty) 'search': search.trim(),
      },
    );
    return _extractList(res.data).map((m) {
      final id = JsonMap.asInt(m, ['id', 'Id']) ?? 0;
      final name = JsonMap.str(m, ['fullName', 'FullName', 'username', 'Username'], fallback: '#$id');
      return (id: id, name: name);
    }).where((p) => p.id > 0).toList();
  }

  /// عقد جديد: نقل الاسم — يمنع المالك السابق بعد النجاح.
  Future<SalesRequestItem?> transferName({
    required int requestId,
    required int toEmployeeId,
    required String reason,
  }) async {
    final res = await _dio.dio.post(
      'api/v1/app/SalesRequests/$requestId/transfer-name',
      data: {'toEmployeeId': toEmployeeId, 'reason': reason},
    );
    await _cache?.invalidatePrefix('sales-requests:');
    if (res.data is Map) {
      return SalesRequestItem.fromJson(Map<String, dynamic>.from(res.data as Map));
    }
    return null;
  }

  /// عدادات تبويبات طلبات الموظف — ليست demo_orders ولا Order.WorkflowStatus وحدها.
  Future<List<SalesBinSummary>> getSalesRequestBins({bool forceRefresh = false}) async {
    const key = 'sales-requests:bins';
    if (!forceRefresh && _cache != null) {
      final cached = await _cache.getJson(key);
      if (cached is List) {
        return JsonMap.asMapList(cached)
            .map(
              (m) => SalesBinSummary(
                key: JsonMap.str(m, ['key', 'Key']),
                label: JsonMap.str(m, ['label', 'Label']),
                count: JsonMap.asInt(m, ['count', 'Count']) ?? 0,
              ),
            )
            .toList();
      }
    }
    final res = await _dio.dio.get('api/v1/app/SalesRequests/counts');
    final bins = _extractList(res.data).map((m) {
      return SalesBinSummary(
        key: JsonMap.str(m, ['key', 'Key']),
        label: JsonMap.str(m, ['label', 'Label']),
        count: JsonMap.asInt(m, ['count', 'Count']) ?? 0,
      );
    }).toList();
    try {
      await _cache?.putJson(
        key,
        bins.map((b) => {'key': b.key, 'label': b.label, 'count': b.count}).toList(),
        ttl: const Duration(seconds: 20),
      );
    } catch (_) {
      // Cache is best-effort (web SharedPreferences / SQLite).
    }
    return bins;
  }

  Future<List<SaleOrder>> getByStatus(String status, {bool forceRefresh = false}) async {
    final cacheKey = 'orders:status:$status';
    if (!forceRefresh && _cache != null) {
      final cached = await _cache.getJson(cacheKey);
      if (cached is List) {
        return JsonMap.asMapList(cached).map(SaleOrder.fromJson).toList();
      }
    }

    List<SaleOrder> orders;
    try {
      final res = await _dio.dio.get(
        'api/v1/app/SalesWorkflow/by-status',
        queryParameters: {'status': status},
      );
      orders = _mapOrders(res.data);
    } catch (_) {
      final all = await getOrders(forceRefresh: forceRefresh);
      if (status == 'all' || status.isEmpty) {
        orders = all;
      } else {
        orders = all
            .where((o) => (o.status ?? 'pending').toLowerCase() == status.toLowerCase())
            .toList();
      }
    }

    await _cache?.putJson(
      cacheKey,
      orders
          .map(
            (o) => {
              'id': o.id,
              'customerName': o.customerName,
              'status': o.status,
              'total': o.total,
              'createdAt': o.createdAt,
              'customerId': o.customerId,
              'customerPhone': o.customerPhone,
              'customer': {
                'id': o.customerId,
                'fullName': o.customerName,
                'phoneNumber': o.customerPhone,
              },
            },
          )
          .toList(),
      ttl: const Duration(seconds: 20),
    );
    return orders;
  }

  Future<int> createOrder({
    required int customerId,
    required List<Map<String, dynamic>> orderItems,
  }) async {
    final res = await _dio.dio.post(
      'api/v1/app/Orders',
      data: {
        'customerId': customerId,
        'orderItems': orderItems,
      },
    );
    await _cache?.invalidatePrefix('orders:');
    final data = res.data;
    if (data is Map) {
      return JsonMap.asInt(data, ['id', 'Id']) ?? 0;
    }
    return 0;
  }

  /// قوائم المحافظة (القائمة = المندوب) لخطوة بيانات الزبون.
  Future<List<OrderListLite>> getOrderLists({bool forceRefresh = false}) async {
    const key = 'orderlists:all';
    if (!forceRefresh && _cache != null) {
      final cached = await _cache.getJson(key);
      if (cached is List) {
        return JsonMap.asMapList(cached).map(OrderListLite.fromJson).toList();
      }
    }
    final res = await _dio.dio.get('api/v1/app/OrderLists');
    final rows = _extractList(res.data).map(OrderListLite.fromJson).where((e) => e.id > 0).toList();
    await _cache?.putJson(
      key,
      rows
          .map((e) => {'id': e.id, 'name': e.name, 'mandobFullName': e.mandobFullName})
          .toList(),
      ttl: const Duration(minutes: 5),
    );
    return rows;
  }

  Future<void> setSellerInfo({
    required int orderId,
    required String creationAddress,
    double? lat,
    double? lng,
    required String saleDate,
    required String saleTime,
    int? orderListId,
  }) async {
    await _dio.dio.put(
      'api/v1/app/Orders/$orderId/seller-info',
      data: {
        'creationAddress': creationAddress,
        if (lat != null && lng != null) 'location': {'Latitude': lat, 'Longitude': lng},
        'saleDate': saleDate,
        'saleTime': saleTime,
        if (orderListId != null) 'orderListId': orderListId,
      },
    );
  }

  Future<void> completeAttachments(int orderId) async {
    await _dio.dio.post('api/v1/app/Orders/$orderId/complete-attachments');
  }

  /// مرفقات الطلب — حقل النموذج `Type` (AttachmentType) وليس attachmentType.
  /// الأنواع الإلزامية لإكمال خطوة Attachments: 4 PurchaseReceipt، 5 TrustReceipt، 6 SaleContract.
  /// على Web استخدم [bytes] (XFile.readAsBytes) — MultipartFile.fromFile يفشل على مسارات blob.
  Future<void> uploadAttachment({
    required int orderId,
    required String fileName,
    String? filePath,
    List<int>? bytes,
    String? attachmentType,
  }) async {
    final MultipartFile part;
    if (bytes != null) {
      part = MultipartFile.fromBytes(bytes, filename: fileName);
    } else if (filePath != null && filePath.isNotEmpty) {
      part = await MultipartFile.fromFile(filePath, filename: fileName);
    } else {
      throw ArgumentError('uploadAttachment requires bytes or filePath');
    }
    await _dio.dio.post(
      'api/v1/app/Attachments',
      data: FormData.fromMap({
        'orderId': orderId,
        // ASP.NET يربط CreateAttachmentCommand.Type — الاسم الخاطئ يُسقِط القيمة إلى 0.
        if (attachmentType != null) 'Type': attachmentType,
        'file': part,
      }),
    );
  }

  /// يبني FormData لرفع مرفق — للاختبارات دون I/O.
  static Map<String, dynamic> attachmentFormFields({
    required int orderId,
    String? attachmentType,
  }) =>
      {
        'orderId': orderId,
        if (attachmentType != null) 'Type': attachmentType,
      };

  Future<void> progress(int id) async {
    await _dio.dio.post('api/v1/app/SalesWorkflow/$id/progress');
    await _cache?.invalidatePrefix('orders:');
  }

  Future<void> complete(int id) async {
    await _dio.dio.post('api/v1/app/SalesWorkflow/$id/complete');
    await _cache?.invalidatePrefix('orders:');
  }

  Future<void> transfer({
    required int id,
    required String targetStatus,
    String? reason,
  }) async {
    await _dio.dio.post(
      'api/v1/app/SalesWorkflow/$id/transfer',
      data: {
        'targetStatus': targetStatus,
        if (reason != null && reason.trim().isNotEmpty) 'reason': reason.trim(),
      },
    );
    await _cache?.invalidatePrefix('orders:');
  }

  List<SaleOrder> _mapOrders(Object? data) =>
      _extractList(data).map(SaleOrder.fromJson).toList();

  List<Map<String, dynamic>> _extractList(Object? data) {
    if (data is List) return JsonMap.asMapList(data);
    if (data is Map) {
      final items = JsonMap.pick(data, [
        'bins',
        'Bins',
        'items',
        'Items',
        'data',
        'Data',
        'orders',
        'Orders',
      ]);
      return JsonMap.asMapList(items);
    }
    return const [];
  }

  String _friendlyLabel(String key, String fallback) {
    switch (key.toLowerCase()) {
      case 'pending':
        return 'طلبات البيع';
      case 'ready':
        return 'جاهز';
      case 'disclosure':
        return 'كشف';
      case 'on_hold':
        return 'معلقة';
      case 'rejected':
        return 'مرفوض';
      case 'sold':
        return 'تم البيع';
      default:
        return fallback.isEmpty ? key : fallback;
    }
  }

  List<SalesBinSummary> _emptyBins() => const [
        SalesBinSummary(key: 'pending', label: 'طلبات البيع', count: 0),
        SalesBinSummary(key: 'ready', label: 'جاهز', count: 0),
        SalesBinSummary(key: 'disclosure', label: 'كشف', count: 0),
        SalesBinSummary(key: 'on_hold', label: 'معلقة', count: 0),
        SalesBinSummary(key: 'rejected', label: 'مرفوض', count: 0),
        SalesBinSummary(key: 'sold', label: 'تم البيع', count: 0),
      ];

  List<SalesBinSummary> _fallbackBins(List<SaleOrder> orders) {
    return _emptyBins()
        .map(
          (b) => SalesBinSummary(
            key: b.key,
            label: b.label,
            count: orders.where((o) => (o.status ?? 'pending').toLowerCase() == b.key).length,
          ),
        )
        .toList();
  }
}
