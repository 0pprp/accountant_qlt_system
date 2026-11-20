import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

import '../../core/formatting/iraq_time.dart';
import '../../core/network/api_errors.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/feedback.dart';
import '../../core/widgets/neu.dart';
import '../../injection.dart';
import '../promoter/models.dart';
import 'pin_detail_page.dart';
import 'pin_form_sheet.dart';

enum PinRange { today, yesterday, week, month, custom }

extension PinRangeX on PinRange {
  String get label => switch (this) {
    PinRange.today => 'اليوم',
    PinRange.yesterday => 'أمس',
    PinRange.week => 'آخر 7 أيام',
    PinRange.month => 'هذا الشهر',
    PinRange.custom => 'تاريخ محدد',
  };
}

(DateTime, DateTime) pinRangeDates(PinRange range, DateTime today, {DateTime? custom}) {
  switch (range) {
    case PinRange.today:
      return (today, today);
    case PinRange.yesterday:
      final y = today.subtract(const Duration(days: 1));
      return (y, y);
    case PinRange.week:
      return (today.subtract(const Duration(days: 6)), today);
    case PinRange.month:
      return (DateTime(today.year, today.month, 1), today);
    case PinRange.custom:
      final d = custom ?? today;
      return (d, d);
  }
}

Future<ShopPin?> createPinFlow(BuildContext context) async {
  final saved = await showPinForm(context);
  if (saved != null && context.mounted) {
    AppFeedback.snack(context, 'تم تثبيت المحل');
    unawaited(Injection.store.afterActivity());
  }
  return saved;
}

class PinsPage extends StatefulWidget {
  const PinsPage({super.key, required this.tab, required this.index});
  final ValueListenable<int> tab;
  final int index;

  @override
  State<PinsPage> createState() => _PinsPageState();
}

class _PinsPageState extends State<PinsPage> {
  final _items = <ShopPin>[];
  final _scroll = ScrollController();
  final _search = TextEditingController();
  PinRange _range = PinRange.today;
  DateTime? _customDay;
  int _page = 0;
  int? _total;
  bool _hasMore = true;
  bool _loading = false;
  String? _error;
  Timer? _debounce;
  CancelToken? _cancel;
  int _seq = 0;
  bool _started = false;
  DateTime? _loadedAt;

  @override
  void initState() {
    super.initState();
    _scroll.addListener(() {
      if (_scroll.position.extentAfter < 300) unawaited(_more());
    });
    widget.tab.addListener(_onTab);
  }

  @override
  void dispose() {
    widget.tab.removeListener(_onTab);
    _debounce?.cancel();
    _cancel?.cancel();
    _scroll.dispose();
    _search.dispose();
    super.dispose();
  }

  void _onTab() {
    if (widget.tab.value != widget.index) return;
    final at = _loadedAt;
    if (!_started || at == null || DateTime.now().difference(at) > const Duration(seconds: 15)) {
      _started = true;
      unawaited(_reload());
    }
  }

  Future<void> _reload() async {
    _page = 0;
    _hasMore = true;
    _error = null;
    await _more(reset: true);
  }

  Future<void> _more({bool reset = false}) async {
    if ((!reset && (_loading || !_hasMore))) return;
    if (reset) _cancel?.cancel();
    final seq = ++_seq;
    final token = CancelToken();
    _cancel = token;
    setState(() => _loading = true);
    final (from, to) = pinRangeDates(_range, IraqTime.today(), custom: _customDay);
    try {
      final next = reset ? 1 : _page + 1;
      final res = await Injection.api.myPins(
        from: IraqTime.key(from),
        to: IraqTime.key(to),
        q: _search.text,
        page: next,
        cancelToken: token,
      );
      if (!mounted || seq != _seq) return;
      setState(() {
        if (reset) _items.clear();
        _items.addAll(res.items);
        _page = next;
        _hasMore = res.hasMore;
        _total = res.total;
        _error = null;
        _loadedAt = DateTime.now();
      });
    } catch (e) {
      if (e is DioException && CancelToken.isCancel(e)) return;
      if (mounted && seq == _seq) setState(() => _error = ApiErrors.message(e));
    } finally {
      if (mounted && seq == _seq) setState(() => _loading = false);
    }
  }

