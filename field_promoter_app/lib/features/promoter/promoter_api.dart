import 'package:dio/dio.dart';

import '../../core/network/api_exception.dart';
import '../../core/network/dio_client.dart';
import 'models.dart';

class OrderSubmission {
  const OrderSubmission({
    required this.submissionKey,
    required this.branchId,
    required this.customerName,
    required this.customerPhone,
    required this.product,
    required this.address,
    this.note,
    this.latitude,
    this.longitude,
    this.photoPath,
  });

  final String submissionKey;
  final int branchId;
  final String customerName;
  final String customerPhone;
  final String product;
  final String address;
  final String? note;
  final double? latitude;
  final double? longitude;
  final String? photoPath;

  Future<FormData> toFormData() async {
    final fields = <MapEntry<String, String>>[
      MapEntry('submissionKey', submissionKey),
      MapEntry('branchId', '$branchId'),
      MapEntry('customerName', customerName),
      MapEntry('customerPhone', customerPhone),
      MapEntry('product', product),
      MapEntry('address', address),
      if (note != null && note!.trim().isNotEmpty) MapEntry('note', note!.trim()),
      if (latitude != null && longitude != null) ...[
        MapEntry('latitude', latitude!.toStringAsFixed(7)),
        MapEntry('longitude', longitude!.toStringAsFixed(7)),
      ],
    ];
    final form = FormData();
    form.fields.addAll(fields);
    if (photoPath != null) {
      form.files.add(
        MapEntry(
          'photo',
          await MultipartFile.fromFile(
            photoPath!,
            filename: 'shop.jpg',
            contentType: DioMediaType('image', 'jpeg'),
          ),
        ),
      );
    }
    return form;
  }
}

class PromoterApi {
  PromoterApi(this._client);

  final DioClient _client;
  static const _base = 'api/v1/app/field-promoter';

  Dio get _dio => _client.dio;

  Map _map(Object? data) {
    if (data is Map) return data;
    throw ApiException('استجابة غير صالحة من الخادم');
  }

  Future<PromoterMe> me() async =>
      PromoterMe.fromJson(_map((await _dio.get('$_base/me')).data));

  Future<ShiftState> startShift() async =>
      ShiftState.fromJson(_map((await _dio.post('$_base/shift/start')).data));

  Future<ShiftState> endShift() async =>
      ShiftState.fromJson(_map((await _dio.post('$_base/shift/end')).data));

  Future<TodaySummary> today() async =>
      TodaySummary.fromJson(_map((await _dio.get('$_base/today')).data));

  Future<IngestResult> sendLocations(List<Map<String, dynamic>> points) async {
    final res = await _dio.post('$_base/locations/batch', data: points);
    return IngestResult.fromJson(_map(res.data));
  }

  Future<OrderReceipt> submitOrder(OrderSubmission s) async {
    final res = await _dio.post(
      '$_base/orders',
      data: await s.toFormData(),
      options: Options(sendTimeout: const Duration(seconds: 90)),
    );
    return OrderReceipt.fromJson(_map(res.data));
  }

  Future<PageResult<PromoterOrder>> orders(OrderTab tab, int page) async {
    final res = await _dio.get(
      '$_base/orders',
      queryParameters: {'tab': tab.apiName, 'page': page},
    );
    final data = _map(res.data);
    final items = data['items'] is List
        ? (data['items'] as List).whereType<Map>().map(PromoterOrder.fromJson).toList()
        : <PromoterOrder>[];
    return PageResult(items, page, data['hasMore'] == true);
  }

  Future<OrderCounts> orderCounts() async =>
      OrderCounts.fromJson(_map((await _dio.get('$_base/orders/counts')).data));

  Future<PromoterOrder> order(int id) async =>
      PromoterOrder.fromJson(_map((await _dio.get('$_base/orders/$id')).data));

  Future<void> cancelOrder(int id, String? reason) async {
    await _dio.post(
      '$_base/orders/$id/cancel',
      data: {
        'reason': (reason ?? '').trim().isEmpty ? null : reason!.trim(),
      },
    );
  }

  Future<List<MapPin>> pinsInBox({
    required double minLat,
    required double minLng,
    required double maxLat,
    required double maxLng,
    CancelToken? cancelToken,
  }) async {
    final res = await _dio.get(
      '$_base/pins',
      queryParameters: {
        'minLat': minLat,
        'minLng': minLng,
        'maxLat': maxLat,
        'maxLng': maxLng,
      },
      cancelToken: cancelToken,
    );
    final data = res.data;
    if (data is! List) return const [];
    return data.whereType<Map>().map(MapPin.fromJson).toList();
  }

  Future<PageResult<ShopPin>> myPins({
    String? from,
    String? to,
    String? q,
    int page = 1,
    CancelToken? cancelToken,
  }) async {
    final res = await _dio.get(
      '$_base/pins/mine',
      queryParameters: {
        'from': ?from,
        'to': ?to,
        if (q != null && q.trim().isNotEmpty) 'q': q.trim(),
        'page': page,
      },
      cancelToken: cancelToken,
    );
    final data = _map(res.data);
    final items = data['items'] is List
        ? (data['items'] as List).whereType<Map>().map(ShopPin.fromJson).toList()
        : <ShopPin>[];
    final total = data['total'];
    return PageResult(
      items,
      page,
      data['hasMore'] == true,
      total: total is num ? total.toInt() : null,
    );
  }

  Future<ShopPin> pin(int id) async =>
      ShopPin.fromJson(_map((await _dio.get('$_base/pins/$id')).data));

  Future<ShopPin> createPin(PinInput input) async => ShopPin.fromJson(
    _map((await _dio.post('$_base/pins', data: input.toJson())).data),
  );

  Future<ShopPin> updatePin(int id, PinInput input) async => ShopPin.fromJson(
    _map((await _dio.put('$_base/pins/$id', data: input.toJson())).data),
  );

  Future<void> deletePin(int id) async {
    await _dio.delete('$_base/pins/$id');
  }
}
