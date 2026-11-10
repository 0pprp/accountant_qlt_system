import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/network/json_map.dart';

class RatingSummary {
  const RatingSummary({
    required this.level,
    required this.label,
    required this.count,
  });

  final String level;
  final String label;
  final int count;
}

class RatingsRemoteDataSource {
  RatingsRemoteDataSource(this._dio);
  final DioClient _dio;

  Future<List<RatingSummary>> summary() async {
    try {
      final res = await _dio.dio.get('api/v1/app/SalesRatings/summary');
      if (res.data is Map) {
        final m = Map<String, dynamic>.from(res.data as Map);
        return [
          RatingSummary(
            level: 'ضعيف',
            label: 'ضعيف',
            count: JsonMap.asInt(m, ['weakCount', 'WeakCount']) ?? 0,
          ),
          RatingSummary(
            level: 'متلكئ',
            label: 'متلكئ',
            count: JsonMap.asInt(m, ['laggingCount', 'LaggingCount']) ?? 0,
          ),
          RatingSummary(
            level: 'سيء',
            label: 'سيء',
            count: JsonMap.asInt(m, ['badCount', 'BadCount']) ?? 0,
          ),
        ];
      }
      final list = _extract(res.data);
      return list
          .map(
            (m) => RatingSummary(
              level: JsonMap.str(m, ['level', 'Level', 'key', 'Key']),
              label: JsonMap.str(m, ['label', 'Label', 'title', 'Title']),
              count: JsonMap.asInt(m, ['count', 'Count']) ?? 0,
            ),
          )
          .toList();
    } catch (_) {
      return const [
        RatingSummary(level: 'ضعيف', label: 'ضعيف', count: 0),
        RatingSummary(level: 'متلكئ', label: 'متلكئ', count: 0),
        RatingSummary(level: 'سيء', label: 'سيء', count: 0),
      ];
    }
  }

  Future<List<Map<String, dynamic>>> byLevel(String level) async {
    try {
      final res = await _dio.dio.get(
        'api/v1/app/SalesRatings/by-level',
        queryParameters: {'level': level},
      );
      return _extract(res.data);
    } catch (_) {
      return const [];
    }
  }

  Future<Map<String, dynamic>?> profileLite(int customerId) async {
    try {
      final res = await _dio.dio.get('api/v1/app/SalesRatings/customer/$customerId/profile-lite');
      if (res.data is Map) return Map<String, dynamic>.from(res.data as Map);
    } catch (_) {}
    return null;
  }

  List<Map<String, dynamic>> _extract(Object? data) {
    if (data is List) return JsonMap.asMapList(data);
    if (data is Map) {
      final items = JsonMap.pick(data, ['items', 'Items', 'data', 'Data']);
      return JsonMap.asMapList(items);
    }
    return const [];
  }
}
