import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

import '../../../injection.dart';
import '../data/submitted_requests_remote.dart';
import 'new_sales_request_page.dart';

/// Inbox of requests authored by this account, not requests assigned to a seller.
class SubmittedSalesRequestsPage extends StatefulWidget {
  const SubmittedSalesRequestsPage({super.key, this.remote});
  final SubmittedRequestsRemote? remote;
  @override
  State<SubmittedSalesRequestsPage> createState() =>
      _SubmittedSalesRequestsPageState();
}

class _SubmittedSalesRequestsPageState
    extends State<SubmittedSalesRequestsPage> {
  static const _tabs = {
    'all': 'الكل',
    'inProgress': 'قيد العمل',
    'rejected': 'مرفوض',
    'completed': 'تم البيع',
  };
  late final _remote =
      widget.remote ??
      SubmittedRequestsRemote(Injection.dioClient.dio, 'Delegate');
  String _tab = 'all';
  int _page = 1, _generation = 0;
  bool _loading = true, _hasMore = false;
  String? _error;
  List<SubmittedRequest> _items = [];
  CancelToken? _cancel;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _generation++;
    _cancel?.cancel();
    super.dispose();
  }

  Future<void> _load() async {
    final generation = ++_generation;
    _cancel?.cancel();
    final token = _cancel = CancelToken();
    setState(() {
      _loading = true;
      _error = null;
      _items = [];
    });
    try {
      final data = await _remote.load(_tab, _page, token);
      if (!mounted || generation != _generation) return;
      setState(() {
        _items = data.items;
        _hasMore = data.hasMore;
        _loading = false;
      });
    } catch (e) {
      if (!mounted ||
          generation != _generation ||
          (e is DioException && CancelToken.isCancel(e)))
        return;
      final forbidden = e is DioException && e.response?.statusCode == 403;
      setState(() {
        _loading = false;
        _error = forbidden
            ? 'لا توجد صلاحية لعرض هذه الطلبات ضمن نطاق حسابك.'
            : 'تعذر تحميل طلبات البيع. تحقق من الاتصال وأعد المحاولة.';
      });
    }
  }

  Future<void> _create() async {
    await Navigator.of(context)
        .push(MaterialPageRoute(builder: (_) => const NewSalesRequestPage()));
    if (!mounted) return;
    _page = 1;
    await _load();
  }

  String _date(DateTime value) {
    final d = value.toUtc().add(const Duration(hours: 3));
    return '${d.day.toString().padLeft(2, '0')}/${d.month.toString().padLeft(2, '0')}/${d.year}';
  }

  Widget _line(String label, String? value) =>
      value == null || value.trim().isEmpty
      ? const SizedBox.shrink()
      : Padding(
          padding: const EdgeInsets.only(top: 6),
          child: SelectableText(
            '$label: $value',
            style: const TextStyle(fontSize: 15, height: 1.6),
          ),
        );

  @override
  Widget build(BuildContext context) => Directionality(
    textDirection: TextDirection.rtl,
    child: Scaffold(
      appBar: AppBar(
        title: const Text('طلبات البيع'),
        actions: [
          IconButton(
            onPressed: _loading ? null : _load,
            tooltip: 'تحديث',
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      body: SafeArea(
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(12),
              child: Row(
                children: [
                  const Expanded(
                    child: Text(
                      'الطلبات التي أرسلتها إلى المبيعات',
                      style: TextStyle(fontSize: 15),
                    ),
                  ),
                  FilledButton.icon(
                    onPressed: _create,
                    icon: const Icon(Icons.add),
                    label: const Text('طلب جديد'),
                  ),
                ],
              ),
            ),
            SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 12),
              child: Row(
                children: _tabs.entries
                    .map(
                      (entry) => Padding(
                        padding: const EdgeInsetsDirectional.only(end: 8),
                        child: ChoiceChip(
                          label: Text(entry.value),
                          selected: _tab == entry.key,
                          onSelected: (_) {
                            if (_tab == entry.key) return;
                            _tab = entry.key;
                            _page = 1;
                            _load();
                          },
                        ),
                      ),
                    )
                    .toList(),
              ),
            ),
            Expanded(
              child: _loading
                  ? const Center(child: CircularProgressIndicator())
                  : _error != null
                  ? Center(
                      child: Padding(
                        padding: const EdgeInsets.all(24),
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Text(_error!, textAlign: TextAlign.center),
                            const SizedBox(height: 12),
                            OutlinedButton(
                              onPressed: _load,
                              child: const Text('إعادة المحاولة'),
                            ),
                          ],
                        ),
                      ),
                    )
                  : RefreshIndicator(
                      onRefresh: _load,
                      child: ListView.builder(
                        physics: const AlwaysScrollableScrollPhysics(),
                        padding: const EdgeInsets.all(12),
                        itemCount: _items.isEmpty ? 1 : _items.length,
                        itemBuilder: (context, index) {
                          if (_items.isEmpty)
                            return const Padding(
                              padding: EdgeInsets.all(32),
                              child: Text(
                                'لا توجد طلبات ضمن هذا التبويب',
                                textAlign: TextAlign.center,
                              ),
                            );
                          final item = _items[index];
                          return Card(
                            margin: const EdgeInsets.only(bottom: 12),
                            child: Padding(
                              padding: const EdgeInsets.all(16),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                children: [
                                  Text(
                                    item.name,
                                    style: const TextStyle(
                                      fontSize: 18,
                                      fontWeight: FontWeight.w700,
                                    ),
                                  ),
                                  _line('رقم الطلب', '${item.id}'),
                                  _line('الحالة', item.status),
                                  _line('الهاتف', item.phone),
                                  _line('المحافظة', item.branch),
                                  _line('العنوان', item.address),
                                  _line('المنتج المطلوب', item.product),
                                  _line('الموظف', item.employee),
                                  if (item.createdAt != null)
                                    _line(
                                      'تاريخ الطلب',
                                      _date(item.createdAt!),
                                    ),
                                  _line('الملاحظة', item.note),
                                  _line('سبب الرفض', item.rejection),
                                  _line('ملاحظة التعليق', item.pendingNote),
                                ],
                              ),
                            ),
                          );
                        },
                      ),
                    ),
            ),
            if (!_loading && _error == null && (_page > 1 || _hasMore))
              Padding(
                padding: const EdgeInsets.all(8),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    TextButton(
                      onPressed: _page > 1
                          ? () {
                              _page--;
                              _load();
                            }
                          : null,
                      child: const Text('السابق'),
                    ),
                    Text('$_page'),
                    TextButton(
                      onPressed: _hasMore
                          ? () {
                              _page++;
                              _load();
                            }
                          : null,
                      child: const Text('التالي'),
                    ),
                  ],
                ),
              ),
          ],
        ),
      ),
    ),
  );
}
