import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';

import 'update_info.dart';
import 'update_service.dart';

/// Full-screen in-app update experience (download → install). No browser path.
class ForceUpdateScreen extends StatefulWidget {
  const ForceUpdateScreen({
    super.key,
    required this.info,
    required this.service,
    this.primaryColor = const Color(0xFF006A64),
    this.foregroundColor = const Color(0xFF00201E),
    this.backgroundColor = const Color(0xFFF4F7F7),
    this.surfaceColor = const Color(0xFFFFFFFF),
    this.logo,
    this.supportLine,
  });

  final UpdateInfo info;
  final UpdateService service;
  final Color primaryColor;
  final Color foregroundColor;
  final Color backgroundColor;
  final Color surfaceColor;
  final Widget? logo;
  final String? supportLine;

  static Future<void> present(
    BuildContext context, {
    required UpdateInfo info,
    UpdateService? service,
    Color primaryColor = const Color(0xFF006A64),
    Color foregroundColor = const Color(0xFF00201E),
    Color backgroundColor = const Color(0xFFF4F7F7),
    Color surfaceColor = const Color(0xFFFFFFFF),
    Widget? logo,
    String? supportLine,
  }) {
    return Navigator.of(context).push(
      PageRouteBuilder<void>(
        opaque: true,
        barrierDismissible: false,
        transitionDuration: const Duration(milliseconds: 420),
        reverseTransitionDuration: const Duration(milliseconds: 280),
        pageBuilder: (context, animation, secondaryAnimation) => ForceUpdateScreen(
          info: info,
          service: service ?? UpdateService(),
          primaryColor: primaryColor,
          foregroundColor: foregroundColor,
          backgroundColor: backgroundColor,
          surfaceColor: surfaceColor,
          logo: logo,
          supportLine: supportLine,
        ),
        transitionsBuilder: (context, animation, secondaryAnimation, child) {
          final curved = CurvedAnimation(
            parent: animation,
            curve: Curves.easeOutCubic,
          );
          return FadeTransition(
            opacity: curved,
            child: SlideTransition(
              position: Tween<Offset>(
                begin: const Offset(0, 0.04),
                end: Offset.zero,
              ).animate(curved),
              child: child,
            ),
          );
        },
      ),
    );
  }

  @override
  State<ForceUpdateScreen> createState() => _ForceUpdateScreenState();
}

enum _Phase {
  ready,
  preparing,
  downloading,
  finalizing,
  openingInstaller,
  needPermission,
  waitingInstall,
  error,
}

