import 'package:sqflite/sqflite.dart';

import '../../../core/database/app_database.dart';
import '../../../core/time/iraq_date.dart';
import '../domain/entities/pending_payment.dart';

class PaymentsLocalDataSource {
  PaymentsLocalDataSource(this._db);

  final AppDatabase _db;

  Future<int> countPending() async {
    final db = await _db.database;
    final rows = await db.rawQuery('''
SELECT COUNT(1) AS c FROM pending_payments
WHERE permanent_failure = 0
  AND sync_status IN (?, ?, ?)
''', [
      PaymentSyncStatuses.pending,
      PaymentSyncStatuses.failed,
      PaymentSyncStatuses.syncing,
    ]);
    return (rows.first['c'] as num?)?.toInt() ?? 0;
  }

  Future<bool> hasDayLock({
    required int customerId,
    required int delegateId,
    required String businessDay,
  }) async {
    final db = await _db.database;
    final rows = await db.query(
      'business_day_locks',
      where: 'customer_id = ? AND delegate_id = ? AND business_day = ?',
      whereArgs: [customerId, delegateId, businessDay],
      limit: 1,
    );
    return rows.isNotEmpty;
  }

  /// زبائن جرى تحصيلهم محلياً (أي تسديد معلّق أو مقفل يومياً) — لمنطق المبيعات الجديدة.
  Future<Set<int>> collectedCustomerIds() async {
    final db = await _db.database;
    final pending = await db.query(
      'pending_payments',
      columns: ['customer_id'],
      distinct: true,
    );
    final locks = await db.query(
      'business_day_locks',
      columns: ['customer_id'],
      distinct: true,
    );
    final ids = <int>{};
    for (final row in [...pending, ...locks]) {
      final id = (row['customer_id'] as num?)?.toInt() ?? 0;
      if (id > 0) ids.add(id);
    }
    return ids;
  }

  Future<bool> hasUnsyncedForCustomer(int customerId) async {
    final db = await _db.database;
    final rows = await db.query(
      'pending_payments',
      where: '''
customer_id = ?
AND permanent_failure = 0
AND sync_status IN (?, ?, ?)
''',
      whereArgs: [
        customerId,
        PaymentSyncStatuses.pending,
        PaymentSyncStatuses.failed,
        PaymentSyncStatuses.syncing,
      ],
      limit: 1,
    );
    return rows.isNotEmpty;
  }

  Future<void> insertDayLock({
    required int customerId,
    required int delegateId,
    required String businessDay,
  }) async {
    final db = await _db.database;
    await db.insert(
      'business_day_locks',
      {
        'customer_id': customerId,
        'delegate_id': delegateId,
        'business_day': businessDay,
      },
      conflictAlgorithm: ConflictAlgorithm.replace,
    );
  }

  Future<void> clearDayLock({
    required int customerId,
    required int delegateId,
    required String businessDay,
  }) async {
    final db = await _db.database;
    await db.delete(
      'business_day_locks',
      where: 'customer_id = ? AND delegate_id = ? AND business_day = ?',
      whereArgs: [customerId, delegateId, businessDay],
    );
  }

  Future<int> insertPending(PendingPayment payment) async {
    final db = await _db.database;
    return db.insert('pending_payments', {
      'client_payment_id': payment.clientPaymentId,
      'customer_id': payment.customerId,
      'customer_name': payment.customerName,
      'delegate_id': payment.delegateId,
      'order_id': payment.orderId,
      'amount': payment.amount,
      'location': payment.location,
      'created_at_utc': payment.createdAtUtc,
      'receipt_number': payment.receiptNumber,
      'sync_status': payment.syncStatus,
      'permanent_failure': payment.permanentFailure ? 1 : 0,
      'owner_async_id': payment.ownerAsyncId,
      'sync_error': payment.syncError,
    });
  }

