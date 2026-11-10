import 'package:uuid/uuid.dart';

import '../../../core/location/location_service.dart';
import '../../../core/network/api_exception.dart';
import '../../../core/security/secure_session_store.dart';
import '../../../core/time/iraq_date.dart';
import '../../customers/data/customers_local_datasource.dart';
import '../../../core/network/json_map.dart';
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

  String? _lastCollectMessage;
  String? _lastSyncError;

  @override
  String? get lastCollectMessage => _lastCollectMessage;

  @override
  String? get lastSyncError => _lastSyncError;

  @override
  Future<int> countPending() => _local.countPending();

  @override
  Future<List<PendingPayment>> listPending() async {
    final owner = await _session.ownerKey ?? '';
    final uid = int.tryParse(await _session.userId ?? '') ?? 0;
    return _local.listUploadable(
      ownerAsyncId: owner,
      delegateId: uid > 0 ? uid : null,
    );
  }

  @override
  Future<List<PendingPayment>> listTodayLocal() => _local.listToday();

  @override
  Future<List<TodayPaymentItem>> listTodayMerged() async {
    final local = await _local.listToday();
    final items = <TodayPaymentItem>[
      for (final p in local)
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
        ),
    ];

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
        final id = JsonMap.asInt(e, ['id', 'Id']) ?? 0;
        final date = JsonMap.str(e, ['date', 'Date']);
        items.add(
          TodayPaymentItem(
            keyId: 'server-$id',
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
      // Keep local rows even if remote fails.
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
    required double amount,
  }) async {
    _lastCollectMessage = null;

    final amountError = PaymentCollectionRules.validateAmount(amount);
    if (amountError != null) {
      _lastCollectMessage = amountError;
      return false;
    }

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
    if (customer == null) {
      _lastCollectMessage =
          'الزبون غير موجود في الكاش المحلي — حدّث القائمة أولاً';
      return false;
    }
    final orderId = customer.activeOrderId;
    if (orderId == null || orderId <= 0) {
      _lastCollectMessage =
          'لا يوجد طلب نشط محفوظ لهذا الزبون — حدّث القائمة ثم أعد المحاولة';
      return false;
    }

    final day = IraqDate.isoDate();
    final hasUnsyncedPending =
        await _local.hasUnsyncedForCustomer(customerId);
    if (hasUnsyncedPending) {
      _lastCollectMessage = PaymentCollectionRules.blockMessage(
        alreadyCollectedToday: false,
        hasUnsyncedPending: true,
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
        await _local.clearDayLock(
          customerId: customerId,
          delegateId: delegateId,
          businessDay: day,
        );
        alreadyCollectedToday = false;
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
    final createdAtUtc = DateTime.now().toUtc().toIso8601String();
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
      await _local.insertPending(payment);
      await _local.insertDayLock(
        customerId: customerId,
        delegateId: delegateId,
        businessDay: day,
      );
      return true;
    } catch (e) {
      _lastCollectMessage = 'تعذر حفظ التسديد محلياً';
      return false;
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
          await _local.deleteById(payment.id);
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
