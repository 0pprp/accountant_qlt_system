import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:intl/intl.dart' hide TextDirection;
import 'package:team/account/infrastructure/repository/account_repository.dart';
import 'package:team/account/mapper.dart';
import 'package:team/account/presentation/profile_page/bloc/profile_bloc.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_cached_network_image.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';
import 'package:team/common/ui/widgets/show_animated_dialog.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/text_util.dart';
import 'package:team/injections.dart';
import 'package:vector_graphics/vector_graphics.dart';

class ProfilePage extends StatelessWidget {
  final PageController pageController;
  final ValueNotifier<int> currentPage;
  final ValueNotifier<bool> isAnimating;
  const ProfilePage({super.key, required this.pageController, required this.currentPage, required this.isAnimating});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create: (BuildContext context) => ProfileBloc(accountRepository: GetIt.I.get<AccountRepository>()),
      child: ProfilePageView(pageController: pageController, currentPage: currentPage, isAnimating: isAnimating),
    );
  }
}

class ProfilePageView extends StatefulWidget {
  final PageController pageController;
  final ValueNotifier<int> currentPage;
  final ValueNotifier<bool> isAnimating;
  const ProfilePageView({
    super.key,
    required this.pageController,
    required this.currentPage,
    required this.isAnimating,
  });

  @override
  State<ProfilePageView> createState() => _ProfilePageViewState();
}

class _ProfilePageViewState extends State<ProfilePageView> with AutomaticKeepAliveClientMixin {
  late final _bloc = context.read<ProfileBloc>();

  @override
  void initState() {
    _bloc.add(InitialProfileEvent());
    super.initState();
  }

