import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/core/formatting/thousands_separator_input_formatter.dart';

void main() {
  final formatter = ThousandsSeparatorInputFormatter();

  TextEditingValue type(String text) =>
      formatter.formatEditUpdate(TextEditingValue.empty, TextEditingValue(text: text));

  test('يضع فاصلة كل ثلاث مراتب أثناء الكتابة', () {
    expect(type('3000000').text, '3,000,000');
    expect(type('50000').text, '50,000');
    expect(type('999').text, '999');
  });

  test('يقبل الأرقام العربية ويتجاهل غير الأرقام', () {
    expect(type('٣٠٠٠٠٠٠').text, '3,000,000');
    expect(type('3a0b0').text, '300');
  });

  test('القراءة تحذف الفواصل فلا تصير المقدمة صفراً', () {
    expect(ThousandsSeparatorInputFormatter.parse('170,000'), 170000);
    expect(ThousandsSeparatorInputFormatter.parse('٣,٠٠٠,٠٠٠'), 3000000);
    expect(ThousandsSeparatorInputFormatter.parse(''), isNull);
  });

  test('المسودة المحفوظة "3000000.0" تُعرض دون تضخيم', () {
    expect(ThousandsSeparatorInputFormatter.formatRaw('3000000.0'), '3,000,000');
    expect(ThousandsSeparatorInputFormatter.formatRaw(170000.0), '170,000');
    expect(ThousandsSeparatorInputFormatter.formatRaw(null), '');
  });
}
