import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';

/// فيديوهات تعليمية لجمهور follower (= مسؤول اللجنة / Motaba).
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
    final res = await Injection.dio.dio.get(
      'api/v1/app/TutorialVideos/follower',
    );
    final data = res.data;
    final List raw = data is List
        ? data
        : (data is Map && data['data'] is List
              ? data['data'] as List
              : const []);
    return raw
        .map((e) {
          final m = Map<String, dynamic>.from(e as Map);
          return _Video(
            question: '${m['questionAr'] ?? m['QuestionAr'] ?? ''}',
            url: '${m['videoUrl'] ?? m['VideoUrl'] ?? ''}',
          );
        })
        .where((v) => v.question.isNotEmpty && v.url.isNotEmpty)
        .toList();
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
        appBar: AppBar(title: const Text('الفيديوهات التعليمية')),
        body: FutureBuilder<List<_Video>>(
          future: _future,
          builder: (context, snap) {
            if (snap.connectionState != ConnectionState.done) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snap.hasError) {
              final err = snap.error;
              final code = err is DioException
                  ? err.response?.statusCode
                  : null;
              final msg = code == 403 || code == 401
                  ? 'لا صلاحية لعرض الفيديوهات.'
                  : DioClient.mapError(
                      err ?? StateError('تعذر تحميل الفيديوهات'),
                    );
              return Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(msg, textAlign: TextAlign.center),
                    const SizedBox(height: 12),
                    FilledButton(
                      onPressed: () => setState(() => _future = _load()),
                      child: const Text('إعادة المحاولة'),
                    ),
                  ],
                ),
              );
            }
            final items = snap.data ?? const [];
            if (items.isEmpty) {
              return const Center(
                child: Text('لا فيديوهات تعليمية مفعّلة حالياً'),
              );
            }
            return ListView.separated(
              padding: EdgeInsets.fromLTRB(
                16,
                16,
                16,
                24 + MediaQuery.paddingOf(context).bottom,
              ),
              itemCount: items.length,
              separatorBuilder: (_, __) => const SizedBox(height: 8),
              itemBuilder: (context, i) {
                final v = items[i];
                return ListTile(
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                    side: BorderSide(
                      color: AppColors.primary.withValues(alpha: 0.2),
                    ),
                  ),
                  title: Text(v.question),
                  trailing: const Icon(
                    Icons.play_circle_outline,
                    color: AppColors.primary,
                  ),
                  onTap: () => _open(v.url),
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