class _ForceUpdateScreenState extends State<ForceUpdateScreen>
    with TickerProviderStateMixin, WidgetsBindingObserver {
  late final AnimationController _enter;
  late final AnimationController _pulse;
  late final AnimationController _progressAnim;

  _Phase _phase = _Phase.ready;
  double _progress = 0;
  String? _error;
  String? _apkPath;
  CancelToken? _cancel;
  bool _started = false;

  bool get _mandatory => widget.info.mandatory;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _enter = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 700),
    )..forward();
    _pulse = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1600),
    )..repeat(reverse: true);
    _progressAnim = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 280),
      value: 0,
    );

    SchedulerBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      if (_mandatory && widget.info.canDownload) {
        _startDownload();
      }
    });
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _cancel?.cancel();
    _enter.dispose();
    _pulse.dispose();
    _progressAnim.dispose();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state != AppLifecycleState.resumed) return;
    if (_phase == _Phase.needPermission && _apkPath != null) {
      _resumeAfterPermission();
    }
  }

  Future<void> _startDownload() async {
    if (_started && _phase == _Phase.downloading) return;
    if (!widget.info.canDownload) {
      setState(() {
        _phase = _Phase.error;
        _error = 'رابط التحديث غير متوفر حالياً. تواصل مع الإدارة.';
      });
      return;
    }

    _cancel?.cancel();
    _cancel = CancelToken();
    _started = true;
    setState(() {
      _phase = _Phase.preparing;
      _progress = 0;
      _error = null;
    });
    await Future<void>.delayed(const Duration(milliseconds: 350));
    if (!mounted) return;

    setState(() => _phase = _Phase.downloading);
    try {
      final result = await widget.service.downloadAndInstall(
        apkUrl: widget.info.apkUrl!,
        expectedSha256: widget.info.sha256,
        cancelToken: _cancel,
        onProgress: (p) {
          if (!mounted) return;
          setState(() {
            _progress = p;
            _phase = p >= 0.98 ? _Phase.finalizing : _Phase.downloading;
          });
          _progressAnim.animateTo(p, curve: Curves.easeOut);
        },
      );
      if (!mounted) return;
      _apkPath = result.apkPath ?? widget.service.lastApkPath;
      await _handleResult(result);
    } on UpdateServiceException catch (e) {
      if (!mounted) return;
      setState(() {
        _phase = _Phase.error;
        _error = e.message;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _phase = _Phase.error;
        _error = 'حدث خطأ غير متوقع أثناء التحديث. أعد المحاولة.';
      });
    }
  }

  Future<void> _handleResult(UpdateInstallResult result) async {
    switch (result.outcome) {
      case UpdateInstallOutcome.needsPermission:
        setState(() => _phase = _Phase.needPermission);
      case UpdateInstallOutcome.openedInstaller:
        setState(() {
          _phase = _Phase.waitingInstall;
          _progress = 1;
        });
      case UpdateInstallOutcome.failed:
        setState(() {
          _phase = _Phase.error;
          _error = 'تعذّر فتح شاشة التثبيت. أعد المحاولة.';
        });
    }
  }

  Future<void> _resumeAfterPermission() async {
    final path = _apkPath;
    if (path == null) {
      await _startDownload();
      return;
    }
    setState(() => _phase = _Phase.openingInstaller);
    try {
      final result = await widget.service.openInstallerForPath(path);
      if (!mounted) return;
      await _handleResult(result);
    } on UpdateServiceException catch (e) {
      if (!mounted) return;
      setState(() {
        _phase = _Phase.error;
        _error = e.message;
      });
    }
  }

  String get _statusText {
    switch (_phase) {
      case _Phase.ready:
        return _mandatory
            ? 'نجهّز التحديث تلقائياً… لا تغلق التطبيق'
            : 'إصدار جديد جاهز لتحسين الاستقرار والأداء';
      case _Phase.preparing:
        return 'جارٍ التحضير… ثوانٍ قليلة';
      case _Phase.downloading:
        return 'جارٍ التنزيل… ${(_progress * 100).round()}٪';
      case _Phase.finalizing:
        return 'يكتمل الملف…';
      case _Phase.openingInstaller:
        return 'جارٍ فتح شاشة التثبيت…';
      case _Phase.needPermission:
        return 'فعّل صلاحية التثبيت ثم ارجع للتطبيق — سنكمل تلقائياً';
      case _Phase.waitingInstall:
        return 'أكمل التثبيت من شاشة النظام ثم افتح التطبيق';
      case _Phase.error:
        return _error ?? 'تعذّر التحديث';
    }
  }

  String get _headline {
    if (_phase == _Phase.waitingInstall) return 'جاهز للتثبيت';
    if (_phase == _Phase.needPermission) return 'خطوة واحدةخيرة';
    if (_phase == _Phase.error) return 'لم يكتمل التحديث';
    return _mandatory ? 'تحديث مهم جاهز' : 'يتوفر تحديث جديد';
  }

  @override
  Widget build(BuildContext context) {
    final support = widget.supportLine ??
        (widget.info.releaseNotes?.trim().isNotEmpty == true
            ? widget.info.releaseNotes!.trim()
            : 'تحسينات استقرار وأداء لإكمال عملك بسلاسة أكبر.');

    return PopScope(
      canPop: !_mandatory,
      child: Directionality(
        textDirection: TextDirection.rtl,
        child: Scaffold(
          backgroundColor: widget.backgroundColor,
          body: SafeArea(
            child: AnimatedBuilder(
              animation: Listenable.merge([_enter, _pulse]),
              builder: (context, _) {
                final t = Curves.easeOutCubic.transform(_enter.value);
                return Stack(
                  children: [
                    _AmbientBackdrop(
                      primary: widget.primaryColor,
                      pulse: _pulse.value,
                    ),
                    Padding(
                      padding: const EdgeInsets.fromLTRB(24, 16, 24, 24),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          Opacity(
                            opacity: t,
                            child: Transform.translate(
                              offset: Offset(0, 16 * (1 - t)),
                              child: _HeroHeader(
                                logo: widget.logo,
                                primary: widget.primaryColor,
                                pulse: _pulse.value,
                                appTitle: widget.info.appTitle,
                              ),
                            ),
                          ),
                          const SizedBox(height: 28),
                          Opacity(
                            opacity: t,
                            child: Transform.translate(
                              offset: Offset(0, 24 * (1 - t)),
                              child: Text(
                                _headline,
                                textAlign: TextAlign.center,
                                style: TextStyle(
                                  fontSize: 26,
                                  height: 1.25,
                                  fontWeight: FontWeight.w700,
                                  color: widget.foregroundColor,
                                ),
                              ),
                            ),
                          ),
                          const SizedBox(height: 10),
                          Opacity(
                            opacity: (t - 0.1).clamp(0.0, 1.0),
                            child: Text(
                              support,
                              textAlign: TextAlign.center,
                              style: TextStyle(
                                fontSize: 15,
                                height: 1.45,
                                color: widget.foregroundColor.withValues(
                                  alpha: 0.72,
                                ),
                              ),
                            ),
                          ),
                          if (_mandatory) ...[
                            const SizedBox(height: 16),
                            Center(
                              child: _MandatoryBadge(primary: widget.primaryColor),
                            ),
                          ],
                          const SizedBox(height: 22),
                          _VersionStrip(
                            currentName: widget.info.currentVersionName,
                            currentCode: widget.info.currentVersionCode,
                            latestName: widget.info.latestVersionName,
                            latestCode: widget.info.latestVersionCode,
                            surface: widget.surfaceColor,
                            foreground: widget.foregroundColor,
                            primary: widget.primaryColor,
                          ),
                          const Spacer(),
                          _ProgressCard(
                            phase: _phase,
                            progress: _progress,
                            statusText: _statusText,
                            primary: widget.primaryColor,
                            foreground: widget.foregroundColor,
                            surface: widget.surfaceColor,
                            pulse: _pulse.value,
                          ),
                          const SizedBox(height: 20),
                          _ActionsRow(
                            mandatory: _mandatory,
                            phase: _phase,
                            primary: widget.primaryColor,
                            onLater: () => Navigator.of(context).maybePop(),
                            onRetry: _startDownload,
                            onStart: _startDownload,
                            onOpenPermission: () async {
                              await widget.service.openUnknownSourcesSettings();
                            },
                            onResumeInstall: _resumeAfterPermission,
                          ),
                        ],
                      ),
                    ),
                  ],
                );
              },
            ),
          ),
        ),
      ),
    );
  }
}

