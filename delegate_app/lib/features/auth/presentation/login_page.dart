import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:provider/provider.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../injection.dart';
import '../../home/presentation/home_shell.dart';
import 'login_controller.dart';

class LoginPage extends StatelessWidget {
  const LoginPage({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (_) => LoginController(Injection.loginUseCase),
      child: const _LoginView(),
    );
  }
}

class _LoginView extends StatefulWidget {
  const _LoginView();

  @override
  State<_LoginView> createState() => _LoginViewState();
}

class _LoginViewState extends State<_LoginView> {
  final _username = TextEditingController(text: 'ahmed');
  final _password = TextEditingController(text: 'ahmed');
  final _obscure = ValueNotifier(true);

  @override
  void dispose() {
    _username.dispose();
    _password.dispose();
    _obscure.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final c = context.watch<LoginController>();
    final top = MediaQuery.paddingOf(context).top;

    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: const SystemUiOverlayStyle(
        statusBarColor: AppColors.primary,
        statusBarIconBrightness: Brightness.light,
        statusBarBrightness: Brightness.dark,
      ),
      child: Scaffold(
        backgroundColor: AppColors.background,
        body: ListView(
          padding: EdgeInsets.zero,
          children: [
            Container(
              width: double.infinity,
              padding: EdgeInsets.fromLTRB(24, top + 36, 24, 36),
              decoration: const BoxDecoration(
                color: AppColors.primary,
                borderRadius: BorderRadius.vertical(
                  bottom: Radius.circular(28),
                ),
              ),
              child: Column(
                children: [
                  SvgPicture.asset(
                    'assets/svg/logo_white.svg',
                    height: 52,
                    colorFilter: const ColorFilter.mode(
                      Colors.white,
                      BlendMode.srcIn,
                    ),
                  ),
                  const SizedBox(height: 12),
                  Text(
                    'قلعة الضمان',
                    style: AppTextStyles.display.copyWith(
                      color: Colors.white,
                      fontSize: 26,
                    ),
                  ),
                  Text(
                    'مندوب التحصيل',
                    style: AppTextStyles.muted.copyWith(
                      color: Colors.white.withValues(alpha: 0.85),
                    ),
                  ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(24, 28, 24, 24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('مرحباً بك', style: AppTextStyles.title),
                  const SizedBox(height: 4),
                  Text('يرجى تسجيل الدخول للمتابعة', style: AppTextStyles.muted),
                  const SizedBox(height: 28),
                  Text('اسم المستخدم', style: AppTextStyles.body),
                  const SizedBox(height: 8),
                  TextField(
                    controller: _username,
                    style: AppTextStyles.body,
                    textInputAction: TextInputAction.next,
                    autofillHints: const [AutofillHints.username],
                    decoration: const InputDecoration(
                      hintText: 'أدخل اسم المستخدم',
                    ),
                  ),
                  const SizedBox(height: 18),
                  Text('كلمة السر', style: AppTextStyles.body),
                  const SizedBox(height: 8),
                  ValueListenableBuilder<bool>(
                    valueListenable: _obscure,
                    builder: (context, hide, _) {
                      return TextField(
                        controller: _password,
                        style: AppTextStyles.body,
                        obscureText: hide,
                        textInputAction: TextInputAction.done,
                        autofillHints: const [AutofillHints.password],
                        onSubmitted: (_) => _submit(c),
                        decoration: InputDecoration(
                          hintText: 'أدخل كلمة السر',
                          suffixIcon: IconButton(
                            onPressed: () => _obscure.value = !hide,
                            icon: Icon(
                              hide
                                  ? Icons.visibility_outlined
                                  : Icons.visibility_off_outlined,
                              color: AppColors.muted,
                            ),
                          ),
                        ),
                      );
                    },
                  ),
                  if (c.error != null) ...[
                    const SizedBox(height: 12),
                    Text(
                      c.error!,
                      style: AppTextStyles.muted.copyWith(
                        color: AppColors.danger,
                      ),
                    ),
                  ],
                  const SizedBox(height: 28),
                  ElevatedButton(
                    onPressed: c.loading ? null : () => _submit(c),
                    child: c.loading
                        ? const SizedBox(
                            height: 22,
                            width: 22,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: AppColors.onPrimary,
                            ),
                          )
                        : Text('تسجيل الدخول', style: AppTextStyles.button),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _submit(LoginController c) async {
    final ok = await c.submit(_username.text, _password.text);
    if (!mounted) return;
    if (ok) {
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const HomeShell()),
      );
    }
  }
}
