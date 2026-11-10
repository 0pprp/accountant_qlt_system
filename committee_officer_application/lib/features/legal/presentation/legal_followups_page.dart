import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/features/field/presentation/field_followups_page.dart';
import 'package:committee_officer_application/features/legal/data/legal_followups_api.dart';
import 'package:committee_officer_application/features/legal/presentation/legal_followup_detail_page.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

/// قضايا LegalFollowupCases المسندة للمتابع — دورة ميدانية حقيقية ضد AppArea.
class LegalFollowupsPage extends StatefulWidget {
  const LegalFollowupsPage({super.key});

  @override
  State<LegalFollowupsPage> createState() => _LegalFollowupsPageState();
}

class _LegalFollowupsPageState extends State<LegalFollowupsPage> {
  late final _api = LegalFollowupsApi(Injection.dio.dio);
  bool _loading = true;
  String? _error;
  List<Map<String, dynamic>> _items = const [];
  String _status = 'InProgress';

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
      final items = await _api.list(status: _status);
      if (!mounted) return;
      setState(() => _items = items);
    } catch (_) {
      if (mounted) {
        setState(() => _error = 'تعذر تحميل القضايا القانونية. تحقق من الاتصال والصلاحية.');
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      appBar: AppBar(title: const Text('المتابعة القانونية')),
      body: AnimatedWavesBackground(
        child: RefreshIndicator(
          onRefresh: _load,
          child: ListView(
            padding: EdgeInsets.fromLTRB(16, 16, 16, 24 + bottom),
            children: [
              const Text(
                'قضايا مسندة إليك — ليست أسماء الفرع القانونية.',
                textAlign: TextAlign.center,
                style: TextStyle(color: AppColors.textSecondary, height: 1.45),
              ),
              const SizedBox(height: 12),
              SegmentedButton<String>(
                segments: const [
                  ButtonSegment(value: 'InProgress', label: Text('قيد العمل')),
                  ButtonSegment(value: 'Completed', label: Text('تمت')),
                ],
                selected: {_status},
                onSelectionChanged: (s) {
                  setState(() => _status = s.first);
                  _load();
                },
              ),
              const SizedBox(height: 16),
              if (_loading) const Center(child: Padding(padding: EdgeInsets.all(24), child: CircularProgressIndicator())),
              if (_error != null)
                Text(_error!, textAlign: TextAlign.center, style: const TextStyle(color: Colors.red, height: 1.5)),
              if (!_loading && _error == null && _items.isEmpty)
                const Text('لا قضايا في هذه الحالة.', textAlign: TextAlign.center),
              ..._items.map((item) {
                final id = (item['id'] as num).toInt();
                return Card(
                  margin: const EdgeInsets.only(bottom: 10),
                  child: ListTile(
                    title: Text('${item['customerName'] ?? ''}', style: const TextStyle(fontWeight: FontWeight.w700)),
                    subtitle: Text('${item['customerPhone'] ?? ''}\n${item['statusLabel'] ?? item['status'] ?? ''}'),
                    isThreeLine: true,
                    trailing: const Icon(Icons.chevron_left),
                    onTap: () async {
                      await Navigator.of(context).push(
                        MaterialPageRoute(builder: (_) => LegalFollowupDetailPage(caseId: id)),
                      );
                      if (mounted) _load();
                    },
                  ),
                );
              }),
              const SizedBox(height: 12),
              OutlinedButton.icon(
                onPressed: () {
                  Navigator.of(context).push(
                    MaterialPageRoute(builder: (_) => const FieldFollowupsPage()),
                  );
                },
                icon: const Icon(Icons.hiking_rounded),
                label: const Text('المتابعات الميدانية'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
