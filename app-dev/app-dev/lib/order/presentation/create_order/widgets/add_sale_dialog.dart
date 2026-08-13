import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/text_util.dart';
import 'package:team/order/domain/order_item_details/order_item_details.dart';
import 'package:team/order/domain/product/product.dart';
import 'package:team/order/domain/product_category/product_category.dart';
import 'package:team/order/presentation/create_order/bloc/create_order_bloc.dart';

class AddSaleDialog extends StatefulWidget {
  const AddSaleDialog({
    super.key,
    this.orderItem,
    required this.isInStorage,
  });

  final OrderItemDetails? orderItem;
  final bool isInStorage;

  @override
  State<AddSaleDialog> createState() => _AddSaleDialogState();
}

class _AddSaleDialogState extends State<AddSaleDialog> {
  late final _bloc = context.read<CreateOrderBloc>();

  final TextEditingController _nameController = TextEditingController();
  final TextEditingController _categoryController = TextEditingController();
  final TextEditingController _productController = TextEditingController();
  final TextEditingController _quantityController = TextEditingController();
  final TextEditingController _sellPriceController = TextEditingController();
  final TextEditingController _buyPriceController = TextEditingController();
  final TextEditingController _dailyInstallmentController = TextEditingController();
  final TextEditingController _downPaymentController = TextEditingController();

  final FocusNode _categoryFocusNode = FocusNode();
  final MenuController _categoryMenuController = MenuController();
  Timer? _categorySearchDebounce;
  final _selectedCategory = ValueNotifier<ProductCategory?>(null);

  final FocusNode _productFocusNode = FocusNode();
  final MenuController _productMenuController = MenuController();
  Timer? _productSearchDebounce;
  final _selectedProduct = ValueNotifier<Product?>(null);

  double? _baseBuyAmount;
  double? _baseSellAmount;
  double? _baseDailyInstallmentAmount;

  bool _buyPriceEditedByUser = false;
  bool _sellPriceEditedByUser = false;
  bool _dailyInstallmentEditedByUser = false;

  bool _isAutoFillingPrices = false;

  bool get _isEditingExistingItem => widget.orderItem != null;

  static final _loadMoreCategoryEntry = ProductCategory(
    id: -1,
    name: 'تحميل المزيد...',
    createdAt: DateTime.fromMillisecondsSinceEpoch(0),
  );

  static final _loadMoreProductEntry = Product(
    id: -1,
    name: 'تحميل المزيد...',
    buyAmount: 0,
    sellAmount: 0,
    dailyInstallmentAmount: 0,
    category: ProductCategory(id: -1, name: ''),
    createdAt: DateTime.fromMillisecondsSinceEpoch(0),
  );

  @override
  void initState() {
    super.initState();

    if (widget.orderItem != null) {
      final item = widget.orderItem!;

      _quantityController.text = item.quantity.toString();
      _buyPriceController.text = IraqiDinarInputFormatter().format(item.buyAmount.toStringAsFixed(0));
      _sellPriceController.text = IraqiDinarInputFormatter().format(item.sellAmount.toStringAsFixed(0));
      _dailyInstallmentController.text = IraqiDinarInputFormatter().format(
        item.dailyInstallmentAmount.toStringAsFixed(0),
      );
      _downPaymentController.text = IraqiDinarInputFormatter().format(item.prepaymentAmount.toStringAsFixed(0));

      if (widget.isInStorage) {
        ProductCategory? existingCategory;
        for (final category in _bloc.productCategories) {
          if (category.id == item.productType) {
            existingCategory = category;
            break;
          }
        }
        if (existingCategory != null) {
          _selectedCategory.value = existingCategory;
          _categoryController.text = existingCategory.name;
        }

        if (item.productId != null) {
          Product? existingProduct;
          for (final product in _bloc.products) {
            if (product.id == item.productId) {
              existingProduct = product;
              break;
            }
          }
          if (existingProduct != null) {
            _selectedProduct.value = existingProduct;
          } else {
            _selectedProduct.value = Product(
              id: item.productId!,
              name: item.productName ?? '',
              buyAmount: item.buyAmount,
              sellAmount: item.sellAmount,
              dailyInstallmentAmount: item.dailyInstallmentAmount,
              category: ProductCategory(id: item.productType ?? 0, name: ''),
              createdAt: DateTime.now(),
            );
          }
          _productController.text = _selectedProduct.value!.name;

          if (existingCategory != null) {
            _bloc.add(GetProductsPaginatedEvent(categoryId: existingCategory.id));
          }
        }
      } else {
        _nameController.text = item.productName ?? '';
      }
    }

    if (widget.isInStorage) {
      if (widget.orderItem == null) {
        _bloc.add(GetProductsPaginatedEvent());
      }

      _categoryController.addListener(_onCategoryTextChanged);
      _productController.addListener(_onProductTextChanged);

      if (!_isEditingExistingItem) {
        _quantityController.addListener(_onQuantityChanged);
        _buyPriceController.addListener(_onBuyPriceChanged);
        _sellPriceController.addListener(_onSellPriceChanged);
        _dailyInstallmentController.addListener(_onDailyInstallmentChanged);
      }

      if (_bloc.productCategories.isEmpty) {
        _bloc.add(GetProductCategoriesPaginatedEvent());
      }
    }
  }

