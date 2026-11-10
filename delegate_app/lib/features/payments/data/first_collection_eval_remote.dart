import 'package:dio/dio.dart';

import '../../../core/network/api_exception.dart';
import '../../../core/network/dio_client.dart';
import '../../../core/network/json_map.dart';
import '../presentation/shop_evaluation_dialog.dart';

/// POST /api/v1/app/WrongSales/first-collection-eval
/// الكاتب من الجلسة؛ لا يُرسل source/createdBy من العميل.
class FirstCollectionEvalRemote {
  FirstCollectionEvalRemote(this._dio);

  final DioClient _dio;

  /// Returns null on success; Arabic error on failure.
  Future<String?> submit({
    required int customerId,
    required int listId,
    required ShopEvaluationResult eval,
    double? saleAmount,
  }) async {
    if (customerId <= 0) return 'معرف الزبون مفقود';
    if (listId <= 0) return 'معرّف قائمة المندوب مفقود';

    try {
      final res = await _dio.dio
          .post(
            'api/v1/app/WrongSales/first-collection-eval',
            data: {
              'customerId': customerId,
              'listId': listId,
              'rating': eval.rating,
              'note': eval.note,
              'shopStockEstimatedValue': eval.shopStockEstimatedValue,
              'estimatedDailyRevenue': eval.estimatedDailyRevenue,
              if (saleAmount != null && saleAmount > 0) 'saleAmount': saleAmount,
            },
          )
          .timeout(const Duration(seconds: 12));

      final code = res.statusCode ?? 0;
      if (code >= 200 && code < 300) return null;

      return _message(res.data) ?? 'تعذّر حفظ تقييم المحل ($code)';
    } on DioException catch (e) {
      final err = e.error;
      if (err is ApiException) return err.message;
      return _message(e.response?.data) ??
          'تعذّر الاتصال لحفظ تقييم المحل';
    } catch (_) {
      return 'تعذّر الاتصال لحفظ تقييم المحل';
    }
  }

  String? _message(dynamic data) {
    if (data is! Map) return null;
    final msg = JsonMap.str(data, ['message', 'Message', 'title', 'Title']);
    return msg.isEmpty ? null : msg;
  }
}
