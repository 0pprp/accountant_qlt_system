import 'package:flutter/services.dart';
import 'package:intl/intl.dart';

/// يفرض أرقام فقط ويعرض فواصل آلاف (1,000,000).
class ThousandsSeparatorInputFormatter extends TextInputFormatter {
  ThousandsSeparatorInputFormatter();

  static final _format = NumberFormat('#,###', 'en');

  static const _arabicDigits = '٠١٢٣٤٥٦٧٨٩';

  static String format(num value) => _format.format(value.round());

  /// يعرض قيمة مخزّنة ("3000000.0" أو "3,000,000") بفواصل، أو فارغاً إن لم تكن رقماً.
  static String formatRaw(Object? raw) {
    final value = parse('${raw ?? ''}');
    return value == null ? '' : format(value);
  }

  static double? parse(String raw) {
    final western = _toWestern(raw);
    final digits = western.replaceAll(RegExp(r'[^0-9.]'), '').trim();
    if (digits.isEmpty) return null;
    return double.tryParse(digits);
  }

  static String _toWestern(String raw) {
    final buffer = StringBuffer();
    for (final ch in raw.split('')) {
      final i = _arabicDigits.indexOf(ch);
      buffer.write(i >= 0 ? '$i' : ch);
    }
    return buffer.toString();
  }

  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    final digits = _toWestern(newValue.text).replaceAll(RegExp(r'[^0-9]'), '');
    if (digits.isEmpty) {
      return const TextEditingValue(
        text: '',
        selection: TextSelection.collapsed(offset: 0),
      );
    }
    final normalized = digits.replaceFirst(RegExp(r'^0+(?=.)'), '');
    final number = int.tryParse(normalized);
    if (number == null) return oldValue;
    final formatted = _format.format(number);
    return TextEditingValue(
      text: formatted,
      selection: TextSelection.collapsed(offset: formatted.length),
    );
  }
}