  @override
  void dispose() {
    _categorySearchDebounce?.cancel();
    _productSearchDebounce?.cancel();

    _categoryController.removeListener(_onCategoryTextChanged);
    _productController.removeListener(_onProductTextChanged);
    _quantityController.removeListener(_onQuantityChanged);
    _buyPriceController.removeListener(_onBuyPriceChanged);
    _sellPriceController.removeListener(_onSellPriceChanged);
    _dailyInstallmentController.removeListener(_onDailyInstallmentChanged);

    _categoryFocusNode.dispose();
    _productFocusNode.dispose();

    _selectedCategory.dispose();
    _selectedProduct.dispose();

    _nameController.dispose();
    _categoryController.dispose();
    _productController.dispose();
    _quantityController.dispose();
    _sellPriceController.dispose();
    _buyPriceController.dispose();
    _dailyInstallmentController.dispose();
    _downPaymentController.dispose();

    super.dispose();
  }

  void _onCategoryTextChanged() {
    _categorySearchDebounce?.cancel();
    final text = _categoryController.text;
    if (text == _loadMoreCategoryEntry.name) return;
    if (text == (_selectedCategory.value?.name ?? '')) return;
    if (_selectedCategory.value != null) {
      _selectedCategory.value = null;
      _selectedProduct.value = null;
      _productController.clear();
      _bloc.add(GetProductsPaginatedEvent());
    }

    _categorySearchDebounce = Timer(const Duration(milliseconds: 350), () {
      final currentText = _categoryController.text;
      if (_selectedCategory.value != null && _selectedCategory.value!.name == currentText) return;
      _bloc.add(
        GetProductCategoriesPaginatedEvent(
          searchText: currentText.isEmpty ? null : currentText,
        ),
      );
    });
  }

  void _onProductTextChanged() {
    _productSearchDebounce?.cancel();
    final text = _productController.text;
    if (text == _loadMoreProductEntry.name) return;
    if (text == (_selectedProduct.value?.name ?? '')) return;
    if (_selectedProduct.value != null) _selectedProduct.value = null;

    _productSearchDebounce = Timer(const Duration(milliseconds: 350), () {
      final currentText = _productController.text;
      if (_selectedProduct.value != null && _selectedProduct.value!.name == currentText) return;
      _bloc.add(
        GetProductsPaginatedEvent(
          searchText: currentText.isEmpty ? null : currentText,
          categoryId: _selectedCategory.value?.id,
        ),
      );
    });
  }

  void _onBuyPriceChanged() {
    if (_isAutoFillingPrices) return;
    _buyPriceEditedByUser = true;
  }

  void _onSellPriceChanged() {
    if (_isAutoFillingPrices) return;
    _sellPriceEditedByUser = true;
  }

  void _onDailyInstallmentChanged() {
    if (_isAutoFillingPrices) return;
    _dailyInstallmentEditedByUser = true;
  }

