class ApiException implements Exception {
  ApiException(
    this.message, {
    this.code,
    this.statusCode,
    this.isNetwork = false,
  });

  final String message;
  final String? code;
  final int? statusCode;
  final bool isNetwork;

  bool get isRetryable =>
      isNetwork || (statusCode != null && statusCode! >= 500);

  @override
  String toString() => message;
}
