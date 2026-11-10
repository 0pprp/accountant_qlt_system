class SalesSession {
  const SalesSession({
    required this.userId,
    required this.userName,
    required this.fullName,
    required this.accessToken,
    required this.refreshToken,
  });

  final int userId;
  final String userName;
  final String fullName;
  final String accessToken;
  final String refreshToken;
}
