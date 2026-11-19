# تصنيف جداول النجف

لكل جدول: هل ننقله، وإلى أي كيان عندنا، ولماذا.

| التصنيف | المعنى | العدد |
|---|---|---:|
| **ينتقل** | له مقابل عندنا ونحتاج أرقامه | 36 |
| **مشتق** | يمكن إعادة حسابه فلا يُنقل | 21 |
| **خارج النطاق** | جلسات وتتبّع وإشعارات وصلاحيات | 25 |
| **فارغ** | صفر صفوف | 59 |
| **يحتاج قراراً** | لم أصنّفه — قرارك | 40 |

---

## ينتقل (36)

| الجدول | الصفوف | الكيان المقابل | الملاحظة |
|---|---:|---|---|
| `AddToBox` | 360,965 | SafeMovement | قيد دخول الصندوق — هو مصدر مبلغ التسديد |
| `CustomersPayments` | 348,666 | InstallmentPayment | رأس التسديد — المبلغ من AddToBox |
| `WithdrawalFromBox` | 14,453 | SafeMovement | سحب من الصندوق |
| `TransferBoxs` | 11,684 | SafeMovement | نقل بين الصناديق — صفّان متقابلان |
| `SelectItemsSales` | 6,487 | OrderItem | بنود المبيع: ItemID + Quantity، السعر من Items |
| `SelectItemsWithdrawal` | 6,126 | — | بنود الإخراج المخزني |
| `Customers` | 5,861 | Customer | بروفايل الزبون |
| `CustomersSales` | 5,832 | Order | رأس المبيع — المبالغ من View_CustomersSales |
| `WithdrawalStores` | 5,449 | — | إخراج مخزني |
| `SuppliersAccounts` | 3,446 | — | حسابات المورّدين |
| `BuysItems` | 3,386 | PurchaseItem | بنود المشتريات |
| `Items` | 2,771 | Product | المواد |
| `SelectItemsAddToStores` | 2,491 | — | بنود الإدخال المخزني |
| `Buys` | 1,721 | Purchase | المشتريات |
| `CustomerLocationEstimates` | 1,577 | — | تقدير موقع الزبون |
| `AddToStores` | 1,306 | — | إدخال مخزني |
| `BarcodeItems` | 1,167 | Product | باركودات المواد |
| `SalesDocuments` | 625 | Attachment | مستندات المبيعات |
| `Documents` | 557 | Attachment | المستندات |
| `SalesCustomerDocuments` | 414 | Attachment | مستندات زبون المبيعات |
| `SalesRequests` | 347 | SalesRequest | طلبات المبيعات |
| `CustomerNotes` | 205 | CustomerNote | ملاحظات الزبون |
| `SalesCustomerNotes` | 170 | CustomerNote | ملاحظات زبون المبيعات |
| `Boxes` | 128 | Safe | الصناديق — تُطابَق بخزائن SafeProvisioner لا تُدرَج |
| `Delegates` | 54 | User + OrderList | المندوب وقائمته |
| `Users` | 48 | User | حسابات التشغيل — بلا Admin |
| `Cities` | 18 | Province/Branch | نطاق المدينة |
| `Groups` | 17 | OrderList | تجميعة قوائم |
| `LegalFollowupCases` | 14 | LegalFollowupCase | القضايا القانونية |
| `LegalFollowupCaseDocuments` | 10 | Attachment | مستندات القضايا |
| `Stores` | 3 | Warehouse | المخازن |
| `DollarAmount` | 1 | — | سعر الصرف — مرجعي للقراءة |
| `Employees` | 1 | User | الموظفون |
| `CompanyInformation` | 1 | — | إعدادات الفرع |
| `LegalConversionRequests` | 1 | LegalConversionRequest | طلبات التحويل القانوني |
| `Suppliers` | 1 | — | المورّدون |

## يحتاج قراراً (40)

