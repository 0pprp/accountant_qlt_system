import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';
import 'package:sqflite/sqflite.dart';

class AppDatabase {
  Database? _db;

  Future<Database> get database async {
    if (_db != null) return _db!;
    final dir = await getApplicationDocumentsDirectory();
    final path = p.join(dir.path, 'delegate_app.db');
    _db = await openDatabase(
      path,
      version: 4,
      onCreate: (db, version) async {
        await db.execute('''
CREATE TABLE customers (
  customer_id INTEGER PRIMARY KEY,
  delegate_id INTEGER NOT NULL,
  name TEXT NOT NULL,
  phone TEXT,
  address TEXT,
  shop_name TEXT,
  sale_price REAL,
  remaining REAL,
  daily_installment REAL,
  last_payment_date TEXT,
  date_sale_device TEXT,
  receipt_total REAL,
  count_receipt INTEGER,
  active_order_id INTEGER,
  payload_json TEXT
)''');
        await db.execute('''
CREATE TABLE pending_payments (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  client_payment_id TEXT NOT NULL UNIQUE,
  customer_id INTEGER NOT NULL,
  customer_name TEXT,
  delegate_id INTEGER NOT NULL,
  order_id INTEGER,
  amount REAL NOT NULL,
  location TEXT,
  created_at_utc TEXT NOT NULL,
  receipt_number TEXT,
  sync_status TEXT NOT NULL,
  permanent_failure INTEGER NOT NULL DEFAULT 0,
  owner_async_id TEXT,
  sync_error TEXT
)''');
        await db.execute('''
CREATE TABLE business_day_locks (
  customer_id INTEGER NOT NULL,
  delegate_id INTEGER NOT NULL,
  business_day TEXT NOT NULL,
  PRIMARY KEY (customer_id, delegate_id, business_day)
)''');
      },
      onUpgrade: (db, oldVersion, newVersion) async {
        if (oldVersion < 2) {
          await db.execute(
            'ALTER TABLE customers ADD COLUMN date_sale_device TEXT',
          );
        }
        if (oldVersion < 3) {
          await db.execute(
            'ALTER TABLE customers ADD COLUMN active_order_id INTEGER',
          );
          await db.execute(
            'ALTER TABLE pending_payments ADD COLUMN order_id INTEGER',
          );
        }
        if (oldVersion < 4) {
          await db.execute(
            'ALTER TABLE pending_payments ADD COLUMN sync_error TEXT',
          );
        }
      },
    );
    return _db!;
  }
}
