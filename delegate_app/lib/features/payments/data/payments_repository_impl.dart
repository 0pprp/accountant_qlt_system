import 'package:flutter/foundation.dart';
import 'package:uuid/uuid.dart';

import '../../../core/location/location_service.dart';
import '../../../core/network/api_exception.dart';
import '../../../core/security/secure_session_store.dart';
import '../../../core/time/iraq_date.dart';
import '../../customers/data/customers_local_datasource.dart';
import '../../../core/network/json_map.dart';
import '../domain/entities/paid_collection.dart';
import '../domain/entities/pending_payment.dart';
import '../domain/entities/today_payment_item.dart';
import '../domain/payment_collection_rules.dart';
import '../domain/repositories/payments_repository.dart';
import 'payments_local_datasource.dart';
import 'payments_remote_datasource.dart';

class PaymentsRepositoryImpl implements PaymentsRepository {
  PaymentsRepositoryImpl({
    required PaymentsLocalDataSource local,
    required PaymentsRemoteDataSource remote,
    required CustomersLocalDataSource customersLocal,
    required SecureSessionStore session,
    required LocationService location,
  })  : _local = local,
        _remote = remote,
        _customersLocal = customersLocal,
        _session = session,
        _location = location;

  final PaymentsLocalDataSource _local;
  final PaymentsRemoteDataSource _remote;
  final CustomersLocalDataSource _customersLocal;
  final SecureSessionStore _session;
  final LocationService _location;
  final _uuid = const Uuid();
  final Set<int> _collectInFlight = <int>{};

  String? _lastCollectMessage;
  String? _lastSyncError;

  @override
  String? get lastCollectMessage => _lastCollectMessage;

  @override
  String? get lastSyncError => _lastSyncError;

  @override
  Future<int> countPending() async => (await listPending()).length;

  @override
  Future<List<PendingPayment>> listPending() async {
    final owner = await _session.ownerKey ?? '';
    final uid = int.tryParse(await _session.userId ?? '') ?? 0;
    if (owner.isEmpty || uid <= 0) return [];
    await _local.reclaimForOwner(ownerKey: owner, delegateId: uid);
    return _local.listUploadable(
      ownerAsyncId: owner,
      delegateId: uid > 0 ? uid : null,
    );
  }

  @override
  Future<List<PendingPayment>> listTodayLocal() => _local.listToday();

  @override
  Future<PaidCollection?> todayPaidForCustomer(int customerId) async {
    final uid = int.tryParse(await _session.userId ?? '') ?? 0;
    if (uid <= 0 || customerId <= 0) return null;
    final paid = await _local.paidCollectionForCustomer(
      customerId: customerId,
      delegateId: uid,
    );
    if (paid != null && paid.hasValidUuid && paid.amount > 0) return paid;

    final local = await _local.listToday();
    for (final p in local) {
      if (p.delegateId != uid || p.customerId != customerId) continue;
      if (p.amount <= 0 || p.clientPaymentId.trim().isEmpty) continue;
      return PaidCollection(
        customerId: p.customerId,
        delegateId: p.delegateId,
        businessDay: IraqDate.isoDate(),
        amount: p.amount,
        clientPaymentId: p.clientPaymentId,
        createdAtUtc: p.createdAtUtc,
        customerName: p.customerName,
      );
    }
    return null;
  }

