import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

import '../../../core/network/dio_client.dart';
import '../../../core/theme/app_colors.dart';
import '../../../injection.dart';
import '../data/archive_remote.dart';
import '../domain/archive_models.dart';
import 'archive_bundle_detail_page.dart';

/// حزم العشرات لموظف المبيعات. كل حزمة كارت فيه «تم استلام x من y»،
/// وزر الإرسال مقفل حتى تنتهي عشرتها.
class ArchiveBundlesPage extends StatefulWidget {
  const ArchiveBundlesPage({super.key, this.stage = ArchiveStage.follower});

  final int stage;

  @override
  State<ArchiveBundlesPage> createState() => _ArchiveBundlesPageState();
}

class _ArchiveBundlesPageState extends State<ArchiveBundlesPage> {
  late final ArchiveRemote _remote = ArchiveRemote(Injection.dio);

  List<ArchiveBundle> _items = const [];
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
      final items = await _remote.bundles(stage: widget.stage);
      if (!mounted) return;
      setState(() {
        _items = items;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _error = e is DioException
            ? DioClient.mapError(e)
            : 'تعذر تحميل حزم الأرشيف';
      });
    }
  }

  Future<void> _open(ArchiveBundle bundle) async {
    await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => ArchiveBundleDetailPage(
          bundleId: bundle.id,
          stage: widget.stage,
          title: bundle.periodLabel,
        ),
      ),
    );
    if (mounted) await _load();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('الأرشيف — الوارد'),
        actions: [
          IconButton(
            onPressed: _loading ? null : _load,
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _load,
        child: _buildBody(),
      ),
    );
  }

  Widget _buildBody() {
    if (_loading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_error != null) {
      return ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(24),
        children: [
          Text(_error!, textAlign: TextAlign.center),
          const SizedBox(height: 12),
          Center(
            child: OutlinedButton(
              onPressed: _load,
              child: const Text('إعادة المحاولة'),
            ),
          ),
        ],
      );
    }

    if (_items.isEmpty) {
      return ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(24),
        children: const [
          Text('لا حزم بعد', textAlign: TextAlign.center),
        ],
      );
    }

    return ListView.separated(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(12),
      itemCount: _items.length,
      separatorBuilder: (_, _) => const SizedBox(height: 10),
      itemBuilder: (_, i) => _card(_items[i]),
    );
  }

  Widget _card(ArchiveBundle b) {
    final isSource = widget.stage == ArchiveStage.salesEmployee;
    final received = isSource ? b.receivedOnwardCount : b.receivedByMeCount;
    final countLabel = isSource ? 'تم استلام' : 'استلمتُ';

    return Card(
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(14),
        side: BorderSide(
          color: b.isPeriodClosed ? AppColors.border : AppColors.primary,
          style: b.isPeriodClosed ? BorderStyle.solid : BorderStyle.none,
        ),
      ),
      child: InkWell(
        borderRadius: BorderRadius.circular(14),
        onTap: () => _open(b),
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text(
                      b.periodLabel,
                      style: const TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 15,
                      ),
                    ),
                  ),
                  if (!b.isPeriodClosed)
                    _chip('العشرة جارية', AppColors.primary)
                  else if (b.openReturnCount > 0)
                    _chip('${b.openReturnCount} مسترجَعة', AppColors.danger),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                '$countLabel $received من ${b.itemCount}',
                style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w700),
              ),
              const SizedBox(height: 6),
              Row(
                children: [
                  Expanded(
                    child: Text(
                      b.pendingSendCount > 0
                          ? '${b.pendingSendCount} بعهدتك'
                          : 'لا شيء بعهدتك',
                      style: TextStyle(
                        fontSize: 13,
                        color: b.pendingSendCount > 0
                            ? AppColors.primary
                            : AppColors.muted,
                      ),
                    ),
                  ),
                  if (!b.isPeriodClosed)
                    Text(
                      'تنتهي ${b.periodEnd}',
                      style: TextStyle(fontSize: 12, color: AppColors.muted),
                    ),
                  const Icon(Icons.chevron_left),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _chip(String text, Color color) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 2),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.12),
          borderRadius: BorderRadius.circular(999),
        ),
        child: Text(
          text,
          style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.w700),
        ),
      );
}
