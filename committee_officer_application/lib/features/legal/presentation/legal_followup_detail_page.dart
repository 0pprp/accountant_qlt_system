import 'package:committee_officer_application/features/legal/data/legal_followups_api.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:geolocator/geolocator.dart';
import 'package:image_picker/image_picker.dart';

class LegalFollowupDetailPage extends StatefulWidget {
  const LegalFollowupDetailPage({super.key, required this.caseId});
  final int caseId;

  @override
  State<LegalFollowupDetailPage> createState() => _LegalFollowupDetailPageState();
}

class _LegalFollowupDetailPageState extends State<LegalFollowupDetailPage> {
  late final _api = LegalFollowupsApi(Injection.dio.dio);
  Map<String, dynamic>? _item;
  bool _loading = true;
  bool _busy = false;
  String? _error;

  static const _required = [
    ('NationalIdFront', 'البطاقة الوطنية - أمامية'),
    ('NationalIdBack', 'البطاقة الوطنية - خلفية'),
    ('ResidenceCardFront', 'بطاقة السكن - أمامية'),
    ('ResidenceCardBack', 'بطاقة السكن - خلفية'),
  ];

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
      final item = await _api.getById(widget.caseId);
      if (!mounted) return;
      setState(() => _item = item);
    } catch (_) {
      if (mounted) setState(() => _error = 'تعذر تحميل التفاصيل.');
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Set<String> get _docTypes {
    final docs = (_item?['documents'] as List?) ?? const [];
    return docs.map((e) => '${(e as Map)['documentType']}').toSet();
  }

  Future<void> _setLocation() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        await Geolocator.requestPermission();
      }
      final pos = await Geolocator.getCurrentPosition();
      final version = (_item?['rowVersion'] as num?)?.toInt();
      final updated = await _api.setLocation(
        widget.caseId,
        lat: pos.latitude,
        lng: pos.longitude,
        version: version,
      );
      if (!mounted) return;
      setState(() => _item = {...?_item, ...updated});
    } catch (_) {
      if (mounted) setState(() => _error = 'تعذر حفظ موقع المحل.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _upload(String type) async {
    final picker = ImagePicker();
    final shot = await picker.pickImage(source: ImageSource.camera, imageQuality: 85);
    if (shot == null) return;
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final bytes = await shot.readAsBytes();
      await _api.uploadDocument(
        widget.caseId,
        type: type,
        file: MultipartFile.fromBytes(bytes, filename: '${type}_${shot.name}'),
      );
      await _load();
    } catch (_) {
      if (mounted) setState(() => _error = 'تعذر رفع المستمسك.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _complete() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final version = (_item?['rowVersion'] as num?)?.toInt();
      await _api.complete(widget.caseId, version: version);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('تم إكمال المتابعة')));
      Navigator.of(context).pop(true);
    } on DioException catch (e) {
      final msg = e.response?.data is Map ? '${(e.response!.data as Map)['errors'] ?? e.message}' : 'تعذر الإكمال';
      if (mounted) setState(() => _error = msg);
    } catch (_) {
      if (mounted) setState(() => _error = 'تعذر الإكمال. تحقق من الموقع والمستمسكات.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    final inProgress = _item?['status'] == 'InProgress';
    final hasLoc = _item?['shopLatitude'] != null && _item?['shopLongitude'] != null;
    return Scaffold(
      appBar: AppBar(title: Text('قضية #${widget.caseId}')),
      body: SafeArea(
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : ListView(
                padding: EdgeInsets.fromLTRB(16, 16, 16, 24 + bottom),
                children: [
                  Text('${_item?['customerName'] ?? ''}', style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w800)),
                  Text('${_item?['customerPhone'] ?? ''}\n${_item?['customerAddress'] ?? ''}', style: const TextStyle(height: 1.5)),
                  const SizedBox(height: 12),
                  Text(hasLoc ? 'موقع المحل محفوظ' : 'يجب تحديد موقع عنوان المحل', style: TextStyle(color: hasLoc ? Colors.green.shade800 : Colors.orange.shade800)),
                  if (inProgress) ...[
                    const SizedBox(height: 8),
                    FilledButton(onPressed: _busy ? null : _setLocation, child: const Text('تثبيت موقع المحل')),
                    const SizedBox(height: 16),
                    const Text('المستمسكات المطلوبة', style: TextStyle(fontWeight: FontWeight.w700)),
                    ..._required.map((e) {
                      final ok = _docTypes.contains(e.$1);
                      return ListTile(
                        contentPadding: EdgeInsets.zero,
                        title: Text(e.$2),
                        trailing: ok
                            ? const Icon(Icons.check_circle, color: Colors.green)
                            : TextButton(onPressed: _busy ? null : () => _upload(e.$1), child: const Text('رفع')),
                      );
                    }),
                    const SizedBox(height: 12),
                    FilledButton(
                      onPressed: _busy ? null : _complete,
                      style: FilledButton.styleFrom(minimumSize: const Size.fromHeight(48), backgroundColor: const Color(0xFF006A64)),
                      child: const Text('إكمال المتابعة'),
                    ),
                  ],
                  if (_error != null) Padding(padding: const EdgeInsets.only(top: 12), child: Text(_error!, style: const TextStyle(color: Colors.red, height: 1.5))),
                ],
              ),
      ),
    );
  }
}
