import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/core/widgets/ui/data_field.dart';
import 'package:committee_officer_application/core/widgets/ui/entity_card.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

/// متابعات ميدانية معلّقة — رحلة Followers/FieldFollowups بهوية قلعة الضمان.
class FieldFollowupsPage extends StatefulWidget {
  const FieldFollowupsPage({super.key});

  @override
  State<FieldFollowupsPage> createState() => _FieldFollowupsPageState();
}

class _FieldFollowupsPageState extends State<FieldFollowupsPage> {
  int _request = 0;
  bool _loading = true;
  String? _error;
  List<_FieldItem> _items = const [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final request = ++_request;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final rows = await Injection.committee.fieldFollowups();
      if (!mounted || request != _request) return;
      setState(() {
        _items = rows.map(_FieldItem.fromJson).toList();
        _loading = false;
      });
    } catch (e) {
      if (!mounted || request != _request) return;
      setState(() {
        _loading = false;
        _error = DioClient.mapError(e);
      });
    }
  }

  Future<void> _complete(_FieldItem item) async {
    var draft = '';
    final ok = await showCenteredDialog<bool>(
      context: context,
      builder: (dialogContext) => Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Text(
            'تمت المتابعة',
            style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 8),
          Text(
            item.customerName.isEmpty ? 'زبون #${item.customerId}' : item.customerName,
            style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
          ),
          const SizedBox(height: 12),
          TextField(
            maxLines: 3,
            onChanged: (v) => draft = v,
            decoration: const InputDecoration(
              labelText: 'ملاحظة إجبارية',
              filled: true,
              fillColor: Color(0xFFF8F8F8),
            ),
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: () => Navigator.of(dialogContext).pop(false),
                  child: const Text('إلغاء'),
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: ElevatedButton(
                  onPressed: () => Navigator.of(dialogContext).pop(true),
                  child: const Text('اعتماد'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
    if (ok != true) return;
    final note = draft.trim();
    if (note.isEmpty) {
      await showAppMessage(
        context,
        title: 'تنبيه',
        message: 'الملاحظة إجبارية قبل اعتماد تمت المتابعة',
        icon: Icons.warning_amber_rounded,
        color: AppColors.danger,
      );
      return;
    }
    try {
      await Injection.committee.completeFieldFollowup(item.id, note);
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم',
        message: 'تم اعتماد المتابعة الميدانية',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
      if (!mounted) return;
      await _load();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تعذر الاعتماد',
        message: DioClient.mapError(e),
        icon: Icons.error_outline,
        color: AppColors.danger,
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      appBar: AppBar(
        title: const Text('المتابعات الميدانية'),
        actions: [
          IconButton(
            tooltip: 'تحديث',
            onPressed: _loading ? null : _load,
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      body: AnimatedWavesBackground(
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : _error != null
            ? Center(
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Text(
                    _error!,
                    textAlign: TextAlign.center,
                    style: const TextStyle(color: AppColors.danger),
                  ),
                ),
              )
            : _items.isEmpty
            ? const Center(
                child: Text(
                  'لا توجد متابعات ميدانية معلّقة',
                  style: TextStyle(color: AppColors.muted),
                ),
              )
            : ListView.separated(
                padding: EdgeInsets.fromLTRB(16, 16, 16, 24 + bottom),
                itemCount: _items.length,
                separatorBuilder: (_, _) => const SizedBox(height: 12),
                itemBuilder: (_, i) {
                  final item = _items[i];
                  return EntityCard(
                    name: item.customerName.isEmpty
                        ? 'زبون #${item.customerId}'
                        : item.customerName,
                    eyebrow: item.listName.isEmpty
                        ? 'قائمة #${item.listId}'
                        : item.listName,
                    statusLabel: 'معلّقة',
                    fields: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        if (item.phone.isNotEmpty)
                          DataField(
                            label: 'الهاتف',
                            value: item.phone,
                            requiredField: true,
                            isPhone: true,
                          ),
                        DataField(
                          label: 'معرف الزبون',
                          value: '${item.customerId}',
                          requiredField: true,
                        ),
                      ],
                    ),
                    actions: SizedBox(
                      width: double.infinity,
                      height: 44,
                      child: ElevatedButton(
                        onPressed: () => _complete(item),
                        child: const Text('تمت المتابعة'),
                      ),
                    ),
                  );
                },
              ),
      ),
    );
  }
}

class _FieldItem {
  const _FieldItem({
    required this.id,
    required this.listId,
    required this.listName,
    required this.customerId,
    required this.customerName,
    required this.phone,
  });

  final int id;
  final int listId;
  final String listName;
  final int customerId;
  final String customerName;
  final String phone;

  factory _FieldItem.fromJson(Map<String, dynamic> j) => _FieldItem(
    id: int.tryParse('${j['id'] ?? j['Id'] ?? 0}') ?? 0,
    listId: int.tryParse('${j['listId'] ?? j['ListId'] ?? 0}') ?? 0,
    listName: '${j['listName'] ?? j['ListName'] ?? ''}',
    customerId: int.tryParse('${j['customerId'] ?? j['CustomerId'] ?? 0}') ?? 0,
    customerName: '${j['customerName'] ?? j['CustomerName'] ?? ''}',
    phone: '${j['phoneNumber'] ?? j['PhoneNumber'] ?? ''}',
  );
}
