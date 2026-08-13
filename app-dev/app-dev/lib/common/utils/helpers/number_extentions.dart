extension IraqiDinarFormatter on double {
  String toIraqiDinarString() {
    String formatted = this % 1 == 0 ? toInt().toString() : toStringAsFixed(2);

    formatted = formatted.replaceAllMapped(RegExp(r'(\d)(?=(\d{3})+(?!\d))'), (match) => '${match[1]},');

    return '$formatted د.ع';
  }
}
