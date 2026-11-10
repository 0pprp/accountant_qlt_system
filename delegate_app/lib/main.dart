import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'app.dart';
import 'injection.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
  await Injection.init();
  await Injection.paymentSyncService.start();
  runApp(const DelegateApp());
}
