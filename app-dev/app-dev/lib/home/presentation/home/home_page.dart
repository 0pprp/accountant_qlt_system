import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:lottie/lottie.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/ui/widgets/show_animated_dialog.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/common/utils/helpers/text_util.dart';
import 'package:team/home/domain/app_version_response/app_version_response.dart';
import 'package:team/home/infrastructure/repositories/home_repository.dart';
import 'package:team/home/presentation/home/bloc/home_bloc.dart';
import 'package:team/home/presentation/home/widgets/home_user_performance.dart';
import 'package:team/home/presentation/home/widgets/report_row.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';
import 'package:team/order/mapper.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:vector_graphics/vector_graphics.dart';

class HomePage extends StatelessWidget {
  final PageController pageController;
  final ValueNotifier<int> currentPage;
  const HomePage({super.key, required this.pageController, required this.currentPage});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create:
          (BuildContext context) => HomeBloc(
            homeRepository: GetIt.I.get<HomeRepository>(),
            accountMapper: GetIt.I.get<AccountMapper>(),
            customerMapper: GetIt.I.get<OrderMapper>(),
          ),
      child: HomePageView(currentPage: currentPage, pageController: pageController),
    );
  }
}

class HomePageView extends StatefulWidget {
  final PageController pageController;
  final ValueNotifier<int> currentPage;
  const HomePageView({super.key, required this.pageController, required this.currentPage});

  @override
  State<HomePageView> createState() => _HomePageViewState();
}

class _HomePageViewState extends State<HomePageView> with AutomaticKeepAliveClientMixin {
  late final _bloc = context.read<HomeBloc>();

  @override
  void initState() {
    _bloc.add(InitialHomeEvent());
    super.initState();
  }

