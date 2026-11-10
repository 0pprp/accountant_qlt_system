import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/features/lists/presentation/list_detail_page.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

class CollectionTab extends StatefulWidget {
  const CollectionTab({super.key});

  @override
  State<CollectionTab> createState() => _CollectionTabState();
}

class _CollectionTabState extends State<CollectionTab> {
  List<Map<String, dynamic>> _lists = const [];
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
      final rows = await Injection.committee.collectionLists();
      if (!mounted) return;
      setState(() {
        _lists = rows;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _error = DioClient.mapError(e);
      });
    }
  }

  Future<void> _endCollectionRatings() async {
    try {
      final missing = await Injection.committee.missingDayRatings();
      if (!mounted) return;
      if (missing.isEmpty) {
        await showAppMessage(context, title: 'التقييمات', message: 'لا تقييمات ناقصة حالياً');
        return;
      }
      for (final m in missing) {
        if (!mounted) return;
        final listId = int.tryParse('${m['listId'] ?? m['ListId'] ?? 0}') ?? 0;
        final name = '${m['listName'] ?? m['ListName'] ?? ''}';
        final score = await showCenteredDialog<int>(
          context: context,
          barrierDismissible: false,
          builder: (dialogContext) => Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('تقييم مندوب القائمة', style: TextStyle(fontWeight: FontWeight.w700)),
              Text(name, style: const TextStyle(color: AppColors.textSecondary)),
              const SizedBox(height: 10),
              Wrap(
                spacing: 6,
                children: List.generate(11, (i) {
                  return ElevatedButton(
                    onPressed: () => Navigator.of(dialogContext).pop(i),
                    child: Text('$i'),
                  );
                }),
              ),
              const SizedBox(height: 8),
              TextButton(
                onPressed: () => Navigator.of(dialogContext).pop(),
                child: const Text('إلغاء'),
              ),
            ],
          ),
        );
        if (score == null) return;
        await Injection.committee.postDayRating(listId: listId, score: score);
      }
      if (!mounted) return;
      await showAppMessage(context, title: 'تم', message: 'اكتملت تقييمات يوم المندوب', color: AppColors.success);
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(context, title: 'فشل', message: DioClient.mapError(e), color: AppColors.danger);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      body: AnimatedWavesBackground(
        child: SafeArea(
          bottom: false,
          child: Column(
            children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
                child: Row(
                  children: [
                    const Expanded(
                      child: Text('الجباية', style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800)),
                    ),
                    TextButton(onPressed: _endCollectionRatings, child: const Text('تقييمات اليوم')),
                  ],
                ),
              ),
              Expanded(
                child: RefreshIndicator(
                  onRefresh: _load,
                  child: _loading
                      ? ListView(children: const [SizedBox(height: 120), Center(child: CircularProgressIndicator())])
                      : _error != null
                          ? ListView(children: [Padding(padding: const EdgeInsets.all(24), child: Text(_error!, textAlign: TextAlign.center))])
                          : _lists.isEmpty
                              ? ListView(
                                  children: const [
                                    SizedBox(height: 120),
                                    Center(child: Text('لا توجد قوائم مفعّلة للجباية')),
                                  ],
                                )
                              : ListView.separated(
                                  padding: EdgeInsets.fromLTRB(16, 8, 16, 100 + bottom),
                                  itemCount: _lists.length,
                                  separatorBuilder: (_, __) => const SizedBox(height: 8),
                                  itemBuilder: (_, i) {
                                    final r = _lists[i];
                                    final id = int.tryParse('${r['listId'] ?? r['ListId'] ?? 0}') ?? 0;
                                    final name = '${r['listName'] ?? r['ListName'] ?? ''}';
                                    final mandob = '${r['mandobName'] ?? r['MandobName'] ?? ''}';
                                    return Material(
                                      color: Colors.white.withValues(alpha: 0.92),
                                      borderRadius: BorderRadius.circular(14),
                                      child: ListTile(
                                        title: Text(name, style: const TextStyle(fontWeight: FontWeight.w700)),
                                        subtitle: Text('المندوب: $mandob'),
                                        trailing: const Icon(Icons.chevron_left),
                                        onTap: () {
                                          Navigator.of(context).push(
                                            MaterialPageRoute(
                                              builder: (_) => ListDetailPage(listId: id, listName: name),
                                            ),
                                          );
                                        },
                                      ),
                                    );
                                  },
                                ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
