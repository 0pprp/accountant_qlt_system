import 'package:flutter/material.dart';

import '../../config/app_env.dart';
import '../../core/network/api_errors.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/brand.dart';
import '../../core/widgets/neu.dart';
import '../../injection.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key, required this.onLoggedIn, this.notice});
  final VoidCallback onLoggedIn;
  final String? notice;

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _form = GlobalKey<FormState>();
  final _user = TextEditingController();
  final _pass = TextEditingController();
  bool _busy = false;
  bool _hide = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _error = widget.notice;
    Injection.session.getUserName().then((u) {
      if (mounted && u != null && _user.text.isEmpty) _user.text = u;
    });
  }

  @override
  void dispose() {
    _user.dispose();
    _pass.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (_busy || !(_form.currentState?.validate() ?? false)) return;
    FocusScope.of(context).unfocus();
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await Injection.auth.login(username: _user.text, password: _pass.text);
      try {
        await Injection.store.refreshMe(force: true);
      } catch (e) {
        final err = ApiErrors.from(e);
        if (err.statusCode == 403 || err.statusCode == 401) {
          await Injection.session.clear();
        }
        rethrow;
      }
      if (mounted) widget.onLoggedIn();
    } catch (e) {
      if (mounted) setState(() => _error = ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: EdgeInsets.fromLTRB(24, 24, 24, 24 + bottom),
            child: AutofillGroup(
              child: Form(
                key: _form,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    const Center(child: LogoTile(size: 100)),
                    const SizedBox(height: 20),
                    const Text(
                      AppEnv.appTitle,
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 26, fontWeight: FontWeight.w700),
                    ),
                    const Text(
                      'قلعة الضمان',
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 15, color: AppColors.muted),
                    ),
                    const SizedBox(height: 28),
                    NeuCard(
                      padding: const EdgeInsets.all(20),
                      radius: 26,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          NeuTextField(
                            controller: _user,
                            label: 'اسم المستخدم',
                            prefixIcon: Icons.person_outline_rounded,
                            textInputAction: TextInputAction.next,
                            textDirection: TextDirection.ltr,
                            autofillHints: const [AutofillHints.username],
                            validator: (v) => (v ?? '').trim().isEmpty
                                ? 'اكتب اسم المستخدم'
                                : null,
                          ),
                          const SizedBox(height: 16),
                          NeuTextField(
                            controller: _pass,
                            label: 'كلمة المرور',
                            prefixIcon: Icons.lock_outline_rounded,
                            obscure: _hide,
                            textDirection: TextDirection.ltr,
                            textInputAction: TextInputAction.done,
                            autofillHints: const [AutofillHints.password],
                            onSubmitted: (_) => _submit(),
                            suffix: IconButton(
                              icon: Icon(
                                _hide
                                    ? Icons.visibility_outlined
                                    : Icons.visibility_off_outlined,
                                color: AppColors.muted,
                              ),
                              onPressed: () => setState(() => _hide = !_hide),
                            ),
                            validator: (v) =>
                                (v ?? '').isEmpty ? 'اكتب كلمة المرور' : null,
                          ),
                          if (_error != null) ...[
                            const SizedBox(height: 14),
                            Text(
                              _error!,
                              textAlign: TextAlign.center,
                              style: const TextStyle(
                                color: AppColors.danger,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ],
                          const SizedBox(height: 20),
                          NeuButton(
                            label: 'دخول',
                            icon: Icons.login_rounded,
                            busy: _busy,
                            onPressed: _submit,
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),
                    const Text(
                      'نسيت كلمة المرور؟ راجع الإدارة',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: AppColors.muted),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
