export enum DeviceType {
  Desktop = 0,
  Mobile = 1,
  Tablet = 2,
  Bot = 3,
  Unknown = 4,
}

export enum ActivityType {
  Login = 0,
  Logout = 1,
  RefreshToken = 2,
  UserCreated = 3,
  UserUpdated = 4,
  UserPermissionsUpdated = 5,
  UserSalaryDetailUpdated = 6,
  UserPersonalDocumentsCompleted = 7,
  BranchCreated = 8,
  BranchUpdated = 9,
  BranchDeleted = 10,
  CustomerCreated = 11,
  CustomerUpdated = 12,
  ProductCreated = 13,
  ProductUpdated = 14,
  ProductDeleted = 15,
  ProductCategoryCreated = 16,
  ProductCategoryUpdated = 17,
  ProductCategoryDeleted = 18,
  OrderCreated = 19,
  OrderUpdated = 20,
  OrderDeleted = 21,
  OrderApprovalStatusChanged = 22,
  OrderSellerInfoSet = 23,
  OrderAttachmentsCompleted = 24,
  OrderListCreated = 25,
  OrderListUpdated = 26,
  InstallmentPaymentCreated = 27,
  InstallmentPaymentUpdated = 28,
  InstallmentPaymentDeleted = 29,
  InstallmentPaymentsSynced = 30,
  PurchaseCreated = 31,
  PurchaseUpdated = 32,
  ExpenseCreated = 33,
  ExpenseUpdated = 34,
  CashDeliveryCreated = 35,
  CashDeliveriesCreated = 36,
  SafeTransferCreated = 37,
  TransactionStatusChanged = 38,
  AttachmentCreated = 39,
  AttachmentUpdated = 40,
  NotificationMarkedAsRead = 41,
  AllNotificationsMarkedAsRead = 42,
}

export enum TargetEntityType {
  User = 0,
  Branch = 1,
  Customer = 2,
  Order = 3,
  OrderList = 4,
  Product = 5,
  ProductCategory = 6,
  InstallmentPayment = 7,
  Purchase = 8,
  Expense = 9,
  Safe = 10,
  Transaction = 11,
  Attachment = 12,
  Notification = 13,
}