  void _onQuantityChanged() => _applyQuantityMultiplier();

  void _applyQuantityMultiplier() {
    if (_baseBuyAmount == null || _baseSellAmount == null || _baseDailyInstallmentAmount == null) return;

    final quantity = int.tryParse(_quantityController.text.trim()) ?? 1;
    final safeQuantity = quantity < 1 ? 1 : quantity;

    _isAutoFillingPrices = true;
    if (!_buyPriceEditedByUser) {
      _buyPriceController.text = IraqiDinarInputFormatter().format(
        (_baseBuyAmount! * safeQuantity).toStringAsFixed(0),
      );
    }
    if (!_sellPriceEditedByUser) {
      _sellPriceController.text = IraqiDinarInputFormatter().format(
        (_baseSellAmount! * safeQuantity).toStringAsFixed(0),
      );
    }
    if (!_dailyInstallmentEditedByUser) {
      _dailyInstallmentController.text = IraqiDinarInputFormatter().format(
        (_baseDailyInstallmentAmount! * safeQuantity).toStringAsFixed(0),
      );
    }
    _isAutoFillingPrices = false;
  }

  void _selectCategory(ProductCategory category) {
    final prevCategoryId = _selectedCategory.value?.id;
    _selectedCategory.value = category;
    _categoryController.text = category.name;
    _categoryMenuController.close();
    _categoryFocusNode.unfocus();

    if (prevCategoryId != category.id) {
      _selectedProduct.value = null;
      _productController.clear();
      _bloc.add(GetProductsPaginatedEvent(categoryId: category.id));
    }
  }

  void _selectProduct(Product product) {
    _selectedProduct.value = product;
    _productController.text = product.name;
    _productMenuController.close();
    _productFocusNode.unfocus();

    if (_isEditingExistingItem) {
      _buyPriceController.text = IraqiDinarInputFormatter().format(product.buyAmount.toStringAsFixed(0));
      _sellPriceController.text = IraqiDinarInputFormatter().format(product.sellAmount.toStringAsFixed(0));
      _dailyInstallmentController.text = IraqiDinarInputFormatter().format(
        product.dailyInstallmentAmount.toStringAsFixed(0),
      );
      return;
    }

    _baseBuyAmount = product.buyAmount;
    _baseSellAmount = product.sellAmount;
    _baseDailyInstallmentAmount = product.dailyInstallmentAmount;

    _buyPriceEditedByUser = false;
    _sellPriceEditedByUser = false;
    _dailyInstallmentEditedByUser = false;

    _applyQuantityMultiplier();
  }

  void _loadMoreCategories() {
    _categoryController.text = _selectedCategory.value?.name ?? '';
    final cats = _bloc.productCategories;
    _bloc.add(
      GetProductCategoriesPaginatedEvent(
        lastDateTime: cats.isNotEmpty ? cats.last.createdAt : null,
        searchText: _bloc.currentCategoriesSearchText,
      ),
    );
  }

  void _loadMoreProducts() {
    _productController.text = _selectedProduct.value?.name ?? '';
    final prods = _bloc.products;
    _bloc.add(
      GetProductsPaginatedEvent(
        lastDateTime: prods.isNotEmpty ? prods.last.createdAt : null,
        searchText: _bloc.currentProductsSearchText,
        categoryId: _selectedCategory.value?.id,
      ),
    );
  }

  bool _isDivisibleBy1000(TextEditingController controller) {
    final digits = controller.text.replaceAll(RegExp(r'[^\d]'), '');
    if (digits.isEmpty) return true;
    return int.parse(digits) % 1000 == 0;
  }

  bool get _isFormValid {
    final commonValid =
        _quantityController.text.trim().isNotEmpty &&
        _sellPriceController.text.trim().isNotEmpty &&
        _buyPriceController.text.trim().isNotEmpty &&
        _dailyInstallmentController.text.trim().isNotEmpty &&
        _downPaymentController.text.trim().isNotEmpty &&
        _isDivisibleBy1000(_sellPriceController) &&
        _isDivisibleBy1000(_buyPriceController) &&
        _isDivisibleBy1000(_dailyInstallmentController) &&
        _isDivisibleBy1000(_downPaymentController);

    if (widget.isInStorage) {
      return commonValid && _selectedProduct.value != null;
    }
    return commonValid && _nameController.text.trim().isNotEmpty;
  }

