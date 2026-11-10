import 'dart:convert';

import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:sales_employee_app/core/database/app_database.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:sqflite/sqflite.dart';

class AppCache {
  AppCache(this._db);
  final AppDatabase _db;

  static const _webPrefix = 'app_cache:';

  Future<void> putJson(String key, Object value, {Duration ttl = const Duration(minutes: 5)}) async {
    final expires = DateTime.now().toUtc().add(ttl).toIso8601String();
    final payload = jsonEncode({'expires_at_utc': expires, 'value': value});
    if (kIsWeb || !_db.supportsSqlite) {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('$_webPrefix$key', payload);
      return;
    }
    final db = await _db.database;
    await db.insert(
      'cache_kv',
      {
        'cache_key': key,
        'payload_json': jsonEncode(value),
        'expires_at_utc': expires,
      },
      conflictAlgorithm: ConflictAlgorithm.replace,
    );
  }

  Future<Object?> getJson(String key) async {
    if (kIsWeb || !_db.supportsSqlite) {
      final prefs = await SharedPreferences.getInstance();
      final raw = prefs.getString('$_webPrefix$key');
      if (raw == null) return null;
      final map = jsonDecode(raw);
      if (map is! Map) return null;
      final expires = DateTime.tryParse('${map['expires_at_utc']}');
      if (expires != null && expires.isBefore(DateTime.now().toUtc())) {
        await prefs.remove('$_webPrefix$key');
        return null;
      }
      return map['value'];
    }
    final db = await _db.database;
    final rows = await db.query('cache_kv', where: 'cache_key = ?', whereArgs: [key], limit: 1);
    if (rows.isEmpty) return null;
    final expires = DateTime.tryParse('${rows.first['expires_at_utc']}');
    if (expires != null && expires.isBefore(DateTime.now().toUtc())) {
      await db.delete('cache_kv', where: 'cache_key = ?', whereArgs: [key]);
      return null;
    }
    return jsonDecode('${rows.first['payload_json']}');
  }

  Future<void> invalidate(String key) async {
    if (kIsWeb || !_db.supportsSqlite) {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove('$_webPrefix$key');
      return;
    }
    final db = await _db.database;
    await db.delete('cache_kv', where: 'cache_key = ?', whereArgs: [key]);
  }

  Future<void> invalidatePrefix(String prefix) async {
    if (kIsWeb || !_db.supportsSqlite) {
      final prefs = await SharedPreferences.getInstance();
      final keys = prefs.getKeys().where((k) => k.startsWith('$_webPrefix$prefix')).toList();
      for (final k in keys) {
        await prefs.remove(k);
      }
      return;
    }
    final db = await _db.database;
    await db.delete('cache_kv', where: 'cache_key LIKE ?', whereArgs: ['$prefix%']);
  }
}
