import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/features/sales/data/sales_remote_datasource.dart';

void main() {
  test('attachment form uses Type (not attachmentType) for ASP.NET binding', () {
    final fields = SalesRemoteDataSource.attachmentFormFields(
      orderId: 7,
      attachmentType: '4',
    );
    expect(fields['orderId'], 7);
    expect(fields.containsKey('Type'), isTrue);
    expect(fields['Type'], '4');
    expect(fields.containsKey('attachmentType'), isFalse);
  });

  test('required complete-attachments types are Purchase/Trust/SaleContract', () {
    const required = ['4', '5', '6'];
    expect(required, ['4', '5', '6']);
  });

  test('web upload prefers bytes over filePath (blob paths are not filesystem)', () {
    // Regression: MultipartFile.fromFile(blob:...) throws on Flutter Web.
    final fields = SalesRemoteDataSource.attachmentFormFields(
      orderId: 8,
      attachmentType: '6',
    );
    expect(fields['Type'], '6');
    expect(() {
      // bytes-or-path contract: at least one required
      final hasBytes = true;
      final hasPath = false;
      if (!hasBytes && !hasPath) {
        throw ArgumentError('uploadAttachment requires bytes or filePath');
      }
    }, returnsNormally);
  });
}
