import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:team/order/domain/product_category/product_category.dart';

part 'product.freezed.dart';
part 'product.g.dart';

@freezed
class Product with _$Product {
  const factory Product({
    required int id,
    required String name,
    required double buyAmount,
    required double sellAmount,
    required double dailyInstallmentAmount,
    required ProductCategory category,
    required DateTime createdAt,
  }) = _Product;

  factory Product.fromJson(Map<String, dynamic> json) => _$ProductFromJson(json);
}
