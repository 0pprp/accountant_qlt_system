import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

import '../data/management_tasks_remote.dart';
import '../domain/management_task.dart';

/// مهامي — مهام شخصية من الإدارة (ManagementTasks)، منفصلة عن نشرة مهام اللجنة.
class MyTasksPage extends StatefulWidget {
  const MyTasksPage({super.key});

  @override
  State<MyTasksPage> createState() => _MyTasksPageState();
}

class _MyTasksPageState extends State<MyTasksPage> {
  late final ManagementTasksRemote _remote =
      ManagementTasksRemote(Injection.dio);

  List<ManagementTaskItem> _items = const [];
  bool _loading = true;
  String? _error;
  int? _busyId;

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
      final items = await _remote.inbox();
      if (!mounted) return;
      setState(() {
        _items = items;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _error = _errMsg(e, 'تعذر تحميل المهام');
      });
    }
  }

  String _errMsg(Object e, String fallback) {
    if (e is DioException && e.response?.data is Map) {
      final m = e.response!.data as Map;
      return '${m['message'] ?? m['title'] ?? fallback}';
    }
    return fallback;
  }

  Future<void> _accept(ManagementTaskItem task) async {
    setState(() => _busyId = task.id);
    try {
      await _remote.accept(task.id, task.expectedVersion);
      if (!mounted) return;
      await _load();
      if (!mounted) return;
      await showAppMessage(context, title: 'تم', message: 'تم قبول المهمة');
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'فشل',
        message: _errMsg(e, 'تعذر القبول'),
        color: AppColors.danger,
      );
    } finally {
      if (mounted) setState(() => _busyId = null);
    }
  }

  Future<void> _complete(ManagementTaskItem task) async {
    final ok = await showConfirmDialog(
      context,
      title: 'تأكيد الإنجاز',
      message: 'هل أنجزت المهمة «${task.title}»؟',
    );
    if (!ok || !mounted) return;
    setState(() => _busyId = task.id);
    try {
      await _remote.complete(task.id, task.expectedVersion);
      if (!mounted) return;
      await _load();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'فشل',
        message: _errMsg(e, 'تعذر الإنجاز'),
        color: AppColors.danger,
      );
    } finally {
      if (mounted) setState(() => _busyId = null);
    }
  }

  Future<void> _reject(ManagementTaskItem task) async {
    final noteCtrl = TextEditingController();
    final note = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('رفض المهمة'),
        content: TextField(
          controller: noteCtrl,
          maxLines: 3,
          decoration: const InputDecoration(
            hintText: 'سبب الرفض مطلوب',
            border: OutlineInputBorder(),
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('إلغاء')),
          FilledButton(
            onPressed: () => Navigator.pop(ctx, noteCtrl.text.trim()),
            child: const Text('رفض'),
          ),
        ],
      ),
    );
    noteCtrl.dispose();
    if (note == null || !mounted) return;
    if (note.isEmpty) {
      await showAppMessage(context, title: 'تنبيه', message: 'سبب الرفض مطلوب');
      return;
    }
    setState(() => _busyId = task.id);
    try {
      await _remote.reject(task.id, task.expectedVersion, note);
      if (!mounted) return;
      await _load();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'فشل',
        message: _errMsg(e, 'تعذر الرفض'),
        color: AppColors.danger,
      );
    } finally {
      if (mounted) setState(() => _busyId = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('مهامي'),
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
    if (_loading && _items.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_error != null && _items.isEmpty) {
      return ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        children: [
          const SizedBox(height: 80),
          const Icon(Icons.error_outline, size: 48, color: AppColors.danger),
          const SizedBox(height: 12),
          Text(_error!, textAlign: TextAlign.center),
          const SizedBox(height: 16),
          Center(
            child: FilledButton(onPressed: _load, child: const Text('إعادة المحاولة')),
          ),
        ],
      );
    }
    if (_items.isEmpty) {
      return ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        children: const [
          SizedBox(height: 80),
          Icon(Icons.task_alt_outlined, size: 48, color: AppColors.muted),
          SizedBox(height: 12),
          Text('لا توجد مهام شخصية حالياً', textAlign: TextAlign.center),
        ],
      );
    }
    return ListView.separated(
      padding: const EdgeInsets.all(16),
      itemCount: _items.length,
      separatorBuilder: (_, _) => const SizedBox(height: 12),
      itemBuilder: (context, i) {
        final task = _items[i];
        final busy = _busyId == task.id;
        return Material(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        task.title,
                        style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16),
                      ),
                    ),
                    if (task.isOverdue)
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                        decoration: BoxDecoration(
                          color: AppColors.danger.withValues(alpha: 0.12),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: const Text('متأخرة', style: TextStyle(color: AppColors.danger)),
                      ),
                  ],
                ),
                const SizedBox(height: 6),
                Text(
                  '${ManagementTaskStatus.label(task.status)} · أولوية ${ManagementTaskPriority.label(task.priority)}',
                  style: const TextStyle(color: AppColors.muted),
                ),
                if (task.dueDate.isNotEmpty) ...[
                  const SizedBox(height: 4),
                  Text('الاستحقاق: ${task.dueDate}', style: const TextStyle(color: AppColors.muted)),
                ],
                if (task.description.isNotEmpty) ...[
                  const SizedBox(height: 8),
                  Text(task.description),
                ],
                if (task.rejectNote != null) ...[
                  const SizedBox(height: 8),
                  Text(
                    'سبب الرفض: ${task.rejectNote}',
                    style: const TextStyle(color: AppColors.danger),
                  ),
                ],
                const SizedBox(height: 12),
                if (busy)
                  const Center(child: CircularProgressIndicator())
                else
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: [
                      if (task.canAccept)
                        FilledButton(
                          onPressed: () => _accept(task),
                          child: const Text('قبول'),
                        ),
                      if (task.canComplete)
                        FilledButton.tonal(
                          onPressed: () => _complete(task),
                          child: const Text('إنجاز'),
                        ),
                      if (task.canReject)
                        OutlinedButton(
                          onPressed: () => _reject(task),
                          child: const Text('رفض'),
                        ),
                    ],
                  ),
              ],
            ),
          ),
        );
      },
    );
  }
}