class _AmbientBackdrop extends StatelessWidget {
  const _AmbientBackdrop({required this.primary, required this.pulse});
  final Color primary;
  final double pulse;

  @override
  Widget build(BuildContext context) {
    final soft = Color.lerp(primary, Colors.white, 0.82)!;
    return IgnorePointer(
      child: Stack(
        children: [
          Positioned(
            top: -80 + 12 * pulse,
            left: -40,
            child: _Blob(color: soft.withValues(alpha: 0.55), size: 220),
          ),
          Positioned(
            bottom: 40 - 10 * pulse,
            right: -60,
            child: _Blob(
              color: primary.withValues(alpha: 0.10 + 0.04 * pulse),
              size: 260,
            ),
          ),
        ],
      ),
    );
  }
}

class _Blob extends StatelessWidget {
  const _Blob({required this.color, required this.size});
  final Color color;
  final double size;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: color,
      ),
    );
  }
}

class _HeroHeader extends StatelessWidget {
  const _HeroHeader({
    required this.logo,
    required this.primary,
    required this.pulse,
    this.appTitle,
  });

  final Widget? logo;
  final Color primary;
  final double pulse;
  final String? appTitle;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Transform.scale(
          scale: 1 + 0.02 * pulse,
          child: Container(
            width: 108,
            height: 108,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: Colors.white,
              boxShadow: [
                BoxShadow(
                  color: primary.withValues(alpha: 0.16),
                  blurRadius: 24,
                  offset: const Offset(0, 10),
                ),
              ],
              border: Border.all(color: primary.withValues(alpha: 0.12)),
            ),
            child: logo ??
                Icon(Icons.system_update_alt_rounded, size: 48, color: primary),
          ),
        ),
        const SizedBox(height: 12),
        Text(
          appTitle ?? 'قلعة الضمان',
          style: TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.w600,
            color: primary,
          ),
        ),
      ],
    );
  }
}

