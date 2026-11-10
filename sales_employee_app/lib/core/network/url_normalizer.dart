/// Normalize branch API bases: https + trailing slash.
abstract final class UrlNormalizer {
  static String normalizeLinkDelegate(String raw) {
    var s = raw.trim();
    if (s.isEmpty) return '';
    if (s.startsWith('http://')) {
      s = 'https://${s.substring(7)}';
    } else if (!s.startsWith('https://') && !s.startsWith('http')) {
      s = 'https://$s';
    }
    if (!s.endsWith('/')) s = '$s/';
    return s;
  }
}
