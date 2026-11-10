import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/core/widgets/qalaat_pattern_background.dart';
import 'package:sales_employee_app/features/ratings/data/ratings_remote_datasource.dart';
import 'package:sales_employee_app/injection.dart';

class RatingsTab extends StatefulWidget {
  const RatingsTab({super.key});

  @override
  State<RatingsTab> createState() => _RatingsTabState();
}

class _RatingsTabState extends State<RatingsTab> {
  List<RatingSummary> _summary = const [];
  List<Map<String, dynamic>> _items = const [];
  String? _level;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final s = await Injection.ratingsRemote.summary();
      if (!mounted) return;
      setState(() {
        _summary = s;
        _loading = false;
      });
      if (s.isNotEmpty) await _open(s.first.level);
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      await showAppMessage(context, title: 'التقييمات', message: DioClient.mapError(e));
    }
  }

  Future<void> _open(String level) async {
    setState(() {
      _level = level;
      _loading = true;
    });
    final items = await Injection.ratingsRemote.byLevel(level);
    if (!mounted) return;
    setState(() {
      _items = items;
      _loading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    return QalaatPatternBackground(
      child: SafeArea(
        child: Column(
          children: [
            const Padding(
              padding: EdgeInsets.fromLTRB(20, 16, 20, 8),
              child: Align(
                alignment: Alignment.centerRight,
                child: Text(
                  'تقييمات زبائني',
                  style: TextStyle(fontSize: 20, fontWeight: FontWeight.w700, color: AppColors.primary),
                ),
              ),
            ),
            SizedBox(
              height: 56,
              child: ListView.separated(
                padding: const EdgeInsets.symmetric(horizontal: 12),
                scrollDirection: Axis.horizontal,
                itemCount: _summary.length,
                separatorBuilder: (_, __) => const SizedBox(width: 8),
                itemBuilder: (_, i) {
                  final s = _summary[i];
                  return ChoiceChip(
                    selected: s.level == _level,
                    label: Text('${s.label} (${s.count})'),
                    onSelected: (_) => _open(s.level),
                  );
                },
              ),
            ),
            Expanded(
              child: _loading
                  ? const Center(child: CircularProgressIndicator())
                  : _items.isEmpty
                      ? const Center(child: Text('لا توجد تقييمات في هذا المستوى'))
                      : ListView.builder(
                          padding: const EdgeInsets.fromLTRB(12, 8, 12, 100),
                          itemCount: _items.length,
                          itemBuilder: (_, i) {
                            final m = _items[i];
                            final name = '${m['fullName'] ?? m['FullName'] ?? m['name'] ?? 'زبون'}';
                            final id = m['customerId'] ?? m['CustomerId'] ?? m['id'] ?? m['Id'];
                            return Card(
                              child: ListTile(
                                title: Text(name),
                                subtitle: Text('#$id'),
                                onTap: () async {
                                  final cid = int.tryParse('$id');
                                  if (cid == null) return;
                                  final profile = await Injection.ratingsRemote.profileLite(cid);
                                  if (!context.mounted) return;
                                  await showAppMessage(
                                    context,
                                    title: name,
                                    message: profile == null
                                        ? 'لا تتوفر بيانات مختصرة'
                                        : profile.entries.map((e) => '${e.key}: ${e.value}').join('\n'),
                                  );
                                },
                              ),
                            );
                          },
                        ),
            ),
          ],
        ),
      ),
    );
  }
}