| الجدول | الصفوف | الكيان المقابل | الملاحظة |
|---|---:|---|---|
| `SalesWorkShifts` | 251 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `Prospects` | 240 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `FollowerFieldFollowups` | 207 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `FollowerWorkShifts` | 149 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `SalesShopProfiles` | 119 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `CustomerShopFollowerCapture` | 109 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `CustomerShopFollowerPhotos` | 109 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `OrderEvents` | 76 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `FollowerSaleAuthentications` | 75 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `UsersSelectedCities` | 73 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `SelectDelegate` | 47 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `SalesFilterHistory` | 45 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `DelegateFirstCollectionEvaluations` | 39 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `Shortcut` | 29 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `Orders` | 24 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `OrderItems` | 21 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `SalesFilterUserCities` | 20 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `WorkTargetDays` | 20 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `LinkCustomer` | 19 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `PinTargetDays` | 14 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `Devices` | 13 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `DelegateComplaints` | 12 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `WorkAlerts` | 12 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `__CustomerAppMigrations` | 11 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `FollowerUserLists` | 7 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `CustomerLocationEstimateRefresh` | 7 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `AppSettings` | 6 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `Materials` | 5 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `WalletEntries` | 4 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `ExchangeItems` | 4 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `ExchangeItemsDebts` | 4 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `__Migrations` | 4 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `Products` | 3 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `TrustReceipts` | 2 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `CustomerWeekDecisions` | 2 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `DelegatesDebts` | 2 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `FollowerComplaints` | 2 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `Advertisement` | 1 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `CrmSalesAssignCursor` | 1 | ؟ | غير مصنَّف بعد — نقرّره سوياً |
| `Payouts` | 1 | ؟ | غير مصنَّف بعد — نقرّره سوياً |

## مشتق (21)

| الجدول | الصفوف | الكيان المقابل | الملاحظة |
|---|---:|---|---|
| `BmCustomerRatingSnapshots` | 20,962 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `FollowerCustomerRatingSnapshots` | 10,711 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SalesRequestHistory` | 1,592 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `FollowerListRatingSnapshots` | 312 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `DirectorBoardOperatorListRatings` | 142 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SalesDrafts` | 121 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SalesDraftItems` | 110 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SalesArchiveItems` | 98 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `BmRatingSnapshots` | 53 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `FollowerListMonthlySnapshots` | 33 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SalesEmployeeListDayRating` | 26 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SalesArchiveBundles` | 11 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `CustomersRequest` | 3 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SalesRequestNameTransfers` | 3 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `FollowerDelegateDayRating` | 2 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `CustomersPaymentsRequest` | 1 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `BmRatingDeclineAlerts` | 1 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SalesArchivePhotos` | 1 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SelectItemsTransferStoresTemporary` | 1 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SelectItemsWithdrawalTemporary` | 1 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |
| `SelectItemSalesTemporaryRequest` | 1 | — | لقطة/مسوّدة قابلة لإعادة الحساب — يحتاج قراراً |

## خارج النطاق (25)

| الجدول | الصفوف | الكيان المقابل | الملاحظة |
|---|---:|---|---|
| `SalesTrackingEvents` | 146,705 | — | تتبّع |
| `FollowerTrackingEvents` | 60,649 | — | تتبّع |
| `Activities` | 20,481 | — | سجل نشاط |
| `DelegateDevicePaymentAudit` | 8,513 | — | تدقيق جهاز |
| `LocationPoints` | 5,543 | — | تتبّع |
| `SalesLocationPoints` | 2,860 | — | تتبّع |
| `DeleteData` | 2,465 | — | سجل حذف |
| `FollowerLocationPoints` | 287 | — | تتبّع |
| `SetPermissionsToPermissionsTypes` | 150 | — | صلاحيات |
| `RefreshTokens` | 124 | — | جلسات |
| `Permissions` | 85 | — | صلاحيات — تُعاد وفق Panel.Tab |
| `AuditLog` | 84 | — | سجل تدقيق |
| `Notifications` | 58 | — | إشعارات |
| `WorkSessions` | 33 | — | جلسات |
| `FollowerLiveLocations` | 5 | — | تتبّع |
| `SetUsersPermissionsTypes` | 5 | — | صلاحيات |
| `Broadcasts` | 3 | — | إشعارات |
| `PayoutMessages` | 3 | — | إشعارات |
| `SalesEmployeeLiveLocations` | 2 | — | تتبّع |
| `PermissionsTypes` | 2 | — | صلاحيات |
| `DecisionNotifications` | 2 | — | إشعارات |
| `MerchantNotification` | 0 | — | إشعارات |
| `HostHistory` | 0 | — | تشغيل وقتي |
| `OtpRequests` | 0 | — | جلسات |
| `CustomerNotification` | 0 | — | إشعارات |

