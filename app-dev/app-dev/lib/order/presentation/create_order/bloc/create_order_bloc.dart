import 'package:collection/collection.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:image_picker/image_picker.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/account/domain/user_info/user_info.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/customer/mapper.dart';
import 'package:team/order/domain/order/order.dart';
import 'package:team/order/domain/order_details/order_details.dart';
import 'package:team/order/domain/order_item_details/order_item_details.dart';
import 'package:team/order/domain/order_list_info/order_list_info.dart';
import 'package:team/order/domain/product/product.dart';
import 'package:team/order/domain/product_category/product_category.dart';
import 'package:team/order/infrastructure/repositories/order_repository.dart';

part 'create_order_event.dart';
part 'create_order_state.dart';

class CreateOrderBloc extends DefaultBloc {
  final OrderRepository orderRepository;
  final AccountMapper accountMapper;
  final CustomerMapper customerMapper;
  final Order? order;
  int? defaultOrderListId;
  CreateOrderBloc({
    required this.orderRepository,
    required this.accountMapper,
    required this.customerMapper,
    this.order,
    this.defaultOrderListId,
  }) : super(CreateOrderInitialState()) {
    safeOn<InitialCreateOrderEvent>(_initialCreateOrder);
    safeOn<GetCustomerDetailsEvent>(_getCustomerEvent);
    safeOn<GetCustomersPaginatedEvent>(_getCustomersPaginatedEvent);
    safeOn<GetProductCategoriesPaginatedEvent>(_getProductCategoriesPaginatedEvent);
    safeOn<GetProductsPaginatedEvent>(_getProductsPaginatedEvent);
    safeOn<CreateCustomerEvent>(_createCustomerEvent);
    safeOn<EditCustomerEvent>(_editCustomerEvent);
    safeOn<UploadOrEditAttachmentEvent>(_uploadOrEditAttachment);
    safeOn<AddOrderItemEvent>(_addOrderItemEvent);
    safeOn<DeleteOrderItemEvent>(_deleteOrderItemEvent);
    safeOn<EditOrderItemEvent>(_editOrderItemEvent);
    safeOn<CreateOrderEvent>(_createOrderEvent);
    safeOn<StepFourCompleted>(_stepFourCompleted);
    safeOn<UpdateOrderWithSellerInfo>(_updateOrderWithSellerInfo);
    safeOn<GetOrderLists>(_getOrderLists);
  }
  final isMotaba = GetIt.I.get<AccountMapper>().isMotaba;

  OrderDetails? orderDetails;
  OrderListInfo? selectedOrderList;
  List<OrderListInfo>? orderLists;
  int? orderId;
  List<OrderItemDetails> inStorageOrderItems = [];
  List<OrderItemDetails> outStorageOrderItems = [];

  // Customers pagination state
  final List<UserInfo> customers = [];
  String? currentSearchText;
  bool hasMoreCustomers = true;
  bool isFetchingCustomers = false;

  // Product categories pagination state
  final List<ProductCategory> productCategories = [];
  String? currentCategoriesSearchText;
  bool hasMoreCategories = true;
  bool isFetchingCategories = false;

  // Products pagination state
  final List<Product> products = [];
  String? currentProductsSearchText;
  int? currentProductsCategoryId;
  bool hasMoreProducts = true;
  bool isFetchingProducts = false;

  List<AttachmentInformation> orderAttachments = [];
  UserInfo? selectedCustomerInfo;
  User? selectedCustomer;

  String? sellerFullName;
  bool hasSendAttachmentsUploadedRequest = false;

  void _initialCreateOrder(InitialCreateOrderEvent event, emit) async {
    if (order?.id != null) {
      orderId = order?.id;
      defaultOrderListId = order?.orderListId;
      orderDetails = await orderRepository.getOrderDetails(order!.id);
      selectedCustomer = await accountMapper.getUser(order!.customer.id, fromServer: true);
      selectedCustomerInfo = UserInfo(
        id: selectedCustomer!.id,
        fullName: selectedCustomer!.fullName,
        motherName: selectedCustomer!.motherName,
        businessName: selectedCustomer!.business.name,
        createdAt: DateTime.now(),
      );
      inStorageOrderItems = (orderDetails!.orderItems ?? []).where((element) => element.productId != null).toList();
      outStorageOrderItems = (orderDetails!.orderItems ?? []).where((element) => element.productId == null).toList();
      orderAttachments = orderDetails!.attachments ?? [];
    }
  }

