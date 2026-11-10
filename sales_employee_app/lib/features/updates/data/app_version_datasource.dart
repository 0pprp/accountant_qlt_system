import 'package:qalaat_in_app_update/qalaat_in_app_update.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/features/updates/data/app_version.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AppVersionDataSource {
  AppVersionDataSource(this._dio);
  final DioClient _dio;

  static const _cacheKey = 'sales_employee_mandatory_update_v1';

  Future<UpdateInfo?> check() async {
    await AppVersion.ensureLoaded();
    final installed = AppVersion.resolvedCode;
    final installedName = AppVersion.resolvedName;
    await _clearCacheIfSatisfied(installed);

    try {
      final res = await _dio.dio.get(
        'api/v1/app/mobile-updates/${AppVersion.appKey}',
        queryParameters: {'currentVersionCode': installed},
      );
      if (res.data is! Map) return null;
      final info = UpdateInfo.fromApiJson(
        Map<String, dynamic>.from(res.data as Map),
        currentVersionCode: installed,
        currentVersionName: installedName,
        appTitle: 'موظف المبيعات',
      );
      await _syncCache(info, installed);
      if (info.kind == MobileUpdateKind.none) return null;
      return info;
    } catch (_) {
      return _fromCache(installed, installedName);
    }
  }

  Future<void> _syncCache(UpdateInfo info, int installed) async {
    final prefs = await SharedPreferences.getInstance();
    if (info.mandatory && info.canDownload) {
      await prefs.setString(
        _cacheKey,
        [
          info.latestVersionCode,
          info.latestVersionName,
          info.apkUrl ?? '',
          info.releaseNotes ?? '',
          info.sha256 ?? '',
        ].join('\u001f'),
      );
      return;
    }
    if (!info.mandatory) await prefs.remove(_cacheKey);
  }

  Future<void> _clearCacheIfSatisfied(int installed) async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_cacheKey);
    if (raw == null) return;
    final parts = raw.split('\u001f');
    final required = int.tryParse(parts.isNotEmpty ? parts[0] : '') ?? 0;
    if (installed >= required) await prefs.remove(_cacheKey);
  }

  Future<UpdateInfo?> _fromCache(int installed, String installedName) async {
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
      appTitle: 'موظف المبيعات',
    );
  }
}
