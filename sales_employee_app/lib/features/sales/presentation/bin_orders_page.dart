import 'dart:async';

import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/config/app_env.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/animated_waves.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sale_order.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';
import 'package:sales_employee_app/features/sales/presentation/complete_sale_wizard_page.dart';
import 'package:sales_employee_app/features/sales/presentation/sale_details_page.dart';
import 'package:sales_employee_app/features/sales/presentation/widgets/customer_status_card.dart';
import 'package:sales_employee_app/features/sales/presentation/widgets/order_actions_sheet.dart';
import 'package:sales_employee_app/injection.dart';

class BinOrdersPage extends StatefulWidget {
  const BinOrdersPage({
    super.key,
    required this.statusKey,
    required this.title,
  });

  final String statusKey;
  final String title;

  @override
  State<BinOrdersPage> createState() => _BinOrdersPageState();
}

class _BinOrdersPageState extends State<BinOrdersPage> {
  List<SaleOrder> _orders = const [];
  bool _loading = true;
  Timer? _poll;

  @override
  void initState() {
    super.initState();
    _load(silent: false);
    _poll = Timer.periodic(const Duration(seconds: 12), (_) => _load(silent: true));
  }

  @override
  void dispose() {
    _poll?.cancel();
    super.dispose();
  }

  Future<void> _load({required bool silent}) async {
    if (!silent && mounted) setState(() => _loading = true);
    try {
      final remote = await Injection.salesRemote.getByStatus(widget.statusKey, forceRefresh: true);
      final demo = AppEnv.allowDemoOrders
          ? await Injection.demoOrders.byStatus(widget.statusKey)
          : const <SaleOrder>[];
      if (!mounted) return;
      setState(() {
        _orders = [...demo, ...remote];
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      if (!silent) {
        await showAppMessage(context, title: widget.title, message: DioClient.mapError(e));
      }
    }
  }

  Future<void> _openSale(SaleOrder o) async {
    final status = SalesWorkflowRules.normalize(o.status);
    if (SalesWorkflowRules.isTerminal(status)) {
      if (o.id > 0) {
        await Navigator.of(context).push(MaterialPageRoute(builder: (_) => SaleDetailsPage(orderId: o.id)));
      }
      return;
    }
    if (!SalesWorkflowRules.canStartSale(status)) {
      await showAppMessage(context, title: 'غير مسموح', message: 'لا يمكن فتح إتمام البيع من هذه الحالة');
      return;
    }
    final done = await Navigator.of(context).push<bool>(
      MaterialPageRoute(builder: (_) => CompleteSaleWizardPage(orderId: o.id)),
    );
    if (done == true && mounted) await _load(silent: false);
  }

  Future<void> _runAction(SaleOrder o) async {
    final status = SalesWorkflowRules.normalize(o.status ?? widget.statusKey);
    if (!SalesWorkflowRules.showCardActions(status)) return;

    final result = await showOrderActionsSheet(context, currentStatus: status);
    if (result == null) return;

    try {
      switch (result.action) {
        case OrderCardAction.startSale:
        case OrderCardAction.resumeSale:
          await _openSale(o);
          return;
        case OrderCardAction.ready:
          await _transfer(o, SalesWorkflowRules.ready, result.reason);
          break;
        case OrderCardAction.hold:
          await _transfer(o, SalesWorkflowRules.onHold, result.reason);
          break;
        case OrderCardAction.reject:
          await _transfer(o, SalesWorkflowRules.rejected, result.reason);
          break;
        case OrderCardAction.reassign:
          // نقل الاسم الحي على SalesRequest لا Order — افتح تفاصيل الطلب إن وُجد، وإلا demo فقط تحت العلم.
          if (o.id < 0 && AppEnv.allowDemoOrders) {
            await Injection.demoOrders.reassign(
              o.id,
              assignee: result.assignee!,
              reason: result.reason!,
            );
          } else {
            await showAppMessage(
              context,
              title: 'نقل الاسم',
              message:
                  'نقل الاسم يتم من تفاصيل طلب البيع (SalesRequest) وليس من سجل Order. افتح صندوق طلبات البيع ثم تفاصيل الطلب.',
            );
            return;
          }
          break;
      }
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم',
        message: 'تم تنفيذ الإجراء',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
      await _load(silent: false);
    } catch (e) {
      if (!mounted) return;
      final msg = '$e'.contains('reason_required') ? 'الملاحظة/السبب إلزامي' : DioClient.mapError(e);
      await showAppMessage(context, title: 'فشل الإجراء', message: msg);
    }
  }

  Future<void> _transfer(SaleOrder o, String target, String? reason) async {
    if (SalesWorkflowRules.requiresReason(target) && (reason == null || reason.trim().isEmpty)) {
      throw StateError('reason_required');
    }
    if (o.id < 0) {
      if (!AppEnv.allowDemoOrders) {
        throw StateError('demo_disabled');
      }
      await Injection.demoOrders.transfer(o.id, target, reason: reason);
    } else {
      await Injection.salesRemote.transfer(id: o.id, targetStatus: target, reason: reason);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    final bin = SalesWorkflowRules.normalize(widget.statusKey);

    return Scaffold(
      appBar: AppBar(title: Text(widget.title)),
      body: AnimatedWavesBackground(
        child: RefreshIndicator(
          onRefresh: () => _load(silent: false),
          child: _loading
              ? ListView(children: const [SizedBox(height: 160), Center(child: CircularProgressIndicator())])
              : _orders.isEmpty
                  ? ListView(children: const [SizedBox(height: 160), Center(child: Text('لا توجد عناصر حالياً'))])
                  : ListView.separated(
                      padding: EdgeInsets.fromLTRB(16, 12, 16, 24 + bottom),
                      itemCount: _orders.length,
                      separatorBuilder: (_, __) => const SizedBox(height: 10),
                      itemBuilder: (_, i) {
                        final o = _orders[i];
                        final st = SalesWorkflowRules.normalize(o.status ?? bin);
                        final showActions = SalesWorkflowRules.showCardActions(st);
                        return CustomerStatusCard(
                          order: o,
                          onActions: showActions ? () => _runAction(o) : null,
                          onCompleteSale: SalesWorkflowRules.canStartSale(st) ? () => _openSale(o) : null,
                          onTap: () async {
                            if (SalesWorkflowRules.canStartSale(st)) {
                              await _openSale(o);
                              return;
                            }
                            if (o.id > 0) {
                              await Navigator.of(context).push(
                                MaterialPageRoute(builder: (_) => SaleDetailsPage(orderId: o.id)),
                              );
                              if (mounted) await _load(silent: true);
                            }
                          },
                        );
                      },
                    ),
        ),
      ),
    );
  }
}