  void _stepFourCompleted(StepFourCompleted event, emit) async {
    if (orderDetails == null || orderDetails?.step == OrderStep.attachments) {
      if (!hasSendAttachmentsUploadedRequest) {
        await orderRepository.completedAttachments(orderId!);
        hasSendAttachmentsUploadedRequest = true;
      }
    }
    if (!GetIt.I.get<AccountMapper>().isMotaba) {
      selectedOrderList ??= accountMapper.user!.orderList;
      sellerFullName = accountMapper.user!.fullName;
    } else {
      add(GetOrderLists());
    }
  }

  void _getOrderLists(GetOrderLists event, emit) async {
    if (orderLists != null) return;
    final response = await orderRepository.getOrdersList();
    orderLists =
        response
            .map(
              (e) => OrderListInfo(id: e.id, name: e.name, mandobFullName: e.mandobFullName),
            )
            .toList();
    selectedOrderList = orderLists?.firstWhereOrNull((element) => element.id == defaultOrderListId);
    sellerFullName = selectedOrderList?.mandobFullName;
  }

  void _createOrderEvent(CreateOrderEvent event, emit) async {
    final orderItems = inStorageOrderItems + outStorageOrderItems;
    if (orderId == null) {
      orderId = await orderRepository.createOrder(orderItems: orderItems, customerId: selectedCustomer!.id);
    } else {
      if (orderDetails != null) {
        orderDetails = orderDetails!.copyWith(orderItems: orderItems);
        await orderRepository.editOrder(orderDetails: orderDetails!, orderListId: defaultOrderListId);
      } else {
        await orderRepository.editOrderItems(orderId: orderId!, orderItems: orderItems);
      }
    }
  }

  void _updateOrderWithSellerInfo(UpdateOrderWithSellerInfo event, emit) async {
    if (orderDetails == null || orderDetails?.step != OrderStep.completed) {
      await orderRepository.updateOrderWithSellerInfo(
        orderId: orderId!,
        creationAddress: event.saleAddress,
        saleDateTime: event.saleDateTime,
        orderListId: GetIt.I.get<AccountMapper>().isMotaba ? selectedOrderList?.id ?? defaultOrderListId : null,
      );
    } else {
      await orderRepository.editOrder(
        orderDetails: orderDetails!.copyWith(
          creationAddress: event.saleAddress,
          saleDate: event.saleDateTime,
          saleTime: TimeOfDay.fromDateTime(event.saleDateTime),
        ),
        // orderListId: selectedOrderList?.id ?? defaultOrderListId,
      );
    }
  }

  void _uploadOrEditAttachment(UploadOrEditAttachmentEvent event, emit) async {
    bool isOrderRelated =
        event.attachmentType == AttachmentType.purchaseReceipt ||
        event.attachmentType == AttachmentType.trustReceipt ||
        event.attachmentType == AttachmentType.saleContract;
    if (event.attachmentId != null) {
      final relativePath = await orderRepository.editAttachment(
        file: event.xFile,
        userId: !isOrderRelated ? selectedCustomer!.id : null,
        attachmentType: event.attachmentType,
        attachmentId: event.attachmentId!,
        orderId: isOrderRelated ? orderId : null,
      );
      if (isOrderRelated) {
        orderAttachments =
            orderAttachments.map((e) {
              if (e.id == event.attachmentId) {
                return e.copyWith(relativePath: relativePath);
              }
              return e;
            }).toList();
      } else {
        final attachments =
            selectedCustomer!.attachments.map((e) {
              if (e.id == event.attachmentId) {
                return e.copyWith(relativePath: relativePath);
              }
              return e;
            }).toList();
        selectedCustomer = selectedCustomer?.copyWith(attachments: attachments);
      }
    } else {
      final attachment = await orderRepository.uploadAttachment(
        file: event.xFile,
        userId: !isOrderRelated ? selectedCustomer!.id : null,
        attachmentType: event.attachmentType,
        orderId: isOrderRelated ? orderId : null,
      );
      if (isOrderRelated) {
        orderAttachments = [...orderAttachments, attachment];
      } else {
        final attachments = [...selectedCustomer!.attachments, attachment];
        selectedCustomer = selectedCustomer?.copyWith(attachments: attachments);
      }
    }
  }

