import 'dart:async';

import 'package:sales_employee_app/core/config/app_env.dart';
import 'package:signalr_netcore/signalr_client.dart';

typedef SalesRequestChangedHandler = void Function(Map<String, dynamic> event);

/// عميل SignalR معزول لتطبيق الموظف — dedup + رفض revision الأقدم.
class SalesRequestRealtimeClient {
  HubConnection? _hub;
  final Set<String> _seen = {};
  final Map<int, int> _rev = {};
  SalesRequestChangedHandler? onChanged;
  void Function(String state)? onConnectionState;
  Timer? _poll;
  String? _token;
  int _sessionGen = 0;

  bool get isConnected =>
      _hub?.state == HubConnectionState.Connected;

  Future<void> start({
    required String accessToken,
    required int sessionGeneration,
    required SalesRequestChangedHandler handler,
  }) async {
    onChanged = handler;
    if (_sessionGen != 0 && _sessionGen != sessionGeneration) {
      await stop();
    }
    _sessionGen = sessionGeneration;
    _token = accessToken;

    await stop();
    final base = AppEnv.apiBaseUrl.replaceAll(RegExp(r'/api(/v\d+)?/?$'), '');
    final url = '$base/hubs/sales-requests';

    final httpOptions = HttpConnectionOptions(
      accessTokenFactory: () async => _token ?? '',
    );

    _hub = HubConnectionBuilder()
        .withUrl(url, options: httpOptions)
        .withAutomaticReconnect(retryDelays: [0, 1000, 2000, 5000, 10000])
        .build();

    _hub!.on('SalesRequestChanged', (args) {
      if (args == null || args.isEmpty) return;
      final raw = args.first;
      if (raw is! Map) return;
      _handle(Map<String, dynamic>.from(raw));
    });

    _hub!.onreconnecting(({error}) {
      onConnectionState?.call('reconnecting');
      _startPoll();
    });
    _hub!.onreconnected(({connectionId}) {
      onConnectionState?.call('connected');
      _stopPoll();
      _hub?.invoke('RefreshSubscriptions');
    });
    _hub!.onclose(({error}) {
      onConnectionState?.call('disconnected');
      _startPoll();
    });

    try {
      await _hub!.start();
      if (_hub?.state == HubConnectionState.Connected) {
        onConnectionState?.call('connected');
        _stopPoll();
      } else {
        onConnectionState?.call('disconnected');
        _startPoll();
      }
    } catch (_) {
      onConnectionState?.call('disconnected');
      _startPoll();
    }
  }

  void _handle(Map<String, dynamic> raw) {
    final eventId = '${raw['eventId'] ?? raw['EventId'] ?? ''}';
    final requestId = int.tryParse('${raw['requestId'] ?? raw['RequestId'] ?? ''}');
    final revision = int.tryParse('${raw['revision'] ?? raw['Revision'] ?? '0'}') ?? 0;
    if (eventId.isEmpty || requestId == null) return;
    if (_seen.contains(eventId)) return;
    _seen.add(eventId);
    if (_seen.length > 400) _seen.remove(_seen.first);
    final prev = _rev[requestId] ?? -1;
    if (revision < prev) return;
    _rev[requestId] = revision;
    onChanged?.call(raw);
  }

  void _startPoll() {
    _poll ??= Timer.periodic(const Duration(seconds: 8), (_) {
      // الصفحة النشطة فقط — المستمع يقرر إعادة الجلب
      onChanged?.call({'changeType': 'PollFallback', 'requestId': 0, 'eventId': 'poll'});
    });
  }

  void _stopPoll() {
    _poll?.cancel();
    _poll = null;
  }

  Future<void> stop() async {
    _stopPoll();
    final h = _hub;
    _hub = null;
    if (h != null) {
      try {
        await h.stop();
      } catch (_) {}
    }
    onConnectionState?.call('disconnected');
  }
}
