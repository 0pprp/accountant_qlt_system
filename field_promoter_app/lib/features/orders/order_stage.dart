abstract final class OrderStages {
  static const steps = ['جديد', 'في المبيعات', 'قيد الكشف', 'تم البيع', 'مكتمل'];

  static const _inspection = {
    'قيد التنفيذ',
    'تم الكشف',
    'مُجهّز للبيع',
    'محوّل لمسودة بيع',
  };

  static bool isRejected(String stage) => stage == 'rejected';

  static int stepIndex(String stage, String statusLabel) {
    switch (stage) {
      case 'finished':
        return 4;
      case 'sold':
        return 3;
      case 'rejected':
        return -1;
    }
    final label = statusLabel.trim();
    if (label == 'جديد' || label.isEmpty) return 0;
    if (_inspection.contains(label)) return 2;
    return 1;
  }
}
