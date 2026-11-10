import 'package:dio/dio.dart';

class SalesRequestSubmission {
  SalesRequestSubmission(this._dio, this._source);
  final Dio _dio;
  final String _source;
  static String phone(String input) => input.trim().split('').map((c) {
    final n = c.codeUnitAt(0);
    if (n >= 0x660 && n <= 0x669) return '${n - 0x660}';
    if (n >= 0x6f0 && n <= 0x6f9) return '${n - 0x6f0}';
    if (c.trim().isEmpty) return '';
    return c;
  }).join();
  static bool validPhone(String input) => RegExp(r'^07[0-9]{9}$').hasMatch(phone(input));
  Future<List<Map<String, dynamic>>> branches() async {
    final response = await _dio.get('api/v1/app/SalesRequests/submission-branches/$_source');
    return (response.data as List).map((e) => Map<String, dynamic>.from(e as Map)).toList();
  }
  Future<int> submit(Map<String, dynamic> payload) async {
    final response = await _dio.post('api/v1/app/SalesRequests/submit/$_source', data: payload);
    return (response.data['requestId'] as num).toInt();
  }
}
