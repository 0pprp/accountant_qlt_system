import 'package:dio/dio.dart';

import '../../../core/network/dio_client.dart';
import '../domain/entities/delegate_home_snapshot.dart';
import '../domain/entities/home_announcement.dart';

class AnnouncementsLoadResult {
  AnnouncementsLoadResult.ok(this.items)
      : state = items.isEmpty
            ? AnnouncementsLoadState.successEmpty
            : AnnouncementsLoadState.successData,
        errorMessage = null;

  const AnnouncementsLoadResult.forbidden([this.errorMessage])
      : items = const [],
        state = AnnouncementsLoadState.forbidden;

  const AnnouncementsLoadResult.error([this.errorMessage])
      : items = const [],
        state = AnnouncementsLoadState.error;

  final List<HomeAnnouncement> items;
  final AnnouncementsLoadState state;
  final String? errorMessage;
}

class AnnouncementsRemoteDataSource {
  AnnouncementsRemoteDataSource(this._dio);
  final DioClient _dio;

  Future<AnnouncementsLoadResult> listForDelegate() async {
    try {
      final res = await _dio.dio.get('api/v1/app/Announcements/delegate');
      final data = res.data;
      final List raw;
      if (data is List) {
        raw = data;
      } else if (data is Map && data['data'] is List) {
        raw = data['data'] as List;
      } else {
        raw = const [];
      }

      final items = raw.map((e) {
        final m = Map<String, dynamic>.from(e as Map);
        final id = '${m['id'] ?? ''}';
        final body = (m['textBody'] ?? m['TextBody'] ?? '').toString();
        final image = (m['bannerUrl'] ?? m['BannerUrl'])?.toString();
        final link = (m['linkUrl'] ?? m['LinkUrl'])?.toString();
        final type = (m['contentType'] ?? m['ContentType'] ?? 'text').toString();
        return HomeAnnouncement(
          id: id,
          title: type == 'image'
              ? 'إعلان'
              : (body.length > 40 ? '${body.substring(0, 40)}…' : body),
          body: type == 'text' ? body : null,
          imageUrl: type == 'image' ? image : null,
          linkUrl: link,
        );
      }).toList();
      return AnnouncementsLoadResult.ok(items);
    } on DioException catch (e) {
      final code = e.response?.statusCode;
      if (code == 401 || code == 403) {
        return const AnnouncementsLoadResult.forbidden(
          'لا صلاحية لعرض الإعلانات.',
        );
      }
      return AnnouncementsLoadResult.error(
        'تعذر تحميل الإعلانات (${code ?? 'شبكة'}).',
      );
    } catch (_) {
      return const AnnouncementsLoadResult.error('تعذر تحميل الإعلانات.');
    }
  }
}
