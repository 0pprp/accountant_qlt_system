import 'package:field_promoter_app/core/formatting/thousands_separator_input_formatter.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('عرض المبالغ بفواصل الآلاف', () {
    expect(ThousandsSeparatorInputFormatter.format(1500000), '1,500,000');
    expect(ThousandsSeparatorInputFormatter.format(999), '999');
    expect(ThousandsSeparatorInputFormatter.format(1250000.4), '1,250,000');
    expect(ThousandsSeparatorInputFormatter.formatRaw('3000000.0'), '3,000,000');
    expect(ThousandsSeparatorInputFormatter.formatRaw('abc'), '');
  });

  test('تحليل الأرقام العربية والفواصل', () {
    expect(ThousandsSeparatorInputFormatter.parse('١٬٠٠٠'), 1000);
    expect(ThousandsSeparatorInputFormatter.parse('2,500,000'), 2500000);
    expect(ThousandsSeparatorInputFormatter.parse(''), isNull);
  });

  test('المُدخل يضيف الفواصل أثناء الكتابة', () {
    final f = ThousandsSeparatorInputFormatter();
    final out = f.formatEditUpdate(
      TextEditingValue.empty,
      const TextEditingValue(text: '0012345'),
    );
    expect(out.text, '12,345');
    expect(out.selection.baseOffset, out.text.length);
  });
}
