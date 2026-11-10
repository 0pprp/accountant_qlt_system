import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:package_info_plus/package_info_plus.dart';
import 'package:qalaat_in_app_update/qalaat_in_app_update.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../network/dio_client.dart';
import '../theme/app_colors.dart';

class AppUpdateGate {
  static const appKey = 'committee';
  static const _cacheKey = 'committee_mandatory_update_v1';
  static bool _running = false;

  /// Returns false when blocked by mandatory update.
  static Future<bool> checkAndPrompt(BuildContext context, DioClient dio) async {
    if (_running) return true;
    _running = true;
    try {
      final pkg = await PackageInfo.fromPlatform();
      final installed = int.tryParse(pkg.buildNumber) ?? 1;
      final installedName = pkg.version.isEmpty ? '1.0.0' : pkg.version;
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

      UpdateInfo? info;
      if (raw != null) {
        info = UpdateInfo.fromApiJson(
          raw,
          currentVersionCode: installed,
          currentVersionName: installedName,
          appTitle: 'مسؤول اللجنة',
        );
        await _syncCache(info, installed);
      } else {
        info = await _fromCache(installed, installedName);
      }

      if (info == null || info.kind == MobileUpdateKind.none) return true;
      if (!context.mounted) return true;

      await ForceUpdateScreen.present(
        context,
        info: info,
        primaryColor: AppColors.primary,
        foregroundColor: AppColors.foreground,
        backgroundColor: AppColors.background,
        surfaceColor: AppColors.surface,
        logo: SvgPicture.asset(
          'assets/svg/logo_green.svg',
          height: 64,
          fit: BoxFit.contain,
        ),
        supportLine: (info.releaseNotes?.isNotEmpty ?? false)
            ? info.releaseNotes
            : 'تحسينات استقرار وأداء لمتابعة اللجنة بثقة أكبر.',
      );

      return info.kind != MobileUpdateKind.mandatory;
    } finally {
      _running = false;
    }
  }

  static Future<void> _syncCache(UpdateInfo info, int installed) async {
    final prefs = await SharedPreferences.getInstance();
    if (info.mandatory && info.canDownload) {
      await prefs.setString(
        _cacheKey,
        [
          info.latestVersionCode,
          info.latestVersionName,
          info.apkUrl!,
          info.releaseNotes ?? '',
          info.sha256 ?? '',
        ].join('\u001f'),
      );
      return;
    }
    if (!info.mandatory) await prefs.remove(_cacheKey);
  }

  static Future<void> _clearIfSatisfied(int installed) async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_cacheKey);
    if (raw == null) return;
    final required = int.tryParse(raw.split('\u001f').first) ?? 0;
    if (installed >= required) await prefs.remove(_cacheKey);
  }

  static Future<UpdateInfo?> _fromCache(int installed, String installedName) async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_cacheKey);
    if (raw == null) return null;
    final parts = raw.split('\u001f');
    if (parts.length < 3) return null;
    final required = int.tryParse(parts[0]) ?? 0;
    final apk = parts[2];
    if (required <= 0 || installed >= required || !apk.startsWith('http')) {
      return null;
    }
    return UpdateInfo(
      kind: MobileUpdateKind.mandatory,
      latestVersionName: parts[1],
      latestVersionCode: required,
      currentVersionCode: installed,
      currentVersionName: installedName,
      apkUrl: apk,
      releaseNotes: parts.length > 3 && parts[3].isNotEmpty ? parts[3] : null,
      sha256: parts.length > 4 && parts[4].isNotEmpty ? parts[4] : null,
      appTitle: 'مسؤول اللجنة',
    );
  }
}
