import 'package:committee_officer_application/core/network/json_map.dart';

/// يطابق ManagementTaskStatus في الخادم.
abstract final class ManagementTaskStatus {
  static const sent = 0;
  static const transferred = 1;
  static const accepted = 2;
  static const returned = 3;
  static const awaitingDelegatedConfirm = 4;
  static const completed = 5;

  static String label(int status) => switch (status) {
        sent => 'مرسلة',
        transferred => 'منقولة',
        accepted => 'مقبولة',
        returned => 'معادة',
        awaitingDelegatedConfirm => 'بانتظار التصديق',
        completed => 'منجزة',
        _ => 'غير معروفة',
      };
}

abstract final class ManagementTaskPriority {
  static const high = 1;
  static const medium = 2;
  static const low = 3;

  static String label(int priority) => switch (priority) {
        high => 'عالية',
        medium => 'متوسطة',
        low => 'منخفضة',
        _ => '—',
      };
}

class ManagementTaskItem {
  const ManagementTaskItem({
    required this.id,
    required this.title,
    required this.description,
    required this.status,
    required this.priority,
    required this.dueDate,
    required this.assigneeDisplayName,
    required this.expectedVersion,
    required this.isOverdue,
    required this.rejectNote,
  });

  final int id;
  final String title;
  final String description;
  final int status;
  final int priority;
  final String dueDate;
  final String assigneeDisplayName;
  final int expectedVersion;
  final bool isOverdue;
  final String? rejectNote;

  bool get canAccept =>
      status == ManagementTaskStatus.sent ||
      status == ManagementTaskStatus.transferred;

  bool get canReject =>
      status == ManagementTaskStatus.sent ||
      status == ManagementTaskStatus.transferred ||
      status == ManagementTaskStatus.accepted;

  bool get canComplete => status == ManagementTaskStatus.accepted;

  factory ManagementTaskItem.fromJson(Map<String, dynamic> json) {
    return ManagementTaskItem(
      id: JsonMap.asInt(json, ['id', 'Id']) ?? 0,
      title: JsonMap.str(json, ['title', 'Title']),
      description: JsonMap.str(json, ['description', 'Description']),
      status: JsonMap.asInt(json, ['status', 'Status']) ?? 0,
      priority: JsonMap.asInt(json, ['priority', 'Priority']) ?? 2,
      dueDate: JsonMap.str(json, ['dueDate', 'DueDate']),
      assigneeDisplayName: JsonMap.str(json, [
        'assigneeDisplayName',
        'AssigneeDisplayName',
      ]),
      expectedVersion: JsonMap.asInt(json, [
            'expectedVersion',
            'ExpectedVersion',
          ]) ??
          0,
      isOverdue: JsonMap.asBool(json, ['isOverdue', 'IsOverdue']),
      rejectNote: () {
        final s = JsonMap.str(json, ['rejectNote', 'RejectNote']);
        return s.isEmpty ? null : s;
      }(),
    );
  }

  ManagementTaskItem withActionResult({
    required int status,
    required int expectedVersion,
  }) {
    return ManagementTaskItem(
      id: id,
      title: title,
      description: description,
      status: status,
      priority: priority,
      dueDate: dueDate,
      assigneeDisplayName: assigneeDisplayName,
      expectedVersion: expectedVersion,
      isOverdue: isOverdue,
      rejectNote: rejectNote,
    );
  }
}
