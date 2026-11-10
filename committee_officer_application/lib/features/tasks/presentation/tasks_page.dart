import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

/// مهام مسؤول اللجنة — نشرة مفعّلة من لوحة الإدارة عبر API الموحّد.
class TasksPage extends StatefulWidget {
  const TasksPage({super.key});

  @override
  State<TasksPage> createState() => _TasksPageState();
}

class _TasksPageState extends State<TasksPage> {
  List<Map<String, dynamic>> _committee = const [];
  bool _loading = true;
  String? _hint;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _hint = null;
    });
    try {
      final rows = await Injection.committee.tasks();
      if (!mounted) return;
      setState(() {
        _committee = rows;
        _loading = false;
        if (rows.isEmpty) {
          _hint = 'لا مهام مفعّلة في النشرة حاليًا.';
        }
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _committee = const [];
        _loading = false;
        _hint = 'تعذر جلب مهام اللجنة من الخادم.';
      });
    }
  }

  void _openBody(Map<String, dynamic> m) {
    final body = '${m['body'] ?? m['Body'] ?? ''}'.trim();
    showAppMessage(
      context,
      title: 'تفاصيل المهمة',
      message: body.isEmpty ? 'لا نص.' : body,
    );
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      appBar: AppBar(title: const Text('مهام مسؤول اللجنة')),
      body: AnimatedWavesBackground(
        child: RefreshIndicator(
          onRefresh: _load,
          child: _loading
              ? ListView(children: const [
                  SizedBox(height: 160),
                  Center(child: CircularProgressIndicator()),
                ])
              : _committee.isEmpty
                  ? ListView(
                      children: [
                        const SizedBox(height: 120),
                        const Center(child: Text('لا مهام معروضة حالياً')),
                        if (_hint != null) ...[
                          const SizedBox(height: 12),
                          Padding(
                            padding: const EdgeInsets.symmetric(horizontal: 24),
                            child: Text(
                              _hint!,
                              textAlign: TextAlign.center,
                              style: const TextStyle(
                                color: AppColors.textSecondary,
                                height: 1.4,
                              ),
                            ),
                          ),
                        ],
                      ],
                    )
                  : ListView.separated(
                      padding: EdgeInsets.fromLTRB(16, 12, 16, 24 + bottom),
                      itemCount: _committee.length,
                      separatorBuilder: (_, __) => const SizedBox(height: 8),
                      itemBuilder: (_, i) {
                        final m = _committee[i];
                        final body =
                            '${m['body'] ?? m['Body'] ?? ''}'.trim();
                        final preview = body.length <= 90
                            ? body
                            : '${body.substring(0, 90)}…';
                        return Material(
                          color: Colors.white.withValues(alpha: 0.92),
                          borderRadius: BorderRadius.circular(14),
                          child: ListTile(
                            title: Text(
                              preview.isEmpty ? 'مهمة' : preview,
                              style:
                                  const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            subtitle: const Text('اضغط لعرض النص كاملًا'),
                            onTap: () => _openBody(m),
                          ),
                        );
                      },
                    ),
        ),
      ),
    );
  }
}
