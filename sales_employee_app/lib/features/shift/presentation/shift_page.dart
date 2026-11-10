import 'dart:async';

import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/time/baghdad_time.dart';
import 'package:sales_employee_app/core/widgets/animated_waves.dart';
import 'package:sales_employee_app/core/widgets/brand_logo.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/injection.dart';

class ShiftPage extends StatefulWidget {
  const ShiftPage({super.key, this.onDutyChanged, this.mandatory = false});

  final VoidCallback? onDutyChanged;
  /// When true, back is blocked until duty starts (daily gate).
  final bool mandatory;

  @override
  State<ShiftPage> createState() => _ShiftPageState();
}

class _ShiftPageState extends State<ShiftPage> {
  bool _active = false;
  String? _startedAt;
  bool _loading = true;
  bool _busy = false;
  int _queued = 0;
  Timer? _uiTimer;

  @override
  void initState() {
    super.initState();
    _refresh();
    _uiTimer = Timer.periodic(const Duration(seconds: 5), (_) => _refreshCounts());
  }

  @override
  void dispose() {
    _uiTimer?.cancel();
    super.dispose();
  }

  Future<void> _refreshCounts() async {
    final queued = await Injection.locationQueue.pendingCount();
    if (!mounted) return;
    setState(() => _queued = queued);
  }

  Future<void> _refresh() async {
    setState(() => _loading = true);
    var active = await Injection.dutySession.isActive();
    var started = await Injection.dutySession.startedAt();
    try {
      final remote = await Injection.shiftRemote.current();
      if (remote.active) {
        active = true;
        started = remote.startedAt ?? started;
        await Injection.dutySession.markStarted();
        await Injection.dutyTracker.start();
      }
    } catch (_) {
      // API may be unavailable — local duty still works.
    }
    final queued = await Injection.locationQueue.pendingCount();
    if (!mounted) return;
    setState(() {
      _active = active;
      _startedAt = started;
      _queued = queued;
      _loading = false;
    });
  }

