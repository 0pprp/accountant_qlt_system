DateTime baghdadNow() => DateTime.now().toUtc().add(const Duration(hours: 3));

String baghdadDateKey([DateTime? dt]) {
  final b = dt ?? baghdadNow();
  final y = b.year.toString().padLeft(4, '0');
  final m = b.month.toString().padLeft(2, '0');
  final d = b.day.toString().padLeft(2, '0');
  return '$y-$m-$d';
}
