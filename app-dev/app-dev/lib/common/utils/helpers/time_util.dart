class TimeUtil {
  ///Duration in a manner form of string
  ///ex:
  ///02:04:32
  ///43:20
  ///
  ///Supports Hour , Minute and Second
  static String formatTime(Duration duration) {
    String strDigits(int n) => n.toString().padLeft(2, '0');

    final seconds = strDigits(duration.inSeconds.remainder(60));
    final minutes = strDigits(duration.inMinutes.remainder(60));
    final hour = strDigits(duration.inHours.remainder(60));

    if (hour == '00') {
      return '$minutes:$seconds';
    }

    return '$hour:$minutes:$seconds';
  }
}

extension DateTimeExtensions on DateTime {
  /// Checks if this DateTime is on the same day as another DateTime
  ///
  /// Example:
  /// ```dart
  /// final date1 = DateTime(2024, 1, 15, 10, 30);
  /// final date2 = DateTime(2024, 1, 15, 18, 45);
  /// print(date1.isSameDay(date2)); // true
  /// ```
  bool isSameDay(DateTime other) {
    return year == other.year && month == other.month && day == other.day;
  }
}
