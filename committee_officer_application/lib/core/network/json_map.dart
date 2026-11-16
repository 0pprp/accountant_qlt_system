/// Flexible JSON field access (PascalCase / camelCase).
abstract final class JsonMap {
  static Object? pick(Map map, List<String> keys) {
    for (final k in keys) {
      if (map.containsKey(k) && map[k] != null) return map[k];
    }
    final lower = {
      for (final e in map.entries) '${e.key}'.toLowerCase(): e.value,
    };
    for (final k in keys) {
      final v = lower[k.toLowerCase()];
      if (v != null) return v;
    }
    return null;
  }

  static String str(Map map, List<String> keys, {String fallback = ''}) {
    final v = pick(map, keys);
    if (v == null) return fallback;
    return '$v'.trim();
  }

  static int? asInt(Map map, List<String> keys) {
    final v = pick(map, keys);
    if (v == null) return null;
    if (v is int) return v;
    if (v is num) return v.toInt();
    return int.tryParse('$v'.trim());
  }

  static bool asBool(Map map, List<String> keys, {bool fallback = false}) {
    final v = pick(map, keys);
    if (v == null) return fallback;
    if (v is bool) return v;
    final s = '$v'.trim().toLowerCase();
    if (s == 'true' || s == '1') return true;
    if (s == 'false' || s == '0') return false;
    return fallback;
  }
}
