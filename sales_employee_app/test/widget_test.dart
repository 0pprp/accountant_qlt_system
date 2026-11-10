import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';

void main() {
  test('Qalaat primary color is defined', () {
    expect(AppColors.primary.toARGB32(), 0xFF006A64);
  });
}
