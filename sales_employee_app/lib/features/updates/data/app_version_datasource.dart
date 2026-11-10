import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/features/updates/data/app_version.dart';
import 'package:sales_employee_app/features/updates/data/update_models.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:url_launcher/url_launcher.dart';

class AppUpdateInfo {
  const AppUpdateInfo({
    required this.forceUpdate,
    this.message,
    this.storeUrl,
    this.kind = MobileUpdateKind.optional,
  });

  final bool forceUpdate;
  final String? message;
  final String? storeUrl;
  final MobileUpdateKind kind;
}

class AppVersionDataSource {
  AppVersionDataSource(this._dio);
  final DioClient _dio;

  static const _cacheKey = 'sales_employee_mandatory_update_v1';

  Future<AppUpdateInfo?> check() async {
    await AppVersion.ensureLoaded();
    final installed = AppVersion.resolvedCode;
    await _clearCacheIfSatisfied(installed);

    try {
      final res = await _dio.dio.get(
        'api/v1/app/mobile-updates/${AppVersion.appKey}',
        queryParameters: {'currentVersionCode': installed},
      );
      if (res.data is! Map) return null;
      final info = MobileUpdateInfo.fromJson(
        Map<String, dynamic>.from(res.data as Map),
        currentVersionCode: installed,
      );
      await _syncCache(info, installed);
      if (info.kind == MobileUpdateKind.none) return null;
      return AppUpdateInfo(
        forceUpdate: info.mandatory,
        message: (info.releaseNotes?.isNotEmpty ?? false)
            ? info.releaseNotes
            : 'يتوفر الإصدار ${info.latestVersionName}',
        storeUrl: info.apkUrl,
        kind: info.kind,
      );
    } catch (_) {
      return _fromCache(installed);
    }
  }

  Future<bool> openDownload(String url) async {
    final uri = Uri.tryParse(url);
    if (uri == null) return false;
    return launchUrl(uri, mode: LaunchMode.externalApplication);
  }

  Future<void> _syncCache(MobileUpdateInfo info, int installed) async {
    final prefs = await SharedPreferences.getInstance();
    if (info.mandatory && info.canDownload) {
      await prefs.setString(
        _cacheKey,
        [
          info.latestVersionCode,
          info.latestVersionName,
          info.apkUrl ?? '',
          info.releaseNotes ?? '',
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

  Future<AppUpdateInfo?> _fromCache(int installed) async {
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
    return AppUpdateInfo(
      forceUpdate: true,
      message: parts.length > 3 && parts[3].isNotEmpty
          ? parts[3]
          : 'يجب تحديث التطبيق للاستمرار (${parts[1]})',
      storeUrl: apk,
      kind: MobileUpdateKind.mandatory,
    );
  }
}