  Future<void> _start() async {
    if (_busy) return;
    setState(() => _busy = true);
    try {
      final point = await Injection.locationService.current();
      if (point == null) {
        await showAppMessage(
          context,
          title: 'الموقع مطلوب',
          message: 'فعّل GPS واسمح بالوصول للموقع لبدء الدوام وتسجيل المسار',
          color: AppColors.warning,
        );
        return;
      }
      try {
        await Injection.shiftRemote.start(lat: point.lat, lng: point.lng);
      } catch (_) {}
      await Injection.dutySession.markStarted();
      await Injection.dutyTracker.start();
      await Injection.locationQueue.flush();
      if (!mounted) return;
      setState(() {
        _active = true;
        _startedAt = DateTime.now().toUtc().toIso8601String();
      });
      widget.onDutyChanged?.call();
      await showAppMessage(
        context,
        title: 'بدأ الدوام',
        message: 'بدأ تسجيل الموقع مباشرةً بالتاريخ والوقت. يمكنك استخدام التطبيق الآن.',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
      if (widget.mandatory && mounted) Navigator.of(context).pop(true);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _end() async {
    final ok = await showConfirmDialog(
      context,
      title: 'إنهاء الدوام',
      message: 'سيتم إيقاف تسجيل الموقع ولن تفتح الشاشات حتى تبدأ دواماً جديداً.',
    );
    if (!ok) return;
    setState(() => _busy = true);
    try {
      final point = await Injection.locationService.current();
      try {
        if (point != null) {
          await Injection.locationQueue.enqueue(
            lat: point.lat,
            lng: point.lng,
            recordedAtUtc: DateTime.now().toUtc(),
          );
        }
        await Injection.dutyTracker.stop();
        await Injection.locationQueue.flush();
        await Injection.shiftRemote.end(lat: point?.lat, lng: point?.lng);
      } catch (_) {
        await Injection.dutyTracker.stop();
      }
      await Injection.dutySession.markEnded();
      if (!mounted) return;
      setState(() {
        _active = false;
        _startedAt = null;
      });
      widget.onDutyChanged?.call();
      await showAppMessage(
        context,
        title: 'انتهى الدوام',
        message: 'لحفظ مسار جديد يجب بدء الدوام من جديد',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final last = Injection.dutyTracker.lastBaghdadStamp;
    return PopScope(
      canPop: !widget.mandatory || _active,
      onPopInvokedWithResult: (didPop, _) async {
        if (didPop) return;
        if (widget.mandatory && !_active) {
          await showAppMessage(
            context,
            title: 'بدء الدوام إلزامي',
            message: 'كل يوم جديد بعد منتصف الليل يتطلب بدء دوام قبل فتح التطبيق',
            color: AppColors.warning,
          );
        }
      },
      child: Scaffold(
        body: AnimatedWavesBackground(
          child: SafeArea(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : Padding(
                    padding: const EdgeInsets.fromLTRB(24, 20, 24, 24),
                    child: Column(
                      children: [
                        const BrandLogo(height: 78),
                        const SizedBox(height: 10),
                        Text(
                          widget.mandatory ? 'بدء الدوام إلزامي اليوم' : 'الدوام',
                          style: const TextStyle(
                            fontSize: 22,
                            fontWeight: FontWeight.w700,
                            color: AppColors.primary,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          _active
                              ? 'الدوام نشط — الموقع يُسجَّل أول بأول على السيرفر'
                              : 'ابدأ الدوام لفتح التطبيق وتسجيل مسار GPS بالتاريخ والوقت',
                          textAlign: TextAlign.center,
                          style: const TextStyle(color: AppColors.textSecondary),
                        ),
                        const Spacer(),
                        Container(
                          width: double.infinity,
                          padding: const EdgeInsets.all(22),
                          decoration: BoxDecoration(
                            color: AppColors.surface.withValues(alpha: 0.95),
                            borderRadius: BorderRadius.circular(18),
                            border: Border.all(color: AppColors.border),
                          ),
                          child: Column(
                            children: [
                              Icon(
                                _active ? Icons.navigation_rounded : Icons.pause_circle_filled,
                                size: 64,
                                color: _active ? AppColors.success : AppColors.textSecondary,
                              ),
                              const SizedBox(height: 10),
                              Text(
                                _active ? 'دوام مفتوح — تتبع حي' : 'لا يوجد دوام',
                                style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
                              ),
                              if (_startedAt != null) ...[
                                const SizedBox(height: 6),
                                Text(
                                  'بدأ: ${formatBaghdadDateTime(DateTime.tryParse(_startedAt!) ?? baghdadNow())}',
                                  style: const TextStyle(color: AppColors.textSecondary, fontSize: 12),
                                ),
                              ],
                              if (last != null) ...[
                                const SizedBox(height: 4),
                                Text(
                                  'آخر نقطة: $last',
                                  style: const TextStyle(color: AppColors.textSecondary, fontSize: 12),
                                ),
                              ],
                              const SizedBox(height: 6),
                              Text(
                                'نقاط بالانتظار للمزامنة: $_queued',
                                style: const TextStyle(color: AppColors.textSecondary),
                              ),
                            ],
                          ),
                        ),
                        const Spacer(),
                        if (!_active)
                          ElevatedButton(
                            onPressed: _busy ? null : _start,
                            child: _busy
                                ? const SizedBox(
                                    width: 22,
                                    height: 22,
                                    child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                                  )
                                : const Text('بدء الدوام'),
                          )
                        else
                          ElevatedButton(
                            style: ElevatedButton.styleFrom(backgroundColor: AppColors.danger),
                            onPressed: _busy ? null : _end,
                            child: const Text('إنهاء الدوام'),
                          ),
                        if (_active) ...[
                          const SizedBox(height: 10),
                          OutlinedButton(
                            onPressed: () async {
                              try {
                                final n = await Injection.locationQueue.flush();
                                await _refreshCounts();
                                if (!mounted) return;
                                await showAppMessage(context, title: 'مزامنة الموقع', message: 'تم إرسال $n نقطة مع التاريخ والوقت');
                              } catch (e) {
                                if (!mounted) return;
                                await showAppMessage(context, title: 'مزامنة', message: DioClient.mapError(e));
                              }
                            },
                            child: const Text('مزامنة طابور GPS الآن'),
                          ),
                        ],
                      ],
                    ),
                  ),
          ),
        ),
      ),
    );
  }
}
