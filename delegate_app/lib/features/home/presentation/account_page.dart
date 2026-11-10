import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/loading_view.dart';
import '../../../injection.dart';

class AccountPage extends StatefulWidget {
  const AccountPage({super.key});

  @override
  State<AccountPage> createState() => _AccountPageState();
}

class _AccountPageState extends State<AccountPage> {
  String? _name;
  String? _city;
  String? _id;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final session = await Injection.authRepository.currentSession();
    if (!mounted) return;
    setState(() {
      _name = session?.delegateName;
      _city = session?.cityName;
      _id = session?.delegateId;
      _loading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: Text('الحساب', style: AppTextStyles.title)),
      body: _loading
          ? const LoadingView()
          : ListView(
              padding: const EdgeInsets.all(16),
              children: [
                CircleAvatar(
                  radius: 40,
                  backgroundColor: AppColors.primary.withValues(alpha: 0.15),
                  child: Text(
                    (_name != null && _name!.isNotEmpty)
                        ? String.fromCharCode(_name!.runes.first)
                        : 'م',
                    style: AppTextStyles.display.copyWith(
                      color: AppColors.primary,
                    ),
                  ),
                ),
                const SizedBox(height: 16),
                _row('الاسم', _name ?? '—'),
                _row('المعرّف', _id ?? '—'),
                _row('القائمة / المدينة', _city ?? '—'),
              ],
            ),
    );
  }

  Widget _row(String label, String value) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        children: [
          Expanded(child: Text(label, style: AppTextStyles.muted)),
          Text(value, style: AppTextStyles.body.copyWith(fontWeight: FontWeight.w700)),
        ],
      ),
    );
  }
}
