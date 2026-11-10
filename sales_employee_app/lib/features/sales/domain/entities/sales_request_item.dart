/// طلب بيع من SalesRequests — منفصل عن SaleOrder / Order.
class SalesRequestItem {
  const SalesRequestItem({
    required this.requestId,
    required this.customerName,
    this.customerPhone,
    this.address,
    this.requestedProduct,
    this.submissionNote,
    this.sourceTypeLabel,
    required this.status,
    required this.statusLabel,
    required this.tabKey,
    this.linkedOrderId,
    this.branchId,
    this.branchName,
    this.lastStatusNote,
    this.statusChangedAtUtc,
    this.viewedAtUtc,
    this.createdAt,
    this.isReturned = false,
  });

  final int requestId;
  final String customerName;
  final String? customerPhone;
  final String? address;
  final String? requestedProduct;
  final String? submissionNote;
  final String? sourceTypeLabel;
  final int status;
  final String statusLabel;
  final String tabKey;
  /// Order id إن وُجد — لا تستخدم requestId كـ orderId.
  final int? linkedOrderId;
  final int? branchId;
  final String? branchName;
  final String? lastStatusNote;
  final DateTime? statusChangedAtUtc;
  final DateTime? viewedAtUtc;
  final DateTime? createdAt;
  final bool isReturned;

  bool get hasLinkedOrder => linkedOrderId != null && linkedOrderId != 0;

  factory SalesRequestItem.fromJson(Map<String, dynamic> json) {
    int? asInt(dynamic v) {
      if (v == null) return null;
      if (v is int) return v;
      return int.tryParse('$v');
    }

    DateTime? asDate(dynamic v) {
      if (v == null) return null;
      return DateTime.tryParse('$v');
    }

    return SalesRequestItem(
      requestId: asInt(json['requestId'] ?? json['RequestId']) ?? 0,
      customerName: '${json['customerName'] ?? json['CustomerName'] ?? ''}',
      customerPhone: json['customerPhone']?.toString() ?? json['CustomerPhone']?.toString(),
      address: json['address']?.toString(),
      requestedProduct: json['requestedProduct']?.toString(),
      submissionNote: json['submissionNote']?.toString(),
      sourceTypeLabel: json['sourceTypeLabel']?.toString(),
      status: asInt(json['status'] ?? json['Status']) ?? 0,
      statusLabel: '${json['statusLabel'] ?? json['StatusLabel'] ?? ''}',
      tabKey: '${json['tabKey'] ?? json['TabKey'] ?? 'incoming'}',
      linkedOrderId: asInt(json['linkedOrderId'] ?? json['LinkedOrderId']),
      branchId: asInt(json['branchId'] ?? json['BranchId']),
      branchName: json['branchName']?.toString() ?? json['BranchName']?.toString(),
      lastStatusNote: json['lastStatusNote']?.toString() ?? json['LastStatusNote']?.toString(),
      statusChangedAtUtc: asDate(json['statusChangedAtUtc'] ?? json['StatusChangedAtUtc']),
      viewedAtUtc: asDate(json['viewedAtUtc'] ?? json['ViewedAtUtc']),
      createdAt: asDate(json['createdAt'] ?? json['CreatedAt']),
      isReturned: json['isReturned'] == true || json['IsReturned'] == true,
    );
  }
}
