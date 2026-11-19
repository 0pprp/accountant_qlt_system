# جرد مصدر النجف

القاعدة: `DatabaseCompanyNajaf` على `169.58.236.52` — SQL Server على Ubuntu.
وقت السحب: 2026-11-18. **النظام حيّ**، فالأرقام لقطة لحظتها لا أكثر.

- الجداول: **181**
- مجموع الصفوف: **1,070,036**
- جداول فيها بيانات: **118**
- جداول فارغة: **63**
- مفاتيح أجنبية: **262**

## المجاميع المالية وقت السحب

| المؤشر | القيمة |
|---|---:|
| عدد المبيعات | 5,832 |
| مجموع البيع | 7,434,756,500 |
| مجموع القسط اليومي | 67,254,606 |
| مجموع الكلفة | 5,310,656,817 |
| عدد التسديدات | 348,666 |
| مجموع التسديدات | 5,364,881,958 |
| المتبقّي المعروض | 2,069,874,000 |

> البيع ناقص التسديدات = 2,069,874,**542**، والمتبقّي المعروض 2,069,874,**000**.
> الفرق 542 ديناراً سببه `ROUND(...,-3)` داخل `View_CustomersRemaining` — موثّق ومقبول.

## الجداول بترتيب الحجم

| الجدول | الصفوف | أعمدة | مفاتيح أجنبية |
|---|---:|---:|---:|
| `AddToBox` | 360,965 | 19 | 10 |
| `CustomersPayments` | 348,666 | 16 | 4 |
| `SalesTrackingEvents` | 146,705 | 6 | 0 |
| `FollowerTrackingEvents` | 60,649 | 6 | 0 |
| `BmCustomerRatingSnapshots` | 20,962 | 9 | 0 |
| `Activities` | 20,481 | 8 | 1 |
| `WithdrawalFromBox` | 14,453 | 23 | 13 |
| `TransferBoxs` | 11,684 | 12 | 3 |
| `FollowerCustomerRatingSnapshots` | 10,711 | 9 | 0 |
| `DelegateDevicePaymentAudit` | 8,513 | 15 | 0 |
| `SelectItemsSales` | 6,487 | 9 | 3 |
| `SelectItemsWithdrawal` | 6,126 | 9 | 3 |
| `Customers` | 5,861 | 28 | 3 |
| `CustomersSales` | 5,832 | 20 | 5 |
| `LocationPoints` | 5,543 | 10 | 1 |
| `WithdrawalStores` | 5,449 | 9 | 2 |
| `SuppliersAccounts` | 3,446 | 12 | 4 |
| `BuysItems` | 3,386 | 9 | 3 |
| `SalesLocationPoints` | 2,860 | 16 | 1 |
| `Items` | 2,771 | 18 | 2 |
| `SelectItemsAddToStores` | 2,491 | 9 | 3 |
| `DeleteData` | 2,465 | 94 | 0 |
| `Buys` | 1,721 | 22 | 4 |
| `SalesRequestHistory` | 1,592 | 11 | 0 |
| `CustomerLocationEstimates` | 1,577 | 8 | 0 |
| `AddToStores` | 1,306 | 9 | 2 |
| `BarcodeItems` | 1,167 | 9 | 3 |
| `SalesDocuments` | 625 | 6 | 1 |
| `Documents` | 557 | 14 | 1 |
| `SalesCustomerDocuments` | 414 | 11 | 0 |
| `SalesRequests` | 347 | 53 | 0 |
| `FollowerListRatingSnapshots` | 312 | 7 | 0 |
| `FollowerLocationPoints` | 287 | 14 | 0 |
| `SalesWorkShifts` | 251 | 12 | 0 |
| `Prospects` | 240 | 18 | 1 |
| `FollowerFieldFollowups` | 207 | 11 | 0 |
| `CustomerNotes` | 205 | 7 | 1 |
| `SalesCustomerNotes` | 170 | 8 | 0 |
| `SetPermissionsToPermissionsTypes` | 150 | 7 | 2 |
| `FollowerWorkShifts` | 149 | 11 | 0 |
| `DirectorBoardOperatorListRatings` | 142 | 7 | 0 |
| `Boxes` | 128 | 7 | 0 |
| `RefreshTokens` | 124 | 12 | 2 |
| `SalesDrafts` | 121 | 43 | 0 |
| `SalesShopProfiles` | 119 | 17 | 0 |
| `SalesDraftItems` | 110 | 7 | 1 |
| `CustomerShopFollowerCapture` | 109 | 8 | 0 |
| `CustomerShopFollowerPhotos` | 109 | 4 | 0 |
| `SalesArchiveItems` | 98 | 17 | 1 |
| `Permissions` | 85 | 8 | 1 |
| `AuditLog` | 84 | 20 | 0 |
| `OrderEvents` | 76 | 13 | 2 |
| `FollowerSaleAuthentications` | 75 | 12 | 0 |
| `UsersSelectedCities` | 73 | 7 | 2 |
| `Notifications` | 58 | 26 | 4 |
| `Delegates` | 54 | 26 | 4 |
| `BmRatingSnapshots` | 53 | 7 | 0 |
| `Users` | 48 | 59 | 4 |
| `SelectDelegate` | 47 | 8 | 3 |
| `SalesFilterHistory` | 45 | 9 | 0 |
| `DelegateFirstCollectionEvaluations` | 39 | 11 | 0 |
| `FollowerListMonthlySnapshots` | 33 | 7 | 0 |
| `WorkSessions` | 33 | 12 | 1 |
| `Shortcut` | 29 | 8 | 0 |
| `SalesEmployeeListDayRating` | 26 | 7 | 0 |
| `Orders` | 24 | 77 | 3 |
| `OrderItems` | 21 | 4 | 1 |
| `SalesFilterUserCities` | 20 | 5 | 1 |
| `WorkTargetDays` | 20 | 9 | 1 |
| `LinkCustomer` | 19 | 6 | 0 |
| `Cities` | 18 | 8 | 1 |
| `Groups` | 17 | 6 | 0 |
| `LegalFollowupCases` | 14 | 19 | 0 |
| `PinTargetDays` | 14 | 7 | 1 |
| `Devices` | 13 | 13 | 2 |
| `DelegateComplaints` | 12 | 11 | 0 |
| `WorkAlerts` | 12 | 12 | 2 |
| `SalesArchiveBundles` | 11 | 13 | 0 |
| `__CustomerAppMigrations` | 11 | 2 | 0 |
| `LegalFollowupCaseDocuments` | 10 | 9 | 0 |
| `FollowerUserLists` | 7 | 4 | 2 |
| `CustomerLocationEstimateRefresh` | 7 | 2 | 0 |
| `AppSettings` | 6 | 6 | 0 |
| `FollowerLiveLocations` | 5 | 10 | 0 |
| `Materials` | 5 | 10 | 0 |
| `SetUsersPermissionsTypes` | 5 | 7 | 2 |
| `WalletEntries` | 4 | 10 | 4 |
| `ExchangeItems` | 4 | 10 | 2 |
| `ExchangeItemsDebts` | 4 | 12 | 2 |
| `__Migrations` | 4 | 2 | 0 |
| `Broadcasts` | 3 | 15 | 0 |
| `CustomersRequest` | 3 | 22 | 2 |
| `Products` | 3 | 9 | 0 |
| `PayoutMessages` | 3 | 12 | 1 |
| `SalesRequestNameTransfers` | 3 | 10 | 0 |
| `Stores` | 3 | 11 | 2 |
| `TrustReceipts` | 2 | 70 | 0 |
| `SalesEmployeeLiveLocations` | 2 | 10 | 1 |
| `PermissionsTypes` | 2 | 11 | 0 |
| `DecisionNotifications` | 2 | 4 | 0 |
| `CustomerWeekDecisions` | 2 | 12 | 0 |
| `DelegatesDebts` | 2 | 12 | 2 |
| `FollowerComplaints` | 2 | 10 | 0 |
| `FollowerDelegateDayRating` | 2 | 8 | 0 |
| `DollarAmount` | 1 | 8 | 1 |
| `Employees` | 1 | 19 | 2 |
| `CustomersPaymentsRequest` | 1 | 21 | 2 |
| `BmRatingDeclineAlerts` | 1 | 12 | 0 |
| `Advertisement` | 1 | 4 | 0 |
| `CompanyInformation` | 1 | 14 | 0 |
| `CrmSalesAssignCursor` | 1 | 2 | 0 |
| `Payouts` | 1 | 14 | 1 |
| `LegalConversionRequests` | 1 | 13 | 0 |
| `SalesArchivePhotos` | 1 | 5 | 1 |
| `SelectItemsTransferStoresTemporary` | 1 | 8 | 2 |
| `Suppliers` | 1 | 15 | 2 |
| `SelectItemsWithdrawalTemporary` | 1 | 8 | 2 |
| `SelectItemSalesTemporaryRequest` | 1 | 8 | 2 |
| `SelectItemsSaleBalanceOldRequest` | 0 | 7 | 2 |
| `SelectItemsSaleBalanceRequest` | 0 | 7 | 2 |
| `SelectItemsSaleBalanceTemporaryRequest` | 0 | 7 | 2 |
| `SelectItemsSalesRequest` | 0 | 8 | 2 |
| `SelectItemsSalesRequestOld` | 0 | 8 | 2 |
| `SelectItemsTransferStores` | 0 | 9 | 3 |
| `SelectPaymentCustomerTemporary` | 0 | 9 | 2 |
| `SupplierItem` | 0 | 13 | 2 |
| `SupplierItemRequest` | 0 | 12 | 1 |
| `SupplierItemRequestOld` | 0 | 12 | 1 |
| `SupplierRequest` | 0 | 11 | 1 |
| `StoreBalance` | 0 | 11 | 2 |
| `TransferStores` | 0 | 10 | 3 |
| `WaiverAccountantRequests` | 0 | 15 | 0 |
| `SalesWaiverApplications` | 0 | 7 | 0 |
| `SelectedBoxes` | 0 | 7 | 2 |
| `SelectedCities` | 0 | 7 | 2 |
| `SelectItemBuyTemporary` | 0 | 8 | 2 |
| `SelectItemCustomer` | 0 | 8 | 2 |
| `SelectItemCustomerAddRequest` | 0 | 8 | 1 |
| `SelectItemCustomerAddRequestTemp` | 0 | 9 | 3 |
| `SelectItemCustomerTemp` | 0 | 8 | 2 |
| `SelectItemDamageTemporary` | 0 | 8 | 2 |
| `SelectItemRestoreBuyTemporary` | 0 | 8 | 2 |
| `SelectBuyBalance` | 0 | 8 | 3 |
| `SelectDamagedItems` | 0 | 9 | 3 |
| `SelectDebtDelegateTemporaries` | 0 | 10 | 4 |
| `SelectItemsAddToStoresTemporary` | 0 | 8 | 2 |
| `SelectItemSaleBalance` | 0 | 8 | 3 |
| `SelectItemSalesTemporary` | 0 | 8 | 2 |
| `Merchant` | 0 | 16 | 1 |
| `MerchantNotification` | 0 | 12 | 2 |
| `HostHistory` | 0 | 6 | 0 |
| `ItemMerchant` | 0 | 13 | 3 |
| `ItemMerchantImage` | 0 | 7 | 1 |
| `ItemMerchantImageTemp` | 0 | 7 | 1 |
| `OtpRequests` | 0 | 11 | 0 |
| `ListSupervisionDayRatings` | 0 | 6 | 0 |
| `SaleBalanceLimit` | 0 | 5 | 1 |
| `CustomerAddRequest` | 0 | 13 | 1 |
| `CustomerBalanceRequest` | 0 | 22 | 2 |
| `CustomerNotification` | 0 | 12 | 2 |
| `CustomerOldRequest` | 0 | 7 | 1 |
| `CustomerPaymentBalance` | 0 | 14 | 4 |
| `CustomerPaymentBalanceRequest` | 0 | 13 | 2 |
| `CustomerSaleBalance` | 0 | 18 | 4 |
| `CustomerSaleBalanceRequest` | 0 | 16 | 3 |
| `CustomerSaleBalanceRequestOld` | 0 | 16 | 3 |
| `Balance` | 0 | 17 | 3 |
| `BalanceType` | 0 | 8 | 1 |
| `BuyBalance` | 0 | 13 | 4 |
| `Category` | 0 | 8 | 1 |
| `CustomersSalesRequest` | 0 | 16 | 3 |
| `CustomersSalesRequestOld` | 0 | 16 | 3 |
| `DamagedItems` | 0 | 9 | 1 |
| `DelegatesSalaries` | 0 | 9 | 2 |
| `DirectorSalesEmployeeBoardRatings` | 0 | 8 | 0 |
| `EmployeesSalaries` | 0 | 9 | 2 |
| `EmployeeDebts` | 0 | 12 | 2 |
| `FollowerCustomerNotes` | 0 | 7 | 0 |
| `FollowerArchiveRequests` | 0 | 20 | 0 |
| `FollowerDelegateNotes` | 0 | 8 | 0 |
| `FollowerProfiles` | 0 | 8 | 1 |

