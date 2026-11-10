import 'dart:convert';

import '../../../core/database/app_database.dart';
import '../domain/entities/customer.dart';

class CustomersLocalDataSource {
  CustomersLocalDataSource(this._db);

  final AppDatabase _db;

  Future<List<Customer>> list({String? query}) async {
    final db = await _db.database;
    final q = query?.trim();
    final rows = (q == null || q.isEmpty)
        ? await db.query('customers', orderBy: 'name COLLATE NOCASE')
        : await db.query(
            'customers',
            where:
                'name LIKE ? OR phone LIKE ? OR shop_name LIKE ? OR CAST(customer_id AS TEXT) LIKE ?',
            whereArgs: ['%$q%', '%$q%', '%$q%', '%$q%'],
            orderBy: 'name COLLATE NOCASE',
          );
    return rows.map(_map).toList();
  }

  Future<Customer?> byId(int id) async {
    final db = await _db.database;
    final rows = await db.query(
      'customers',
      where: 'customer_id = ?',
      whereArgs: [id],
      limit: 1,
    );
    if (rows.isEmpty) return null;
    return _map(rows.first);
  }

  Future<void> replaceAll(List<Customer> customers) async {
    final db = await _db.database;
    final batch = db.batch();
    batch.delete('customers');
    for (final c in customers) {
      batch.insert('customers', {
        'customer_id': c.customerId,
        'delegate_id': c.delegateId,
        'name': c.name,
        'phone': c.phone,
        'address': c.address,
        'shop_name': c.shopName,
        'sale_price': c.salePrice,
        'remaining': c.remaining,
        'daily_installment': c.dailyInstallment,
        'last_payment_date': c.lastPaymentDate,
        'date_sale_device': c.dateSaleDevice,
        'receipt_total': c.receiptTotal,
        'count_receipt': c.countReceipt,
        'active_order_id': c.activeOrderId,
        'payload_json': jsonEncode({
          'customerId': c.customerId,
          'name': c.name,
          'activeOrderId': c.activeOrderId,
        }),
      });
    }
    await batch.commit(noResult: true);
  }

  Customer _map(Map<String, Object?> row) {
    return Customer(
      customerId: row['customer_id'] as int,
      name: '${row['name']}',
      phone: row['phone'] as String?,
      address: row['address'] as String?,
      shopName: row['shop_name'] as String?,
      salePrice: (row['sale_price'] as num?)?.toDouble(),
      remaining: (row['remaining'] as num?)?.toDouble(),
      dailyInstallment: (row['daily_installment'] as num?)?.toDouble(),
      lastPaymentDate: row['last_payment_date'] as String?,
      dateSaleDevice: row['date_sale_device'] as String?,
      receiptTotal: (row['receipt_total'] as num?)?.toDouble(),
      countReceipt: row['count_receipt'] as int?,
      delegateId: row['delegate_id'] as int,
      activeOrderId: (row['active_order_id'] as num?)?.toInt(),
    );
  }
}
