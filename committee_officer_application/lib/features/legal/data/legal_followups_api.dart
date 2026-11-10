import 'package:dio/dio.dart';

class LegalFollowupsApi {
  LegalFollowupsApi(this._dio);
  final Dio _dio;
  static const _base = 'api/v1/app/committee/legal-followups';

  Future<List<Map<String, dynamic>>> list({String status = 'InProgress'}) async {
    final res = await _dio.get(_base, queryParameters: {'status': status, 'page': 1, 'pageSize': 50});
    final items = (res.data as Map)['items'] as List? ?? const [];
    return items.map((e) => Map<String, dynamic>.from(e as Map)).toList();
  }

  Future<Map<String, dynamic>> getById(int id) async {
    final res = await _dio.get('$_base/$id');
    return Map<String, dynamic>.from(res.data as Map);
  }

  Future<Map<String, dynamic>> setLocation(int id, {required double lat, required double lng, int? version}) async {
    final res = await _dio.post('$_base/$id/location', data: {
      'latitude': lat,
      'longitude': lng,
      if (version != null) 'expectedVersion': version,
    });
    return Map<String, dynamic>.from(res.data as Map);
  }

  Future<void> uploadDocument(int id, {required String type, required MultipartFile file}) async {
    final form = FormData.fromMap({'documentType': type, 'file': file});
    await _dio.post('$_base/$id/documents', data: form);
  }

  Future<Map<String, dynamic>> complete(int id, {String? note, int? version}) async {
    final res = await _dio.post('$_base/$id/complete', data: {
      if (note != null) 'note': note,
      if (version != null) 'expectedVersion': version,
    });
    return Map<String, dynamic>.from(res.data as Map);
  }
}
