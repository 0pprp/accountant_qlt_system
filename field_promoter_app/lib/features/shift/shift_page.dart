import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:geolocator/geolocator.dart';
import 'package:latlong2/latlong.dart';

import '../../core/formatting/iraq_time.dart';
import '../../core/location/gps.dart';
import '../../core/network/api_errors.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/feedback.dart';
import '../../core/widgets/map_parts.dart';
import '../../core/widgets/neu.dart';
import '../../injection.dart';
import '../pins/pin_detail_page.dart';
import '../pins/pins_page.dart';
import '../promoter/models.dart';

class ShiftPage extends StatefulWidget {
  const ShiftPage({super.key, required this.tab, required this.index});
  final ValueListenable<int> tab;
  final int index;

  @override
  State<ShiftPage> createState() => _ShiftPageState();
}

class _ShiftPageState extends State<ShiftPage> {
  final ValueNotifier<List<LatLng>> _route = ValueNotifier(const []);
  final GlobalKey<_ShiftMapState> _mapKey = GlobalKey();
  TodaySummary? _today;
  String? _error;
  bool _loading = false;
  bool _busy = false;
  bool _started = false;
  Timer? _timer;

  bool get _active => widget.tab.value == widget.index;

  @override
  void initState() {
    super.initState();
    widget.tab.addListener(_onTab);
    Injection.store.addListener(_onStore);
  }

  void _onStore() {
    if (mounted && _started) setState(() {});
  }

  @override
  void dispose() {
    widget.tab.removeListener(_onTab);
    Injection.store.removeListener(_onStore);
    _timer?.cancel();
    _route.dispose();
    super.dispose();
  }

  void _onTab() {
    if (_active) {
      if (!_started) {
        setState(() => _started = true);
        unawaited(_locateOnce());
      }
      unawaited(_load());
      _timer ??= Timer.periodic(const Duration(seconds: 60), (_) {
        if (_active) unawaited(_load(silent: true));
      });
    } else {
      _timer?.cancel();
      _timer = null;
    }
  }

  Future<void> _locateOnce() async {
    if (Injection.tracker.lastPosition.value != null) return;
    try {
      final p = await Gps.current();
      Injection.tracker.lastPosition.value = p;
    } catch (_) {}
  }

