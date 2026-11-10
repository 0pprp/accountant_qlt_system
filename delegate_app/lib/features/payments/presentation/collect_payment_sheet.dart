import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/app_dialog.dart';
import '../../../injection.dart';
import '../../customers/domain/entities/customer.dart';
import '../domain/new_sale_list_logic.dart';
import '../domain/payment_collection_rules.dart';
import 'shop_evaluation_dialog.dart';

class CollectPaymentResult {
  const CollectPaymentResult({required this.amount});
  final double amount;
}

/// Centered system dialog for collecting a payment.
/// للمبيعات الجديدة: تقييم المحل أولاً (مصدر WrongSale عند ≤6) ثم مبلغ التسديد.
Future<CollectPaymentResult?> showCollectPaymentSheet(
  BuildContext context,
  Customer customer,
) async {
  final session = await Injection.authRepository.currentSession();
  if (session?.followerCollectionEnabled == true) {
    await AppDialog.message(
      context,
      title: 'الجباية مقفلة على هذه القائمة',
      body:
          'جباية المتابع مفعّلة على قائمتك الحالية — لا يمكن تسجيل دفعة لهذه القائمة. بقية وظائف المبيعات تبقى متاحة.',
    );
    return null;
  }

  final localIds = await Injection.paymentsLocal.collectedCustomerIds();
  final needsShopEval = NewSaleListLogic.isNewSaleAwaitingFirstCollection(
    customer,
    locallyCollectedCustomerIds: localIds,
  );

  ShopEvaluationResult? pendingShopEval;
  if (needsShopEval) {
    pendingShopEval = await showShopEvaluationDialog(context);
    if (pendingShopEval == null || !context.mounted) return null;
  }

  final amountCtrl = TextEditingController(
    text: customer.dailyInstallment != null && customer.dailyInstallment! > 0
        ? customer.dailyInstallment!.toStringAsFixed(0)
        : '',
  );
  var busy = false;
  String? error;

  final result = await AppDialog.show<CollectPaymentResult>(
    context: context,
    title: 'تحصيل من ${customer.name}',
    barrierDismissible: !busy,
    child: StatefulBuilder(
      builder: (ctx, setModal) {
        return Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              pendingShopEval != null
                  ? 'بعد تقييم المحل — المبلغ من مضاعفات 1000. يُحفظ محلياً ثم يُرسل تلقائياً.'
                  : 'المبلغ من مضاعفات 1000. يُحفظ محلياً ثم يُرسل تلقائياً.',
              style: AppTextStyles.muted,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 14),
            TextField(
              controller: amountCtrl,
              style: AppTextStyles.body,
              enabled: !busy,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              inputFormatters: [
                FilteringTextInputFormatter.allow(RegExp(r'[0-9.]')),
              ],
              decoration: const InputDecoration(labelText: 'المبلغ'),
              textAlign: TextAlign.center,
            ),
            if (error != null) ...[
              const SizedBox(height: 8),
              Text(
                error!,
                textAlign: TextAlign.center,
                style: AppTextStyles.muted.copyWith(color: AppColors.danger),
              ),
            ],
            const SizedBox(height: 16),
            FilledButton(
              onPressed: busy
                  ? null
                  : () async {
                      final amount = double.tryParse(amountCtrl.text.trim());
                      final amountError =
                          PaymentCollectionRules.validateAmount(amount ?? 0);
                      if (amountError != null) {
                        setModal(() => error = amountError);
                        return;
                      }
                      setModal(() {
                        busy = true;
                        error = null;
                      });
                      try {
                        if (pendingShopEval != null) {
                          final session =
                              await Injection.authRepository.currentSession();
                          final listId = session?.orderListId ?? 0;
                          final evalErr =
                              await Injection.firstCollectionEvalRemote.submit(
                            customerId: customer.customerId,
                            listId: listId,
                            eval: pendingShopEval!,
                            saleAmount: customer.salePrice,
                          );
                          if (evalErr != null) {
                            setModal(() {
                              error = evalErr;
                              busy = false;
                            });
                            return;
                          }
                        }

                        final saved = await Injection.collectPayment(
                          customerId: customer.customerId,
                          customerName: customer.name,
                          amount: amount!,
                        );
                        if (!saved) {
                          setModal(() {
                            error =
                                Injection.collectPaymentUseCase.lastMessage ??
                                    'تعذر التسديد';
                            busy = false;
                          });
                          return;
                        }
                        if (!ctx.mounted) return;
                        Navigator.pop(
                          ctx,
                          CollectPaymentResult(amount: amount),
                        );
                        // ignore: unawaited_futures
                        Injection.paymentSyncService.syncNow();
                      } catch (e) {
                        setModal(() {
                          error = e
                              .toString()
                              .replaceFirst(RegExp(r'^[^:]+:\s*'), '');
                          busy = false;
                        });
                      }
                    },
              child: busy
                  ? const SizedBox(
                      height: 22,
                      width: 22,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: AppColors.onPrimary,
                      ),
                    )
                  : Text(
                      pendingShopEval != null
                          ? 'تأكيد التقييم والتسديد'
                          : 'تأكيد التحصيل',
                      style: AppTextStyles.button,
                    ),
            ),
            TextButton(
              onPressed: busy ? null : () => Navigator.pop(ctx),
              child: Text(
                'إلغاء',
                style: AppTextStyles.body.copyWith(color: AppColors.muted),
              ),
            ),
          ],
        );
      },
    ),
  );

  amountCtrl.dispose();
  return result;
}
