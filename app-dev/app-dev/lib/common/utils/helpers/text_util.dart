import 'package:flutter/services.dart';
import 'package:intl/intl.dart' hide TextDirection;

class TextUtil {
  static TextDirection getDirection(String v, {TextDirection defaultDirection = TextDirection.rtl}) {
    final string = v.trim();
    if (string.isEmpty) return defaultDirection;
    final firstUnit = string.codeUnitAt(0);
    if (firstUnit > 0x0600 && firstUnit < 0x06FF ||
        firstUnit > 0x0750 && firstUnit < 0x077F ||
        firstUnit > 0x07C0 && firstUnit < 0x07EA ||
        firstUnit > 0x0840 && firstUnit < 0x085B ||
        firstUnit > 0x08A0 && firstUnit < 0x08B4 ||
        firstUnit > 0x08E3 && firstUnit < 0x08FF ||
        firstUnit > 0xFB50 && firstUnit < 0xFBB1 ||
        firstUnit > 0xFBD3 && firstUnit < 0xFD3D ||
        firstUnit > 0xFD50 && firstUnit < 0xFD8F ||
        firstUnit > 0xFD92 && firstUnit < 0xFDC7 ||
        firstUnit > 0xFDF0 && firstUnit < 0xFDFC ||
        firstUnit > 0xFE70 && firstUnit < 0xFE74 ||
        firstUnit > 0xFE76 && firstUnit < 0xFEFC ||
        firstUnit > 0x10800 && firstUnit < 0x10805 ||
        firstUnit > 0x1B000 && firstUnit < 0x1B0FF ||
        firstUnit > 0x1D165 && firstUnit < 0x1D169 ||
        firstUnit > 0x1D16D && firstUnit < 0x1D172 ||
        firstUnit > 0x1D17B && firstUnit < 0x1D182 ||
        firstUnit > 0x1D185 && firstUnit < 0x1D18B ||
        firstUnit > 0x1D1AA && firstUnit < 0x1D1AD ||
        firstUnit > 0x1D242 && firstUnit < 0x1D244) {
      return TextDirection.rtl;
    }
    return TextDirection.ltr;
  }

  static String addComma(String price) {
    try {
      final numValue = double.parse(price);

      // Convert to int if it's a whole number (no decimal part)
      final formattedValue = numValue % 1 == 0 ? numValue.toInt().toString() : numValue.toString();

      return formattedValue.replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]},');
    } catch (e) {
      return price;
    }
  }

  static int parse(String number) {
    String p = number.trim().replaceAll(',', '');
    return int.parse(p.isEmpty ? '0' : p);
  }

  static String formatPhoneNumber(String? input) {
    if (input == null) return '';
    // Remove all non-digit characters
    String digits = input.replaceAll(RegExp(r'\D'), '');

    if (digits.length != 11) return input; // handle invalid length

    // Format: XXXX XXX XXXX
    return '${digits.substring(0, 4)} ${digits.substring(4, 7)} ${digits.substring(7)}';
  }

  static String withPersianNumbers(String txt) {
    if (txt.isEmpty) {
      return txt;
    }

    var x = txt;
    x = x.replaceAll('0', '\u06F0');
    x = x.replaceAll('1', '\u06F1');
    x = x.replaceAll('2', '\u06F2');
    x = x.replaceAll('3', '\u06F3');
    x = x.replaceAll('4', '\u06F4');
    x = x.replaceAll('5', '\u06F5');
    x = x.replaceAll('6', '\u06F6');
    x = x.replaceAll('7', '\u06F7');
    x = x.replaceAll('8', '\u06F8');
    x = x.replaceAll('9', '\u06F9');

    x = x.replaceAll('\u0660', '\u06F0');
    x = x.replaceAll('\u0661', '\u06F1');
    x = x.replaceAll('\u0662', '\u06F2');
    x = x.replaceAll('\u0663', '\u06F3');
    x = x.replaceAll('\u0664', '\u06F4');
    x = x.replaceAll('\u0665', '\u06F5');
    x = x.replaceAll('\u0666', '\u06F6');
    x = x.replaceAll('\u0667', '\u06F7');
    x = x.replaceAll('\u0668', '\u06F8');
    x = x.replaceAll('\u0669', '\u06F9');

    return x;
  }

  static bool isValidNumber(String number) => RegExp(r'^(0|0098|\+98|)9[0|1|2|3|9]\d{8}$').hasMatch(number);

  static bool isValidNationalCode(String code) => RegExp(r'^[0-9]{10}$').hasMatch(code);

  static bool isValidShabaNumber(String code) => RegExp(r'^IR(?=.{24}$)[0-9]*$').hasMatch(code);

  static bool isValidEmail(String email) => RegExp(r'^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$').hasMatch(email);

  static bool isValidUsername(String email) =>
      RegExp(r'^(?=.{5,30}$)(?![_0-9])(?!.*[_]{2})[a-zA-Z0-9_]+(?<![_])$').hasMatch(email);

  static String correctNumber(String value) {
    String number = value.replaceAll(' ', '');
    number = number.replaceFirst('+98', '0');
    number = number.replaceFirst('0098', '0');
    return number;
  }

  static String insertCharAtIndex(String originalString, int index, String charToInsert) {
    if (index < 0 || index > originalString.length) {
      throw Exception("Invalid index");
    }

    String prefix = originalString.substring(0, index);
    String suffix = originalString.substring(index);

    return prefix + charToInsert + suffix;
  }
}

extension formatText on TextInputFormatter {
  String format(String text) {
    return formatEditUpdate(
      const TextEditingValue(),
      TextEditingValue(text: text, selection: TextSelection(baseOffset: text.length, extentOffset: text.length)),
    ).text;
  }
}

// Custom formatter for Iraqi Dinar (د.ع)
class IraqiDinarInputFormatter extends TextInputFormatter {
  @override
  TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
    // Allow only numbers
    if (newValue.text.isEmpty) {
      return newValue;
    }

    // Filter to only allow digits
    final filtered = newValue.text.replaceAll(RegExp(r'[^\d]'), '');

    if (filtered.isEmpty) {
      return TextEditingValue(text: '', selection: const TextSelection.collapsed(offset: 0));
    }

    // Format number with commas as thousand separators
    final numberFormat = NumberFormat('#,###', 'en_US');
    final formattedNumber = numberFormat.format(int.parse(filtered));

    // Format with Iraqi Dinar symbol
    final formattedValue = '$formattedNumber د.ع';

    // Calculate cursor position (accounting for added commas)
    final int cursorPos = formattedNumber.length;

    return TextEditingValue(text: formattedValue, selection: TextSelection.collapsed(offset: cursorPos));
  }
}
