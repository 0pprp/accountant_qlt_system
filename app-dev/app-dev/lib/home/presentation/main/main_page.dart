import 'dart:io';

import 'package:curved_navigation_bar/curved_navigation_bar.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:get_it/get_it.dart';
import 'package:team/account/mapper.dart';
import 'package:team/account/presentation/profile_page/profile_page.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/snackbar.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/customer/presentation/customers/customers_page.dart';
import 'package:team/home/presentation/home/home_page.dart';
import 'package:team/home/presentation/main/bloc/main_bloc.dart';
import 'package:team/order/presentation/orders/orders_page.dart';
import 'package:team/payment/presentation/payments/payments_page.dart';
import 'package:vector_graphics/vector_graphics.dart';

part 'widgets/main_bottom_navigation_bar.dart';

class MainPage extends StatefulWidget {
  const MainPage({super.key});

  @override
  State<MainPage> createState() => _MainPageState();
}

class _MainPageState extends State<MainPage> {
  DateTime? currentBackPressTime;
  final pageController = PageController(initialPage: 2);
  final currentPage = ValueNotifier<int>(2);
  final isAnimating = ValueNotifier<bool>(false);

  void onWillPop(bool canPop, dynamic res) {
    DateTime now = DateTime.now();
    if (currentBackPressTime == null || now.difference(currentBackPressTime!) > const Duration(milliseconds: 1000)) {
      currentBackPressTime = now;
      if (!GetIt.I.get<AccountMapper>().isMotaba) {
        showToast('انقر مرة أخرى للخروج');
      }
      return;
    }
    SystemNavigator.pop();
  }

  @override
  Widget build(BuildContext context) {
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: const SystemUiOverlayStyle(
        statusBarColor: AppColor.surface1,
        statusBarIconBrightness: Brightness.dark,
        statusBarBrightness: Brightness.dark,
      ),
      child: DefaultListener<MainBloc>(
        listener: (context, state) async {},
        child: SafeArea(
          top: false,
          bottom: kIsWeb ? false : Platform.isIOS,
          child: PopScope(
            canPop: false,
            onPopInvokedWithResult: onWillPop,
            child: Scaffold(
              extendBody: true,
              body: PageView(
                controller: pageController,
                onPageChanged: (value) {
                  if (!isAnimating.value) {
                    currentPage.value = value;
                  }
                },
                children: [
                  CustomersPage(pageController: pageController, currentPage: currentPage),
                  OrdersPage(pageController: pageController, currentPage: currentPage),
                  HomePage(pageController: pageController, currentPage: currentPage),
                  PaymentsPage(pageController: pageController, currentPage: currentPage),
                  ProfilePage(pageController: pageController, currentPage: currentPage, isAnimating: isAnimating),
                ],
              ),
              bottomNavigationBar: ValueListenableBuilder(
                valueListenable: currentPage,
                builder:
                    (context, value, child) => MainBottomNavigationBar(
                      currentPage: currentPage,
                      pageController: pageController,
                      isAnimating: isAnimating,
                    ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
