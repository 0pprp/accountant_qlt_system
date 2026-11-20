import 'package:flutter/material.dart';

import '../../core/formatting/iraq_time.dart';

int _int(Object? v) {
  if (v is int) return v;
  if (v is num) return v.toInt();
  return int.tryParse('${v ?? ''}') ?? 0;
}

double _double(Object? v) {
  if (v is num) return v.toDouble();
  return double.tryParse('${v ?? ''}') ?? 0;
}

String _str(Object? v) => v == null ? '' : '$v'.trim();

String? _strOrNull(Object? v) {
  final s = _str(v);
  return s.isEmpty ? null : s;
}

bool _bool(Object? v) => v == true || '$v'.toLowerCase() == 'true';

List<Map> _maps(Object? v) => v is List ? v.whereType<Map>().toList() : const [];

class Branch {
  const Branch(this.id, this.name);
  final int id;
  final String name;

  factory Branch.fromJson(Map j) => Branch(_int(j['id']), _str(j['name']));
}

class PromoterMe {
  const PromoterMe({
    required this.id,
    required this.fullName,
    required this.username,
    required this.branches,
    required this.isWorking,
    required this.shiftStartedAt,
    required this.todayPins,
    required this.todayOrders,
    required this.todayWorkMinutes,
    required this.targetMinutes,
    required this.monthOrders,
    required this.monthSold,
    required this.supportPhone,
    required this.supportWhatsapp,
    required this.products,
  });

  final int id;
  final String fullName;
  final String username;
  final List<Branch> branches;
  final bool isWorking;
  final DateTime? shiftStartedAt;
  final int todayPins;
  final int todayOrders;
  final int todayWorkMinutes;
  final int targetMinutes;
  final int monthOrders;
  final int monthSold;
  final String? supportPhone;
  final String? supportWhatsapp;
  final List<String> products;

  factory PromoterMe.fromJson(Map j) => PromoterMe(
    id: _int(j['id']),
    fullName: _str(j['fullName']),
    username: _str(j['username']),
    branches: _maps(j['branches']).map(Branch.fromJson).toList(),
    isWorking: _bool(j['isWorking']),
    shiftStartedAt: IraqTime.parse(j['shiftStartedAt']),
    todayPins: _int(j['todayPins']),
    todayOrders: _int(j['todayOrders']),
    todayWorkMinutes: _int(j['todayWorkMinutes']),
    targetMinutes: _int(j['targetMinutes']),
    monthOrders: _int(j['monthOrders']),
    monthSold: _int(j['monthSold']),
    supportPhone: _strOrNull(j['supportPhone']),
    supportWhatsapp: _strOrNull(j['supportWhatsapp']),
    products: j['products'] is List
        ? (j['products'] as List)
              .map((e) => '$e'.trim())
              .where((e) => e.isNotEmpty)
              .toList()
        : const [],
  );
}

class ShiftState {
  const ShiftState(this.isWorking, this.startedAt, this.endedAt);
  final bool isWorking;
  final DateTime? startedAt;
  final DateTime? endedAt;

  factory ShiftState.fromJson(Map j) => ShiftState(
    _bool(j['isWorking']),
    IraqTime.parse(j['startedAt']),
    IraqTime.parse(j['endedAt']),
  );
}

class WorkPeriod {
  const WorkPeriod(this.start, this.end, this.minutes, this.events);
  final DateTime? start;
  final DateTime? end;
  final int minutes;
  final int events;

  factory WorkPeriod.fromJson(Map j) => WorkPeriod(
    IraqTime.parse(j['start']),
    IraqTime.parse(j['end']),
    _int(j['minutes']),
    _int(j['events']),
  );
}

class RoutePoint {
  const RoutePoint(this.lat, this.lng, this.at);
  final double lat;
  final double lng;
  final DateTime? at;

  factory RoutePoint.fromJson(Map j) =>
      RoutePoint(_double(j['lat']), _double(j['lng']), IraqTime.parse(j['at']));
}