class _MandatoryBadge extends StatelessWidget {
  const _MandatoryBadge({required this.primary});
  final Color primary;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
      decoration: BoxDecoration(
        color: primary.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: primary.withValues(alpha: 0.25)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.lock_outline_rounded, size: 16, color: primary),
          const SizedBox(width: 6),
          Text(
            'تحديث إجباري',
            style: TextStyle(
              color: primary,
              fontWeight: FontWeight.w700,
              fontSize: 13,
            ),
          ),
        ],
      ),
    );
  }
}

class _VersionStrip extends StatelessWidget {
  const _VersionStrip({
    required this.currentName,
    required this.currentCode,
    required this.latestName,
    required this.latestCode,
    required this.surface,
    required this.foreground,
    required this.primary,
  });

  final String currentName;
  final int currentCode;
  final String latestName;
  final int latestCode;
  final Color surface;
  final Color foreground;
  final Color primary;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      decoration: BoxDecoration(
        color: surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFFD5E2E0)),
      ),
      child: Row(
        children: [
          Expanded(
            child: _VerCell(
              label: 'الحالي',
              value: '$currentName ($currentCode)',
              color: foreground.withValues(alpha: 0.7),
            ),
          ),
          Icon(Icons.arrow_back_rounded, color: primary, size: 22),
          Expanded(
            child: _VerCell(
              label: 'الجديد',
              value: '${latestName.isEmpty ? '—' : latestName} ($latestCode)',
              color: primary,
              emphasize: true,
            ),
          ),
        ],
      ),
    );
  }
}

class _VerCell extends StatelessWidget {
  const _VerCell({
    required this.label,
    required this.value,
    required this.color,
    this.emphasize = false,
  });

  final String label;
  final String value;
  final Color color;
  final bool emphasize;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Text(
          label,
          style: TextStyle(
            fontSize: 12,
            color: color.withValues(alpha: 0.85),
          ),
        ),
        const SizedBox(height: 4),
        Text(
          value,
          textAlign: TextAlign.center,
          style: TextStyle(
            fontSize: 15,
            fontWeight: emphasize ? FontWeight.w700 : FontWeight.w600,
            color: color,
          ),
        ),
      ],
    );
  }
}

class _ProgressCard extends StatelessWidget {
  const _ProgressCard({
    required this.phase,
    required this.progress,
    required this.statusText,
    required this.primary,
    required this.foreground,
    required this.surface,
    required this.pulse,
  });

  final _Phase phase;
  final double progress;
  final String statusText;
  final Color primary;
  final Color foreground;
  final Color surface;
  final double pulse;

  bool get _busy =>
      phase == _Phase.preparing ||
      phase == _Phase.downloading ||
      phase == _Phase.finalizing ||
      phase == _Phase.openingInstaller;