  @override
  Future<List<TodayPaymentItem>> listTodayMerged() async {
    final uid = int.tryParse(await _session.userId ?? '') ?? 0;
    final local = await _local.listToday();
    final ledger = await _local.listPaidCollectionsToday(
      delegateId: uid > 0 ? uid : null,
    );

    final seenCustomers = <int>{};
    final seenClientPaymentIds = <String>{};
    final items = <TodayPaymentItem>[];
    var skippedLocalDup = 0;

    // 1) طابور محلي — صف واحد لكل زبون (الأحدث أولاً من listToday)
    for (final p in local) {
      if (p.delegateId != uid) continue;
      if (p.customerId > 0 && seenCustomers.contains(p.customerId)) {
        skippedLocalDup++;
        debugPrint(
          'PaymentDup: skip local id=${p.id} customer=${p.customerId} '
          'cpid=${p.clientPaymentId} status=${p.syncStatus}',
        );
        continue;
      }
      if (p.customerId > 0) seenCustomers.add(p.customerId);
      final cpid = p.clientPaymentId.trim();
      if (cpid.isNotEmpty) seenClientPaymentIds.add(cpid.toLowerCase());
      items.add(
        TodayPaymentItem(
          keyId: 'local-${p.id}',
          customerId: p.customerId,
          customerName: p.customerName ?? 'زبون #${p.customerId}',
          amount: p.amount,
          isLocal: true,
          statusLabel: _localStatusLabel(p.syncStatus),
          orderId: p.orderId,
          syncError: p.syncError,
          createdAtUtc: p.createdAtUtc,
          clientPaymentId: p.clientPaymentId,
        ),
      );
    }

    // 2) مخزن المسددين (يبقى بعد الحذف من الطابور)
    for (final p in ledger) {
      if (seenCustomers.contains(p.customerId)) continue;
      if (!p.hasValidUuid || p.amount <= 0) continue;
      seenCustomers.add(p.customerId);
      seenClientPaymentIds.add(p.clientPaymentId.trim().toLowerCase());
      items.add(
        TodayPaymentItem(
          keyId: 'paid-${p.clientPaymentId}',
          customerId: p.customerId,
          customerName: p.customerName ?? 'زبون #${p.customerId}',
          amount: p.amount,
          isLocal: false,
          statusLabel: 'مسدد اليوم',
          createdAtUtc: p.createdAtUtc,
          clientPaymentId: p.clientPaymentId,
          dateLabel: p.businessDay,
        ),
      );
    }

    // 3) سيرفر — احتياطي
    try {
      final day = IraqDate.isoDate();
      final remote = await _remote.fetchTodayPayments(
        startDate: day,
        endDate: day,
      );
      for (final e in remote) {
        final name = JsonMap.str(e, [
          'customerFullName',
          'CustomerFullName',
          'fullName',
          'FullName',
        ]);
        final amount = JsonMap.asDouble(e, ['amount', 'Amount']) ?? 0;
        final orderId = JsonMap.asInt(e, ['orderId', 'OrderId']);
        final customerId = JsonMap.asInt(e, ['customerId', 'CustomerId']);
        final id = JsonMap.asInt(e, ['id', 'Id']) ?? 0;
        final date = JsonMap.str(e, ['date', 'Date']);
        final remoteCpid = JsonMap.str(e, [
          'clientPaymentId',
          'ClientPaymentId',
        ]).trim().toLowerCase();
        if (remoteCpid.isNotEmpty &&
            seenClientPaymentIds.contains(remoteCpid)) {
          continue;
        }
        if (customerId != null &&
            customerId > 0 &&
            seenCustomers.contains(customerId)) {
          continue;
        }
        if (customerId != null && customerId > 0) {
          seenCustomers.add(customerId);
        }
        if (remoteCpid.isNotEmpty) seenClientPaymentIds.add(remoteCpid);
        items.add(
          TodayPaymentItem(
            keyId: 'server-$id',
            customerId: customerId,
            customerName: name.isEmpty ? 'زبون' : name,
            amount: amount,
            isLocal: false,
            statusLabel: 'مُرسل للسيرفر',
            orderId: orderId,
            dateLabel: date.isEmpty ? day : date,
          ),
        );
      }
    } catch (_) {
      // Keep local/ledger rows even if remote fails.
    }

    if (skippedLocalDup > 0) {
      debugPrint(
        'PaymentDup: collapsed $skippedLocalDup local row(s); '
        'shown=${items.length}',
      );
    }

    items.sort((a, b) {
      if (a.isLocal != b.isLocal) return a.isLocal ? -1 : 1;
      return b.keyId.compareTo(a.keyId);
    });
    return items;
  }

  String _localStatusLabel(String status) {
    switch (status) {
      case PaymentSyncStatuses.pending:
        return 'بانتظار الإرسال';
      case PaymentSyncStatuses.syncing:
        return 'جارٍ الإرسال';
      case PaymentSyncStatuses.failed:
        return 'فشل الإرسال';
      default:
        return status;
    }
  }