class TodaySummary {
  const TodaySummary({
    required this.isWorking,
    required this.startedAt,
    required this.firstStart,
    required this.periods,
    required this.workMinutes,
    required this.targetMinutes,
    required this.pins,
    required this.orders,
    required this.route,
  });

  final bool isWorking;
  final DateTime? startedAt;
  final DateTime? firstStart;
  final List<WorkPeriod> periods;
  final int workMinutes;
  final int targetMinutes;
  final int pins;
  final int orders;
  final List<RoutePoint> route;

  factory TodaySummary.fromJson(Map j) {
    final sessions = _maps(j['sessions']);
    return TodaySummary(
      isWorking: _bool(j['isWorking']),
      startedAt: IraqTime.parse(j['startedAt']),
      firstStart: sessions.isEmpty
          ? null
          : IraqTime.parse(sessions.first['startedAt']),
      periods: _maps(j['periods']).map(WorkPeriod.fromJson).toList(),
      workMinutes: _int(j['workMinutes']),
      targetMinutes: _int(j['targetMinutes']),
      pins: _int(j['pins']),
      orders: _int(j['orders']),
      route: _maps(j['route']).map(RoutePoint.fromJson).toList(),
    );
  }
}

class IngestResult {
  const IngestResult(this.accepted, this.duplicate, this.rejected, this.isWorking);
  final int accepted;
  final int duplicate;
  final int rejected;
  final bool isWorking;

  int get total => accepted + duplicate + rejected;

  factory IngestResult.fromJson(Map j) => IngestResult(
    _int(j['accepted']),
    _int(j['duplicate']),
    _int(j['rejected']),
    j['isWorking'] == null ? true : _bool(j['isWorking']),
  );
}

class OrderReceipt {
  const OrderReceipt(this.id, this.alreadySubmitted);
  final int id;
  final bool alreadySubmitted;

  factory OrderReceipt.fromJson(Map j) =>
      OrderReceipt(_int(j['id']), _bool(j['alreadySubmitted']));
}

class SaleProgress {
  const SaleProgress({
    required this.total,
    required this.paid,
    required this.remaining,
    required this.paymentsCount,
    required this.lastPaymentDate,
    required this.percent,
  });

  final double total;
  final double paid;
  final double remaining;
  final int paymentsCount;
  final DateTime? lastPaymentDate;
  final int percent;

  factory SaleProgress.fromJson(Map j) => SaleProgress(
    total: _double(j['total']),
    paid: _double(j['paid']),
    remaining: _double(j['remaining']),
    paymentsCount: _int(j['paymentsCount']),
    lastPaymentDate: IraqTime.parse(j['lastPaymentDate']),
    percent: _int(j['percent']).clamp(0, 100),
  );
}

enum OrderTab { inProgress, sold, finished, rejected }

extension OrderTabX on OrderTab {
  String get apiName => name;

  String get label => switch (this) {
    OrderTab.inProgress => 'قيد المتابعة',
    OrderTab.sold => 'قيد التسديد',
    OrderTab.finished => 'مكتملة',
    OrderTab.rejected => 'مرفوضة وملغاة',
  };
}

class PromoterOrder {
  const PromoterOrder({
    required this.id,
    required this.customerName,
    required this.customerPhone,
    required this.product,
    required this.address,
    required this.note,
    required this.branchName,
    required this.stage,
    required this.statusLabel,
    required this.reason,
    required this.createdAt,
    required this.canCancel,
    required this.sale,
  });

  final int id;
  final String customerName;
  final String customerPhone;
  final String product;
  final String address;
  final String? note;
  final String branchName;
  final String stage;
  final String statusLabel;
  final String? reason;
  final DateTime? createdAt;
  final bool canCancel;
  final SaleProgress? sale;

  factory PromoterOrder.fromJson(Map j) => PromoterOrder(
    id: _int(j['id']),
    customerName: _str(j['customerName']),
    customerPhone: _str(j['customerPhone']),
    product: _str(j['product']),
    address: _str(j['address']),
    note: _strOrNull(j['note']),
    branchName: _str(j['branchName']),
    stage: _str(j['stage']),
    statusLabel: _str(j['statusLabel']),
    reason: _strOrNull(j['reason']),
    createdAt: IraqTime.parse(j['createdAt']),
    canCancel: _bool(j['canCancel']),
    sale: j['sale'] is Map ? SaleProgress.fromJson(j['sale'] as Map) : null,
  );
}

