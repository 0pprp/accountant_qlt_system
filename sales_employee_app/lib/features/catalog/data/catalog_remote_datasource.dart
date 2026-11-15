import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/network/json_map.dart';
import 'package:sales_employee_app/features/catalog/domain/entities/product_item.dart';

class CatalogRemoteDataSource {
  CatalogRemoteDataSource(this._dio);
  final DioClient _dio;

  Future<List<ProductItem>> getProducts({String? search}) async {
    final res = await _dio.dio.get(
      'api/v1/app/Products',
      queryParameters: {
        if (search != null && search.trim().isNotEmpty) 'SearchTerm': search.trim(),
      },
    );
    return _mapList(res.data);
  }

  Future<List<ProductItem>> getInventory({int? orderId, int? requestId}) async {
    try {
      final res = await _dio.dio.get('api/v1/app/SalesInventory/inventory', queryParameters: {if (orderId != null) 'orderId': orderId, if (requestId != null) 'requestId': requestId});
      return _mapList(res.data);
    } catch (_) {
      if (orderId != null || requestId != null) rethrow;
      return getProducts();
    }
  }

  Future<List<ProductItem>> getPriceBoard() async {
    try {
      final res = await _dio.dio.get('api/v1/app/SalesPriceBoard/price-board');
      return _mapList(res.data);
    } catch (_) {
      return getProducts();
    }
  }

  List<ProductItem> _mapList(Object? data) {
    final raw = _extract(data);
    return raw.map(ProductItem.fromJson).toList();
  }

  List<Map<String, dynamic>> _extract(Object? data) {
    if (data is List) return JsonMap.asMapList(data);
    if (data is Map) {
      final items = JsonMap.pick(data, ['items', 'Items', 'data', 'Data', 'products', 'Products']);
      return JsonMap.asMapList(items);
    }
    return const [];
  }
}
