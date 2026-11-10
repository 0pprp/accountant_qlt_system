import 'dart:convert';

import 'package:sales_employee_app/core/database/app_database.dart';
import 'package:sales_employee_app/core/time/baghdad_time.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sale_order.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';
import 'package:sqflite/sqflite.dart';

/// Tracked demo path (parity P1): still registered for offline/demo bins.
/// Live API failure must not silently fall back here — see home_tab comments.
class DemoOrdersStore {
  DemoOrdersStore(this._db);
  final AppDatabase _db;

  static const demoCustomerName = 'زبون تجريبي';
  static const demoPhone = '07701234567';
  static const demoProvince = 'النجف';
  /// Stable UUID — one demo customer = one card in one bin only.
  static const demoCustomerUuid = '00000000-0000-4000-8000-demo00000001';

  Future<void> ensurePendingCompleteDemo() async {
    final db = await _db.database;
    await db.delete('demo_orders', where: 'customer_name = ?', whereArgs: ['ahmed']);
    await db.delete('demo_orders', where: 'customer_name = ?', whereArgs: ['احمد حسن']);

    await _dedupeByCustomerUuid(db);

    final rows = await db.query('demo_orders', orderBy: 'id DESC');
    for (final r in rows) {
      final p = _payloadOf(r);
      if (p['reassigned'] == true) continue;
      final uuid = '${p['customerUuid'] ?? ''}';
      final sameName = '${r['customer_name']}' == demoCustomerName;
      if (uuid == demoCustomerUuid || (uuid.isEmpty && sameName)) {
        // Already exists in some bin (pending/sold/…) — never recreate a duplicate card.
        if (uuid.isEmpty) {
          p['customerUuid'] = demoCustomerUuid;
          await db.update(
            'demo_orders',
            {'payload_json': jsonEncode(p)},
            where: 'id = ?',
            whereArgs: [r['id']],
          );
        }
        return;
      }
    }

    final now = baghdadNow().toUtc().toIso8601String();
    await db.insert(
      'demo_orders',
      {
        'customer_name': demoCustomerName,
        'status': 'pending',
        'total': 1350000,
        'created_at': now,
        'payload_json': jsonEncode({
          'customerUuid': demoCustomerUuid,
          'phone': demoPhone,
          'branchName': demoProvince,
          'maxSellAmount': 2000000,
          'source': 'complete_sale_demo',
          'note': 'طلب تجريبي معيّن — جاهز لمسار إتمام البيع',
        }),
      },
      conflictAlgorithm: ConflictAlgorithm.replace,
    );
  }

  /// Keep newest row per customerUuid; delete older duplicates.
  Future<void> _dedupeByCustomerUuid(Database db) async {
    final raw = await db.query('demo_orders', orderBy: 'id DESC');
    final seen = <String>{};
    for (final r in raw) {
      final p = _payloadOf(r);
      if (p['reassigned'] == true) continue;
      var uuid = '${p['customerUuid'] ?? ''}';
      if (uuid.isEmpty && '${r['customer_name']}' == demoCustomerName) {
        uuid = demoCustomerUuid;
        p['customerUuid'] = demoCustomerUuid;
        await db.update(
          'demo_orders',
          {'payload_json': jsonEncode(p)},
          where: 'id = ?',
          whereArgs: [r['id']],
        );
      }
      if (uuid.isEmpty) continue;
      if (seen.contains(uuid)) {
        await db.delete('demo_orders', where: 'id = ?', whereArgs: [r['id']]);
      } else {
        seen.add(uuid);
      }
    }
  }

  Future<List<SaleOrder>> allVisible() async {
    final db = await _db.database;
    await _dedupeByCustomerUuid(db);
    final raw = await db.query('demo_orders', orderBy: 'id DESC');
    final out = <SaleOrder>[];
    final seenUuid = <String>{};
    for (final r in raw) {
      final p = _payloadOf(r);
      if (p['reassigned'] == true) continue;
      final uuid = '${p['customerUuid'] ?? ''}';
      if (uuid.isNotEmpty) {
        if (seenUuid.contains(uuid)) continue;
        seenUuid.add(uuid);
      }
      out.add(_fromRow(r));
    }
    return out;
  }

