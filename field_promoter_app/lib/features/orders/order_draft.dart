import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';
import 'package:uuid/uuid.dart';

class OrderDraft {
  const OrderDraft({
    this.product = '',
    this.otherProduct = '',
    this.customerName = '',
    this.customerPhone = '',
    this.branchId,
    this.address = '',
    this.note = '',
    this.latitude,
    this.longitude,
    this.photoPath,
    this.submissionKey,
    this.keyFingerprint,
  });

  final String product;
  final String otherProduct;
  final String customerName;
  final String customerPhone;
  final int? branchId;
  final String address;
  final String note;
  final double? latitude;
  final double? longitude;
  final String? photoPath;
  final String? submissionKey;
  final String? keyFingerprint;

  bool get isEmpty =>
      product.isEmpty &&
      otherProduct.trim().isEmpty &&
      customerName.trim().isEmpty &&
      customerPhone.trim().isEmpty &&
      address.trim().isEmpty &&
      note.trim().isEmpty &&
      latitude == null &&
      photoPath == null;

  Map<String, dynamic> contentJson() => {
    'product': product,
    'otherProduct': otherProduct.trim(),
    'customerName': customerName.trim(),
    'customerPhone': customerPhone.trim(),
    'branchId': branchId,
    'address': address.trim(),
    'note': note.trim(),
    'latitude': latitude,
    'longitude': longitude,
    'photoPath': photoPath,
  };

  String get fingerprint => jsonEncode(contentJson());

  Map<String, dynamic> toJson() => {
    ...contentJson(),
    'submissionKey': submissionKey,
    'keyFingerprint': keyFingerprint,
  };

  factory OrderDraft.fromJson(Map j) {
    double? d(Object? v) => v is num ? v.toDouble() : null;
    String s(Object? v) => v == null ? '' : '$v';
    return OrderDraft(
      product: s(j['product']),
      otherProduct: s(j['otherProduct']),
      customerName: s(j['customerName']),
      customerPhone: s(j['customerPhone']),
      branchId: j['branchId'] is num ? (j['branchId'] as num).toInt() : null,
      address: s(j['address']),
      note: s(j['note']),
      latitude: d(j['latitude']),
      longitude: d(j['longitude']),
      photoPath: j['photoPath'] == null ? null : '${j['photoPath']}',
      submissionKey: j['submissionKey'] == null ? null : '${j['submissionKey']}',
      keyFingerprint: j['keyFingerprint'] == null ? null : '${j['keyFingerprint']}',
    );
  }
}

class SubmissionKeyKeeper {
  SubmissionKeyKeeper({this._key, this._fingerprint, String Function()? generate})
    : _generate = generate ?? (() => const Uuid().v4());

  String? _key;
  String? _fingerprint;
  final String Function() _generate;

  String? get currentKey => _key;
  String? get currentFingerprint => _fingerprint;

  String keyFor(String fingerprint) {
    if (_key == null || _fingerprint != fingerprint) {
      _key = _generate();
      _fingerprint = fingerprint;
    }
    return _key!;
  }

  void renew() {
    _key = null;
    _fingerprint = null;
  }
}

class OrderDraftStore {
  static const _key = 'fp_order_draft_v1';

  Future<OrderDraft?> load() async {
    try {
      final raw = (await SharedPreferences.getInstance()).getString(_key);
      if (raw == null || raw.isEmpty) return null;
      final decoded = jsonDecode(raw);
      return decoded is Map ? OrderDraft.fromJson(decoded) : null;
    } catch (_) {
      return null;
    }
  }

  Future<void> save(OrderDraft draft) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      if (draft.isEmpty && draft.submissionKey == null) {
        await prefs.remove(_key);
      } else {
        await prefs.setString(_key, jsonEncode(draft.toJson()));
      }
    } catch (_) {}
  }

  Future<void> clear() async {
    try {
      await (await SharedPreferences.getInstance()).remove(_key);
    } catch (_) {}
  }
}
