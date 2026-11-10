import 'package:committee_officer_application/app.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
  await Injection.init();
  runApp(const CommitteeOfficerApp());
}
