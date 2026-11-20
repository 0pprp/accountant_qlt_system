import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../core/formatting/iraq_time.dart';
import '../../core/network/api_errors.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/feedback.dart';
import '../../core/widgets/map_parts.dart';
import '../../core/widgets/neu.dart';
import '../../injection.dart';
import '../promoter/models.dart';
import 'pin_form_sheet.dart';

class PinDetailPage extends StatefulWidget {
  const PinDetailPage({super.key, required this.pinId, this.initial});
  final int pinId;
  final ShopPin? initial;

  @override
  State<PinDetailPage> createState() => _PinDetailPageState();
}

class _PinDetailPageState extends State<PinDetailPage> {
  ShopPin? _pin;
  String? _error;
  bool _loading = false;
  bool _deleting = false;
  bool _changed = false;

  @override
  void initState() {
    super.initState();
    _pin = widget.initial;
    unawaited(_load());
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final p = await Injection.api.pin(widget.pinId);
      if (mounted) {
        setState(() {
          _pin = p;
          _error = null;
        });
      }
    } catch (e) {
      if (mounted) setState(() => _error = ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _call(String phone) async {
    final ok = await launchUrl(Uri(scheme: 'tel', path: phone));
    if (!ok && mounted) AppFeedback.snack(context, 'تعذر فتح الاتصال', error: true);
  }

  Future<void> _directions(ShopPin p) async {
    final lat = p.lat.toStringAsFixed(6);
    final lng = p.lng.toStringAsFixed(6);
    final geo = Uri.parse('geo:$lat,$lng?q=$lat,$lng(${Uri.encodeComponent(p.name)})');
    var ok = false;
    try {
      ok = await launchUrl(geo, mode: LaunchMode.externalApplication);
    } catch (_) {}
    if (ok) return;
    final web = Uri.parse(
      'https://www.google.com/maps/dir/?api=1&destination=$lat,$lng',
    );
    ok = await launchUrl(web, mode: LaunchMode.externalApplication);
    if (!ok && mounted) AppFeedback.snack(context, 'تعذر فتح الخرائط', error: true);
  }

  Future<void> _edit(ShopPin p) async {
    final saved = await showPinForm(context, existing: p);
    if (saved == null || !mounted) return;
    setState(() {
      _pin = saved;
      _changed = true;
    });
    AppFeedback.snack(context, 'تم حفظ التعديل');
  }

  Future<void> _delete(ShopPin p) async {
    final ok = await AppFeedback.confirm(
      context,
      title: 'حذف المحل',
      message: 'سيتم حذف «${p.name}» نهائياً. لا يمكن التراجع عن الحذف.',
      confirmLabel: 'حذف',
      danger: true,
      icon: Icons.delete_outline_rounded,
    );
    if (!ok || !mounted) return;
    setState(() => _deleting = true);
    try {
      await Injection.api.deletePin(p.id);
      unawaited(Injection.store.afterActivity());
      if (mounted) Navigator.of(context).pop(true);
    } catch (e) {
      if (mounted) await AppFeedback.error(context, ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _deleting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final p = _pin;
    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (!didPop) Navigator.of(context).pop(_changed);
      },
      child: Scaffold(
        appBar: AppBar(title: Text(p?.name ?? 'تفاصيل المحل')),
        body: p == null
            ? (_loading
                  ? const Center(child: CircularProgressIndicator())
                  : StateView(
                      icon: Icons.cloud_off_rounded,
                      message: _error ?? ApiErrors.generic,
                      actionLabel: 'إعادة المحاولة',
                      onAction: _load,
                    ))
            : ListView(
                padding: EdgeInsets.fromLTRB(
                  18,
                  8,
                  18,
                  24 + MediaQuery.paddingOf(context).bottom,
                ),
                children: [
                  ClipRRect(
                    borderRadius: BorderRadius.circular(20),
                    child: SizedBox(
                      height: 220,
                      child: FlutterMap(
                        options: MapOptions(
                          initialCenter: LatLng(p.lat, p.lng),
                          initialZoom: 16,
                        ),
                        children: [
                          MapParts.tiles(),
                          MarkerLayer(
                            markers: [
                              MapParts.pinMarker(
                                MapPin(p.id, p.lat, p.lng, p.name, p.interest, p.mine),
                                size: 40,
                              ),
                            ],
                          ),
                          MapParts.attribution(),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  NeuCard(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Expanded(
                              child: Text(
                                p.name,
                                style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
                              ),
                            ),
                            StatusPill(p.interestLabel, color: Interest.color(p.interest)),
                          ],
                        ),
                        const SizedBox(height: 6),
                        Text(
                          'ثُبّت ${IraqTime.dateTime(p.createdAt)}',
                          style: const TextStyle(color: AppColors.muted, fontSize: 13),
                        ),
                        if (p.phone != null) ...[
                          const SizedBox(height: 10),
                          Text(
                            p.phone!,
                            textDirection: TextDirection.ltr,
                            style: const TextStyle(fontWeight: FontWeight.w600),
                          ),
                        ],
                        if (p.note != null) ...[
                          const SizedBox(height: 10),
                          Text(p.note!, style: const TextStyle(color: AppColors.ink)),
                        ],
                      ],
                    ),
                  ),
                  const SizedBox(height: 18),
                  Row(
                    children: [
                      if (p.phone != null) ...[
                        Expanded(
                          child: NeuButton(
                            label: 'اتصال',
                            icon: Icons.call_outlined,
                            primary: false,
                            height: 48,
                            onPressed: () => _call(p.phone!),
                          ),
                        ),
                        const SizedBox(width: 12),
                      ],
                      Expanded(
                        child: NeuButton(
                          label: 'الاتجاهات',
                          icon: Icons.directions_outlined,
                          primary: false,
                          height: 48,
                          onPressed: () => _directions(p),
                        ),
                      ),
                    ],
                  ),
                  if (p.mine) ...[
                    const SizedBox(height: 12),
                    Row(
                      children: [
                        Expanded(
                          child: NeuButton(
                            label: 'تعديل',
                            icon: Icons.edit_outlined,
                            height: 48,
                            onPressed: () => _edit(p),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: NeuButton(
                            label: 'حذف',
                            icon: Icons.delete_outline_rounded,
                            primary: false,
                            danger: true,
                            height: 48,
                            busy: _deleting,
                            onPressed: () => _delete(p),
                          ),
                        ),
                      ],
                    ),
                  ],
                ],
              ),
      ),
    );
  }
}
