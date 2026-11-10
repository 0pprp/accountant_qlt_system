import 'dart:async';
import 'dart:typed_data';

import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/session/session_store.dart';
import 'package:committee_officer_application/features/auth/data/auth_remote.dart';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';

class MemorySession extends SessionStore {
  String? token = 'old';
  bool cleared = false;
  @override
  Future<String?> getAccessToken() async => token;
  @override
  Future<void> clear() async {
    cleared = true;
    token = null;
  }

  @override
  Future<void> saveSession({
    required String accessToken,
    required String refreshToken,
    required int userId,
    required String userName,
    required String fullName,
  }) async {
    token = accessToken;
  }
}

class Adapter implements HttpClientAdapter {
  Adapter(this.respond);
  final FutureOr<ResponseBody> Function(RequestOptions) respond;
  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async => respond(options);
  @override
  void close({bool force = false}) {}
}

ResponseBody json(String body, int status) => ResponseBody.fromString(
  body,
  status,
  headers: {
    Headers.contentTypeHeader: ['application/json'],
  },
);

void main() {
  test(
    'concurrent unauthorized requests share one refresh and retry once',
    () async {
      final session = MemorySession();
      final gate = Completer<bool>();
      var refreshes = 0;
      final client = DioClient(
        sessionStore: session,
        onRefresh: () {
          refreshes++;
          return gate.future;
        },
      );
      var initial = 0;
      final bothStarted = Completer<void>();
      client.dio.httpClientAdapter = Adapter((request) {
        if (request.headers['Authorization'] == 'Bearer old') {
          if (++initial == 2) bothStarted.complete();
          return json('{}', 401);
        }
        return json('{}', 200);
      });
      final results = Future.wait([
        client.dio.get('first'),
        client.dio.get('second'),
      ]);
      await bothStarted.future;
      await Future<void>.delayed(const Duration(milliseconds: 10));
      session.token = 'new';
      gate.complete(true);
      expect((await results).map((r) => r.statusCode), [200, 200]);
      expect(refreshes, 1);
    },
  );

  test(
    'login 401 never refreshes and foreign destinations never reach transport',
    () async {
      var refreshes = 0;
      var requests = 0;
      final client = DioClient(
        sessionStore: MemorySession(),
        onRefresh: () async {
          refreshes++;
          return false;
        },
      );
      client.dio.httpClientAdapter = Adapter((request) {
        requests++;
        expect(request.headers.containsKey('Authorization'), isFalse);
        return json('{}', 401);
      });
      await expectLater(
        client.dio.post('api/v1/app/Auth/login'),
        throwsA(isA<DioException>()),
      );
      await expectLater(
        client.dio.get('https://untrusted.example/resource'),
        throwsA(isA<DioException>()),
      );
      expect(refreshes, 0);
      expect(requests, 1);
    },
  );

  test('missing role confirmation clears a newly created session', () async {
    final session = MemorySession();
    final client = DioClient(
      sessionStore: session,
      onRefresh: () async => false,
    );
    client.dio.httpClientAdapter = Adapter(
      (request) => request.path.endsWith('/login')
          ? json('{"accessToken":"new"}', 200)
          : json('{}', 200),
    );
    await expectLater(
      AuthRemote(client, session).login(userName: 'user', password: 'password'),
      throwsException,
    );
    expect(session.cleared, isTrue);
  });
}
