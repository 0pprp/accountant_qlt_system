import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/brand_logo.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key, required this.onLoggedIn});

  final VoidCallback onLoggedIn;

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _user = TextEditingController();
  final _pass = TextEditingController();
  bool _busy = false;
  bool _obscure = true;

  @override
  void dispose() {
    _user.dispose();
    _pass.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (_user.text.trim().isEmpty || _pass.text.isEmpty) {
      await showAppMessage(context, title: 'مطلوب', message: 'أدخل اسم المستخدم وكلمة المرور');
      return;
    }
    setState(() => _busy = true);
    try {
      await Injection.auth.login(userName: _user.text.trim(), password: _pass.text);
      if (!mounted) return;
      widget.onLoggedIn();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'فشل الدخول',
        message: DioClient.mapError(e),
        color: AppColors.danger,
        icon: Icons.lock_outline,
      );
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      body: AnimatedWavesBackground(
        child: SafeArea(
          child: ListView(
            padding: EdgeInsets.fromLTRB(24, 24, 24, 24 + bottom),
            children: [
              const SizedBox(height: 24),
              const BrandLogo(height: 86),
              const SizedBox(height: 16),
              const Text(
                'تطبيق مسؤول اللجنة',
                textAlign: TextAlign.center,
                style: TextStyle(fontSize: 22, fontWeight: FontWeight.w700, color: AppColors.primary),
              ),
              const SizedBox(height: 6),
              const Text(
                'قلعة الضمان — إشراف القوائم الميدانية',
                textAlign: TextAlign.center,
                style: TextStyle(color: AppColors.textSecondary),
              ),
              const SizedBox(height: 28),
              TextField(
                controller: _user,
                textInputAction: TextInputAction.next,
                decoration: const InputDecoration(labelText: 'اسم المستخدم'),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: _pass,
                obscureText: _obscure,
                onSubmitted: (_) => _submit(),
                decoration: InputDecoration(
                  labelText: 'كلمة المرور',
                  suffixIcon: IconButton(
                    onPressed: () => setState(() => _obscure = !_obscure),
                    icon: Icon(_obscure ? Icons.visibility_outlined : Icons.visibility_off_outlined),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              ElevatedButton(
                onPressed: _busy ? null : _submit,
                child: _busy
                    ? const SizedBox(
                        width: 22,
                        height: 22,
                        child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                      )
                    : const Text('تسجيل الدخول'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
