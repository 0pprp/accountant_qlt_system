import 'dart:async';

import 'package:flutter/material.dart';
import 'package:package_info_plus/package_info_plus.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../config/app_env.dart';
import '../../core/formatting/iraq_time.dart';
import '../../core/theme/app_theme.dart';
import '../../core/validation/validators.dart';
import '../../core/widgets/feedback.dart';
import '../../core/widgets/neu.dart';
import '../../injection.dart';

String? whatsappDigits(String? raw) {
  var digits = Validators.toWesternDigits(raw).replaceAll(RegExp(r'[^0-9]'), '');
  if (digits.isEmpty) return null;
  if (digits.startsWith('00')) digits = digits.substring(2);
  if (digits.startsWith('0')) digits = '964${digits.substring(1)}';
  return digits.length >= 8 ? digits : null;
}

class ProfilePage extends StatefulWidget {
  const ProfilePage({super.key, required this.onLogout});
  final Future<void> Function() onLogout;

  @override
  State<ProfilePage> createState() => _ProfilePageState();
}

class _ProfilePageState extends State<ProfilePage> {
  String _version = '';
  bool _loggingOut = false;

  @override
  void initState() {
    super.initState();
    Injection.store.addListener(_onStore);
    PackageInfo.fromPlatform().then((info) {
      if (mounted) setState(() => _version = '${info.version} (${info.buildNumber})');
    });
  }

  @override
  void dispose() {
    Injection.store.removeListener(_onStore);
    super.dispose();
  }

  void _onStore() {
    if (mounted) setState(() {});
  }

  Future<void> _open(Uri uri) async {
    var ok = false;
    try {
      ok = await launchUrl(uri, mode: LaunchMode.externalApplication);
    } catch (_) {}
    if (!ok && mounted) AppFeedback.snack(context, 'تعذر فتح التطبيق المطلوب', error: true);
  }

  Future<void> _logout() async {
    final ok = await AppFeedback.confirm(
      context,
      title: 'تسجيل الخروج',
      message: 'هل تريد تسجيل الخروج من التطبيق؟',
      confirmLabel: 'خروج',
      danger: true,
      icon: Icons.logout_rounded,
    );
    if (!ok || !mounted) return;
    setState(() => _loggingOut = true);
    try {
      await Injection.tracker.flush();
    } catch (_) {}
    await widget.onLogout();
  }

  void _about() {
    AppFeedback.dialog<void>(
      context,
      title: AppEnv.appTitle,
      message: 'قلعة الضمان\nالإصدار $_version',
      icon: Icons.info_outline_rounded,
      actions: const [DialogAction(label: 'حسناً', value: null)],
    );
  }

  @override
  Widget build(BuildContext context) {
    final me = Injection.store.me;
    final name = me?.fullName ?? '';
    final initial = name.trim().isEmpty ? '؟' : name.trim().characters.first;
    final phone = me?.supportPhone;
    final wa = whatsappDigits(me?.supportWhatsapp ?? me?.supportPhone);
    return RefreshIndicator(
      onRefresh: () async {
        try {
          await Injection.store.refreshMe(force: true);
        } catch (_) {}
      },
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(18, 24, 18, 28),
        children: [
          Center(
            child: Container(
              width: 92,
              height: 92,
              alignment: Alignment.center,
              decoration: BoxDecoration(
                color: AppColors.primarySoft,
                shape: BoxShape.circle,
                boxShadow: neuShadows(),
              ),
              child: Text(
                initial,
                style: const TextStyle(
                  fontSize: 38,
                  fontWeight: FontWeight.w700,
                  color: AppColors.primary,
                ),
              ),
            ),
          ),
          const SizedBox(height: 14),
          Text(
            name.isEmpty ? '—' : name,
            textAlign: TextAlign.center,
            style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w700),
          ),
          Text(
            '${me?.username ?? ''} · مروّج ميداني',
            textAlign: TextAlign.center,
            style: const TextStyle(color: AppColors.muted),
          ),
          const SizedBox(height: 20),
          Row(
            children: [
              _tile('الهدف اليومي', IraqTime.minutesAsHm(me?.targetMinutes ?? 0)),
              const SizedBox(width: 10),
              _tile('طلبات الشهر', '${me?.monthOrders ?? 0}'),
              const SizedBox(width: 10),
              _tile('مبيعات الشهر', '${me?.monthSold ?? 0}'),
            ],
          ),
          const SizedBox(height: 20),
          NeuCard(
            padding: const EdgeInsets.symmetric(vertical: 6),
            child: Column(
              children: [
                _menu(
                  Icons.call_outlined,
                  'اتصل بالإدارة',
                  enabled: phone != null,
                  onTap: () => _open(Uri(scheme: 'tel', path: phone)),
                ),
                const Divider(height: 1, color: AppColors.line),
                _menu(
                  Icons.chat_outlined,
                  'تواصل عبر واتساب',
                  enabled: wa != null,
                  onTap: () => _open(Uri.parse('https://wa.me/$wa')),
                ),
                const Divider(height: 1, color: AppColors.line),
                _menu(
                  Icons.info_outline_rounded,
                  'عن التطبيق',
                  trailing: _version.isEmpty ? null : 'الإصدار $_version',
                  onTap: _about,
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),
          NeuButton(
            label: 'تسجيل الخروج',
            icon: Icons.logout_rounded,
            primary: false,
            danger: true,
            busy: _loggingOut,
            onPressed: _logout,
          ),
        ],
      ),
    );
  }

  Widget _tile(String label, String value) => Expanded(
    child: NeuCard(
      padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 6),
      radius: 18,
      child: Column(
        children: [
          Text(
            value,
            textDirection: TextDirection.ltr,
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.w700,
              color: AppColors.primary,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            textAlign: TextAlign.center,
            style: const TextStyle(fontSize: 12, color: AppColors.muted),
          ),
        ],
      ),
    ),
  );

  Widget _menu(
    IconData icon,
    String label, {
    required VoidCallback onTap,
    bool enabled = true,
    String? trailing,
  }) => ListTile(
    enabled: enabled,
    leading: Icon(icon, color: enabled ? AppColors.primary : AppColors.muted),
    title: Text(label, style: const TextStyle(fontWeight: FontWeight.w600)),
    trailing: trailing != null
        ? Text(trailing, style: const TextStyle(color: AppColors.muted, fontSize: 12))
        : const Icon(Icons.chevron_left_rounded, color: AppColors.muted),
    onTap: enabled ? onTap : null,
  );
}
