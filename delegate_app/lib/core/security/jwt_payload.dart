import 'dart:convert';

/// Minimal JWT payload decode (no signature verify — HTTPS + server trust).
abstract final class JwtPayload {
  static Map<String, dynamic>? decode(String token) {
    final parts = token.split('.');
    if (parts.length < 2) return null;
    try {
      final normalized = base64Url.normalize(parts[1]);
      final json = utf8.decode(base64Url.decode(normalized));
      final map = jsonDecode(json);
      return map is Map<String, dynamic> ? map : null;
    } catch (_) {
      return null;
    }
  }

  static String? userId(String accessToken) {
    final p = decode(accessToken);
    if (p == null) return null;
    const keys = [
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
      'nameid',
      'sub',
      'uid',
      'userId',
      'UserId',
    ];
    for (final k in keys) {
      final v = p[k];
      if (v != null && '$v'.trim().isNotEmpty) return '$v'.trim();
    }
    return null;
  }

  static String? username(String accessToken) {
    final p = decode(accessToken);
    if (p == null) return null;
    const keys = [
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
      'unique_name',
      'username',
      'Username',
      'preferred_username',
    ];
    for (final k in keys) {
      final v = p[k];
      if (v != null && '$v'.trim().isNotEmpty) return '$v'.trim();
    }
    return null;
  }
}
