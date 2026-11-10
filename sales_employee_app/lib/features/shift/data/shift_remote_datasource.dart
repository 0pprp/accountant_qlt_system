import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/network/json_map.dart';

class ShiftInfo {
  const ShiftInfo({
    required this.active,
    this.startedAt,
    this.id,
  });

  final bool active;
  final String? startedAt;
  final int? id;

  factory ShiftInfo.fromJson(Map<String, dynamic> json) {
    return ShiftInfo(
      active: JsonMap.asBool(json, [
        'active',
        'Active',
        'isActive',
        'IsActive',
        'hasActiveShift',
        'HasActiveShift',
      ]),
      startedAt: JsonMap.str(json, [
        'startedAt',
        'StartedAt',
        'startedAtUtc',
        'StartedAtUtc',
        'startAt',
        'StartAt',
      ]).isEmpty
          ? null
          : JsonMap.str(json, [
              'startedAt',
              'StartedAt',
              'startedAtUtc',
              'StartedAtUtc',
              'startAt',
              'StartAt',
            ]),
      id: JsonMap.asInt(json, ['id', 'Id', 'shiftId', 'ShiftId']),
    );
  }
}

class ShiftRemoteDataSource {
  ShiftRemoteDataSource(this._dio);
  final DioClient _dio;

  Future<ShiftInfo> current() async {
    final res = await _dio.dio.get('api/v1/app/SalesShifts/current');
    if (res.data is Map) {
      return ShiftInfo.fromJson(Map<String, dynamic>.from(res.data as Map));
    }
    return const ShiftInfo(active: false);
  }

  Future<ShiftInfo> start({double? lat, double? lng}) async {
    final res = await _dio.dio.post(
      'api/v1/app/SalesShifts/start',
      data: {
        if (lat != null) 'latitude': lat,
        if (lng != null) 'longitude': lng,
      },
    );
    if (res.data is Map) {
      final info = ShiftInfo.fromJson(Map<String, dynamic>.from(res.data as Map));
      if (info.active) return info;
      return ShiftInfo(active: true, startedAt: info.startedAt, id: info.id);
    }
    return const ShiftInfo(active: true);
  }

  Future<void> end({double? lat, double? lng}) async {
    await _dio.dio.post(
      'api/v1/app/SalesShifts/end',
      data: {
        if (lat != null) 'latitude': lat,
        if (lng != null) 'longitude': lng,
      },
    );
  }
}
