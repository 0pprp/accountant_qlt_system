import 'package:dio/dio.dart';

class SubmittedRequest {
  SubmittedRequest.fromJson(Map<String, dynamic> json)
    : id = (json['requestId'] as num).toInt(),
      name = json['customerName'] as String? ?? '',
      phone = json['customerPhone'] as String?,
      address = json['address'] as String?,
      product = json['requestedProduct'] as String?,
      note = json['submissionNote'] as String?,
      status = json['statusLabel'] as String? ?? '',
      employee = json['targetEmployeeName'] as String?,
      rejection = json['rejectionReason'] as String?,
      pendingNote = json['pendingNote'] as String?,
      createdAt = DateTime.tryParse(json['createdAt']?.toString() ?? ''),
      branch = json['branchName'] as String?;
  final int id;
  final String name, status;
  final String? phone,
      address,
      product,
      note,
      employee,
      rejection,
      pendingNote,
      branch;
  final DateTime? createdAt;
}

class SubmittedRequestsPageData {
  const SubmittedRequestsPageData(this.items, this.hasMore);
  final List<SubmittedRequest> items;
  final bool hasMore;
}

class SubmittedRequestsRemote {
  SubmittedRequestsRemote(this.dio, this.source);
  final Dio dio;
  final String source;

  Future<SubmittedRequestsPageData> load(
    String tab,
    int page,
    CancelToken token,
  ) async {
    final response = await dio.get<Map<String, dynamic>>(
      'api/v1/app/SalesRequests/submitted/$source',
      queryParameters: {'tab': tab, 'page': page},
      cancelToken: token,
    );
    final data = response.data!;
    return SubmittedRequestsPageData(
      (data['items'] as List)
          .map(
            (x) =>
                SubmittedRequest.fromJson(Map<String, dynamic>.from(x as Map)),
          )
          .toList(),
      data['hasMore'] == true,
    );
  }
}