  void _listener(BuildContext context, DefaultState state) {
    if (state.event is CheckForUpdates) {
      if (state is ResponseState) {
        final AppVersionResponse versionData = state.data;
        if (versionData.shouldUpdate) {
          showDialog(
            context: context,
            barrierDismissible: !versionData.isForce,
            builder: (context) {
              return Center(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 20),
                  child: Container(
                    decoration: ShapeDecoration(
                      color: Colors.white,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      crossAxisAlignment: CrossAxisAlignment.center,
                      children: [
                        Expanded(
                          child: Padding(
                            padding: const EdgeInsets.symmetric(vertical: 24),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.center,
                              mainAxisAlignment: MainAxisAlignment.center,
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                const Text(
                                  'تحديث جديد',
                                  style: TextStyle(
                                    color: AppColor.primary,
                                    fontSize: 16,
                                    fontWeight: FontWeight.w700,
                                  ),
                                ),
                                const Padding(
                                  padding: EdgeInsets.symmetric(vertical: 24, horizontal: 20),
                                  child: Text(
                                    'عدنا نزلنا تحديث جديد حتى نحسن تجربتك ويه التطبيق. رجاءً حمّل النسخة الجديدة واستفاد من التحسينات والمزايا الإضافية.',
                                    textAlign: TextAlign.center,
                                    textDirection: TextDirection.rtl,
                                    style: TextStyle(
                                      color: Color(0xFF2F2E41),
                                      fontSize: 14,
                                      fontWeight: FontWeight.w400,
                                    ),
                                  ),
                                ),
                                Padding(
                                  padding: const EdgeInsets.symmetric(horizontal: 40),
                                  child: Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      if (!versionData.isForce)
                                        Expanded(
                                          child: SizedBox(
                                            height: 40,
                                            child: OutlinedButton(
                                              style: FilledButton.styleFrom(
                                                foregroundColor: AppColor.primary,
                                                elevation: 0,
                                                side: const BorderSide(
                                                  color: AppColor.primary,
                                                  width: 2,
                                                ),
                                                shape: RoundedRectangleBorder(
                                                  borderRadius: BorderRadius.circular(8),
                                                ),
                                              ),
                                              onPressed: () {
                                                Navigator.pop(context);
                                              },
                                              child: const Text(
                                                'بعدين',
                                                textAlign: TextAlign.center,
                                                style: TextStyle(
                                                  fontSize: 14,
                                                  fontWeight: FontWeight.w700,
                                                ),
                                              ),
                                            ),
                                          ),
                                        ),
                                      if (!versionData.isForce) const SizedBox(width: 16),
                                      Expanded(
                                        child: SizedBox(
                                          height: 40,
                                          child: FilledButton(
                                            onPressed: () async {
                                              launchUrl(
                                                Uri.parse(versionData.url),
                                                mode: LaunchMode.externalApplication,
                                              );
                                              Navigator.pop(context);
                                            },
                                            child: const Text('تحميل'),
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              );
            },
          );
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return DefaultListener(
      bloc: GetIt.I.get<MainBloc>(),
      listener: (context, state) {
        if (state is OrderPayment || state is OrderCreated) {
          _bloc.add(InitialHomeEvent());
        }
      },
      child: DefaultConsumer<HomeBloc>(
        // listenWhen: (previous, state) => state.event is CheckForUpdates,
        listener: _listener,
        buildWhen: (previous, state) => state.event is InitialHomeEvent,
        builder: (context, state) {
          if (state is ErrorState) {
            return DefaultErrorWidget(error: state.error, onRetry: () => _bloc.add(InitialHomeEvent()));
          }
          if (_bloc.currentOrderList != null) {
            return Stack(
              children: [
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
                    child: VectorGraphic(
                      loader: AssetBytesLoader('assets/svg/lines_top_left.svg'),
                      colorFilter: ColorFilter.mode(AppColor.primary.withValues(alpha: 0.5), BlendMode.srcIn),
                    ),
                  ),
                ),

                RefreshIndicator(
                  onRefresh: () async {
                    _bloc.add(InitialHomeEvent());
                  },
                  color: AppColor.primary,
                  backgroundColor: Colors.white,
                  child: ListView(
                    children: [
                      Padding(
                        padding: EdgeInsets.only(top: 16, right: 24, left: 24),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            IconButton(
                              // onPressed: () => Navigator.pushNamed(context, RouteNames.notifications),
                              onPressed: () {},
                              icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/notification_icon.svg')),
                            ),
                            VectorGraphic(loader: AssetBytesLoader('assets/svg/logo_green.svg')),
                            DefaultBuilder<HomeBloc>(
                              buildWhen: (previous, state) => state.event is CheckPendingCollects,
                              builder: (context, state) {
                                return IconButton(
                                  onPressed:
                                      !_bloc.hasPendingCollects
                                          ? null
                                          : () async {
                                            _bloc.add(UpdateDataEvent());
                                            AssetLottie(
                                              'assets/images/dialog_success_animated.json',
                                            ).load().then((value) => debugPrint('success loaded'));
                                            AssetLottie(
                                              'assets/images/dialog_error_animated.json',
                                            ).load().then((value) => debugPrint('error loaded'));
                                            showAnimatedDialog(
                                              context: context,
                                              child: Dialog(
                                                shape: RoundedRectangleBorder(
                                                  borderRadius: BorderRadius.circular(24.0),
                                                ),
                                                backgroundColor: Colors.transparent,
                                                child: Container(
                                                  width: 250,
                                                  height: 250,
                                                  decoration: BoxDecoration(
                                                    color: Colors.white,
                                                    borderRadius: BorderRadius.circular(24),
                                                  ),
                                                  child: Column(
                                                    children: [
                                                      DefaultBuilder(
                                                        bloc: _bloc,
                                                        buildWhen:
                                                            (previous, state) =>
                                                                state.event is UpdateDataEvent &&
                                                                state is! ChangeDialogTextState,
                                                        builder: (context, state) {
                                                          return Lottie.asset(
                                                            'assets/images/dialog_${state.event is UpdateDataEvent && state is LoadingState
                                                                ? 'loading'
                                                                : state.event is UpdateDataEvent && state is ErrorState
                                                                ? 'error'
                                                                : 'success'}_animated.json',
                                                            height: 183,
                                                            key: Key(state.toString()),
                                                            repeat: state is LoadingState,
                                                          );
                                                        },
                                                      ),
                                                      DefaultBuilder(
                                                        bloc: _bloc,
                                                        buildWhen: (previous, state) => state is ChangeDialogTextState,
                                                        builder: (context, state) {
                                                          return Column(
                                                            mainAxisSize: MainAxisSize.min,
                                                            children: [
                                                              Text(
                                                                state is ChangeDialogTextState && state.isSuccess
                                                                    ? 'تحديث ناجح'
                                                                    : state is ChangeDialogTextState && !state.isSuccess
                                                                    ? 'فشل التسديد'
                                                                    : 'جاري التحديث  ....',
                                                                style: AppTextStyle.titleLarge,
                                                              ),
                                                              SizedBox(height: 2),
                                                              if (state is! LoadingState)
                                                                Text(
                                                                  state is ChangeDialogTextState && state.isSuccess
                                                                      ? 'تم انهاء عملية التحديث بنجاح'
                                                                      : 'حدثت مشكلة اثناء عملية التحديث',
                                                                  style: AppTextStyle.labelMedium,
                                                                ),
                                                            ],
                                                          );
                                                        },
                                                      ),
                                                    ],
                                                  ),
                                                ),
                                              ),
                                            );
                                          },
                                  icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/refresh_icon.svg')),
                                );
                              },
                            ),
                          ],
                        ),
                      ),
                      Padding(
                        padding: EdgeInsetsGeometry.symmetric(vertical: 24),
                        child: Stack(
                          children: [
                            Padding(
                              padding: const EdgeInsets.symmetric(horizontal: 24),
                              child: Container(
                                // Keep this in sync with HomeUserPerformance's SizedBox
                                // height for each role - it needs to clear that card.
                                margin: EdgeInsets.only(top: _bloc.isMotaba ? 304 : 238),
                                height: 400,
                                child: Stack(
                                  fit: StackFit.passthrough,
                                  clipBehavior: Clip.none,
                                  alignment: Alignment.center,
                                  children: [
                                    Animate(
                                      effects: [
                                        SlideEffect(
                                          end: Offset(0, 0),
                                          begin: Offset(0, 1),
                                          duration: 1700.ms,
                                          curve: ElasticOutCurve(0.9),
                                        ),
                                      ],
                                      child: Container(
                                        decoration: BoxDecoration(
                                          color: AppColor.surface3,
                                          borderRadius: BorderRadius.circular(16),
                                        ),
                                        child: Image.asset('assets/images/card_background.png', fit: BoxFit.fitHeight),
                                      ),
                                    ),
                                    Positioned(
                                      height: MediaQuery.sizeOf(context).height,
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
                                          colorFilter: ColorFilter.mode(
                                            Color(0xffCCF5F2).withValues(alpha: 0.75),
                                            BlendMode.srcIn,
                                          ),
                                        ),
                                      ),
                                    ),
                                    Animate(
                                      effects: [
                                        SlideEffect(
                                          end: Offset(0, 0),
                                          begin: Offset(0, 1),
                                          duration: 1700.ms,
                                          curve: ElasticOutCurve(0.9),
                                        ),
                                      ],
                                      child: Padding(
                                        padding: const EdgeInsets.all(16.0),
                                        child: Column(
                                          mainAxisSize: MainAxisSize.min,
                                          crossAxisAlignment: CrossAxisAlignment.start,
                                          children: [
                                            Padding(
                                              padding: const EdgeInsets.only(right: 8),
                                              child: Text(
                                                "تقرير القائمة",
                                                style: AppTextStyle.headlineMedium.withColor(AppColor.text2),
                                              ),
                                            ),
                                            const SizedBox(height: 16),
                                            DefaultBuilder<HomeBloc>(
                                              buildWhen: (previous, state) => state.event is ChangeOrderListEvent,
                                              builder: (context, state) {
                                                final orderList = _bloc.currentOrderList!;
                                                return ClipRect(
                                                  child: AnimatedSwitcher(
                                                    duration: const Duration(milliseconds: 350),
                                                    switchInCurve: Curves.easeOut,
                                                    switchOutCurve: Curves.easeIn,
                                                    transitionBuilder: (child, animation) {
                                                      final isIncoming =
                                                          (child.key as ValueKey).value == _bloc.currentOrderListIndex;
                                                      final dir = _bloc.slideDirection;
                                                      final beginOffset =
                                                          isIncoming ? Offset(dir * 0.06, 0) : Offset(-dir * 0.06, 0);
                                                      return SlideTransition(
                                                        position: Tween<Offset>(
                                                          begin: beginOffset,
                                                          end: Offset.zero,
                                                        ).animate(animation),
                                                        child: FadeTransition(opacity: animation, child: child),
                                                      );
                                                    },
                                                    child: Column(
                                                      key: ValueKey(_bloc.currentOrderListIndex),
                                                      mainAxisSize: MainAxisSize.min,
                                                      crossAxisAlignment: CrossAxisAlignment.start,
                                                      children: [
                                                        ReportRow(
                                                          "سعر البيع",
                                                          "مجموع سعر القائمة",
                                                          TextUtil.addComma(orderList.totalSellAmount.toString()),
                                                        ),
                                                        const SizedBox(height: 8),
                                                        ReportRow(
                                                          "الأقساط",
                                                          "مجموع سعر الأقساط",
                                                          TextUtil.addComma(
                                                            orderList.totalDailyInstallmentAmount.toString(),
                                                          ),
                                                        ),
                                                        const SizedBox(height: 8),
                                                        ReportRow(
                                                          "المتراكمات",
                                                          "مجموع تراكمات القائمة",
                                                          TextUtil.addComma(
                                                            orderList.totalOverdueInstallmentAmount.toString(),
                                                          ),
                                                        ),
                                                        const SizedBox(height: 8),
                                                        ReportRow(
                                                          "التسديد",
                                                          "مجموع الأموال المستلمة",
                                                          TextUtil.addComma(
                                                            orderList.totalCollectedInstallmentAmount.toString(),
                                                          ),
                                                        ),
                                                        const SizedBox(height: 8),
                                                        ReportRow(
                                                          "الباقي",
                                                          "مجموع الأموال المتبقية",
                                                          TextUtil.addComma(
                                                            orderList.totalUnpaidInstallmentAmount.toString(),
                                                          ),
                                                        ),
                                                      ],
                                                    ),
                                                  ),
                                                );
                                              },
                                            ),
                                          ],
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ),
                            Animate(
                              effects: [
                                SlideEffect(
                                  end: Offset(0, 0),
                                  begin: Offset(0, -1),
                                  duration: 1700.ms,
                                  curve: ElasticOutCurve(0.9),
                                ),
                              ],
                              child: HomeUserPerformance(),
                            ),
                          ],
                        ),
                      ),
                    ],
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
