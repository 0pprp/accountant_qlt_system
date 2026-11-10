import 'package:committee_officer_application/injection.dart';
import 'package:dio/dio.dart';

enum AnnouncementsLoadState {
  loading,
  successEmpty,
  successData,
  forbidden,
  error,
}

class AnnouncementItem {
  const AnnouncementItem({
    required this.id,
    required this.contentType,
    required this.textBody,
    this.bannerUrl,
    this.linkUrl,
  });

  final int id;
  final String contentType;
  final String textBody;
  final String? bannerUrl;
  final String? linkUrl;
}

class AnnouncementsResult {
  const AnnouncementsResult({required this.state, this.items = const []});

  final AnnouncementsLoadState state;
  final List<AnnouncementItem> items;
}

class AnnouncementsRemote {
  Future<AnnouncementsResult> listFollower() async {
    try {
      final res = await Injection.dio.dio.get('api/v1/app/Announcements/follower');
      final data = res.data;
      final List raw = data is List
          ? data
          : (data is Map && data['data'] is List ? data['data'] as List : const []);
      final items = raw.map((e) {
        final m = Map<String, dynamic>.from(e as Map);
        return AnnouncementItem(
          id: (m['id'] ?? m['Id'] ?? 0) as int,
          contentType: '${m['contentType'] ?? m['ContentType'] ?? 'text'}',
          textBody: '${m['textBody'] ?? m['TextBody'] ?? ''}',
          bannerUrl: (m['bannerUrl'] ?? m['BannerUrl'])?.toString(),
          linkUrl: (m['linkUrl'] ?? m['LinkUrl'])?.toString(),
        );
      }).toList();
      return AnnouncementsResult(
        state: items.isEmpty
            ? AnnouncementsLoadState.successEmpty
            : AnnouncementsLoadState.successData,
        items: items,
      );
    } on DioException catch (e) {
      final code = e.response?.statusCode;
      if (code == 401 || code == 403) {
        return const AnnouncementsResult(state: AnnouncementsLoadState.forbidden);
      }
      return const AnnouncementsResult(state: AnnouncementsLoadState.error);
    } catch (_) {
      return const AnnouncementsResult(state: AnnouncementsLoadState.error);
    }
  }
}
