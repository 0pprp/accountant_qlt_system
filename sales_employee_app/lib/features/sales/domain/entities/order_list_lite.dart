import 'package:sales_employee_app/core/network/json_map.dart';

class OrderListLite {
  const OrderListLite({
    required this.id,
    required this.name,
    this.mandobFullName,
  });

  final int id;
  final String name;
  final String? mandobFullName;

  /// القائمة والمندوب مفهوم واحد — عرض اسم القائمة فقط.
  String get label => name.trim().isEmpty ? (mandobFullName ?? '#$id') : name.trim();

  factory OrderListLite.fromJson(Map<String, dynamic> json) {
    return OrderListLite(
      id: JsonMap.asInt(json, ['id', 'Id']) ?? 0,
      name: JsonMap.str(json, ['name', 'Name', 'listName', 'ListName']),
      mandobFullName: () {
        final m = JsonMap.str(json, ['mandobFullName', 'MandobFullName', 'mandobName', 'MandobName']);
        return m.isEmpty ? null : m;
      }(),
    );
  }
}
