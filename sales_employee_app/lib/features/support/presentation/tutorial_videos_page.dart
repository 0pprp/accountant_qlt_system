import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/injection.dart';
import 'package:url_launcher/url_launcher.dart';

/// فيديوهات جمهور sales-employee — عقد TutorialAppKey.SalesEmployee الموسّع.
class TutorialVideosPage extends StatefulWidget {
  const TutorialVideosPage({super.key});

  @override
  State<TutorialVideosPage> createState() => _TutorialVideosPageState();
}

class _TutorialVideosPageState extends State<TutorialVideosPage> {
  late Future<List<_Video>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<_Video>> _load() async {
    final res = await Injection.dioClient.dio.get(
      'api/v1/app/TutorialVideos/sales-employee',
    );
    final data = res.data;
    final List raw = data is List
        ? data
        : (data is Map && data['data'] is List ? data['data'] as List : const []);
    return raw.map((e) {
      final m = Map<String, dynamic>.from(e as Map);
      return _Video(
        question: '${m['questionAr'] ?? m['QuestionAr'] ?? ''}',
        url: '${m['videoUrl'] ?? m['VideoUrl'] ?? ''}',
      );
    }).where((v) => v.question.isNotEmpty && v.url.isNotEmpty).toList();
  }

  Future<void> _open(String url) async {
    final uri = Uri.tryParse(url);
    if (uri == null) return;
    await launchUrl(uri, mode: LaunchMode.externalApplication);
  }

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(title: const Text('الفيديوهات التعليمية')),
        body: FutureBuilder<List<_Video>>(
          future: _future,
          builder: (context, snap) {
            if (snap.connectionState != ConnectionState.done) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snap.hasError) {
              final err = snap.error;
              final code = err is DioException ? err.response?.statusCode : null;
              final msg = code == 403 || code == 401
                  ? 'لا صلاحية لعرض الفيديوهات.'
                  : 'تعذر تحميل الفيديوهات.';
              return Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(msg),
                    const SizedBox(height: 12),
                    FilledButton(
                      style: FilledButton.styleFrom(minimumSize: const Size(44, 44)),
                      onPressed: () => setState(() => _future = _load()),
                      child: const Text('إعادة المحاولة'),
                    ),
                  ],
                ),
              );
            }
            final items = snap.data ?? const [];
            if (items.isEmpty) {
              return const Center(child: Text('لا فيديوهات مفعّلة لجمهور موظف المبيعات حالياً'));
            }
            return ListView.separated(
              padding: const EdgeInsets.all(16),
              itemCount: items.length,
              separatorBuilder: (_, __) => const SizedBox(height: 12),
              itemBuilder: (_, i) {
                final v = items[i];
                return Material(
                  color: AppColors.card,
                  borderRadius: BorderRadius.circular(16),
                  child: ListTile(
                    contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                    title: Text(
                      v.question,
                      style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600, height: 1.5),
                    ),
                    trailing: const Icon(Icons.play_circle_outline, color: AppColors.primary),
                    onTap: () => _open(v.url),
                  ),
                );
              },
            );
          },
        ),
      ),
    );
  }
}

class _Video {
  const _Video({required this.question, required this.url});
  final String question;
  final String url;
}