  Future<void> _load({bool silent = false}) async {
    if (_loading) return;
    if (!silent) setState(() => _loading = true);
    try {
      final t = await Injection.api.today();
      if (!mounted) return;
      _route.value = [
        for (final r in t.route)
          if (r.lat.isFinite && r.lng.isFinite) LatLng(r.lat, r.lng),
      ];
      final wasWorking = Injection.store.isWorking;
      setState(() {
        _today = t;
        _error = null;
      });
      if (wasWorking != t.isWorking) {
        unawaited(Injection.store.refreshMe(force: true).then((_) {}, onError: (_) {}));
      }
    } catch (e) {
      if (mounted) setState(() => _error = ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _start() async {
    setState(() => _busy = true);
    try {
      final s = await Injection.tracker.startShift();
      Injection.store.applyShift(s);
      unawaited(Injection.store.afterActivity());
      await _load(silent: true);
      if (mounted) AppFeedback.snack(context, 'بدأ الدوام');
    } catch (e) {
      if (mounted) await AppFeedback.error(context, ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _end() async {
    final ok = await AppFeedback.confirm(
      context,
      title: 'إنهاء الدوام',
      message: 'سيتوقف تسجيل موقعك. يمكنك بدء دوام جديد لاحقاً.',
      confirmLabel: 'إنهاء الدوام',
      danger: true,
      icon: Icons.logout_rounded,
    );
    if (!ok || !mounted) return;
    setState(() => _busy = true);
    try {
      final s = await Injection.tracker.endShift();
      Injection.store.applyShift(s);
      unawaited(Injection.store.afterActivity());
      await _load(silent: true);
      if (mounted) AppFeedback.snack(context, 'انتهى الدوام');
    } catch (e) {
      if (mounted) await AppFeedback.error(context, ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _addPin() async {
    final saved = await createPinFlow(context);
    if (saved != null) {
      unawaited(_mapKey.currentState?.refreshPins());
      unawaited(_load(silent: true));
    }
  }

  void _showPeriods() {
    final periods = _today?.periods ?? const <WorkPeriod>[];
    showModalBottomSheet<void>(
      context: context,
      builder: (ctx) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 0, 20, 20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                'فترات اليوم (${periods.length})',
                style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
              ),
              const SizedBox(height: 4),
              const Text(
                'تُحسب الفترة من أول تثبيت أو طلب حتى آخر نشاط قبل انقطاع 15 دقيقة',
                style: TextStyle(color: AppColors.muted, fontSize: 12),
              ),
              const SizedBox(height: 14),
              if (periods.isEmpty)
                const Padding(
                  padding: EdgeInsets.all(20),
                  child: Text(
                    'لا توجد فترات عمل بعد',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: AppColors.muted),
                  ),
                )
              else
                Flexible(
                  child: ListView.separated(
                    shrinkWrap: true,
                    itemCount: periods.length,
                    separatorBuilder: (_, _) => const SizedBox(height: 10),
                    itemBuilder: (_, i) {
                      final p = periods[i];
                      return NeuCard(
                        padding: const EdgeInsets.all(14),
                        child: Row(
                          children: [
                            Text(
                              '${IraqTime.hm(p.start)} – ${IraqTime.hm(p.end)}',
                              textDirection: TextDirection.ltr,
                              style: const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            const Spacer(),
                            Text(
                              '${IraqTime.minutesAsHm(p.minutes)} س · ${p.events} نشاط',
                              style: const TextStyle(color: AppColors.muted),
                            ),
                          ],
                        ),
                      );
                    },
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (!_started) return const SizedBox.shrink();
    return Column(
      children: [
        Expanded(
          child: Stack(
            children: [
              Positioned.fill(child: _ShiftMap(key: _mapKey, route: _route)),
              PositionedDirectional(
                top: 12,
                start: 12,
                child: _loading
                    ? const NeuCard(
                        padding: EdgeInsets.all(10),
                        radius: 14,
                        child: SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        ),
                      )
                    : const SizedBox.shrink(),
              ),
            ],
          ),
        ),
        _statusPanel(),
      ],
    );
  }

  Widget _statusPanel() {
    final t = _today;
    final me = Injection.store.me;
    final working = t?.isWorking ?? me?.isWorking ?? false;
    final started = t?.startedAt ?? me?.shiftStartedAt;
    return Container(
      padding: const EdgeInsets.fromLTRB(18, 16, 18, 14),
      decoration: const BoxDecoration(
        color: AppColors.background,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Icon(
                working ? Icons.radio_button_checked_rounded : Icons.radio_button_off_rounded,
                color: working ? AppColors.success : AppColors.muted,
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  working ? 'في الدوام منذ ${IraqTime.hm(started)}' : 'لم يبدأ الدوام اليوم',
                  style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w700),
                ),
              ),
              ValueListenableBuilder<int>(
                valueListenable: Injection.tracker.pending,
                builder: (_, n, _) => n > 0
                    ? StatusPill(
                        '$n نقطة بانتظار الإرسال',
                        color: AppColors.warning,
                        icon: Icons.cloud_upload_outlined,
                      )
                    : const SizedBox.shrink(),
              ),
            ],
          ),
          if (_error != null && t == null) ...[
            const SizedBox(height: 6),
            Text(_error!, style: const TextStyle(color: AppColors.danger, fontSize: 12)),
          ],
          const SizedBox(height: 12),
          Row(
            children: [
              _stat('محلات اليوم', '${t?.pins ?? me?.todayPins ?? 0}'),
              const SizedBox(width: 10),
              _stat('بداية اليوم', IraqTime.hm(t?.firstStart)),
              const SizedBox(width: 10),
              _stat(
                'ساعات العمل',
                IraqTime.minutesAsHm(t?.workMinutes ?? me?.todayWorkMinutes ?? 0),
                sub: (t?.targetMinutes ?? 0) > 0
                    ? 'من ${IraqTime.minutesAsHm(t!.targetMinutes)}'
                    : null,
              ),
            ],
          ),
          const SizedBox(height: 14),
          if (working) ...[
            Row(
              children: [
                Expanded(
                  child: NeuButton(
                    label: '＋ تثبيت محل',
                    height: 46,
                    onPressed: _busy ? null : _addPin,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: NeuButton(
                    label: 'فترات اليوم (${t?.periods.length ?? 0})',
                    primary: false,
                    height: 46,
                    onPressed: _showPeriods,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 10),
            NeuButton(
              label: 'إنهاء الدوام',
              icon: Icons.stop_circle_outlined,
              primary: false,
              danger: true,
              height: 46,
              busy: _busy,
              onPressed: _end,
            ),
          ] else ...[
            NeuButton(
              label: 'بدء الدوام',
              icon: Icons.play_circle_outline_rounded,
              busy: _busy,
              onPressed: _start,
            ),
            const SizedBox(height: 8),
            const Text(
              'يبدأ الدوام تلقائياً أيضاً مع أول تثبيت محل أو أول طلب.',
              textAlign: TextAlign.center,
              style: TextStyle(color: AppColors.muted, fontSize: 12),
            ),
            if ((t?.periods.length ?? 0) > 0)
              TextButton(
                onPressed: _showPeriods,
                child: Text('فترات اليوم (${t!.periods.length})'),
              ),
          ],
        ],
      ),
    );
  }

  Widget _stat(String label, String value, {String? sub}) => Expanded(
    child: NeuCard(
      padding: const EdgeInsets.symmetric(vertical: 10, horizontal: 8),
      radius: 16,
      depth: 0.7,
      child: Column(
        children: [
          Text(
            value,
            textDirection: TextDirection.ltr,
            style: const TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w700,
              color: AppColors.primary,
            ),
          ),
          Text(label, style: const TextStyle(fontSize: 12, color: AppColors.muted)),
          if (sub != null)
            Text(sub, style: const TextStyle(fontSize: 10.5, color: AppColors.muted)),
        ],
      ),
    ),
  );
}

class _ShiftMap extends StatefulWidget {
  const _ShiftMap({super.key, required this.route});
  final ValueListenable<List<LatLng>> route;

  @override
  State<_ShiftMap> createState() => _ShiftMapState();
}

class _ShiftMapState extends State<_ShiftMap> {
  final _controller = MapController();
  late final PinBoxLoader _loader = PinBoxLoader(Injection.api);
  bool _centered = false;

  @override
  void initState() {
    super.initState();
    Injection.tracker.lastPosition.addListener(_onPosition);
  }

  @override
  void dispose() {
    Injection.tracker.lastPosition.removeListener(_onPosition);
    _loader.dispose();
    _controller.dispose();
    super.dispose();
  }

  Future<void> refreshPins() => _loader.refresh();

  void _onPosition() {
    if (_centered) return;
    final p = Injection.tracker.lastPosition.value;
    if (p == null) return;
    _centered = true;
    try {
      _controller.move(LatLng(p.latitude, p.longitude), 15);
    } catch (_) {}
  }

  void _recenter() {
    final p = Injection.tracker.lastPosition.value;
    if (p != null) {
      _controller.move(LatLng(p.latitude, p.longitude), 16);
      return;
    }
    final route = widget.route.value;
    if (route.isNotEmpty) _controller.move(route.last, 15);
  }

  Future<void> _openPin(MapPin pin) async {
    await Navigator.of(context).push<bool>(
      MaterialPageRoute(builder: (_) => PinDetailPage(pinId: pin.id)),
    );
    unawaited(_loader.refresh());
  }

  @override
  Widget build(BuildContext context) {
    final p = Injection.tracker.lastPosition.value;
    final start = p != null ? LatLng(p.latitude, p.longitude) : MapParts.baghdad;
    _centered = _centered || p != null;
    return Stack(
      children: [
        FlutterMap(
          mapController: _controller,
          options: MapOptions(
            initialCenter: start,
            initialZoom: p != null ? 15 : 12,
            minZoom: 5,
            maxZoom: 19,
            interactionOptions: const InteractionOptions(
              flags: InteractiveFlag.all & ~InteractiveFlag.rotate,
            ),
            onMapReady: () => _loader.onCamera(_controller.camera),
            onPositionChanged: (camera, _) => _loader.onCamera(camera),
          ),
          children: [
            MapParts.tiles(),
            ValueListenableBuilder<List<LatLng>>(
              valueListenable: widget.route,
              builder: (_, route, _) => route.length < 2
                  ? const SizedBox.shrink()
                  : PolylineLayer(
                      polylines: [
                        Polyline(
                          points: route,
                          strokeWidth: 4,
                          color: AppColors.primary.withValues(alpha: 0.8),
                        ),
                      ],
                    ),
            ),
            ValueListenableBuilder<List<MapPin>>(
              valueListenable: _loader.pins,
              builder: (_, pins, _) => MarkerLayer(
                markers: [
                  for (final pin in pins)
                    MapParts.pinMarker(pin, onTap: () => _openPin(pin)),
                ],
              ),
            ),
            ValueListenableBuilder<Position?>(
              valueListenable: Injection.tracker.lastPosition,
              builder: (_, pos, _) => pos == null
                  ? const SizedBox.shrink()
                  : MarkerLayer(
                      markers: [MapParts.meMarker(LatLng(pos.latitude, pos.longitude))],
                    ),
            ),
            MapParts.attribution(),
          ],
        ),
        PositionedDirectional(
          top: 12,
          end: 12,
          child: NeuIconButton(
            icon: Icons.my_location_rounded,
            tooltip: 'موقعي',
            onPressed: _recenter,
          ),
        ),
        ValueListenableBuilder<bool>(
          valueListenable: _loader.zoomTooLow,
          builder: (_, low, _) => low
              ? const Positioned(
                  bottom: 24,
                  left: 0,
                  right: 0,
                  child: Center(
                    child: StatusPill(
                      'قرّب الخريطة لعرض المحلات',
                      color: AppColors.ink,
                      icon: Icons.zoom_in_rounded,
                    ),
                  ),
                )
              : const SizedBox.shrink(),
        ),
      ],
    );
  }
}
