import 'package:sales_employee_app/features/auth/domain/entities/sales_session.dart';

abstract class AuthRepository {
  Future<SalesSession> login({required String username, required String password});
  Future<bool> refresh();
  Future<void> logout();
  Future<bool> hasSession();
  Future<SalesSession?> currentSession();
}