  @override
  Widget build(BuildContext context) {
    return Material(
      borderRadius: BorderRadius.circular(16),
      child: Padding(
        padding: EdgeInsets.only(
          right: 24,
          left: 24,
          top: 16,
          bottom: 16 + MediaQuery.of(context).viewInsets.bottom,
        ),
        child: ListView(
          shrinkWrap: true,
          children: [
            if (widget.isInStorage)
              DefaultBuilder<CreateOrderBloc>(
                buildWhen: (_, state) => state.event is GetProductCategoriesPaginatedEvent,
                builder: (context, state) {
                  final categories = _bloc.productCategories;
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      spacing: 6,
                      children: [
                        Text('فئة المبيع', style: AppTextStyle.bodyMedium.withColor(AppColor.text2)),
                        Theme(
                          data: Theme.of(context).copyWith(iconButtonTheme: IconButtonThemeData()),
                          child: DropdownMenu<ProductCategory?>(
                            controller: _categoryController,
                            focusNode: _categoryFocusNode,
                            menuController: _categoryMenuController,
                            expandedInsets: EdgeInsets.zero,
                            enableFilter: false,
                            enableSearch: false,
                            requestFocusOnTap: true,
                            closeBehavior: DropdownMenuCloseBehavior.none,
                            textInputAction: TextInputAction.next,
                            textStyle: AppTextStyle.bodyMedium,
                            hintText: 'اختر أو أدخل فئة المبيع',
                            inputDecorationTheme: InputDecorationTheme(
                              filled: true,
                              fillColor: AppColor.surface1,
                              hintStyle: TextStyle(
                                color: AppColor.text4,
                                fontSize: 10,
                                fontWeight: FontWeight.w500,
                              ),
                              focusedBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                                borderSide: BorderSide(width: 1.5, color: AppColor.primary),
                              ),
                              enabledBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                                borderSide: BorderSide(
                                  width: 1.5,
                                  color: _categoryController.text.isEmpty ? AppColor.stroke : AppColor.text2,
                                ),
                              ),
                            ),
                            alignmentOffset: const Offset(0, 8),
                            dropdownMenuEntries: [
                              ...categories.map((category) {
                                final isSelected = _selectedCategory.value?.id == category.id;
                                return DropdownMenuEntry<ProductCategory>(
                                  value: category,
                                  label: category.name,
                                  trailingIcon:
                                      isSelected ? Icon(Icons.check, size: 18, color: AppColor.primary) : null,
                                  style:
                                      isSelected
                                          ? MenuItemButton.styleFrom(
                                            backgroundColor: AppColor.primary.withValues(alpha: 0.08),
                                          )
                                          : null,
                                );
                              }),
                              if ((_bloc.hasMoreCategories && categories.isNotEmpty && categories.length > 19) ||
                                  _bloc.isFetchingCategories)
                                DropdownMenuEntry<ProductCategory>(
                                  value: _loadMoreCategoryEntry,
                                  label: _loadMoreCategoryEntry.name,
                                  enabled: !_bloc.isFetchingCategories,
                                  leadingIcon:
                                      _bloc.isFetchingCategories
                                          ? const SizedBox(
                                            width: 16,
                                            height: 16,
                                            child: CircularProgressIndicator(
                                              strokeWidth: 2,
                                              color: AppColor.primary,
                                            ),
                                          )
                                          : const Icon(Icons.expand_more),
                                ),
                            ],
                            onSelected: (category) {
                              if (category == null) return;
                              if (category.id == _loadMoreCategoryEntry.id) {
                                _loadMoreCategories();
                                return;
                              }
                              _selectCategory(category);
                            },
                          ),
                        ),
                      ],
                    ),
                  );
                },
              ),
            if (widget.isInStorage)
              DefaultBuilder<CreateOrderBloc>(
                buildWhen: (_, state) => state.event is GetProductsPaginatedEvent,
                builder: (context, state) {
                  final prods = _bloc.products;
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      spacing: 6,
                      children: [
                        Text('المنتج', style: AppTextStyle.bodyMedium.withColor(AppColor.text2)),
                        Theme(
                          data: Theme.of(context).copyWith(iconButtonTheme: IconButtonThemeData()),
                          child: DropdownMenu<Product?>(
                            controller: _productController,
                            focusNode: _productFocusNode,
                            menuController: _productMenuController,
                            expandedInsets: EdgeInsets.zero,
                            enableFilter: false,
                            enableSearch: false,
                            requestFocusOnTap: true,
                            closeBehavior: DropdownMenuCloseBehavior.none,
                            textInputAction: TextInputAction.next,
                            textStyle: AppTextStyle.bodyMedium,
                            hintText: 'اختر أو ابحث عن المنتج',
                            inputDecorationTheme: InputDecorationTheme(
                              filled: true,
                              fillColor: AppColor.surface1,
                              hintStyle: TextStyle(
                                color: AppColor.text4,
                                fontSize: 10,
                                fontWeight: FontWeight.w500,
                              ),
                              focusedBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                                borderSide: BorderSide(width: 1.5, color: AppColor.primary),
                              ),
                              enabledBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                                borderSide: BorderSide(
                                  width: 1.5,
                                  color: _productController.text.isEmpty ? AppColor.stroke : AppColor.text2,
                                ),
                              ),
                            ),
                            alignmentOffset: const Offset(0, 8),
                            dropdownMenuEntries: [
                              ...prods.map((product) {
                                final isSelected = _selectedProduct.value?.id == product.id;
                                return DropdownMenuEntry<Product>(
                                  value: product,
                                  label: product.name,
                                  trailingIcon:
                                      isSelected ? Icon(Icons.check, size: 18, color: AppColor.primary) : null,
                                  style:
                                      isSelected
                                          ? MenuItemButton.styleFrom(
                                            backgroundColor: AppColor.primary.withValues(alpha: 0.08),
                                          )
                                          : null,
                                );
                              }),
                              if ((_bloc.hasMoreProducts && prods.isNotEmpty && prods.length > 19) ||
                                  _bloc.isFetchingProducts)
                                DropdownMenuEntry<Product>(
                                  value: _loadMoreProductEntry,
                                  label: _loadMoreProductEntry.name,
                                  enabled: !_bloc.isFetchingProducts,
                                  leadingIcon:
                                      _bloc.isFetchingProducts
                                          ? const SizedBox(
                                            width: 16,
                                            height: 16,
                                            child: CircularProgressIndicator(
                                              strokeWidth: 2,
                                              color: AppColor.primary,
                                            ),
                                          )
                                          : const Icon(Icons.expand_more),
                                ),
                            ],
                            onSelected: (product) {
                              if (product == null) return;
                              if (product.id == _loadMoreProductEntry.id) {
                                _loadMoreProducts();
                                return;
                              }
                              _selectProduct(product);
                            },
                          ),
                        ),
                      ],
                    ),
                  );
                },
              ),
            if (!widget.isInStorage)
              InstallmentDetailsInput(
                controller: _nameController,
                title: 'المبيع',
                hint: 'ادخل اسم المبيع لاضافته',
                isPrice: false,
              ),
            InstallmentDetailsInput(
              controller: _quantityController,
              title: 'الكمية',
              hint: 'أدخل كمية المادة',
              isNumberOnly: true,
              isPrice: false,
            ),
            InstallmentDetailsInput(
              controller: _buyPriceController,
              title: 'سعر الشراء',
              hint: 'سعر الشراء للمنصر المباع',
              isNumberOnly: true,
            ),
            InstallmentDetailsInput(
              controller: _sellPriceController,
              title: 'السعر بالقسط',
              hint: 'السعر بالقسط للمنصر المباع',
              isNumberOnly: true,
            ),
            InstallmentDetailsInput(
              controller: _dailyInstallmentController,
              title: 'القسط اليومي',
              hint: 'القسط اليومي للمنصر المباع',
              isNumberOnly: true,
            ),
            InstallmentDetailsInput(
              controller: _downPaymentController,
              title: 'المقدمة',
              hint: 'الدفعة الأولى من القسط',
              isNumberOnly: true,
            ),
            AnimatedBuilder(
              animation: Listenable.merge([
                _nameController,
                _quantityController,
                _sellPriceController,
                _buyPriceController,
                _dailyInstallmentController,
                _downPaymentController,
                _selectedCategory,
                _selectedProduct,
              ]),
              builder: (context, _) {
                final isValid = _isFormValid;
                return FilledButton(
                  onPressed:
                      isValid
                          ? () {
                            final itemDetails = OrderItemDetails(
                              productId: widget.isInStorage ? _selectedProduct.value!.id : null,
                              productName:
                                  widget.isInStorage ? _selectedProduct.value!.name : _nameController.text.trim(),
                              productType: widget.isInStorage ? 0 : 1,
                              quantity: int.parse(_quantityController.text.trim()),
                              buyAmount: double.parse(
                                _buyPriceController.text.replaceAll(',', '').replaceAll('د.ع', '').trim(),
                              ),
                              sellAmount: double.parse(
                                _sellPriceController.text.replaceAll(',', '').replaceAll('د.ع', '').trim(),
                              ),
                              prepaymentAmount: double.parse(
                                _downPaymentController.text.replaceAll(',', '').replaceAll('د.ع', '').trim(),
                              ),
                              dailyInstallmentAmount: double.parse(
                                _dailyInstallmentController.text.replaceAll(',', '').replaceAll('د.ع', '').trim(),
                              ),
                            );
                            if (widget.orderItem != null) {
                              _bloc.add(
                                EditOrderItemEvent(
                                  orderItemDetails: itemDetails,
                                  oldOrderItemDetails: widget.orderItem!,
                                ),
                              );
                            } else {
                              _bloc.add(AddOrderItemEvent(itemDetails));
                            }
                            Navigator.pop(context);
                          }
                          : null,
                  style: FilledButton.styleFrom(
                    minimumSize: const Size(double.infinity, 52),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  ),
                  child: Text(
                    'متابعة',
                    style: AppTextStyle.headlineMedium.withColor(AppColor.white),
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}

class InstallmentDetailsInput extends StatelessWidget {
  const InstallmentDetailsInput({
    super.key,
    required this.controller,
    required this.title,
    required this.hint,
    this.isNumberOnly = false,
    this.isPrice = true,
  });

  final TextEditingController controller;
  final String title;
  final String hint;
  final bool isNumberOnly;
  final bool isPrice;

  String? _validate(String text) {
    if (!isPrice) return null;

    final digits = text.replaceAll(RegExp(r'[^\d]'), '');
    if (digits.isEmpty) return null;

    final value = int.parse(digits);
    if (value % 1000 != 0) {
      return 'المبلغ يجب أن يكون قابلاً للقسمة على 1000';
    }
    return null;
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        spacing: 6,
        children: [
          Text(title, style: AppTextStyle.bodyMedium.withColor(AppColor.text2)),
          ValueListenableBuilder<TextEditingValue>(
            valueListenable: controller,
            builder: (context, value, _) {
              final errorText = _validate(value.text);
              return DefaultTextField(
                textEditingController: controller,
                hint: hint,
                filled: true,
                errorText: errorText,
                onTap: () {
                  if (isPrice) {
                    controller.selection = TextSelection(
                      baseOffset: 0,
                      extentOffset: controller.text.length,
                    );
                  }
                },
                keyboardType: isNumberOnly ? TextInputType.number : null,
                inputFormatters:
                    isNumberOnly
                        ? [FilteringTextInputFormatter.digitsOnly, if (isPrice) IraqiDinarInputFormatter()]
                        : null,
                textInputAction: TextInputAction.next,
                textDirection: TextDirection.rtl,
              );
            },
          ),
        ],
      ),
    );
  }
}
