import 'package:flutter/services.dart';
import 'package:intl/intl.dart';

/// يفرض أرقام فقط ويعرض فواصل آلاف (1,000,000).
class ThousandsSeparatorInputFormatter extends TextInputFormatter {
  ThousandsSeparatorInputFormatter();

  static final _format = NumberFormat('#,###', 'en');

  static String format(num value) => _format.format(value.round());

  static double? parse(String raw) {
    final digits = raw.replaceAll(RegExp(r'[^0-9.]'), '').trim();
    if (digits.isEmpty) return null;
    return double.tryParse(digits);
  }

  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    final digits = newValue.text.replaceAll(RegExp(r'[^0-9]'), '');
    if (digits.isEmpty) {
      return const TextEditingValue(
        text: '',
        selection: TextSelection.collapsed(offset: 0),
      );
    }
    // تجاهل أصفار بادئة زائدة مع الإبقاء على "0" أثناء المسح.
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
