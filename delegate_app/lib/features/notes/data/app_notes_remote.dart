import '../../../core/network/dio_client.dart';

class AppNotesRemote {
  AppNotesRemote(this._dio);
  final DioClient _dio;

  Future<void> createManagerNote(String body, {String? subject}) async {
    await _dio.dio.post(
      'api/v1/app/AppNotes',
      data: {
        'body': body,
        if (subject != null && subject.trim().isNotEmpty) 'subject': subject.trim(),
      },
    );
  }
}