  void _onSearch(String _) {
    _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 450), _reload);
  }

  Future<void> _pickRange(PinRange r) async {
    if (r == PinRange.custom) {
      final today = IraqTime.today();
      final picked = await showDatePicker(
        context: context,
        initialDate: _customDay ?? today,
        firstDate: today.subtract(const Duration(days: 365)),
        lastDate: today,
        helpText: 'اختر اليوم',
      );
      if (picked == null) return;
      _customDay = DateTime(picked.year, picked.month, picked.day);
    }
    setState(() => _range = r);
    unawaited(_reload());
  }

  Future<void> _add() async {
    final saved = await createPinFlow(context);
    if (saved != null) unawaited(_reload());
  }

  Future<void> _open(ShopPin p) async {
    final changed = await Navigator.of(context).push<bool>(
      MaterialPageRoute(builder: (_) => PinDetailPage(pinId: p.id, initial: p)),
    );
    if (changed == true) unawaited(_reload());
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(18, 16, 18, 12),
          child: PageHeader(
            title: 'المحلات',
            subtitle: _total == null ? 'المحلات التي ثبّتها' : 'العدد: $_total',
            trailing: SizedBox(
              width: 140,
              child: NeuButton(
                label: '＋ تثبيت محل',
                height: 44,
                onPressed: _add,
              ),
            ),
          ),
        ),
        SizedBox(
          height: 46,
          child: ListView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 18),
            children: [
              for (final r in PinRange.values)
                Padding(
                  padding: const EdgeInsetsDirectional.only(end: 10, bottom: 6),
                  child: NeuChip(
                    label: r == PinRange.custom && _customDay != null && _range == r
                        ? IraqTime.dateOnly(_customDay)
                        : r.label,
                    selected: _range == r,
                    onTap: () => _pickRange(r),
                  ),
                ),
            ],
          ),
        ),
        Padding(
          padding: const EdgeInsets.fromLTRB(18, 8, 18, 8),
          child: NeuTextField(
            controller: _search,
            hint: 'بحث بالاسم أو الهاتف',
            prefixIcon: Icons.search_rounded,
            onChanged: _onSearch,
            textInputAction: TextInputAction.search,
          ),
        ),
        Expanded(child: _list()),
      ],
    );
  }

  Widget _list() {
    if (_items.isEmpty) {
      if (_loading || !_started) return const Center(child: CircularProgressIndicator());
      if (_error != null) {
        return StateView(
          icon: Icons.cloud_off_rounded,
          message: _error!,
          actionLabel: 'إعادة المحاولة',
          onAction: _reload,
        );
      }
      return RefreshIndicator(
        onRefresh: _reload,
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          children: const [
            SizedBox(height: 80),
            StateView(icon: Icons.storefront_outlined, message: 'لا توجد محلات في هذه الفترة'),
          ],
        ),
      );
    }
    return RefreshIndicator(
      onRefresh: _reload,
      child: ListView.separated(
        controller: _scroll,
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(18, 8, 18, 24),
        itemCount: _items.length + 1,
        separatorBuilder: (_, _) => const SizedBox(height: 12),
        itemBuilder: (context, i) {
          if (i == _items.length) {
            if (_loading) {
              return const Padding(
                padding: EdgeInsets.all(12),
                child: Center(child: CircularProgressIndicator()),
              );
            }
            if (_error != null) {
              return TextButton(onPressed: _more, child: Text('$_error — إعادة المحاولة'));
            }
            return const SizedBox(height: 8);
          }
          final p = _items[i];
          final color = Interest.color(p.interest);
          return NeuCard(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
            onTap: () => _open(p),
            child: Row(
              children: [
                Container(
                  width: 14,
                  height: 14,
                  decoration: BoxDecoration(color: color, shape: BoxShape.circle),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        p.name,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15),
                      ),
                      Text(
                        p.interestLabel,
                        style: TextStyle(color: color, fontSize: 13, fontWeight: FontWeight.w600),
                      ),
                    ],
                  ),
                ),
                Text(
                  IraqTime.smart(p.createdAt),
                  style: const TextStyle(color: AppColors.muted, fontSize: 12),
                ),
                const Icon(Icons.chevron_left_rounded, color: AppColors.muted),
              ],
            ),
          );
        },
      ),
    );
  }
}
