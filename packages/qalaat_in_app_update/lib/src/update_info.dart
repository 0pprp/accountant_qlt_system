import 'mobile_update_rules.dart';

class UpdateInfo {
  const UpdateInfo({
    required this.kind,
    required this.latestVersionName,
    required this.latestVersionCode,
    required this.currentVersionCode,
    required this.currentVersionName,
    this.apkUrl,
    this.sha256,
    this.releaseNotes,
    this.appTitle,
  });

  final MobileUpdateKind kind;
  final String latestVersionName;
  final int latestVersionCode;
  final int currentVersionCode;
  final String currentVersionName;
  final String? apkUrl;
  final String? sha256;
  final String? releaseNotes;
  final String? appTitle;

  bool get mandatory => kind == MobileUpdateKind.mandatory;
  bool get canDownload => (apkUrl ?? '').startsWith('http');

  factory UpdateInfo.fromApiJson(
    Map<String, dynamic> json, {
    required int currentVersionCode,
    required String currentVersionName,
    String? appTitle,
  }) {
    final latest = _asInt(json['latestVersionCode']);
    final minimum = _asInt(json['minimumSupportedVersionCode']);
    final force = json['forceUpdate'] == true;
    return UpdateInfo(
      kind: MobileUpdateRules.evaluate(
        currentVersionCode: currentVersionCode,
        latestVersionCode: latest,
        minimumSupportedVersionCode: minimum,
        forceUpdate: force,
      ),
      latestVersionName: '${json['latestVersionName'] ?? ''}',
      latestVersionCode: latest,
      currentVersionCode: currentVersionCode,
      currentVersionName: currentVersionName,
      apkUrl: _opt(json['apkUrl']),
      sha256: _opt(json['sha256'])?.toLowerCase(),
      releaseNotes: _opt(json['releaseNotes']),
      appTitle: appTitle,
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
