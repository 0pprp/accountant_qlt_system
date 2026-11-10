import 'package:sales_employee_app/core/network/json_map.dart';

class ProductItem {
  const ProductItem({
    required this.id,
    required this.name,
    this.price,
    this.dailyInstallment,
    this.quantity,
    this.categoryName,
  });

  final int id;
  final String name;
  final double? price;
  final double? dailyInstallment;
  final double? quantity;
  final String? categoryName;

  factory ProductItem.fromJson(Map<String, dynamic> json) {
    return ProductItem(
      id: JsonMap.asInt(json, ['id', 'Id', 'productId', 'ProductId']) ?? 0,
      name: JsonMap.str(json, ['name', 'Name', 'productName', 'ProductName']),
      price: JsonMap.asDouble(json, [
        'price',
        'Price',
        'sellingPrice',
        'SellingPrice',
        'sellAmount',
        'SellAmount',
      ]),
      dailyInstallment: JsonMap.asDouble(json, [
        'dailyInstallment',
        'DailyInstallment',
        'dailyInstallmentAmount',
        'DailyInstallmentAmount',
        'dailyPayment',
        'DailyPayment',
      ]),
      quantity: JsonMap.asDouble(json, [
        'quantity',
        'Quantity',
        'stock',
        'Stock',
        'availableQty',
        'remainingCount',
        'RemainingCount',
      ]),
      categoryName: () {
        final nested = JsonMap.pick(json, ['category', 'Category']);
        if (nested is Map) {
          final n = JsonMap.str(nested, ['name', 'Name']);
          if (n.isNotEmpty) return n;
        }
        final flat = JsonMap.str(json, ['categoryName', 'CategoryName']);
        return flat.isEmpty ? null : flat;
      }(),
    );
  }
}