  Future<void> _getCustomersPaginatedEvent(GetCustomersPaginatedEvent event, emit) async {
    if (isFetchingCustomers) return;

    final isNewSearch = event.searchText != currentSearchText;
    if (isNewSearch) {
      customers.clear();
      hasMoreCustomers = true;
      currentSearchText = event.searchText;
    }

    if (!hasMoreCustomers) return;

    if (!isNewSearch && event.lastDateTime == null && customers.isNotEmpty) return;

    isFetchingCustomers = true;
    try {
      final newCustomers = await customerMapper.getCustomers(
        lastDateTime: isNewSearch ? null : event.lastDateTime,
        searchText: event.searchText,
      );

      if (newCustomers.isEmpty) {
        hasMoreCustomers = false;
        return;
      }

      customers.addAll(newCustomers);
    } finally {
      isFetchingCustomers = false;
    }
  }

  Future<void> _getProductCategoriesPaginatedEvent(GetProductCategoriesPaginatedEvent event, emit) async {
    if (isFetchingCategories) return;

    final isNewSearch = event.searchText != currentCategoriesSearchText;
    if (isNewSearch) {
      productCategories.clear();
      hasMoreCategories = true;
      currentCategoriesSearchText = event.searchText;
    }

    if (!hasMoreCategories) return;

    if (!isNewSearch && event.lastDateTime == null && productCategories.isNotEmpty) return;

    isFetchingCategories = true;
    try {
      final newCategories = await orderRepository.getProductCategories(
        lastDateTime: isNewSearch ? null : event.lastDateTime,
        searchText: event.searchText,
      );

      if (newCategories.isEmpty) {
        hasMoreCategories = false;
        return;
      }

      productCategories.addAll(newCategories);
    } finally {
      isFetchingCategories = false;
    }
  }

  Future<void> _getProductsPaginatedEvent(GetProductsPaginatedEvent event, emit) async {
    if (isFetchingProducts) return;

    // A new search is triggered by either a different search text OR a different category
    final isNewSearch = event.searchText != currentProductsSearchText || event.categoryId != currentProductsCategoryId;
    if (isNewSearch) {
      products.clear();
      hasMoreProducts = true;
      currentProductsSearchText = event.searchText;
      currentProductsCategoryId = event.categoryId;
    }

    if (!hasMoreProducts) return;

    if (!isNewSearch && event.lastDateTime == null && products.isNotEmpty) return;

    isFetchingProducts = true;
    try {
      final newProducts = await orderRepository.getProducts(
        lastDateTime: isNewSearch ? null : event.lastDateTime,
        searchText: event.searchText,
        categoryId: event.categoryId,
      );

      if (newProducts.isEmpty) {
        hasMoreProducts = false;
        return;
      }

      products.addAll(newProducts);
    } finally {
      isFetchingProducts = false;
    }
  }

  void _getCustomerEvent(GetCustomerDetailsEvent event, emit) async {
    selectedCustomer = await accountMapper.getUser(event.id, fromServer: true);
  }

  void _createCustomerEvent(CreateCustomerEvent event, emit) async {
    final customerId = await customerMapper.createCustomer(event.customer);
    selectedCustomer = event.customer.copyWith(id: customerId);
    selectedCustomerInfo = selectedCustomerInfo?.copyWith(id: customerId);
  }

  void _editCustomerEvent(EditCustomerEvent event, emit) async {
    await customerMapper.editCustomer(event.customer);
    selectedCustomer = event.customer;
  }

  void _addOrderItemEvent(AddOrderItemEvent event, emit) {
    if (event.orderItemDetails.productId != null) {
      inStorageOrderItems.add(event.orderItemDetails);
    } else {
      outStorageOrderItems.add(event.orderItemDetails);
    }
  }

  void _editOrderItemEvent(EditOrderItemEvent event, emit) {
    if (event.oldOrderItemDetails.productId != null) {
      inStorageOrderItems =
          inStorageOrderItems.map((item) => item == event.oldOrderItemDetails ? event.orderItemDetails : item).toList();
    } else {
      outStorageOrderItems =
          outStorageOrderItems
              .map((item) => item == event.oldOrderItemDetails ? event.orderItemDetails : item)
              .toList();
    }
  }

  void _deleteOrderItemEvent(DeleteOrderItemEvent event, emit) {
    if (event.isInStorage) {
      inStorageOrderItems.removeAt(event.index);
    } else {
      outStorageOrderItems.removeAt(event.index);
    }
  }
}
