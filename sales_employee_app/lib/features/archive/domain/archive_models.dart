import '../../../core/network/json_map.dart';

String? _orNull(String value) => value.isEmpty ? null : value;

/// يطابق ArchiveStage في الخادم. الجسم يرسل أرقاماً — لا محوّل تعداد نصّي في الـAPI.
abstract final class ArchiveStage {
  static const salesEmployee = 0;
  static const branchManagement = 1;
  static const follower = 2;
  static const archive = 3;

  static String label(int stage) => switch (stage) {
        salesEmployee => 'موظف المبيعات',
        branchManagement => 'إدارة الفرع',
        follower => 'المتابع',
        archive => 'الأرشيف',
        _ => '—',
      };
}

abstract final class ArchiveStatus {
  static const withSalesEmployee = 0;
  static const sentToBranch = 1;
  static const atBranch = 2;
  static const sentToFollower = 3;
  static const atFollower = 4;
  static const sentToArchive = 5;
  static const archived = 6;
}

class ArchiveBundle {
  const ArchiveBundle({
    required this.id,
    required this.periodLabel,
    required this.salesEmployeeName,
    required this.periodEnd,
    required this.isPeriodClosed,
    required this.canSend,
    required this.pendingSendCount,
    required this.itemCount,
    required this.receivedOnwardCount,
    required this.receivedByMeCount,
    required this.openReturnCount,
    required this.archivedCount,
  });

  final int id;
  final String periodLabel;
  final String salesEmployeeName;
  final String periodEnd;
  final bool isPeriodClosed;
  final bool canSend;
  final int pendingSendCount;
  final int itemCount;
  final int receivedOnwardCount;
  final int receivedByMeCount;
  final int openReturnCount;
  final int archivedCount;

  factory ArchiveBundle.fromJson(Map<String, dynamic> json) => ArchiveBundle(
        id: JsonMap.asInt(json, ['id', 'Id']) ?? 0,
        periodLabel: JsonMap.str(json, ['periodLabel', 'PeriodLabel']),
        salesEmployeeName:
            JsonMap.str(json, ['salesEmployeeName', 'SalesEmployeeName']),
        periodEnd: JsonMap.str(json, ['periodEnd', 'PeriodEnd']),
        isPeriodClosed:
            JsonMap.asBool(json, ['isPeriodClosed', 'IsPeriodClosed']),
        canSend: JsonMap.asBool(json, ['canSend', 'CanSend']),
        pendingSendCount:
            JsonMap.asInt(json, ['pendingSendCount', 'PendingSendCount']) ?? 0,
        itemCount: JsonMap.asInt(json, ['itemCount', 'ItemCount']) ?? 0,
        receivedOnwardCount:
            JsonMap.asInt(json, ['receivedOnwardCount', 'ReceivedOnwardCount']) ?? 0,
        receivedByMeCount:
            JsonMap.asInt(json, ['receivedByMeCount', 'ReceivedByMeCount']) ?? 0,
        openReturnCount:
            JsonMap.asInt(json, ['openReturnCount', 'OpenReturnCount']) ?? 0,
        archivedCount: JsonMap.asInt(json, ['archivedCount', 'ArchivedCount']) ?? 0,
      );
}

class ArchiveTransactionRow {
  const ArchiveTransactionRow({
    required this.id,
    required this.orderId,
    required this.customerName,
    required this.statusLabel,
    required this.status,
    required this.awaitsMyReceipt,
    required this.hasOpenReturn,
    required this.isVoided,
    required this.isInTransit,
    this.lastReturnReason,
    this.periodSourceDate,
  });

  final int id;
  final int orderId;
  final String customerName;
  final String statusLabel;
  final int status;
  final bool awaitsMyReceipt;
  final bool hasOpenReturn;
  final bool isVoided;
  final bool isInTransit;
  final String? lastReturnReason;
  final String? periodSourceDate;

  factory ArchiveTransactionRow.fromJson(Map<String, dynamic> json) =>
      ArchiveTransactionRow(
        id: JsonMap.asInt(json, ['id', 'Id']) ?? 0,
        orderId: JsonMap.asInt(json, ['orderId', 'OrderId']) ?? 0,
        customerName: JsonMap.str(json, ['customerName', 'CustomerName']),
        statusLabel: JsonMap.str(json, ['statusLabel', 'StatusLabel']),
        status: JsonMap.asInt(json, ['status', 'Status']) ?? 0,
        awaitsMyReceipt:
            JsonMap.asBool(json, ['awaitsMyReceipt', 'AwaitsMyReceipt']),
        hasOpenReturn:
            JsonMap.asBool(json, ['hasOpenReturn', 'HasOpenReturn']),
        isVoided: JsonMap.asBool(json, ['isVoided', 'IsVoided']),
        isInTransit: JsonMap.asBool(json, ['isInTransit', 'IsInTransit']),
        lastReturnReason:
            _orNull(JsonMap.str(json, ['lastReturnReason', 'LastReturnReason'])),
        periodSourceDate:
            _orNull(JsonMap.str(json, ['periodSourceDate', 'PeriodSourceDate'])),
      );
}

class ArchiveBundleDetail {
  const ArchiveBundleDetail({
    required this.bundle,
    required this.transactions,
    required this.myStage,
    required this.canSend,
    required this.canReceive,
    required this.canUndoSend,
  });

  final ArchiveBundle bundle;
  final List<ArchiveTransactionRow> transactions;
  final int? myStage;
  final bool canSend;
  final bool canReceive;
  final bool canUndoSend;

  factory ArchiveBundleDetail.fromJson(Map<String, dynamic> json) {
    final raw = JsonMap.pick(json, ['transactions', 'Transactions']);
    final bundleRaw = JsonMap.pick(json, ['bundle', 'Bundle']);

    return ArchiveBundleDetail(
      bundle: ArchiveBundle.fromJson(
        bundleRaw is Map ? Map<String, dynamic>.from(bundleRaw) : <String, dynamic>{},
      ),
      transactions: raw is List
          ? raw
              .whereType<Map>()
              .map((m) => ArchiveTransactionRow.fromJson(Map<String, dynamic>.from(m)))
              .toList()
          : const [],
      myStage: JsonMap.asInt(json, ['myStage', 'MyStage']),
      canSend: JsonMap.asBool(json, ['canSend', 'CanSend']),
      canReceive: JsonMap.asBool(json, ['canReceive', 'CanReceive']),
      canUndoSend: JsonMap.asBool(json, ['canUndoSend', 'CanUndoSend']),
    );
  }
}

class ArchiveActionResult {
  const ArchiveActionResult({required this.message, required this.affectedCount});

  final String message;
  final int affectedCount;

  factory ArchiveActionResult.fromJson(Map<String, dynamic> json) =>
      ArchiveActionResult(
        message: JsonMap.str(json, ['message', 'Message']),
        affectedCount: JsonMap.asInt(json, ['affectedCount', 'AffectedCount']) ?? 0,
      );
}
