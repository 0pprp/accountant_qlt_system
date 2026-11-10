import 'dart:io';

import 'package:crypto/crypto.dart';
import 'package:dio/dio.dart';
import 'package:flutter/services.dart';
import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';

enum UpdateInstallOutcome {
  openedInstaller,
  needsPermission,
  failed,
}

class UpdateInstallResult {
  const UpdateInstallResult({
    required this.outcome,
    this.apkPath,
  });

  final UpdateInstallOutcome outcome;
  final String? apkPath;
}

class UpdateServiceException implements Exception {
  UpdateServiceException(this.message, {this.code});
  final String message;
  final String? code;

  @override
  String toString() => message;
}

/// Downloads the APK inside the app and opens the system package installer.
/// Does not open a browser.
class UpdateService {
  UpdateService({Dio? dio, MethodChannel? channel})
      : _dio = dio ?? Dio(),
        _channel =
            channel ?? const MethodChannel('com.qalaat.dhaman/apk_installer');

  final Dio _dio;
  final MethodChannel _channel;
  String? lastApkPath;

  Future<bool> canInstallPackages() async {
    if (!Platform.isAndroid) return false;
    try {
      final ok = await _channel.invokeMethod<bool>('canInstallPackages');
      return ok ?? false;
    } on PlatformException {
      return false;
    }
  }

  Future<void> openUnknownSourcesSettings() async {
    if (!Platform.isAndroid) return;
    await _channel.invokeMethod<void>('openUnknownSourcesSettings');
  }

  Future<UpdateInstallResult> downloadAndInstall({
    required String apkUrl,
    String? expectedSha256,
    void Function(double progress)? onProgress,
    CancelToken? cancelToken,
  }) async {
    if (!apkUrl.startsWith('http')) {
      throw UpdateServiceException('رابط التحديث غير صالح', code: 'bad_url');
    }
    if (!Platform.isAndroid) {
      throw UpdateServiceException(
        'التثبيت داخل التطبيق متاح على أندرويد فقط',
        code: 'unsupported',
      );
    }

    onProgress?.call(0);

    final dir = await getTemporaryDirectory();
    final file = File(
      p.join(
        dir.path,
        'qalaat_update_${DateTime.now().millisecondsSinceEpoch}.apk',
      ),
    );
    if (await file.exists()) {
      await file.delete();
    }

    try {
      await _dio.download(
        apkUrl,
        file.path,
        cancelToken: cancelToken,
        options: Options(
          responseType: ResponseType.bytes,
          followRedirects: true,
          validateStatus: (s) => s != null && s >= 200 && s < 400,
          headers: const {
            HttpHeaders.acceptHeader: '*/*',
            HttpHeaders.userAgentHeader: 'QalaatInAppUpdate/1.0',
          },
        ),
        onReceiveProgress: (received, total) {
          if (total > 0) {
            onProgress?.call((received / total).clamp(0.0, 1.0));
          } else if (received > 0) {
            onProgress?.call(
              (0.15 + (received % 900000) / 1000000).clamp(0.1, 0.9),
            );
          }
        },
      );
    } on DioException catch (e) {
      if (CancelToken.isCancel(e)) {
        throw UpdateServiceException('تم إلغاء التنزيل', code: 'cancelled');
      }
      throw UpdateServiceException(
        'تعذّر تنزيل التحديث. تحقق من الاتصال ثم أعد المحاولة.',
        code: 'download',
      );
    }

    onProgress?.call(1);
    lastApkPath = file.path;

    final expected = expectedSha256?.trim().toLowerCase();
    if (expected != null && expected.isNotEmpty) {
      final digest = sha256.convert(await file.readAsBytes()).toString();
      if (digest != expected) {
        await file.delete();
        lastApkPath = null;
        throw UpdateServiceException(
          'ملف التحديث تالف أو غير مطابق. أعد المحاولة.',
          code: 'checksum',
        );
      }
    }

    return openInstallerForPath(file.path);
  }

  Future<UpdateInstallResult> openInstallerForPath(String path) async {
    final canInstall = await canInstallPackages();
    if (!canInstall) {
      await openUnknownSourcesSettings();
      return UpdateInstallResult(
        outcome: UpdateInstallOutcome.needsPermission,
        apkPath: path,
      );
    }
    try {
      final opened = await _channel.invokeMethod<bool>(
        'openApkInstaller',
        {'path': path},
      );
      if (opened == true) {
        return UpdateInstallResult(
          outcome: UpdateInstallOutcome.openedInstaller,
          apkPath: path,
        );
      }
      return UpdateInstallResult(
        outcome: UpdateInstallOutcome.failed,
        apkPath: path,
      );
    } on PlatformException catch (e) {
      throw UpdateServiceException(
        e.message ?? 'تعذّر فتح شاشة التثبيت',
        code: e.code,
      );
    }
  }
}
