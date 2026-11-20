import 'package:field_promoter_app/features/promoter/models.dart';
import 'package:field_promoter_app/features/shift/location_queue.dart';
import 'package:flutter_test/flutter_test.dart';

QueuedPoint point(String id, {int minute = 0}) => QueuedPoint(
  lat: 33.3 + minute / 1000,
  lng: 44.4,
  accuracy: 12,
  recordedAt: DateTime.utc(2026, 11, 21, 8, minute),
  clientEventId: id,
);

void main() {
  late LocationQueue queue;

  setUp(() => queue = LocationQueue(MemoryQueueStorage(), batchSize: 200));

  test('فشل الشبكة لا يحذف أي نقطة', () async {
    await queue.add(point('a'));
    await queue.add(point('b', minute: 1));
    final r = await queue.flushOnce((_) async => throw Exception('offline'));
    expect(r, FlushOutcome.failed);
    expect(await queue.length(), 2);
  });

  test('تُحذف النقاط المقبولة والمكررة والمرفوضة بعد رد الخادم فقط', () async {
    await queue.add(point('a'));
    await queue.add(point('b', minute: 1));
    await queue.add(point('c', minute: 2));
    final r = await queue.flushOnce(
      (batch) async => IngestResult(1, 1, 1, true),
    );
    expect(r, FlushOutcome.sent);
    expect(await queue.length(), 0);
  });

  test('رد ناقص لا يحذف شيئاً حتى يؤكد الخادم كل نقطة', () async {
    await queue.add(point('a'));
    await queue.add(point('b', minute: 1));
    final r = await queue.flushOnce((_) async => IngestResult(1, 0, 0, true));
    expect(r, FlushOutcome.failed);
    expect(await queue.length(), 2);
  });

  test('النقاط المضافة أثناء الإرسال تبقى في الطابور', () async {
    await queue.add(point('a'));
    final r = await queue.flushOnce((batch) async {
      await queue.add(point('late', minute: 5));
      return IngestResult(batch.length, 0, 0, true);
    });
    expect(r, FlushOutcome.sent);
    final left = await queue.snapshot();
    expect(left.map((p) => p.clientEventId), ['late']);
  });

  test('كل نقطة تحمل الدقة والمعرّف ووقت التسجيل', () async {
    await queue.add(point('x'));
    List<Map<String, dynamic>>? sent;
    await queue.flushOnce((batch) async {
      sent = batch;
      return IngestResult(1, 0, 0, true);
    });
    expect(sent!.single['clientEventId'], 'x');
    expect(sent!.single['accuracy'], 12);
    expect(sent!.single['recordedAt'], '2026-11-21T08:00:00.000Z');
    expect(sent!.single.containsKey('lat'), isTrue);
  });

  test('دفعات بحد أقصى batchSize', () async {
    final small = LocationQueue(MemoryQueueStorage(), batchSize: 2);
    for (var i = 0; i < 5; i++) {
      await small.add(point('p$i', minute: i));
    }
    final sizes = <int>[];
    final r = await small.flushAll((batch) async {
      sizes.add(batch.length);
      return IngestResult(batch.length, 0, 0, true);
    });
    expect(sizes, [2, 2, 1]);
    expect(r, FlushOutcome.empty);
    expect(await small.length(), 0);
  });

  test('isWorking=false يوقف التتبع ويفرغ الطابور', () async {
    await queue.add(point('a'));
    await queue.add(point('b', minute: 1));
    final r = await queue.flushOnce((batch) async => IngestResult(0, 0, 2, false));
    expect(r, FlushOutcome.stopped);
    expect(await queue.length(), 0);
  });

  test('المعرّف المكرر لا يُضاف مرتين والحد الأقصى يحذف الأقدم', () async {
    final capped = LocationQueue(MemoryQueueStorage(), maxPoints: 3);
    await capped.add(point('a'));
    await capped.add(point('a'));
    expect(await capped.length(), 1);
    for (var i = 0; i < 4; i++) {
      await capped.add(point('n$i', minute: i + 1));
    }
    final ids = (await capped.snapshot()).map((p) => p.clientEventId).toList();
    expect(ids, ['n1', 'n2', 'n3']);
  });
}
