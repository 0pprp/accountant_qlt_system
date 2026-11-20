abstract final class Validators {
  static const maxNameLength = 150;
  static const maxAddressLength = 400;
  static const maxNoteLength = 500;
  static const maxPinNameLength = 150;
  static const maxProductLength = 100;
  static const maxCancelReasonLength = 200;

  static final _diacritics = RegExp('[ً-ٰٟـ​-‏]');
  static final _arabicWord = RegExp('^[ء-يٱ-ۓ]{2,30}\$');
  static final _iraqiMobile = RegExp(r'^07[3-9]\d{8}$');
  static const _joinWithNext = ['عبد', 'ابو', 'أبو', 'ابي', 'أبي'];

  static String toWesternDigits(String? value) {
    final buffer = StringBuffer();
    for (final rune in (value ?? '').runes) {
      if (rune >= 0x0660 && rune <= 0x0669) {
        buffer.writeCharCode(0x30 + rune - 0x0660);
      } else if (rune >= 0x06F0 && rune <= 0x06F9) {
        buffer.writeCharCode(0x30 + rune - 0x06F0);
      } else {
        buffer.writeCharCode(rune);
      }
    }
    return buffer.toString();
  }

  static String? normalizePhone(String? value) {
    var digits = toWesternDigits(value).replaceAll(RegExp(r'[^0-9]'), '');
    if (digits.startsWith('00964')) {
      digits = '0${digits.substring(5)}';
    } else if (digits.startsWith('964')) {
      digits = '0${digits.substring(3)}';
    } else if (digits.length == 10 && digits.startsWith('7')) {
      digits = '0$digits';
    }
    return _iraqiMobile.hasMatch(digits) ? digits : null;
  }

  static String? normalizeTripleName(String? value) {
    final cleaned = (value ?? '').replaceAll(_diacritics, '');
    final words = cleaned
        .split(' ')
        .map((w) => w.trim())
        .where((w) => w.isNotEmpty)
        .toList();
    var parts = 0;
    for (var i = 0; i < words.length; i++) {
      if (_joinWithNext.contains(words[i]) && i + 1 < words.length) {
        i++;
      }
      parts++;
    }
    if (parts < 3) return null;
    if (words.any((w) => !_arabicWord.hasMatch(w) && !_joinWithNext.contains(w))) {
      return null;
    }
    final joined = words.join(' ');
    return joined.length <= maxNameLength ? joined : null;
  }

  static String? normalizeProduct(String? value) {
    final v = (value ?? '').trim().replaceAll(RegExp(r'\s+'), ' ');
    return v.length >= 2 && v.length <= maxProductLength ? v : null;
  }

  static bool insideIraq(double lat, double lng) =>
      lat.isFinite &&
      lng.isFinite &&
      lat >= 29 &&
      lat <= 38 &&
      lng >= 38 &&
      lng <= 49;

  static String? customerName(String? value) {
    if ((value ?? '').trim().isEmpty) return 'اكتب اسم الزبون الثلاثي';
    return normalizeTripleName(value) == null
        ? 'اكتب الاسم الثلاثي بالحروف العربية'
        : null;
  }

  static String? customerPhone(String? value) {
    if ((value ?? '').trim().isEmpty) return 'اكتب رقم هاتف الزبون';
    return normalizePhone(value) == null
        ? 'أدخل رقماً عراقياً صحيحاً مثل 07701234567'
        : null;
  }

  static String? optionalPhone(String? value) {
    if ((value ?? '').trim().isEmpty) return null;
    return normalizePhone(value) == null
        ? 'أدخل رقماً عراقياً صحيحاً مثل 07701234567'
        : null;
  }

  static String? address(String? value) {
    final v = (value ?? '').trim();
    if (v.length < 3) return 'اكتب عنوان الزبون';
    if (v.length > maxAddressLength) return 'العنوان أطول من المسموح';
    return null;
  }

  static String? note(String? value) {
    final v = (value ?? '').trim();
    return v.length > maxNoteLength ? 'الملاحظة أطول من المسموح' : null;
  }

  static String? product(String? value) =>
      normalizeProduct(value) == null ? 'اختر المنتج أو اكتب اسمه' : null;

  static String? pinName(String? value) {
    final v = (value ?? '').trim();
    if (v.length < 2) return 'اكتب اسم المحل (حرفان على الأقل)';
    if (v.length > maxPinNameLength) return 'اسم المحل أطول من المسموح';
    return null;
  }

  static String? interest(int? value) =>
      value == null || value < 1 || value > 5
      ? 'اختر مدى اهتمام صاحب المحل'
      : null;

  static String? cancelReason(String? value) =>
      (value ?? '').trim().length > maxCancelReasonLength
      ? 'السبب أطول من المسموح'
      : null;
}