  @override
  Future<bool> collectOffline({
    required int customerId,
    required String customerName,
    int? saleOrderId,
    required double amount,
  }) async {
    _lastCollectMessage = null;
    debugPrint('PaymentDup: collect start customer=$customerId amount=$amount');

    if (!_collectInFlight.add(customerId)) {
      debugPrint('PaymentDup: blocked in-flight customer=$customerId');
      _lastCollectMessage = 'جاري حفظ تسديد هذا الزبون — انتظر لحظة';
      return false;
    }

    try {
      final ownerKey = await _session.ownerKey;
      final userIdStr = await _session.userId;
      if (ownerKey == null || userIdStr == null) {
        _lastCollectMessage = 'يجب تسجيل الدخول أولاً';
        return false;
      }
      final delegateId = int.tryParse(userIdStr) ?? 0;
      if (delegateId <= 0) {
        _lastCollectMessage = 'جلسة المندوب غير صالحة';
        return false;
      }

      final customer = await _customersLocal.byId(customerId);
      if (customer == null && saleOrderId == null) {
        _lastCollectMessage =
            'الزبون غير موجود في الكاش المحلي — حدّث القائمة أولاً';
        return false;
      }
      final amountError = PaymentCollectionRules.validateAmount(
        amount,
        remaining: customer?.remaining,
      );
      if (amountError != null) {
        _lastCollectMessage = amountError;
        return false;
      }
      final orderId = saleOrderId ?? customer?.activeOrderId;
      if (orderId == null || orderId <= 0) {
        _lastCollectMessage =
            'لا يوجد طلب نشط محفوظ لهذا الزبون — حدّث القائمة ثم أعد المحاولة';
        return false;
      }

      final collectedAt = DateTime.now().toUtc();
      final day = IraqDate.isoDate(collectedAt);
      final hasUnsyncedPending =
          await _local.hasUnsyncedForCustomer(customerId, delegateId, day);
      if (hasUnsyncedPending) {
        debugPrint('PaymentDup: blocked unsynced pending customer=$customerId');
        _lastCollectMessage = PaymentCollectionRules.blockMessage(
          alreadyCollectedToday: false,
          hasUnsyncedPending: true,
        );
        return false;
      }

      // مخزن المسددين يمنع إعادة التحصيل حتى لو اُزيل قفل اليوم بالخطأ.
      final existingPaid = await _local.paidCollectionForCustomer(
        customerId: customerId,
        delegateId: delegateId,
      );
      if (existingPaid != null &&
          existingPaid.hasValidUuid &&
          existingPaid.amount > 0) {
        debugPrint(
          'PaymentDup: blocked paid-wallet customer=$customerId '
          'cpid=${existingPaid.clientPaymentId}',
        );
        _lastCollectMessage = PaymentCollectionRules.blockMessage(
          alreadyCollectedToday: true,
          hasUnsyncedPending: false,
        );
        return false;
      }

      var alreadyCollectedToday = await _local.hasDayLock(
        customerId: customerId,
        delegateId: delegateId,
        businessDay: day,
      );
      var offlineStaleLock = false;

      if (alreadyCollectedToday) {
        final onServer = await _remote.customerHasPaymentOnDate(
          customerId: customerId,
          isoDate: day,
        );
        if (onServer == false) {
          // لا تمسح القفل إن وُجدت حافظة محلية — فقط قفل يتيم بلا أثر محلي.
          final paidAfter = await _local.paidCollectionForCustomer(
            customerId: customerId,
            delegateId: delegateId,
          );
          if (paidAfter == null) {
            await _local.clearDayLock(
              customerId: customerId,
              delegateId: delegateId,
              businessDay: day,
            );
            alreadyCollectedToday = false;
            debugPrint('PaymentDup: cleared orphan day-lock customer=$customerId');
          } else {
            debugPrint(
              'PaymentDup: keep lock; paid wallet exists customer=$customerId',
            );
          }
        } else if (onServer == null) {
          offlineStaleLock = true;
        }
      }

      if (PaymentCollectionRules.blocksNewCollection(
        alreadyCollectedToday: alreadyCollectedToday,
        hasUnsyncedPending: false,
      )) {
        _lastCollectMessage = PaymentCollectionRules.blockMessage(
          alreadyCollectedToday: alreadyCollectedToday,
          hasUnsyncedPending: false,
          offlineStaleLock: offlineStaleLock,
        );
        return false;
      }

      final location = await _location.tryGetLocation() ?? '';
      final clientPaymentId = _uuid.v4();
      final createdAtUtc = collectedAt.toIso8601String();
      final receiptNumber = 'R-${DateTime.now().millisecondsSinceEpoch}';

      final payment = PendingPayment(
        id: 0,
        customerId: customerId,
        customerName: customerName,
        delegateId: delegateId,
        orderId: orderId,
        amount: amount,
        location: location,
        clientPaymentId: clientPaymentId,
        createdAtUtc: createdAtUtc,
        receiptNumber: receiptNumber,
        ownerAsyncId: ownerKey,
        syncStatus: PaymentSyncStatuses.pending,
      );

      try {
        final rejected = await _local.insertCollectAtomic(
          payment: payment,
          businessDay: day,
        );
        if (rejected != null) {
          debugPrint(
            'PaymentDup: atomic reject reason=$rejected customer=$customerId',
          );
          _lastCollectMessage = PaymentCollectionRules.blockMessage(
            alreadyCollectedToday: rejected == 'collected',
            hasUnsyncedPending: rejected == 'pending',
          );
          return false;
        }
        debugPrint(
          'PaymentDup: collect ok customer=$customerId cpid=$clientPaymentId',
        );
        return true;
      } catch (e) {
        debugPrint('PaymentDup: collect insert failed customer=$customerId err=$e');
        _lastCollectMessage = 'تعذر حفظ التسديد محلياً';
        return false;
      }
    } finally {
      _collectInFlight.remove(customerId);
    }
  }

