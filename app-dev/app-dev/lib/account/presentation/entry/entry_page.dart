import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:team/account/infrastructure/repository/account_repository.dart';
import 'package:team/account/presentation/entry/bloc/entry_bloc.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:vector_graphics/vector_graphics.dart';

class EntryPage extends StatelessWidget {
  const EntryPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => EntryBloc(accountRepository: GetIt.I.get<AccountRepository>()),
      child: const EntryPageView(),
    );
  }
}

class EntryPageView extends StatefulWidget {
  const EntryPageView({super.key});

  @override
  State<EntryPageView> createState() => _EntryPageViewState();
}

class _EntryPageViewState extends State<EntryPageView> {
  final usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  final _isPasswordVisible = ValueNotifier(false);

  @override
  void dispose() {
    usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  String? _validateUsername(String? value) {
    if (value == null || value.isEmpty) {
      return 'يرجى إدخال اسم المستخدم';
    }
    if (value.length < 3) {
      return 'اسم المستخدم يجب أن يكون على الأقل 3 أحرف';
    }
    return null;
  }

  String? _validatePassword(String? value) {
    if (value == null || value.isEmpty) {
      return 'يرجى إدخال الرمز السري';
    }
    if (value.length < 6) {
      return 'الرمز السري يجب أن يكون على الأقل 6 أحرف';
    }
    return null;
  }

  void _attemptLogin() {
    if (_formKey.currentState!.validate()) {
      final bloc = context.read<EntryBloc>();
      bloc.add(LoginEvent(usernameController.text, _passwordController.text));
    }
  }

  @override
  Widget build(BuildContext context) {
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: const SystemUiOverlayStyle(
        statusBarColor: AppColor.primary,
        statusBarIconBrightness: Brightness.light,
        statusBarBrightness: Brightness.dark,
      ),
      child: SafeArea(
        top: false,
        child: DefaultListener<EntryBloc>(
          listener: (context, state) {
            if (state.event is LoginEvent && state is ResponseState) {
              Navigator.of(context).pushReplacementNamed(RouteNames.home);
            }
          },
          child: Scaffold(
            extendBody: true,
            body: ListView(
              children: [
                SizedBox(
                  height: MediaQuery.sizeOf(context).height - 82 - MediaQuery.paddingOf(context).top,
                  child: Stack(
                    fit: StackFit.expand,
                    children: [
                      Positioned(
                        top: 0,
                        left: 0,
                        right: 0,
                        child: VectorGraphic(
                          loader: AssetBytesLoader('assets/svg/entry_top.svg'),
                          fit: BoxFit.fill,
                          width: MediaQuery.sizeOf(context).width,
                          height: 317,
                        ),
                      ),
                      Positioned(
                        top: 0,
                        left: 0,
                        child: Animate(
                          effects: [
                            ScaleEffect(
                              duration: 700.ms,
                              begin: Offset(1.3, 1.3),
                              end: Offset(1, 1),
                              curve: Curves.easeOutQuad,
                            ),
                          ],
                          child: VectorGraphic(loader: AssetBytesLoader('assets/svg/lines_top_left.svg')),
                        ),
                      ),
                      Positioned(
                        bottom: 0,
                        right: 0,
                        left: 0,
                        top: 160,
                        child: Animate(
                          effects: [
                            ScaleEffect(
                              duration: 700.ms,
                              begin: Offset(1.3, 1.3),
                              end: Offset(1, 1),
                              curve: Curves.easeOutQuad,
                            ),
                          ],
                          child: VectorGraphic(
                            loader: AssetBytesLoader('assets/svg/lines_bottom_right.svg'),
                            fit: BoxFit.fill,
                            colorFilter: ColorFilter.mode(Color(0xffCCF5F2).withValues(alpha: 0.4), BlendMode.srcIn),
                          ),
                        ),
                      ),
                      Positioned(
                        right: 0,
                        left: 0,
                        top: 63,
                        child: Column(
                          children: [
                            VectorGraphic(loader: AssetBytesLoader('assets/svg/logo_white.svg')),
                            Padding(
                              padding: const EdgeInsets.only(top: 8),
                              child: Text('قلعة الضمان', style: AppTextStyle.headlineLarge.withColor(Colors.white)),
                            ),
                          ],
                        ),
                      ),
                      Positioned(
                        right: 24,
                        top: 225,
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text('مرحبا بك', style: AppTextStyle.headlineLarge.withColor(AppColor.surface2)),
                            SizedBox(height: 2),
                            Text('يرجى تسجيل الدخول', style: AppTextStyle.bodySmall.withColor(AppColor.text4)),
                          ],
                        ),
                      ),
                      Center(
                        child: Padding(
                          padding: EdgeInsets.only(
                            right: 24,
                            left: 24,
                            top: MediaQuery.sizeOf(context).height * 0.3 + 50 + MediaQuery.paddingOf(context).top,
                          ),
                          child: Form(
                            key: _formKey,
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Text('أسم المستخدم', style: AppTextStyle.bodyMedium.withColor(AppColor.text2)),
                                Padding(
                                  padding: const EdgeInsets.only(top: 6, bottom: 20),
                                  child: DefaultTextField(
                                    textEditingController: usernameController,
                                    textInputAction: TextInputAction.next,
                                    hint: 'يرجى أدخال اسم المستخدم',
                                    fillColor: AppColor.surface2,
                                    filled: true,
                                    validator: _validateUsername,
                                  ),
                                ),
                                Text('الرمز السري', style: AppTextStyle.bodyMedium.withColor(AppColor.text2)),
                                Padding(
                                  padding: const EdgeInsets.only(top: 6),
                                  child: ValueListenableBuilder(
                                    valueListenable: _passwordController,
                                    builder:
                                        (context, value, child) => ValueListenableBuilder(
                                          valueListenable: _isPasswordVisible,
                                          builder:
                                              (context, isVisible, child) => DefaultTextField(
                                                textEditingController: _passwordController,
                                                textInputAction: TextInputAction.done,
                                                hint: 'يرجى أدخال الرمز السري للمستخدم',
                                                fillColor: AppColor.surface2,
                                                filled: true,
                                                validator: _validatePassword,
                                                obscureText: !isVisible,
                                                suffixIcon:
                                                    value.text.trim().isNotEmpty
                                                        ? IconButton(
                                                          style: IconButton.styleFrom(
                                                            elevation: 0,
                                                            backgroundColor: Colors.transparent,
                                                          ),
                                                          icon: Icon(
                                                            isVisible ? Icons.visibility : Icons.visibility_off,
                                                            color: AppColor.text3,
                                                          ),
                                                          onPressed: () {
                                                            setState(() {
                                                              _isPasswordVisible.value = !isVisible;
                                                            });
                                                          },
                                                        )
                                                        : null,
                                              ),
                                        ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            bottomNavigationBar: Padding(
              padding: const EdgeInsets.only(left: 40, right: 40, bottom: 32),
              child: DefaultBuilder<EntryBloc>(
                buildWhen: (previous, state) => state.event is LoginEvent,
                builder: (context, state) {
                  final isLoading = state is LoadingState;
                  return FilledButton(
                    onPressed: isLoading ? null : _attemptLogin,
                    child:
                        isLoading
                            ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(color: AppColor.surface2),
                            )
                            : const Text('تسجيل دخول'),
                  );
                },
              ),
            ),
          ),
        ),
      ),
    );
  }
}
