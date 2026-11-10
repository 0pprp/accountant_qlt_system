import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:sales_employee_app/core/database/app_database.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';

class LocationQueueDataSource {
  LocationQueueDataSource(this._db, this._dio);

  final AppDatabase _db;
  final DioClient _dio;

  /// In-memory queue for web (no SQLite / path_provider).
  final List<Map<String, Object?>> _webQueue = [];
  int _webIdSeq = 1;

  Future<void> enqueue({
    required double lat,
    required double lng,
    required DateTime recordedAtUtc,
  }) async {
    if (kIsWeb || !_db.supportsSqlite) {
      _webQueue.add({
        'id': _webIdSeq++,
        'lat': lat,
        'lng': lng,
        'recorded_at_utc': recordedAtUtc.toUtc().toIso8601String(),
        'synced': 0,
      });
      return;
    }
    final db = await _db.database;
    await db.insert('location_queue', {
      'lat': lat,
      'lng': lng,
      'recorded_at_utc': recordedAtUtc.toUtc().toIso8601String(),
      'synced': 0,
    });
  }

  Future<int> pendingCount() async {
    if (kIsWeb || !_db.supportsSqlite) {
      return _webQueue.where((r) => r['synced'] == 0).length;
    }
    final db = await _db.database;
    final r = await db.rawQuery(
      'SELECT COUNT(*) AS c FROM location_queue WHERE synced = 0',
    );
    return (r.first['c'] as int?) ?? 0;
  }

  Future<int> flush() async {
    if (kIsWeb || !_db.supportsSqlite) {
      final rows = _webQueue.where((r) => r['synced'] == 0).take(200).toList();
      if (rows.isEmpty) return 0;
      final batch = rows
          .map(
            (r) => {
              'lat': r['lat'],
              'lng': r['lng'],
              'recordedAtUtc': r['recorded_at_utc'],
            },
          )
          .toList();
      try {
        await _dio.dio.post('api/v1/app/SalesLocations/batch', data: batch);
        for (final r in rows) {
          r['synced'] = 1;
        }
        return rows.length;
      } catch (_) {
        return 0;
      }
    }

    final db = await _db.database;
    final rows = await db.query(
      'location_queue',
      where: 'synced = 0',
      orderBy: 'id ASC',
      limit: 200,
    );
    if (rows.isEmpty) return 0;

    // API expects a raw JSON array of points (with recordedAtUtc).
    final batch = rows
        .map(
          (r) => {
            'lat': r['lat'],
            'lng': r['lng'],
            'recordedAtUtc': r['recorded_at_utc'],
          },
        )
        .toList();

    try {
      await _dio.dio.post('api/v1/app/SalesLocations/batch', data: batch);
      final ids = rows.map((r) => r['id'] as int).toList();
      final batchOp = db.batch();
      for (final id in ids) {
        batchOp.update('location_queue', {'synced': 1}, where: 'id = ?', whereArgs: [id]);
      }
      await batchOp.commit(noResult: true);
      return ids.length;
    } catch (_) {
      return 0;
    }
  }

  /// Live ping. When [enqueueOnFailOnly] is true, queue only if live POST fails
  /// (caller already enqueued). Otherwise always enqueue on fail as before.
  Future<void> sendLive({
    required double lat,
    required double lng,
    DateTime? recordedAtUtc,
    bool enqueueOnFailOnly = false,
  }) async {
    final stamp = (recordedAtUtc ?? DateTime.now().toUtc()).toUtc();
    try {
      await _dio.dio.post(
        'api/v1/app/SalesLocations/live',
        data: {
          'lat': lat,
          'lng': lng,
          'recordedAtUtc': stamp.toIso8601String(),
        },
      );
    } catch (_) {
      if (!enqueueOnFailOnly) {
        await enqueue(lat: lat, lng: lng, recordedAtUtc: stamp);
      }
    }
  }
}
