import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/app_dialog.dart';
import '../../../injection.dart';
import '../data/management_tasks_remote.dart';
import '../domain/management_task.dart';

class MyTasksPage extends StatefulWidget {
  const MyTasksPage({super.key});

  @override
  State<MyTasksPage> createState() => _MyTasksPageState();
}

class _MyTasksPageState extends State<MyTasksPage> {
  late final ManagementTasksRemote _remote =
      ManagementTasksRemote(Injection.dioClient);

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
      final r = await _remote.accept(task.id, task.expectedVersion);
      if (!mounted) return;
      await _load();
      if (!mounted) return;
      await AppDialog.message(
        context,
        title: 'تم',
        body: 'تم قبول المهمة (حالة: ${ManagementTaskStatus.label(r.status)}).',
      );
    } catch (e) {
      if (!mounted) return;
      await AppDialog.message(context, title: 'فشل', body: _errMsg(e, 'تعذر القبول'));
    } finally {
      if (mounted) setState(() => _busyId = null);
    }
  }

  Future<void> _complete(ManagementTaskItem task) async {
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('تأكيد الإنجاز'),
        content: Text('هل أنجزت المهمة «${task.title}»؟'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('إلغاء')),
          FilledButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('إنجاز')),
        ],
      ),
    );
    if (ok != true || !mounted) return;
    setState(() => _busyId = task.id);
    try {
      await _remote.complete(task.id, task.expectedVersion);
      if (!mounted) return;
      await _load();
    } catch (e) {
      if (!mounted) return;
      await AppDialog.message(context, title: 'فشل', body: _errMsg(e, 'تعذر الإنجاز'));
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
      await AppDialog.message(context, title: 'تنبيه', body: 'سبب الرفض مطلوب');
      return;
    }
    setState(() => _busyId = task.id);
    try {
      await _remote.reject(task.id, task.expectedVersion, note);
      if (!mounted) return;
      await _load();
    } catch (e) {
      if (!mounted) return;
      await AppDialog.message(context, title: 'فشل', body: _errMsg(e, 'تعذر الرفض'));
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
          Icon(Icons.error_outline, size: 48, color: AppColors.danger),
          const SizedBox(height: 12),
          Text(_error!, textAlign: TextAlign.center, style: AppTextStyles.body),
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
        children: [
          const SizedBox(height: 80),
          Icon(Icons.task_alt_outlined, size: 48, color: AppColors.muted),
          const SizedBox(height: 12),
          Text(
            'لا توجد مهام حالياً',
            textAlign: TextAlign.center,
            style: AppTextStyles.muted,
          ),
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
                      child: Text(task.title, style: AppTextStyles.title),
                    ),
                    if (task.isOverdue)
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                        decoration: BoxDecoration(
                          color: AppColors.danger.withValues(alpha: 0.12),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(
                          'متأخرة',
                          style: AppTextStyles.muted.copyWith(color: AppColors.danger),
                        ),
                      ),
                  ],
                ),
                const SizedBox(height: 6),
                Text(
                  '${ManagementTaskStatus.label(task.status)} · أولوية ${ManagementTaskPriority.label(task.priority)}',
                  style: AppTextStyles.muted,
                ),
                if (task.dueDate.isNotEmpty) ...[
                  const SizedBox(height: 4),
                  Text('الاستحقاق: ${task.dueDate}', style: AppTextStyles.muted),
                ],
                if (task.description.isNotEmpty) ...[
                  const SizedBox(height: 8),
                  Text(task.description, style: AppTextStyles.body),
                ],
                if (task.rejectNote != null) ...[
                  const SizedBox(height: 8),
                  Text(
                    'سبب الرفض: ${task.rejectNote}',
                    style: AppTextStyles.muted.copyWith(color: AppColors.danger),
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