class OrderCounts {
  const OrderCounts(this.inProgress, this.sold, this.finished, this.rejected);
  final int inProgress;
  final int sold;
  final int finished;
  final int rejected;

  static const zero = OrderCounts(0, 0, 0, 0);

  int of(OrderTab tab) => switch (tab) {
    OrderTab.inProgress => inProgress,
    OrderTab.sold => sold,
    OrderTab.finished => finished,
    OrderTab.rejected => rejected,
  };

  factory OrderCounts.fromJson(Map j) => OrderCounts(
    _int(j['inProgress']),
    _int(j['sold']),
    _int(j['finished']),
    _int(j['rejected']),
  );
}

class PageResult<T> {
  const PageResult(this.items, this.page, this.hasMore, {this.total});
  final List<T> items;
  final int page;
  final bool hasMore;
  final int? total;
}

class MapPin {
  const MapPin(this.id, this.lat, this.lng, this.name, this.interest, this.mine);
  final int id;
  final double lat;
  final double lng;
  final String name;
  final int interest;
  final bool mine;

  factory MapPin.fromJson(Map j) => MapPin(
    _int(j['id']),
    _double(j['lat']),
    _double(j['lng']),
    _str(j['name']),
    _int(j['interest']),
    _bool(j['mine']),
  );
}

class ShopPin {
  const ShopPin({
    required this.id,
    required this.lat,
    required this.lng,
    required this.name,
    required this.interest,
    required this.interestLabel,
    required this.phone,
    required this.note,
    required this.createdAt,
    required this.updatedAt,
    required this.mine,
  });

  final int id;
  final double lat;
  final double lng;
  final String name;
  final int interest;
  final String interestLabel;
  final String? phone;
  final String? note;
  final DateTime? createdAt;
  final DateTime? updatedAt;
  final bool mine;

  factory ShopPin.fromJson(Map j) {
    final interest = _int(j['interest']);
    final label = _str(j['interestLabel']);
    return ShopPin(
      id: _int(j['id']),
      lat: _double(j['lat']),
      lng: _double(j['lng']),
      name: _str(j['name']),
      interest: interest,
      interestLabel: label.isEmpty ? Interest.label(interest) : label,
      phone: _strOrNull(j['phone']),
      note: _strOrNull(j['note']),
      createdAt: IraqTime.parse(j['createdAt']),
      updatedAt: IraqTime.parse(j['updatedAt']),
      mine: _bool(j['mine']),
    );
  }
}

class PinInput {
  const PinInput({
    required this.lat,
    required this.lng,
    required this.name,
    required this.interest,
    this.phone,
    this.note,
    this.agentLat,
    this.agentLng,
  });

  final double lat;
  final double lng;
  final String name;
  final int interest;
  final String? phone;
  final String? note;
  final double? agentLat;
  final double? agentLng;

  Map<String, dynamic> toJson() => {
    'lat': lat,
    'lng': lng,
    'name': name.trim(),
    'interest': interest,
    'phone': (phone ?? '').trim().isEmpty ? null : phone!.trim(),
    'note': (note ?? '').trim().isEmpty ? null : note!.trim(),
    'agentLat': agentLat,
    'agentLng': agentLng,
  };
}

abstract final class Interest {
  static const labels = [
    'غير مهتم إطلاقاً',
    'قليل الاهتمام',
    'مهتم نوعاً ما',
    'مهتم',
    'مهتم جداً',
  ];

  static const colors = [
    Color(0xFFC62828),
    Color(0xFFEF6C00),
    Color(0xFFF9A825),
    Color(0xFF7CB342),
    Color(0xFF2E7D32),
  ];

  static String label(int interest) =>
      interest >= 1 && interest <= 5 ? labels[interest - 1] : '';

  static Color color(int interest) =>
      interest >= 1 && interest <= 5 ? colors[interest - 1] : const Color(0xFF5A6B6A);
}