  @override
  Widget build(BuildContext context) {
    final pct = (progress * 100).clamp(0, 100).round();
    return Container(
      padding: const EdgeInsets.fromLTRB(18, 20, 18, 18),
      decoration: BoxDecoration(
        color: surface,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: const Color(0xFFD5E2E0)),
        boxShadow: [
          BoxShadow(
            color: primary.withValues(alpha: 0.06),
            blurRadius: 18,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  statusText,
                  style: TextStyle(
                    fontSize: 15,
                    height: 1.4,
                    fontWeight: FontWeight.w600,
                    color: foreground,
                  ),
                ),
              ),
              if (_busy || phase == _Phase.waitingInstall)
                Text(
                  '$pct٪',
                  style: TextStyle(
                    fontSize: 28,
                    fontWeight: FontWeight.w800,
                    color: primary,
                  ),
                ),
            ],
          ),
          const SizedBox(height: 14),
          ClipRRect(
            borderRadius: BorderRadius.circular(999),
            child: TweenAnimationBuilder<double>(
              tween: Tween(begin: 0, end: progress.clamp(0.0, 1.0)),
              duration: const Duration(milliseconds: 280),
              curve: Curves.easeOutCubic,
              builder: (context, value, _) {
                return LinearProgressIndicator(
                  value: phase == _Phase.preparing
                      ? null
                      : (phase == _Phase.ready ? 0 : value),
                  minHeight: 10,
                  backgroundColor: primary.withValues(alpha: 0.12),
                  color: primary,
                );
              },
            ),
          ),
          if (_busy) ...[
            const SizedBox(height: 12),
            Opacity(
              opacity: 0.55 + 0.35 * pulse,
              child: Text(
                'لا تغلق التطبيق أثناء التنزيل',
                style: TextStyle(
                  fontSize: 13,
                  color: foreground.withValues(alpha: 0.65),
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }
}

class _ActionsRow extends StatelessWidget {
  const _ActionsRow({
    required this.mandatory,
    required this.phase,
    required this.primary,
    required this.onLater,
    required this.onRetry,
    required this.onStart,
    required this.onOpenPermission,
    required this.onResumeInstall,
  });

  final bool mandatory;
  final _Phase phase;
  final Color primary;
  final VoidCallback onLater;
  final VoidCallback onRetry;
  final VoidCallback onStart;
  final VoidCallback onOpenPermission;
  final VoidCallback onResumeInstall;

  @override
  Widget build(BuildContext context) {
    final buttons = <Widget>[];

    if (phase == _Phase.error) {
      buttons.add(
        FilledButton(
          style: FilledButton.styleFrom(
            backgroundColor: primary,
            minimumSize: const Size.fromHeight(48),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
            ),
          ),
          onPressed: onRetry,
          child: const Text('إعادة المحاولة'),
        ),
      );
      if (!mandatory) {
        buttons.add(const SizedBox(height: 8));
        buttons.add(
          TextButton(onPressed: onLater, child: const Text('لاحقاً')),
        );
      }
    } else if (phase == _Phase.needPermission) {
      buttons.add(
        FilledButton(
          style: FilledButton.styleFrom(
            backgroundColor: primary,
            minimumSize: const Size.fromHeight(48),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
            ),
          ),
          onPressed: onOpenPermission,
          child: const Text('فتح إعدادات التثبيت'),
        ),
      );
      buttons.add(const SizedBox(height: 8));
      buttons.add(
        OutlinedButton(
          style: OutlinedButton.styleFrom(
            foregroundColor: primary,
            minimumSize: const Size.fromHeight(48),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
            ),
          ),
          onPressed: onResumeInstall,
          child: const Text('تابعتُ التفعيل — أكمل'),
        ),
      );
    } else if (phase == _Phase.ready && !mandatory) {
      buttons.add(
        FilledButton(
          style: FilledButton.styleFrom(
            backgroundColor: primary,
            minimumSize: const Size.fromHeight(48),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
            ),
          ),
          onPressed: onStart,
          child: const Text('تحديث الآن'),
        ),
      );
      buttons.add(const SizedBox(height: 8));
      buttons.add(
        TextButton(onPressed: onLater, child: const Text('لاحقاً')),
      );
    } else if (phase == _Phase.waitingInstall) {
      buttons.add(
        FilledButton(
          style: FilledButton.styleFrom(
            backgroundColor: primary,
            minimumSize: const Size.fromHeight(48),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
            ),
          ),
          onPressed: onResumeInstall,
          child: const Text('إعادة فتح المثبت'),
        ),
      );
    }

    if (buttons.isEmpty) {
      return const SizedBox(height: 48);
    }
    return Column(children: buttons);
  }
}
