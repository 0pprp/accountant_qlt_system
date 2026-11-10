import 'package:sales_employee_app/core/network/json_map.dart';

class CustomerLite {
  const CustomerLite({
    required this.id,
    required this.fullName,
    this.phone,
    this.address,
  });

  final int id;
  final String fullName;
  final String? phone;
  final String? address;

  factory CustomerLite.fromJson(Map<String, dynamic> json) {
    return CustomerLite(
      id: JsonMap.asInt(json, ['id', 'Id', 'customerId', 'CustomerId']) ?? 0,
      fullName: JsonMap.str(json, ['fullName', 'FullName', 'name', 'Name']),
      phone: JsonMap.str(json, ['phone', 'Phone', 'mobile', 'Mobile']).isEmpty
          ? null
          : JsonMap.str(json, ['phone', 'Phone', 'mobile', 'Mobile']),
      address: JsonMap.str(json, ['address', 'Address']).isEmpty
          ? null
          : JsonMap.str(json, ['address', 'Address']),
    );
  }
}
