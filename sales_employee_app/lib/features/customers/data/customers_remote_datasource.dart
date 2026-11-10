import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/network/json_map.dart';
import 'package:sales_employee_app/features/customers/domain/entities/customer_lite.dart';

class CustomersRemoteDataSource {
  CustomersRemoteDataSource(this._dio);
  final DioClient _dio;

  Future<List<CustomerLite>> search({String? query, int take = 40}) async {
    final res = await _dio.dio.get(
      'api/v1/app/Customers',
        queryParameters: {
        if (query != null && query.trim().isNotEmpty) 'SearchTerm': query.trim(),
        'Size': take,
      },
    );
    final list = _extractList(res.data);
    return list.map(CustomerLite.fromJson).toList();
  }

  Future<CustomerLite?> getById(int id) async {
    final res = await _dio.dio.get('api/v1/app/Customers/$id');
    if (res.data is Map) return CustomerLite.fromJson(Map<String, dynamic>.from(res.data as Map));
    return null;
  }

  List<Map<String, dynamic>> _extractList(Object? data) {
    if (data is List) return JsonMap.asMapList(data);
    if (data is Map) {
      final items = JsonMap.pick(data, ['items', 'Items', 'data', 'Data', 'results', 'Results']);
      return JsonMap.asMapList(items);
    }
    return const [];
  }
}