  Future<List<SaleOrder>> all() => allVisible();

  Future<List<SaleOrder>> byStatus(String status) async {
    final mapped = await allVisible();
    if (status == 'all' || status.isEmpty) return mapped;
    return mapped
        .where((o) => SalesWorkflowRules.normalize(o.status) == SalesWorkflowRules.normalize(status))
        .toList();
  }

  Future<SaleOrder?> byId(int negativeId) async {
    final id = negativeId.abs();
    final db = await _db.database;
    final rows = await db.query('demo_orders', where: 'id = ?', whereArgs: [id], limit: 1);
    if (rows.isEmpty) return null;
    final p = _payloadOf(rows.first);
    if (p['reassigned'] == true) return null;
    return _fromRow(rows.first);
  }

  Future<Map<String, dynamic>?> draftOf(int negativeId) async {
    final id = negativeId.abs();
    final db = await _db.database;
    final rows = await db.query('demo_orders', where: 'id = ?', whereArgs: [id], limit: 1);
    if (rows.isEmpty) return null;
    final draft = _payloadOf(rows.first)['draft'];
    if (draft is Map) return Map<String, dynamic>.from(draft);
    return null;
  }

  Future<void> transfer(int negativeId, String targetStatus, {String? reason}) async {
    final id = negativeId.abs();
    final db = await _db.database;
    final rows = await db.query('demo_orders', where: 'id = ?', whereArgs: [id], limit: 1);
    if (rows.isEmpty) throw StateError('order_not_found');
    final current = SalesWorkflowRules.normalize('${rows.first['status']}');
    if (SalesWorkflowRules.isTerminal(current)) {
      throw StateError('terminal_status');
    }
    final target = SalesWorkflowRules.normalize(targetStatus);
    final allowed = SalesWorkflowRules.allowedTransferTargets(current);
    // sold/disclosure come from wizard paths, not free transfer sheet
    if (target != SalesWorkflowRules.sold &&
        target != SalesWorkflowRules.disclosure &&
        !allowed.contains(target)) {
      throw StateError('transfer_not_allowed');
    }
    if (SalesWorkflowRules.requiresReason(target) && (reason == null || reason.trim().isEmpty)) {
      throw StateError('reason_required');
    }
    final payload = _payloadOf(rows.first);
    if (reason != null) payload['note'] = reason.trim();
    payload['lastAction'] = target;
    await db.update(
      'demo_orders',
      {'status': target, 'payload_json': jsonEncode(payload)},
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  Future<void> reassign(int negativeId, {required String assignee, required String reason}) async {
    final id = negativeId.abs();
    if (!SalesWorkflowRules.canReassignWith(assignee: assignee, reason: reason)) {
      throw StateError('reassign_required');
    }
    final db = await _db.database;
    final rows = await db.query('demo_orders', where: 'id = ?', whereArgs: [id], limit: 1);
    if (rows.isEmpty) throw StateError('order_not_found');
    final current = SalesWorkflowRules.normalize('${rows.first['status']}');
    if (!SalesWorkflowRules.canReassign(current)) {
      throw StateError('terminal_status');
    }
    final payload = _payloadOf(rows.first);
    payload['reassigned'] = true;
    payload['reassignedTo'] = assignee.trim();
    payload['note'] = reason.trim();
    await db.update(
      'demo_orders',
      {'payload_json': jsonEncode(payload)},
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  Future<double?> maxSellOf(int negativeId) async {
    final id = negativeId.abs();
    final db = await _db.database;
    final rows = await db.query('demo_orders', where: 'id = ?', whereArgs: [id], limit: 1);
    if (rows.isEmpty) return null;
    final v = _payloadOf(rows.first)['maxSellAmount'];
    if (v is num) return v.toDouble();
    return null;
  }

  Future<void> saveDraft({
    required int negativeId,
    required Map<String, dynamic> draft,
    required String status,
  }) async {
    final id = negativeId.abs();
    final db = await _db.database;
    final rows = await db.query('demo_orders', where: 'id = ?', whereArgs: [id], limit: 1);
    if (rows.isEmpty) throw StateError('order_not_found');
    final current = SalesWorkflowRules.normalize('${rows.first['status']}');
    if (SalesWorkflowRules.isTerminal(current)) {
      throw StateError('terminal_status');
    }
    final payload = _payloadOf(rows.first);
    payload['draft'] = draft;
    payload['note'] = 'تم الكشف — مسودة محفوظة';
    await db.update(
      'demo_orders',
      {
        'status': SalesWorkflowRules.normalize(status),
        'customer_name': '${draft['fullName'] ?? rows.first['customer_name']}',
        'total': (draft['total'] as num?)?.toDouble() ?? rows.first['total'],
        'payload_json': jsonEncode(payload),
      },
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  Future<void> completeSale({
    required int negativeId,
    required Map<String, dynamic> payload,
  }) async {
    if (!SalesWorkflowRules.hasLinkedOrder(negativeId.abs())) {
      throw StateError('sale_requires_linked_order');
    }
    final id = negativeId.abs();
    final db = await _db.database;
    final rows = await db.query('demo_orders', where: 'id = ?', whereArgs: [id], limit: 1);
    if (rows.isEmpty) throw StateError('order_not_found');
    final current = SalesWorkflowRules.normalize('${rows.first['status']}');
    if (current == SalesWorkflowRules.rejected) {
      throw StateError('rejected_cannot_sell');
    }
    if (current == SalesWorkflowRules.sold) {
      throw StateError('already_sold');
    }
    if (!SalesWorkflowRules.canStartSale(current)) {
      throw StateError('sale_not_allowed');
    }
    final prev = _payloadOf(rows.first);
    final maxSell = (prev['maxSellAmount'] as num?)?.toDouble();
    final total = (payload['total'] as num?)?.toDouble() ?? 0;
    if (!SalesWorkflowRules.isValidSellTotal(total, maxSell: maxSell)) {
      throw StateError('invalid_price');
    }
    final products = payload['products'];
    if (products is! List || products.isEmpty) {
      throw StateError('products_required');
    }
    await db.update(
      'demo_orders',
      {
        'status': SalesWorkflowRules.sold,
        'customer_name': '${payload['fullName'] ?? demoCustomerName}',
        'total': total > 0 ? total : 1350000,
        'payload_json': jsonEncode({
          ...prev,
          ...payload,
          'customerUuid': prev['customerUuid'] ?? payload['customerUuid'] ?? demoCustomerUuid,
          'phone': payload['phone'] ?? demoPhone,
          'draft': payload,
          'completedAt': baghdadNow().toUtc().toIso8601String(),
          'note': 'تم البيع',
        }),
      },
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  Map<String, dynamic> _payloadOf(Map<String, Object?> r) {
    try {
      final raw = r['payload_json'];
      if (raw == null) return {};
      final m = jsonDecode('$raw');
      if (m is Map) return Map<String, dynamic>.from(m);
    } catch (_) {}
    return {};
  }

  SaleOrder _fromRow(Map<String, Object?> r) {
    final p = _payloadOf(r);
    final phone = '${p['phone'] ?? ''}';
    final note = '${p['note'] ?? ''}';
    final branch = '${p['branchName'] ?? demoProvince}';
    final uuid = '${p['customerUuid'] ?? ''}';
    return SaleOrder(
      id: -(r['id'] as int),
      customerName: '${r['customer_name']}',
      status: '${r['status']}',
      total: (r['total'] as num?)?.toDouble(),
      createdAt: '${r['created_at']}',
      customerUuid: uuid.isEmpty ? null : uuid,
      customerPhone: phone.isEmpty ? null : phone,
      branchName: branch,
      workflowReason: note.isEmpty ? null : note,
      orderListName: p['orderListName']?.toString(),
    );
  }
}
