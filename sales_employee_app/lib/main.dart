import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:sales_employee_app/app.dart';
import 'package:sales_employee_app/injection.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await SystemChrome.setPreferredOrientations([
    DeviceOrientation.portraitUp,
    DeviceOrientation.portraitDown,
  ]);
  await Injection.init();
  runApp(const SalesEmployeeApp());
}