  void _showLogoutDialog() {
    showAnimatedDialog(
      context: context,
      child: StatefulBuilder(
        builder: (context, setState) {
          return Dialog(
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24.0)),
            insetPadding: EdgeInsets.symmetric(horizontal: 24),
            backgroundColor: Colors.transparent,
            child: ClipRRect(
              borderRadius: BorderRadius.circular(24),
              child: Container(
                decoration: BoxDecoration(color: Colors.white),
                child: Stack(
                  children: [
                    Positioned.fill(
                      child: VectorGraphic(
                        loader: AssetBytesLoader('assets/svg/dialog_background.svg'),
                        fit: BoxFit.fill,
                        height: 230,
                        colorFilter: ColorFilter.mode(AppColor.error.withValues(alpha: 0.25), BlendMode.srcIn),
                      ),
                    ),
                    Container(
                      // height: 230,
                      padding: EdgeInsets.all(32),
                      decoration: BoxDecoration(borderRadius: BorderRadius.circular(24)),
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text('تسجيل خروج', style: AppTextStyle.headlineMedium.withColor(AppColor.text2)),
                              VectorGraphic(loader: AssetBytesLoader('assets/svg/logout_icon.svg')),
                            ],
                          ),
                          Padding(
                            padding: const EdgeInsets.only(top: 32, bottom: 24),
                            child: Text(
                              'هل أنت متأكد من تسجيل الخروج',
                              style: AppTextStyle.headlineMedium.withColor(AppColor.text2),
                            ),
                          ),
                          Row(
                            children: [
                              Expanded(
                                flex: 2,
                                child: FilledButton(
                                  style: FilledButton.styleFrom(backgroundColor: AppColor.error),
                                  onPressed: () async {
                                    await GetIt.I.get<AccountMapper>().logout();
                                    if (context.mounted) {
                                      Navigator.pushNamedAndRemoveUntil(context, RouteNames.main, (route) => false);
                                    }
                                  },
                                  child: Text('تسجيل الخروج'),
                                ),
                              ),
                              SizedBox(width: 24),
                              FilledButton(
                                style: FilledButton.styleFrom(
                                  backgroundColor: AppColor.surface2,
                                  elevation: 7,
                                  shadowColor: Color(0xffD3D3D3).withValues(alpha: 0.3),
                                ),
                                onPressed: () => Navigator.pop(context),
                                child: Text('ألغاء', style: AppTextStyle.headlineMedium.withColor(AppColor.error)),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return Scaffold(
      appBar: DefaultAppBar(
        title: 'المستخدم',
        showIcons: true,
        firstIcon: IconButton(
          onPressed: _showLogoutDialog,
          icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/logout_icon.svg')),
        ),
        onBackPressed: () async {
          widget.isAnimating.value = true;
          widget.currentPage.value = 2;
          await widget.pageController.animateToPage(2, duration: Duration(milliseconds: 500), curve: Curves.easeOut);
          widget.isAnimating.value = false;
        },
      ),
      body: DefaultBuilder<ProfileBloc>(
        buildWhen: (previous, state) => state.event is InitialProfileEvent,
        builder: (context, state) {
          if (state is ErrorState) {
            return DefaultErrorWidget(error: state.error, onRetry: () => _bloc.add(InitialProfileEvent()));
          }
          if (_bloc.user != null) {
            return ListView(
              padding: EdgeInsets.symmetric(vertical: 24),
              children: [
                Animate(
                  effects: [FadeEffect(duration: 1000.ms, curve: Curves.linear)],
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      DefaultCachedNetworkImage(
                        imageUrl:
                            _bloc.user?.profilePicture == null
                                ? null
                                : Injections.baseUrl + _bloc.user!.profilePicture!.relativePath,
                        height: 124,
                        isAvatar: true,
                        width: 124,
                        borderRadius: 124,
                      ),
                      SizedBox(height: 8),
                      Text(_bloc.user?.fullName ?? '', style: AppTextStyle.bodyLarge),
                      SizedBox(height: 16),
                      Row(
                        children: [
                          Expanded(
                            child: Padding(
                              padding: const EdgeInsets.symmetric(horizontal: 56),
                              child: FilledButton(
                                onPressed:
                                    () => Navigator.pushNamed(
                                      context,
                                      RouteNames.customerOrders,
                                      arguments: {'userId': _bloc.user!.id, 'isCurrentAdmin': true},
                                    ),
                                child: Text('الديون'),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                Animate(
                  effects: [
                    SlideEffect(end: Offset(0, 0), begin: Offset(0, 1), duration: 2000.ms, curve: ElasticOutCurve(1.2)),
                  ],
                  child: Padding(
                    padding: const EdgeInsets.only(top: 24),
                    child: Container(
                      decoration: BoxDecoration(
                        color: AppColor.surface4,
                        borderRadius: BorderRadius.circular(48),
                        border: Border.all(color: AppColor.stroke, width: 2),
                      ),
                      child: Stack(
                        children: [
                          Positioned.fill(
                            child: ClipRRect(
                              borderRadius: BorderRadius.circular(48),
                              child: VectorGraphic(
                                loader: AssetBytesLoader('assets/svg/lines_card.svg'),
                                fit: BoxFit.fill,
                                colorFilter: ColorFilter.mode(
                                  Color(0xff000000).withValues(alpha: 0.15),
                                  BlendMode.srcIn,
                                ),
                              ),
                            ),
                          ),
                          Padding(
                            padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 40),
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                ProfileData(icon: 'user_icon.svg', title: 'أسم المندوب', value: _bloc.user!.fullName),
                                ProfileData(
                                  icon: 'call_icon.svg',
                                  title: 'رقم الهاتف',
                                  value: TextUtil.formatPhoneNumber(_bloc.user!.phoneNumber),
                                  fontFamily: 'Rubik',
                                ),
                                ProfileData(
                                  icon: 'location_icon.svg',
                                  title: 'عنوان السكن',
                                  value: _bloc.user?.address ?? '',
                                ),
                                ProfileData(
                                  icon: 'clock_icon.svg',
                                  title: 'تاريخ الانظمام',
                                  value: DateFormat('d / M / y').format(_bloc.user!.createdAt),
                                  fontFamily: 'Rubik',
                                ),
                                SizedBox(height: 44),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ],
            );
          }
          return const DefaultLoadingWidget();
        },
      ),
    );
  }

  @override
  bool get wantKeepAlive => true;
}

class ProfileData extends StatelessWidget {
  const ProfileData({
    super.key,
    required this.icon,
    required this.title,
    required this.value,
    this.fontFamily = 'ElMessiri',
  });

  final String icon;
  final String title;
  final String value;
  final String fontFamily;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(vertical: 4, horizontal: 8),
            child: Row(
              children: [
                VectorGraphic(loader: AssetBytesLoader('assets/svg/$icon'), width: 16, height: 16),
                SizedBox(width: 4),
                Text(title, style: AppTextStyle.bodySmall.withColor(AppColor.text2)),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.only(top: 4),
            child: DefaultTextField(
              filled: true,
              fillColor: Colors.white,
              textStyle: AppTextStyle.bodyMedium.copyWith(fontFamily: null),
              readOnly: true,
              enabled: false,
              label: value,
            ),
          ),
        ],
      ),
    );
  }
}
