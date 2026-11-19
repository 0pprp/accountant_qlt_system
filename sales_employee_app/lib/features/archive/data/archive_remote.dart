import '../../../core/network/dio_client.dart';
import '../../../core/network/json_map.dart';
import '../domain/archive_models.dart';

class ArchiveRemote {
  ArchiveRemote(this._dio);
  final DioClient _dio;

  static const _base = 'api/v1/app/Archive';

  Future<List<ArchiveBundle>> bundles({
    int? stage,
    bool pendingOnly = false,
    int pageSize = 50,
  }) async {
    final res = await _dio.dio.get(
      '$_base/bundles',
      queryParameters: {
        'pageIndex': 1,
        'pageSize': pageSize,
        'stage': ?stage,
        'pendingOnly': pendingOnly,
      },
    );

    final data = res.data;
    if (data is! Map) return const [];
    final items = JsonMap.pick(data, ['items', 'Items']);
    if (items is! List) return const [];

    return items
        .whereType<Map>()
        .map((m) => ArchiveBundle.fromJson(Map<String, dynamic>.from(m)))
        .toList();
  }

  Future<ArchiveBundleDetail> bundle(int id, {int? stage}) async {
    final res = await _dio.dio.get(
      '$_base/bundles/$id',
      queryParameters: {'stage': ?stage},
    );

    final data = res.data;
    if (data is! Map) {
      throw StateError('Unexpected bundle response');
    }
    return ArchiveBundleDetail.fromJson(Map<String, dynamic>.from(data));
  }

  Future<ArchiveActionResult> send(int bundleId, int fromStage, {String? note}) =>
      _post('$_base/bundles/$bundleId/send', {'fromStage': fromStage, 'note': note});

  Future<ArchiveActionResult> receive(List<int> ids, int atStage, {String? note}) =>
      _post('$_base/transactions/receive',
          {'transactionIds': ids, 'atStage': atStage, 'note': note});

  Future<ArchiveActionResult> undoSend(List<int> ids, int fromStage) =>
      _post('$_base/transactions/undo-send',
          {'transactionIds': ids, 'fromStage': fromStage});

  Future<ArchiveActionResult> _post(String path, Map<String, dynamic> body) async {
    final res = await _dio.dio.post(path, data: body);
    final data = res.data;
    if (data is Map) {
      return ArchiveActionResult.fromJson(Map<String, dynamic>.from(data));
    }
    return const ArchiveActionResult(message: 'تم', affectedCount: 0);
  }
}
