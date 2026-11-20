import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

import '../promoter/models.dart';

abstract class QueueStorage {
  Future<String?> read();
  Future<void> write(String value);
  Future<void> clear();
}

class PrefsQueueStorage implements QueueStorage {
  static const _key = 'fp_location_queue_v1';

  @override
  Future<String?> read() async =>
      (await SharedPreferences.getInstance()).getString(_key);

  @override
  Future<void> write(String value) async =>
      (await SharedPreferences.getInstance()).setString(_key, value);

  @override
  Future<void> clear() async =>
      (await SharedPreferences.getInstance()).remove(_key);
}

class MemoryQueueStorage implements QueueStorage {
  String? value;

  @override
  Future<String?> read() async => value;

  @override
  Future<void> write(String v) async => value = v;

  @override
  Future<void> clear() async => value = null;
}

class QueuedPoint {
  const QueuedPoint({
    required this.lat,
    required this.lng,
    required this.accuracy,
    required this.recordedAt,
    required this.clientEventId,
  });

  final double lat;
  final double lng;
  final double? accuracy;
  final DateTime recordedAt;
  final String clientEventId;

  Map<String, dynamic> toJson() => {
    'lat': lat,
    'lng': lng,
    'accuracy': accuracy,
    'recordedAt': recordedAt.toUtc().toIso8601String(),
    'clientEventId': clientEventId,
  };

  static QueuedPoint? fromJson(Map j) {
    final lat = j['lat'];
    final lng = j['lng'];
    final at = DateTime.tryParse('${j['recordedAt'] ?? ''}');
    final id = '${j['clientEventId'] ?? ''}';
    if (lat is! num || lng is! num || at == null || id.isEmpty) return null;
    final acc = j['accuracy'];
    return QueuedPoint(
      lat: lat.toDouble(),
      lng: lng.toDouble(),
      accuracy: acc is num ? acc.toDouble() : null,
      recordedAt: at.toUtc(),
      clientEventId: id,
    );
  }
}

typedef BatchSender = Future<IngestResult> Function(List<Map<String, dynamic>> points);

enum FlushOutcome { empty, sent, failed, stopped, busy }

class LocationQueue {
  LocationQueue(this._storage, {this.maxPoints = 3000, this.batchSize = 200});

  final QueueStorage _storage;
  final int maxPoints;
  final int batchSize;
  bool _flushing = false;
  Future<void> _lock = Future.value();

  Future<T> _locked<T>(Future<T> Function() action) {
    final previous = _lock;
    final done = previous.then((_) => action());
    _lock = done.then((_) {}, onError: (_) {});
    return done;
  }

  Future<List<QueuedPoint>> _read() async {
    final raw = await _storage.read();
    if (raw == null || raw.isEmpty) return [];
    try {
      final decoded = jsonDecode(raw);
      if (decoded is! List) return [];
      return decoded
          .whereType<Map>()
          .map(QueuedPoint.fromJson)
          .whereType<QueuedPoint>()
          .toList();
    } catch (_) {
      return [];
    }
  }

  Future<void> _write(List<QueuedPoint> points) async {
    if (points.isEmpty) {
      await _storage.clear();
      return;
    }
    await _storage.write(jsonEncode(points.map((p) => p.toJson()).toList()));
  }

  Future<int> length() => _locked(() async => (await _read()).length);

  Future<List<QueuedPoint>> snapshot() => _locked(_read);

  Future<int> add(QueuedPoint point) => _locked(() async {
    final all = await _read();
    if (all.any((p) => p.clientEventId == point.clientEventId)) return all.length;
    all.add(point);
    final trimmed = all.length > maxPoints
        ? all.sublist(all.length - maxPoints)
        : all;
    await _write(trimmed);
    return trimmed.length;
  });

  Future<void> clear() => _locked(_storage.clear);

  Future<FlushOutcome> flushOnce(BatchSender send) async {
    if (_flushing) return FlushOutcome.busy;
    _flushing = true;
    try {
      final all = await snapshot();
      if (all.isEmpty) return FlushOutcome.empty;
      final batch = all.take(batchSize).toList();
      IngestResult result;
      try {
        result = await send(batch.map((p) => p.toJson()).toList());
      } catch (_) {
        return FlushOutcome.failed;
      }
      if (!result.isWorking) {
        await clear();
        return FlushOutcome.stopped;
      }
      if (result.total < batch.length) return FlushOutcome.failed;
      final sentIds = batch.map((p) => p.clientEventId).toSet();
      await _locked(() async {
        final current = await _read();
        await _write(
          current.where((p) => !sentIds.contains(p.clientEventId)).toList(),
        );
      });
      return FlushOutcome.sent;
    } finally {
      _flushing = false;
    }
  }

  Future<FlushOutcome> flushAll(BatchSender send, {int maxRounds = 20}) async {
    var last = FlushOutcome.empty;
    for (var i = 0; i < maxRounds; i++) {
      last = await flushOnce(send);
      if (last != FlushOutcome.sent) return last;
    }
    return last;
  }
}