export const ACTIVITY_TYPE_KEYS: Record<ActivityType, string> = {
  [ActivityType.Login]: 'activityLog.activityType.login',
  [ActivityType.Logout]: 'activityLog.activityType.logout',
  [ActivityType.RefreshToken]: 'activityLog.activityType.refreshToken',
  [ActivityType.UserCreated]: 'activityLog.activityType.userCreated',
  [ActivityType.UserUpdated]: 'activityLog.activityType.userUpdated',
  [ActivityType.UserPermissionsUpdated]:
    'activityLog.activityType.userPermissionsUpdated',
  [ActivityType.UserSalaryDetailUpdated]:
    'activityLog.activityType.userSalaryDetailUpdated',
  [ActivityType.UserPersonalDocumentsCompleted]:
    'activityLog.activityType.userPersonalDocumentsCompleted',
  [ActivityType.BranchCreated]: 'activityLog.activityType.branchCreated',
  [ActivityType.BranchUpdated]: 'activityLog.activityType.branchUpdated',
  [ActivityType.BranchDeleted]: 'activityLog.activityType.branchDeleted',
  [ActivityType.CustomerCreated]: 'activityLog.activityType.customerCreated',
  [ActivityType.CustomerUpdated]: 'activityLog.activityType.customerUpdated',
  [ActivityType.ProductCreated]: 'activityLog.activityType.productCreated',
  [ActivityType.ProductUpdated]: 'activityLog.activityType.productUpdated',
  [ActivityType.ProductDeleted]: 'activityLog.activityType.productDeleted',
  [ActivityType.ProductCategoryCreated]:
    'activityLog.activityType.productCategoryCreated',
  [ActivityType.ProductCategoryUpdated]:
    'activityLog.activityType.productCategoryUpdated',
  [ActivityType.ProductCategoryDeleted]:
    'activityLog.activityType.productCategoryDeleted',
  [ActivityType.OrderCreated]: 'activityLog.activityType.orderCreated',
  [ActivityType.OrderUpdated]: 'activityLog.activityType.orderUpdated',
  [ActivityType.OrderDeleted]: 'activityLog.activityType.orderDeleted',
  [ActivityType.OrderApprovalStatusChanged]:
    'activityLog.activityType.orderApprovalStatusChanged',
  [ActivityType.OrderSellerInfoSet]:
    'activityLog.activityType.orderSellerInfoSet',
  [ActivityType.OrderAttachmentsCompleted]:
    'activityLog.activityType.orderAttachmentsCompleted',
  [ActivityType.OrderListCreated]: 'activityLog.activityType.orderListCreated',
  [ActivityType.OrderListUpdated]: 'activityLog.activityType.orderListUpdated',
  [ActivityType.InstallmentPaymentCreated]:
    'activityLog.activityType.installmentPaymentCreated',
  [ActivityType.InstallmentPaymentUpdated]:
    'activityLog.activityType.installmentPaymentUpdated',
  [ActivityType.InstallmentPaymentDeleted]:
    'activityLog.activityType.installmentPaymentDeleted',
  [ActivityType.InstallmentPaymentsSynced]:
    'activityLog.activityType.installmentPaymentsSynced',
  [ActivityType.PurchaseCreated]: 'activityLog.activityType.purchaseCreated',
  [ActivityType.PurchaseUpdated]: 'activityLog.activityType.purchaseUpdated',
  [ActivityType.ExpenseCreated]: 'activityLog.activityType.expenseCreated',
  [ActivityType.ExpenseUpdated]: 'activityLog.activityType.expenseUpdated',
  [ActivityType.CashDeliveryCreated]:
    'activityLog.activityType.cashDeliveryCreated',
  [ActivityType.CashDeliveriesCreated]:
    'activityLog.activityType.cashDeliveriesCreated',
  [ActivityType.SafeTransferCreated]:
    'activityLog.activityType.safeTransferCreated',
  [ActivityType.TransactionStatusChanged]:
    'activityLog.activityType.transactionStatusChanged',
  [ActivityType.AttachmentCreated]:
    'activityLog.activityType.attachmentCreated',
  [ActivityType.AttachmentUpdated]:
    'activityLog.activityType.attachmentUpdated',
  [ActivityType.NotificationMarkedAsRead]:
    'activityLog.activityType.notificationMarkedAsRead',
  [ActivityType.AllNotificationsMarkedAsRead]:
    'activityLog.activityType.allNotificationsMarkedAsRead',
}

export const TARGET_ENTITY_TYPE_KEYS: Record<TargetEntityType, string> = {
  [TargetEntityType.User]: 'activityLog.targetEntityType.user',
  [TargetEntityType.Branch]: 'activityLog.targetEntityType.branch',
  [TargetEntityType.Customer]: 'activityLog.targetEntityType.customer',
  [TargetEntityType.Order]: 'activityLog.targetEntityType.order',
  [TargetEntityType.OrderList]: 'activityLog.targetEntityType.orderList',
  [TargetEntityType.Product]: 'activityLog.targetEntityType.product',
  [TargetEntityType.ProductCategory]:
    'activityLog.targetEntityType.productCategory',
  [TargetEntityType.InstallmentPayment]:
    'activityLog.targetEntityType.installmentPayment',
  [TargetEntityType.Purchase]: 'activityLog.targetEntityType.purchase',
  [TargetEntityType.Expense]: 'activityLog.targetEntityType.expense',
  [TargetEntityType.Safe]: 'activityLog.targetEntityType.safe',
  [TargetEntityType.Transaction]: 'activityLog.targetEntityType.transaction',
  [TargetEntityType.Attachment]: 'activityLog.targetEntityType.attachment',
  [TargetEntityType.Notification]: 'activityLog.targetEntityType.notification',
}

export const DEVICE_TYPE_KEYS: Record<DeviceType, string> = {
  [DeviceType.Desktop]: 'activityLog.deviceType.desktop',
  [DeviceType.Mobile]: 'activityLog.deviceType.mobile',
  [DeviceType.Tablet]: 'activityLog.deviceType.tablet',
  [DeviceType.Bot]: 'activityLog.deviceType.bot',
  [DeviceType.Unknown]: 'activityLog.deviceType.unknown',
}
