part of 'create_order_bloc.dart';

class InitialCreateOrderEvent extends DefaultEvent {}

class GetCustomersPaginatedEvent extends DefaultEvent {
  final DateTime? lastDateTime;
  final String? searchText;

  GetCustomersPaginatedEvent({this.lastDateTime, this.searchText});
}

class GetProductCategoriesPaginatedEvent extends DefaultEvent {
  final DateTime? lastDateTime;
  final String? searchText;

  GetProductCategoriesPaginatedEvent({this.lastDateTime, this.searchText});
}

class GetProductsPaginatedEvent extends DefaultEvent {
  final DateTime? lastDateTime;
  final String? searchText;
  final int? categoryId;

  GetProductsPaginatedEvent({this.lastDateTime, this.searchText, this.categoryId});
}

class GetOrderLists extends DefaultEvent {}

class CreateCustomerEvent extends DefaultEvent {
  final User customer;

  CreateCustomerEvent(this.customer);
}

class EditCustomerEvent extends DefaultEvent {
  final User customer;

  EditCustomerEvent(this.customer);
}

class UploadOrEditAttachmentEvent extends DefaultEvent {
  final XFile xFile;
  final AttachmentType attachmentType;
  final int? attachmentId;

  UploadOrEditAttachmentEvent({required this.xFile, required this.attachmentType, this.attachmentId});
}

class GetCustomerDetailsEvent extends DefaultEvent {
  final int id;

  GetCustomerDetailsEvent(this.id);
}

class AddOrderItemEvent extends DefaultEvent {
  final OrderItemDetails orderItemDetails;

  AddOrderItemEvent(this.orderItemDetails);
}

class EditOrderItemEvent extends DefaultEvent {
  final OrderItemDetails orderItemDetails;
  final OrderItemDetails oldOrderItemDetails;

  EditOrderItemEvent({required this.orderItemDetails, required this.oldOrderItemDetails});
}

class DeleteOrderItemEvent extends DefaultEvent {
  final bool isInStorage;
  final int index;

  DeleteOrderItemEvent({required this.isInStorage, required this.index});
}

class CreateOrderEvent extends DefaultEvent {}

class StepFourCompleted extends DefaultEvent {}

class UpdateOrderWithSellerInfo extends DefaultEvent {
  final String saleAddress;
  final DateTime saleDateTime;

  UpdateOrderWithSellerInfo({required this.saleAddress, required this.saleDateTime});
}
