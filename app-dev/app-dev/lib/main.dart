import 'dart:developer';

import 'package:dio/dio.dart';
import 'package:firebase_analytics/firebase_analytics.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:hive_ce_flutter/hive_flutter.dart';
import 'package:team/account/mapper.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/services/navigation/routes.dart';
import 'package:team/common/services/request/request.dart';
import 'package:team/firebase_options.dart';
import 'package:team/injections.dart';

import 'app.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  try {
    await Hive.initFlutter();

    await Injections.ensureInitialized(_onTokenExpire);

    final request = GetIt.I.get<Request>();
    final authMapper = GetIt.I.get<AccountMapper>();
    final authToken = authMapper.authToken();
    if (authToken != null) {
      request.setAuthToken(authToken);
    }
    if (authToken != null) {
      await authMapper.getCurrentAdminUser();
    }
  } catch (e) {
    log(e.toString());
  }

  try {
    Firebase.initializeApp(options: DefaultFirebaseOptions.currentPlatform)
        .then((value) {
          initializeFirebaseCrashlytics();
          FirebaseAnalytics analytics = FirebaseAnalytics.instance;
          analytics.logAppOpen();
        })
        .timeout(Duration(seconds: 5));
  } catch (e) {
    log(e.toString());
  }

  runApp(const App());
}

bool _refreshingToken = false;

Future<void> _onTokenExpire(DioException e, ErrorInterceptorHandler handler) async {
  if (_refreshingToken) {
    return;
  }
  _refreshingToken = true;

  try {
    await GetIt.I.get<AccountMapper>().refreshToken();
  } catch (error) {
    logout();
    rethrow;
  } finally {
    _refreshingToken = false;
  }
}

Future<void> logout() async {
  try {
    await GetIt.I.get<AccountMapper>().logout();
  } catch (e) {
    debugPrint(e.toString());
  }
  Routes.navigationKey.currentState?.pushNamedAndRemoveUntil(RouteNames.main, (route) => false);
}

Future<void> initializeFirebaseCrashlytics() async {
  try {
    // Enable Crashlytics
    FlutterError.onError = FirebaseCrashlytics.instance.recordFlutterError;

    // Log uncaught errors from the Dart layer
    PlatformDispatcher.instance.onError = (error, stack) {
      FirebaseCrashlytics.instance.recordError(error, stack, fatal: true);
      return true;
    };
  } catch (e, stack) {
    debugPrint('Error initializing Firebase Crashlytics: $e');
    debugPrint(stack.toString());
  }
}
