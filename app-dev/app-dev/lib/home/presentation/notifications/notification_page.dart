import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get_it/get_it.dart';
import 'package:team/common/ui/widgets/default_appbar.dart';
import 'package:team/common/ui/widgets/default_error_widget.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/home/infrastructure/repositories/home_repository.dart';
import 'package:team/home/presentation/notifications/bloc/notifications_bloc.dart';
import 'package:team/home/presentation/notifications/widgets/notification_widget.dart';
import 'package:vector_graphics/vector_graphics.dart';

class NotificationsPage extends StatelessWidget {
  const NotificationsPage({super.key});

  @override
  Widget build(BuildContext context) {
    return DefaultBlocProvider(
      create: (BuildContext context) => NotificationsBloc(homeRepository: GetIt.I.get<HomeRepository>()),
      child: NotificationsPageView(),
    );
  }
}

class NotificationsPageView extends StatefulWidget {
  const NotificationsPageView({super.key});

  @override
  State<NotificationsPageView> createState() => _NotificationsPageViewState();
}

class _NotificationsPageViewState extends State<NotificationsPageView> {
  late final _bloc = context.read<NotificationsBloc>();

  @override
  void initState() {
    _bloc.add(InitialNotificationsEvent());
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: DefaultAppBar(
        title: 'الأشعارات',
        showLines: true,
        firstIcon: IconButton(
          onPressed: () {},
          icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/notification_icon.svg')),
        ),
      ),
      body: Stack(
        fit: StackFit.expand,
        children: [
          Positioned(
            height: MediaQuery.sizeOf(context).height,
            child: Animate(
              effects: [
                ScaleEffect(duration: 700.ms, begin: Offset(1.3, 1.3), end: Offset(1, 1), curve: Curves.easeOutQuad),
              ],
              child: VectorGraphic(
                loader: AssetBytesLoader('assets/svg/lines_bottom_right.svg'),
                fit: BoxFit.fill,
                colorFilter: ColorFilter.mode(Color(0xffCCF5F2).withValues(alpha: 0.75), BlendMode.srcIn),
              ),
            ),
          ),
          DefaultBuilder<NotificationsBloc>(
            buildWhen: (previous, state) => state.event is InitialNotificationsEvent,
            builder: (context, state) {
              if (state is ErrorState) {
                return DefaultErrorWidget(error: state.error, onRetry: () => _bloc.add(InitialNotificationsEvent()));
              }
              if (state is ResponseState) {
                return Animate(
                  effects: [
                    SlideEffect(
                      begin: Offset(0, -1.2),
                      duration: Duration(milliseconds: 1500),
                      end: Offset(0, 0),
                      curve: ElasticOutCurve(1),
                    ),
                    FadeEffect(begin: 0, end: 1, duration: Duration(milliseconds: 1500), curve: Curves.easeOut),
                  ],
                  child: ListView.builder(
                    padding: EdgeInsets.symmetric(horizontal: 24, vertical: 8),
                    itemCount: 10,
                    itemBuilder: (context, index) {
                      return NotificationWidget(isNew: index > 1);
                    },
                  ),
                );
              }
              return const DefaultLoadingWidget();
            },
          ),
        ],
      ),
    );
  }
}