  @override
  Future<int> syncPending({SyncProgressCallback? onProgress}) async {
    _lastSyncError = null;
    final owner = await _session.ownerKey;
    final userIdStr = await _session.userId;
    if (owner == null || userIdStr == null) {
      _lastSyncError = 'الجلسة غير صالحة — سجّل الدخول مجدداً';
      return 0;
    }
    final delegateId = int.tryParse(userIdStr) ?? 0;

    await _local.reclaimForOwner(ownerKey: owner, delegateId: delegateId);

    final rows = await _local.listUploadable(
      ownerAsyncId: owner,
      delegateId: delegateId > 0 ? delegateId : null,
    );
    final total = rows.length;
    onProgress?.call(0, total);
    var uploaded = 0;
    var processed = 0;
    var stop = false;
    for (final row in rows) {
      if (stop) break;
      await _local.markSyncing(row.id);
      await _local.clearSyncError(row.id);

      final payment = row;
      try {
        if (payment.orderId == null || payment.orderId! <= 0) {
          final err =
              'طلب البيع مفقود لهذا التسديد — حدّث البيانات دون نقل المبلغ لبيع آخر';
          _lastSyncError = err;
          await _local.markFailed(payment.id, permanent: false, error: err);
          continue;
        }

        final result = await _remote.postPaymentIdempotent(payment);
        if (result.success) {
          // حذف محلي فقط بعد إقرار يثبت نفس العملية.
          await _local.confirmPayment(payment, PaymentsRemoteDataSource.collectionDateOf(payment));
          uploaded++;
        } else {
          final err = result.message ?? 'فشل الإرسال';
          _lastSyncError = err;
          await _local.markFailed(payment.id, error: err);
        }
      } on ApiException catch (e) {
        // لا تُغيّر OrderId ولا تحذف عند غموض/فقدان وصول.
        _lastSyncError = e.message;
        await _local.markFailed(
          payment.id,
          permanent: false,
          error: e.message,
        );
        if (e.statusCode == 401) stop = true;
      } catch (e) {
        final err = '$e'.replaceFirst(RegExp(r'^[^:]+:\s*'), '');
        _lastSyncError = err;
        await _local.markFailed(payment.id, error: err);
      } finally {
        processed++;
        onProgress?.call(processed, total);
      }
    }
    return uploaded;
  }
}
