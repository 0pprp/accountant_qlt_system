enum MobileUpdateKind { none, optional, mandatory }

/// Central evaluation — keep in sync with API MobileAppReleaseMapper.
abstract final class MobileUpdateRules {
  static MobileUpdateKind evaluate({
    required int currentVersionCode,
    required int latestVersionCode,
    required int minimumSupportedVersionCode,
    required bool forceUpdate,
  }) {
    if (currentVersionCode >= latestVersionCode) return MobileUpdateKind.none;
    if (currentVersionCode < minimumSupportedVersionCode) {
      return MobileUpdateKind.mandatory;
    }
    if (forceUpdate) return MobileUpdateKind.mandatory;
    return MobileUpdateKind.optional;
  }
}
