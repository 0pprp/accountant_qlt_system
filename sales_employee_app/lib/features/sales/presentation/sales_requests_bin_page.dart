import 'widgets/sales_request_card.dart';

import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sales_request_item.dart';
import 'package:sales_employee_app/features/sales/domain/sales_request_tab_map.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_nav.dart';
import 'package:sales_employee_app/injection.dart';

/// قائمة طلبات SalesRequest للموظف — منفصلة عن BinOrdersPage (Orders/SalesWorkflow).
class SalesRequestsBinPage extends StatefulWidget {
  const SalesRequestsBinPage({
    super.key,
    required this.uiStatusKey,
    required this.title,
  });

  /// مفتاح واجهة قديم (pending/ready/...) يُحوَّل عبر SalesRequestTabMap.
  final String uiStatusKey;
  final String title;

  @override
  State<SalesRequestsBinPage> createState() => _SalesRequestsBinPageState();
}

class _SalesRequestsBinPageState extends State<SalesRequestsBinPage> {
  List<SalesRequestItem> _items = const [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final list = await Injection.salesRemote.getSalesRequestsByTab(
        widget.uiStatusKey,
        forceRefresh: true,
      );
      if (!mounted) return;
      setState(() {
        _items = list;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _error = DioClient.mapError(e);
        _items = const [];
      });
      // لا تحويل إلى demo_orders عند فشل الشبكة — ولا نجاح وهمي
      await showAppMessage(
        context,
        title: widget.title,
        message: _error ?? 'خطأ',
      );
    }
  }

  Future<void> _open(SalesRequestItem item) async {
    await Navigator.of(context).push(SalesNav.toRequestDetail(item.requestId));
    if (mounted) await _load();
  }

  @override
  Widget build(BuildContext context) {
    final apiTab = SalesRequestTabMap.apiTabForUiKey(widget.uiStatusKey);
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(widget.title),
        backgroundColor: AppColors.primary,
      ),
      body: RefreshIndicator(
        onRefresh: _load,
        child: _loading
            ? ListView(
                children: const [
                  SizedBox(height: 120),
                  Center(child: CircularProgressIndicator()),
                ],
              )
            : _items.isEmpty
            ? ListView(
                children: [
                  const SizedBox(height: 120),
                  Center(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 24),
                      child: Column(
                        children: [
                          Text(
                            _error != null
                                ? _error!
                                : 'لا طلبات في «${SalesRequestTabMap.label(apiTab)}»',
                            textAlign: TextAlign.center,
                          ),
                          if (_error != null) ...[
                            const SizedBox(height: 16),
                            ElevatedButton(
                              onPressed: _load,
                              child: const Text('إعادة المحاولة'),
                            ),
                          ],
                        ],
                      ),
                    ),
                  ),
                ],
              )
            : ListView.builder(
                padding: EdgeInsets.fromLTRB(12, 8, 12, 24 + bottom),
                itemCount: _items.length,
                itemBuilder: (_, i) {
                  final item = _items[i];
                  return SalesRequestCard(
                    item: item,
                    onOpen: () => _open(item),
                  );
                },
              ),
      ),
    );
  }
}