## نموذج المال — مُتحقَّق منه حيّاً

المبالغ **ليست** في جداول المبيعات والتسديدات. هذه خريطة المصدر الحقيقي:

| ما نريده | من أين يُؤخذ | تحذير |
|---|---|---|
| سعر البيع | `View_CustomersSales.AmountTotalSalesDenar` | `CustomersSales` **بلا أعمدة مبالغ** إطلاقاً |
| القسط اليومي | `View_CustomersSales.AmountDaySalesDenar` | — |
| كلفة الشراء | `View_CustomersSales.AmountTotalCostDenar` | — |
| مبلغ التسديد | `View_AddToBox.AmountDenar` | `CustomersPayments` **بلا عمود مبلغ** |
| المتبقّي | `View_CustomersRemaining.AmountRemaining` | مُدوَّر بـ`ROUND(...,-3)` |
| بنود المبيع | `SelectItemsSales` = `ItemID` + `Quantity` فقط | **بلا سعر** — السعر من `Items` |

### الخطر الأول: العمود الخام بالدولار

`AddToBox.Amount` مجموعه **٣٬٧٠٥٬٠٢٨** بينما `View_AddToBox.AmountDenar` مجموعه **٥٬٣٦٤٬٨٨١٬٩٥٨`.
النسبة ≈ **١٤٤٨** وهو سعر الصرف. أي أن العمود الخام بالدولار والـview يحوّله.

> **المحمِّل يقرأ من الـviews حصراً.** قراءة `Amount` الخام تُدخل أرقاماً أصغر بـ١٤٤٨ مرة
> ولن يشتكي أي قيد في القاعدة — وهذا أسوأ أنواع الأعطاب.

### الخطر الثاني: حُلَّ

كان الفارق بين `AddToBox` (٣٦٠٬٩٦٥) و`CustomersPayments` (٣٤٨٬٦٦٦) مجهولاً — وهو ١٢٬٢٩٩ صفاً.
التفكيك الحيّ أجاب:

| الفئة | الصفوف | ماذا تعني |
|---|---:|---|
| `CustomerPaymentID IS NOT NULL` | 348,666 | تسديد زبون — يطابق جدول التسديدات تماماً |
| `TransferBoxID IS NOT NULL` | 11,639 | نقل بين صندوقين |
| لا هذا ولا ذاك | 660 | إيداع مباشر في الصندوق |
| **المجموع** | **360,965** | ✓ |

و`COUNT(DISTINCT CustomerPaymentID)` = ٣٤٨٬٦٦٦ أيضاً، فالعلاقة **١:١ تماماً**.
**لا مال مجهول المصدر.** كل صف له تصنيف وكل تصنيف له نوع حركة مقابل عندنا:

```
تسديد زبون  → SafeMovementKind.Collection   (موجب)
نقل بين صندوقين → TransferOut + TransferIn   (صفّان بـTransferGroupId واحد)
إيداع مباشر  → SafeMovementKind.Deposit      (موجب)
سحب (WithdrawalFromBox) → Withdrawal         (سالب)
```

### الصناديق

`Boxes` فيه **١٢٨** صندوقاً. عندنا الخزائن يُنشئها `SafeProvisioner` عند الإقلاع،
فالمحمِّل **لا يُدرج خزائن إطلاقاً** بل يبني خريطة `صندوق → خزينة` ويكتب الحركات عليها.
أي صندوق بلا مقابل تُوجَّه حركاته لقاصة الفرع بملاحظة صريحة، ولا تُسقَط أبداً.
