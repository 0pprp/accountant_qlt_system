import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:package_info_plus/package_info_plus.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:url_launcher/url_launcher.dart';

import '../network/dio_client.dart';
import '../theme/app_colors.dart';

enum _Kind { none, optional, mandatory }

class AppUpdateGate {
  static const appKey = 'delegate';
  static const _cacheKey = 'delegate_mandatory_update_v1';
  static bool _running = false;

  /// Returns false when blocked by mandatory update.
  static Future<bool> checkAndPrompt(BuildContext context, DioClient dio) async {
    if (_running) return true;
    _running = true;
    try {
      final installed = await _installedCode();
      await _clearIfSatisfied(installed);

      Map<String, dynamic>? raw;
      try {
        final res = await dio.dio.get(
          'api/v1/app/mobile-updates/$appKey',
          queryParameters: {'currentVersionCode': installed},
        );
        if (res.data is Map) raw = Map<String, dynamic>.from(res.data as Map);
      } on DioException {
        raw = null;
      }

      if (!context.mounted) return true;

      if (raw != null) {
        final info = _parse(raw, installed);
        await _syncCache(info, installed);
        if (info.kind == _Kind.mandatory) {
          await _show(context, info, mandatory: true);
          return false;
        }
        if (info.kind == _Kind.optional) {
          await _show(context, info, mandatory: false);
          return true;
        }
      } else {
        final cached = await _fromCache(installed);
        if (cached != null) {
          await _show(context, cached, mandatory: true);
          return false;
        }
      }
      return true;
    } finally {
      _running = false;
    }
  }

  static Future<int> _installedCode() async {
    try {
      final info = await PackageInfo.fromPlatform();
      return int.tryParse(info.buildNumber) ?? 1;
    } catch (_) {
      return 1;
    }
  }

  static _Info _parse(Map<String, dynamic> json, int current) {
    final latest = _asInt(json['latestVersionCode']);
    final min = _asInt(json['minimumSupportedVersionCode']);
    final force = json['forceUpdate'] == true;
    final kind = current >= latest
        ? _Kind.none
        : (current < min || force ? _Kind.mandatory : _Kind.optional);
    return _Info(
      kind: kind,
      name: '${json['latestVersionName'] ?? ''}',
      code: latest,
      apkUrl: _opt(json['apkUrl']),
      notes: _opt(json['releaseNotes']),
    );
  }

  static Future<void> _show(BuildContext context, _Info info, {required bool mandatory}) {
    return showDialog<void>(
      context: context,
      barrierDismissible: !mandatory,
      builder: (ctx) => PopScope(
        canPop: !mandatory,
        child: Directionality(
          textDirection: TextDirection.rtl,
          child: AlertDialog(
            title: Text(
              mandatory ? 'يجب تحديث التطبيق للاستمرار' : 'يتوفر تحديث جديد',
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
            content: SingleChildScrollView(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('الإصدار ${info.name}'),
                  if ((info.notes ?? '').isNotEmpty) ...[
                    const SizedBox(height: 8),
                    Text(info.notes!),
                  ],
                  if (mandatory) ...[
                    const SizedBox(height: 12),
                    const Text('حدّث التطبيق ثم افتحه من جديد للمتابعة.'),
                  ],
                ],
              ),
            ),
            actions: [
              if (!mandatory)
                TextButton(
                  onPressed: () => Navigator.of(ctx).pop(),
                  child: const Text('لاحقاً'),
                ),
              FilledButton(
                style: FilledButton.styleFrom(backgroundColor: AppColors.primary),
                onPressed: () async {
                  if (!mandatory) Navigator.of(ctx).pop();
                  final url = info.apkUrl;
                  if (url != null && url.startsWith('http')) {
                    await launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
                  }
                },
                child: const Text('تحديث الآن'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  static Future<void> _syncCache(_Info info, int installed) async {
    final prefs = await SharedPreferences.getInstance();
    if (info.kind == _Kind.mandatory && (info.apkUrl ?? '').startsWith('http')) {
      await prefs.setString(
        _cacheKey,
        [info.code, info.name, info.apkUrl!, info.notes ?? ''].join('\u001f'),
      );
      return;
    }
    if (info.kind != _Kind.mandatory) await prefs.remove(_cacheKey);
  }

  static Future<void> _clearIfSatisfied(int installed) async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_cacheKey);
    if (raw == null) return;
    final required = int.tryParse(raw.split('\u001f').first) ?? 0;
    if (installed >= required) await prefs.remove(_cacheKey);
  }

  static Future<_Info?> _fromCache(int installed) async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_cacheKey);
    if (raw == null) return null;
    final parts = raw.split('\u001f');
    if (parts.length < 3) return null;
    final required = int.tryParse(parts[0]) ?? 0;
    final apk = parts[2];
    if (required <= 0 || installed >= required || !apk.startsWith('http')) return null;
    return _Info(
      kind: _Kind.mandatory,
      name: parts[1],
      code: required,
      apkUrl: apk,
      notes: parts.length > 3 ? parts[3] : null,
    );
  }

  static int _asInt(dynamic v) {
    if (v is int) return v;
    if (v is num) return v.toInt();
    return int.tryParse('$v') ?? 0;
  }

  static String? _opt(dynamic v) {
    if (v == null) return null;
    final s = '$v'.trim();
    return s.isEmpty ? null : s;
  }
}

class _Info {
  const _Info({
    required this.kind,
    required this.name,
    required this.code,
    this.apkUrl,
    this.notes,
  });
  final _Kind kind;
  final String name;
  final int code;
  final String? apkUrl;
  final String? notes;
}