  Future<void> updateOrderId(int id, int orderId) async {
    final db = await _db.database;
    await db.update(
      'pending_payments',
      {'order_id': orderId},
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  /// استرداد طابور نفس المندوب بعد تغيير مفتاح المالك (JWT) — لا يستولي على حساب آخر.
  Future<int> reclaimForOwner({
    required String ownerKey,
    required int delegateId,
  }) async {
    final db = await _db.database;
    return db.rawUpdate('''
UPDATE pending_payments
SET owner_async_id = ?,
    permanent_failure = 0
WHERE delegate_id = ?
  AND (
    owner_async_id IS NULL
    OR owner_async_id = ''
    OR owner_async_id = ?
    OR permanent_failure = 1
  )
''', [ownerKey, delegateId, ownerKey]);
  }

  Future<List<PendingPayment>> listUploadable({
    required String ownerAsyncId,
    int? delegateId,
  }) async {
    final db = await _db.database;
    // عزل الحساب: لا تُرفع طوابير مالك آخر. يُسمح فقط بمالك الجلسة الحالية
    // أو صفوف نفس المندوب ذات مالك فارغ/قديم (استرداد صريح لنفس المستخدم).
    final rows = await db.query(
      'pending_payments',
      where: '''
permanent_failure = 0
AND sync_status IN (?, ?, ?)
AND (
  owner_async_id = ?
  OR (
    (owner_async_id IS NULL OR owner_async_id = '')
    AND delegate_id = ?
  )
)
''',
      whereArgs: [
        PaymentSyncStatuses.pending,
        PaymentSyncStatuses.failed,
        PaymentSyncStatuses.syncing,
        ownerAsyncId,
        delegateId ?? -1,
      ],
      orderBy: 'id ASC',
    );
    return rows.map(_map).toList();
  }

  Future<List<PendingPayment>> listAll() async {
    final db = await _db.database;
    final rows = await db.query('pending_payments', orderBy: 'id DESC');
    return rows.map(_map).toList();
  }

  Future<List<PendingPayment>> listToday() async {
    final day = IraqDate.isoDate();
    final db = await _db.database;
    final rows = await db.query(
      'pending_payments',
      orderBy: 'id DESC',
    );
    final mapped = rows.map(_map).where((p) {
      try {
        final created = DateTime.parse(p.createdAtUtc).toUtc();
        return IraqDate.isoDate(created) == day;
      } catch (_) {
        return p.createdAtUtc.startsWith(day);
      }
    }).toList();
    return mapped;
  }

  Future<void> markSyncing(int id) async {
    final db = await _db.database;
    await db.update(
      'pending_payments',
      {
        'sync_status': PaymentSyncStatuses.syncing,
        'permanent_failure': 0,
      },
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  Future<void> markFailed(
    int id, {
    bool permanent = false,
    String? error,
  }) async {
    final db = await _db.database;
    await db.update(
      'pending_payments',
      {
        'sync_status': PaymentSyncStatuses.failed,
        'permanent_failure': permanent ? 1 : 0,
        if (error != null) 'sync_error': error,
      },
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  Future<void> clearSyncError(int id) async {
    final db = await _db.database;
    await db.update(
      'pending_payments',
      {'sync_error': null},
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  Future<void> deleteById(int id) async {
    final db = await _db.database;
    await db.delete('pending_payments', where: 'id = ?', whereArgs: [id]);
  }

  int? _asInt(Object? v) {
    if (v == null) return null;
    if (v is int) return v;
    if (v is num) return v.toInt();
    return int.tryParse('$v');
  }

  PendingPayment _map(Map<String, Object?> row) {
    return PendingPayment(
      id: _asInt(row['id']) ?? 0,
      customerId: _asInt(row['customer_id']) ?? 0,
      customerName: row['customer_name'] as String?,
      delegateId: _asInt(row['delegate_id']) ?? 0,
      orderId: _asInt(row['order_id']),
      amount: (row['amount'] as num?)?.toDouble() ?? 0,
      location: '${row['location'] ?? ''}',
      clientPaymentId: '${row['client_payment_id']}',
      createdAtUtc: '${row['created_at_utc']}',
      receiptNumber: row['receipt_number'] as String?,
      ownerAsyncId: row['owner_async_id'] as String?,
      syncStatus: '${row['sync_status']}',
      syncError: row['sync_error'] as String?,
      permanentFailure: (_asInt(row['permanent_failure']) ?? 0) == 1,
    );
  }
}