## فارغ (59)

| الجدول | الصفوف | الكيان المقابل | الملاحظة |
|---|---:|---|---|
| `SelectItemsSaleBalanceOldRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemsSaleBalanceRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemsSaleBalanceTemporaryRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemsSalesRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemsSalesRequestOld` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemsTransferStores` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectPaymentCustomerTemporary` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SupplierItem` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SupplierItemRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SupplierItemRequestOld` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SupplierRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `StoreBalance` | 0 | — | لا صفوف — لا شيء يُنقل |
| `TransferStores` | 0 | — | لا صفوف — لا شيء يُنقل |
| `WaiverAccountantRequests` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SalesWaiverApplications` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectedBoxes` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectedCities` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemBuyTemporary` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemCustomer` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemCustomerAddRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemCustomerAddRequestTemp` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemCustomerTemp` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemDamageTemporary` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemRestoreBuyTemporary` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectBuyBalance` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectDamagedItems` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectDebtDelegateTemporaries` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemsAddToStoresTemporary` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemSaleBalance` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SelectItemSalesTemporary` | 0 | — | لا صفوف — لا شيء يُنقل |
| `Merchant` | 0 | — | لا صفوف — لا شيء يُنقل |
| `ItemMerchant` | 0 | — | لا صفوف — لا شيء يُنقل |
| `ItemMerchantImage` | 0 | — | لا صفوف — لا شيء يُنقل |
| `ItemMerchantImageTemp` | 0 | — | لا صفوف — لا شيء يُنقل |
| `ListSupervisionDayRatings` | 0 | — | لا صفوف — لا شيء يُنقل |
| `SaleBalanceLimit` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomerAddRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomerBalanceRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomerOldRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomerPaymentBalance` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomerPaymentBalanceRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomerSaleBalance` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomerSaleBalanceRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomerSaleBalanceRequestOld` | 0 | — | لا صفوف — لا شيء يُنقل |
| `Balance` | 0 | — | لا صفوف — لا شيء يُنقل |
| `BalanceType` | 0 | — | لا صفوف — لا شيء يُنقل |
| `BuyBalance` | 0 | — | لا صفوف — لا شيء يُنقل |
| `Category` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomersSalesRequest` | 0 | — | لا صفوف — لا شيء يُنقل |
| `CustomersSalesRequestOld` | 0 | — | لا صفوف — لا شيء يُنقل |
| `DamagedItems` | 0 | — | لا صفوف — لا شيء يُنقل |
| `DelegatesSalaries` | 0 | — | لا صفوف — لا شيء يُنقل |
| `DirectorSalesEmployeeBoardRatings` | 0 | — | لا صفوف — لا شيء يُنقل |
| `EmployeesSalaries` | 0 | — | لا صفوف — لا شيء يُنقل |
| `EmployeeDebts` | 0 | — | لا صفوف — لا شيء يُنقل |
| `FollowerCustomerNotes` | 0 | — | لا صفوف — لا شيء يُنقل |
| `FollowerArchiveRequests` | 0 | — | لا صفوف — لا شيء يُنقل |
| `FollowerDelegateNotes` | 0 | — | لا صفوف — لا شيء يُنقل |
| `FollowerProfiles` | 0 | — | لا صفوف — لا شيء يُنقل |

