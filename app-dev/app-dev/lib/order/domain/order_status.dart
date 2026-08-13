import 'package:freezed_annotation/freezed_annotation.dart';

/// Installment lifecycle status (API: `executionStatus`).
enum ExecutionStatus {
  @JsonValue(0)
  notStarted,
  @JsonValue(1)
  inProgress,
  @JsonValue(2)
  completed,
}

/// Admin approval status (API: `approvalStatus`).
enum ApprovalStatus {
  @JsonValue(0)
  pending,
  @JsonValue(1)
  approved,
  @JsonValue(2)
  rejected,
}

extension ExecutionStatusLabel on ExecutionStatus {
  String get arabicLabel => switch (this) {
    ExecutionStatus.notStarted => 'لم يبدأ بعد',
    ExecutionStatus.inProgress => 'جاري',
    ExecutionStatus.completed => 'مكتمل',
  };

  bool get isDraft => this == ExecutionStatus.notStarted || this == ExecutionStatus.inProgress;
}

extension ApprovalStatusLabel on ApprovalStatus {
  String get arabicLabel => switch (this) {
    ApprovalStatus.pending => 'قيد الموافقة',
    ApprovalStatus.approved => 'موافق عليه',
    ApprovalStatus.rejected => 'مرفوض',
  };
}
