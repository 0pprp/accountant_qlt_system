import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:get_it/get_it.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:vector_graphics/vector_graphics.dart';

class SplashPage extends StatefulWidget {
  const SplashPage({super.key});

  @override
  State<SplashPage> createState() => _SplashPageState();
}

class _SplashPageState extends State<SplashPage> {
  @override
  void initState() {
    Future.delayed(3800.ms, () {
      if (mounted) {
        if (GetIt.I.get<AccountMapper>().isLoggedIn) {
          Navigator.pushReplacementNamed(context, RouteNames.home);
        } else {
          Navigator.pushReplacementNamed(context, RouteNames.entry);
        }
      }
    });
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;
    return Scaffold(
      body: Stack(
        fit: StackFit.passthrough,
        children: [
          Positioned(top: 0, left: 0, child: VectorGraphic(loader: AssetBytesLoader('assets/svg/lines_top_left.svg'))),
          Positioned(
            bottom: 0,
            right: 0,
            left: 0,
            top: 0,
            child: VectorGraphic(loader: AssetBytesLoader('assets/svg/lines_bottom_right.svg'), fit: BoxFit.fill),
          ),
          Center(
            child: Animate(
              effects: [
                FadeEffect(begin: 0, end: 1, delay: 2500.ms, duration: 400.ms, curve: Curves.easeInOut),
                MoveEffect(
                  begin: Offset(0, 35),
                  end: Offset(0, 85),
                  delay: 2500.ms,
                  duration: 400.ms,
                  curve: Curves.easeInOut,
                ),
              ],
              child: Text('قلعة الضمان', style: AppTextStyle.displayMedium.withColor(AppColor.primary)),
            ),
          ),
          Center(
            child: Animate(
              effects: [
                FadeEffect(begin: 0, end: 1, delay: 1600.ms, duration: 300.ms, curve: Curves.easeOutQuint),
                ScaleEffect(
                  begin: Offset(0.5, 0.5),
                  end: Offset(1, 1),
                  delay: 1600.ms,
                  duration: 500.ms,
                  curve: Curves.easeOutQuint,
                ),
              ],
              child: VectorGraphic(loader: AssetBytesLoader('assets/svg/logo_green.svg'), width: 140, height: 102),
            ),
          ),
          Animate(
            effects: [
              ScaleEffect(
                begin: Offset(4, 4),
                end: Offset(0.25, 0.25),
                delay: 500.ms,
                duration: 500.ms,
                curve: Curves.easeOutQuint,
              ),
              ScaleEffect(end: Offset(2.5, 2.5), delay: 1500.ms, duration: 800.ms, curve: Curves.easeOutQuint),
              FadeEffect(begin: 1, delay: 1500.ms, end: 0, duration: 800.ms, curve: Curves.easeOutQuint),
            ],
            child: Container(
              width: size.height,
              height: size.height,
              decoration: ShapeDecoration(shape: CircleBorder(), color: AppColor.primary),
            ),
          ),
        ],
      ),
    );
  }
}
