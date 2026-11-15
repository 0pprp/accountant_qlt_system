import 'dart:convert';
import 'dart:io';

import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';
import 'package:sales_employee_app/core/database/app_database.dart';
import 'package:sqflite/sqflite.dart';

/// مسودة محلية لمعالج إتمام البيع — نص + مسارات صور منسوخة لدليل التطبيق.
class SaleWizardLocalDraftStore {
  SaleWizardLocalDraftStore(this._db);
  final AppDatabase _db;

  static String keyFor({required int orderId, int? requestId}) {
    if (requestId != null && requestId > 0) return 'req_$requestId';
    return 'ord_$orderId';
  }

  Future<void> save({
    required String draftKey,
    required int step,
    required Map<String, dynamic> fields,
    required Map<String, String?> photoPaths,
  }) async {
    if (!_db.supportsSqlite) return;
    final db = await _db.database;
    final dir = await _photosDir(draftKey);
    final stored = <String, String?>{};
    for (final e in photoPaths.entries) {
      final src = e.value;
      if (src == null || src.isEmpty) {
        stored[e.key] = null;
        continue;
      }
      stored[e.key] = await _copyInto(dir, e.key, src);
    }
    final payload = jsonEncode({
      'step': step,
      'fields': fields,
      'photos': stored,
      'savedAt': DateTime.now().toUtc().toIso8601String(),
    });
    await db.insert(
      'sale_wizard_drafts',
      {
        'draft_key': draftKey,
        'payload_json': payload,
        'updated_at': DateTime.now().toUtc().toIso8601String(),
      },
      conflictAlgorithm: ConflictAlgorithm.replace,
    );
  }

  Future<Map<String, dynamic>?> load(String draftKey) async {
    if (!_db.supportsSqlite) return null;
    final db = await _db.database;
    final rows = await db.query(
      'sale_wizard_drafts',
      where: 'draft_key = ?',
      whereArgs: [draftKey],
      limit: 1,
    );
    if (rows.isEmpty) return null;
    final raw = rows.first['payload_json'];
    if (raw is! String || raw.isEmpty) return null;
    final map = jsonDecode(raw);
    if (map is! Map) return null;
    return Map<String, dynamic>.from(map);
  }

  Future<void> clear(String draftKey) async {
    if (!_db.supportsSqlite) return;
    final db = await _db.database;
    await db.delete('sale_wizard_drafts', where: 'draft_key = ?', whereArgs: [draftKey]);
    try {
      final dir = await _photosDir(draftKey);
      if (await dir.exists()) await dir.delete(recursive: true);
    } catch (_) {}
  }

  Future<Directory> _photosDir(String draftKey) async {
    final root = await getApplicationDocumentsDirectory();
    final dir = Directory(p.join(root.path, 'sale_wizard_drafts', draftKey));
    if (!await dir.exists()) await dir.create(recursive: true);
    return dir;
  }

  Future<String> _copyInto(Directory dir, String key, String sourcePath) async {
    final src = File(sourcePath);
    if (!await src.exists()) return sourcePath;
    final safe = key.replaceAll(RegExp(r'[^a-zA-Z0-9_-]'), '_');
    final ext = p.extension(sourcePath);
    final dest = File(p.join(dir.path, '$safe${ext.isEmpty ? '.jpg' : ext}'));
    // نفس الملف مسبقاً
    if (p.equals(src.path, dest.path)) return dest.path;
    await src.copy(dest.path);
    return dest.path;
  }
}
