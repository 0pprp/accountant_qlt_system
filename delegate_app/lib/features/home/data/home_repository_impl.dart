import '../../../core/security/secure_session_store.dart';
import '../../../core/time/iraq_date.dart';
import '../../customers/data/customers_local_datasource.dart';
import '../../payments/data/payments_local_datasource.dart';
import '../../ratings/domain/services/customer_rating_calculator.dart';
import '../domain/entities/delegate_home_snapshot.dart';
import '../domain/entities/home_announcement.dart';
import '../domain/repositories/home_repository.dart';
import 'announcements_remote_datasource.dart';

class HomeRepositoryImpl implements HomeRepository {
  HomeRepositoryImpl({
    required CustomersLocalDataSource customersLocal,
    required PaymentsLocalDataSource paymentsLocal,
    required SecureSessionStore session,
    AnnouncementsRemoteDataSource? announcementsRemote,
  })  : _customersLocal = customersLocal,
        _paymentsLocal = paymentsLocal,
        _session = session,
        _announcementsRemote = announcementsRemote;

  final CustomersLocalDataSource _customersLocal;
  final PaymentsLocalDataSource _paymentsLocal;
  final SecureSessionStore _session;
  final AnnouncementsRemoteDataSource? _announcementsRemote;

  @override
  Future<DelegateHomeSnapshot> loadHomeSnapshot() async {
    final asOf = IraqDate.calendarDate();
    final name = (await _session.delegateName)?.trim();
    final city = (await _session.cityName)?.trim();
    final customers = await _customersLocal.list();

    final scores = <int>[];
    var installmentSum = 0.0;
    for (final c in customers) {
      final last = CustomerRatingCalculator.parseDate(c.lastPaymentDate);
      final remaining = c.remaining ?? 0;
      if (!CustomerRatingCalculator.inOverallCohort(
        remaining: remaining,
        lastPayment: last,
        asOf: asOf,
      )) {
        continue;
      }
      final score = CustomerRatingCalculator.scoreFor(
        amountTotalSales: c.salePrice ?? 0,
        receiptsTotal: c.receiptTotal ?? 0,
        amountRemaining: remaining,
        dateSaleDevice: CustomerRatingCalculator.parseDate(c.dateSaleDevice),
        lastPaymentDate: last,
        asOf: asOf,
      );
      if (score == null) continue;
      scores.add(score);
      installmentSum += c.dailyInstallment ?? 0;
    }

    final current = CustomerRatingCalculator.averageScore(scores);
    // Preview prior week: slightly lower seed until historical API exists.
    final previous = scores.isEmpty ? 0.0 : (current - 0.8);
    final trend = CustomerRatingCalculator.trendArrow(current, previous);

    final yesterday = await _yesterdayIncome();
    final announcements = await _loadAnnouncements();

    return DelegateHomeSnapshot(
      delegateName: (name != null && name.isNotEmpty)
          ? name
          : 'مندوب #${await _session.delegateId ?? ''}',
      listName: (city != null && city.isNotEmpty) ? city : 'قائمة التحصيل',
      currentRating: current,
      previousWeekRating: previous,
      trend: trend,
      installmentSum: installmentSum,
      yesterdayIncome: yesterday,
      announcements: announcements.items,
      announcementsState: announcements.state,
      announcementsError: announcements.errorMessage,
      tutorialVideoUrl: null,
    );
  }

  Future<AnnouncementsLoadResult> _loadAnnouncements() async {
    final remote = _announcementsRemote;
    if (remote == null) {
      return const AnnouncementsLoadResult.error(
        'خدمة الإعلانات غير مهيأة.',
      );
    }
    return remote.listForDelegate();
  }

  Future<double> _yesterdayIncome() async {
    try {
      final yesterday = IraqDate.calendarDate().subtract(const Duration(days: 1));
      final rows = await _paymentsLocal.listAll();
      var sum = 0.0;
      for (final p in rows) {
        final created = DateTime.tryParse(p.createdAtUtc)?.toUtc();
        if (created == null) continue;
        final bag = created.add(const Duration(hours: 3));
        final day = DateTime(bag.year, bag.month, bag.day);
        if (day == yesterday) sum += p.amount;
      }
      return sum;
    } catch (_) {
      return 0;
    }
  }

}
