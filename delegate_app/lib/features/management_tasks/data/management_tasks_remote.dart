import '../../../core/network/dio_client.dart';
import '../../../core/network/json_map.dart';
import '../domain/management_task.dart';

class ManagementTasksRemote {
  ManagementTasksRemote(this._dio);
  final DioClient _dio;

  static const _base = 'api/v1/app/ManagementTasks';

  Future<List<ManagementTaskItem>> inbox({int pageIndex = 1, int pageSize = 50}) async {
    final res = await _dio.dio.get(
      '$_base/inbox',
      queryParameters: {
        'PageIndex': pageIndex,
        'PageSize': pageSize,
      },
    );
    final data = res.data;
    if (data is! Map) return const [];
    final items = JsonMap.pick(data, ['items', 'Items']);
    if (items is! List) return const [];
    return items
        .whereType<Map>()
        .map((m) => ManagementTaskItem.fromJson(Map<String, dynamic>.from(m)))
        .toList();
  }

  Future<ManagementTaskItem> accept(int id, int expectedVersion) =>
      _action(id, 'accept', {'expectedVersion': expectedVersion});

  Future<ManagementTaskItem> complete(int id, int expectedVersion) =>
      _action(id, 'complete', {'expectedVersion': expectedVersion});

  Future<ManagementTaskItem> reject(int id, int expectedVersion, String note) =>
      _action(id, 'reject', {
        'expectedVersion': expectedVersion,
        'note': note,
      });

  Future<ManagementTaskItem> _action(
    int id,
    String path,
    Map<String, dynamic> body,
  ) async {
    final res = await _dio.dio.post('$_base/$id/$path', data: body);
    final data = res.data;
    if (data is Map) {
      return ManagementTaskItem(
        id: JsonMap.asInt(data, ['id', 'Id']) ?? id,
        title: '',
        description: '',
        status: JsonMap.asInt(data, ['status', 'Status']) ?? 0,
        priority: 2,
        dueDate: '',
        assigneeDisplayName: '',
        expectedVersion: JsonMap.asInt(data, ['expectedVersion', 'ExpectedVersion']) ?? expectedVersionOf(body),
        isOverdue: false,
        rejectNote: null,
      );
    }
    throw StateError('Unexpected action response');
  }

  static int expectedVersionOf(Map<String, dynamic> body) =>
      (body['expectedVersion'] as int?) ?? 0;
}
