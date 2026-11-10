import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';
import 'package:sqflite/sqflite.dart';

class AppDatabase {
  Database? _db;

  /// Web has no path_provider documents directory / native sqflite.
  /// Callers that need local persistence on web use in-memory fallbacks instead.
  bool get supportsSqlite => !kIsWeb;

  Future<Database> get database async {
    if (kIsWeb) {
      throw UnsupportedError(
        'SQLite/path_provider unavailable on web; use web memory fallbacks.',
      );
    }
    if (_db != null) return _db!;
    final dir = await getApplicationDocumentsDirectory();
    final path = p.join(dir.path, 'sales_employee_app.db');
    _db = await openDatabase(
      path,
      version: 2,
      onCreate: (db, version) async {
        await _createV1(db);
        await _createV2(db);
      },
      onUpgrade: (db, oldVersion, newVersion) async {
        if (oldVersion < 2) await _createV2(db);
      },
    );
    return _db!;
  }

  Future<void> _createV1(Database db) async {
    await db.execute('''
CREATE TABLE IF NOT EXISTS location_queue (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  lat REAL NOT NULL,
  lng REAL NOT NULL,
  recorded_at_utc TEXT NOT NULL,
  synced INTEGER NOT NULL DEFAULT 0
)''');
    await db.execute('''
CREATE TABLE IF NOT EXISTS customers_cache (
  customer_id INTEGER PRIMARY KEY,
  full_name TEXT NOT NULL,
  phone TEXT,
  payload_json TEXT
)''');
    await db.execute('''
CREATE TABLE IF NOT EXISTS products_cache (
  product_id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  price REAL,
  daily_installment REAL,
  payload_json TEXT
)''');
  }

  Future<void> _createV2(Database db) async {
    await db.execute('''
CREATE TABLE IF NOT EXISTS cache_kv (
  cache_key TEXT PRIMARY KEY,
  payload_json TEXT NOT NULL,
  expires_at_utc TEXT NOT NULL
)''');
    await db.execute('''
CREATE TABLE IF NOT EXISTS demo_orders (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  customer_name TEXT NOT NULL,
  status TEXT NOT NULL,
  total REAL,
  created_at TEXT NOT NULL,
  payload_json TEXT
)''');
  }
}
