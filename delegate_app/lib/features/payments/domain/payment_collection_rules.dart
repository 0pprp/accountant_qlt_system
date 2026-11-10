/// Pure domain rules for collection — no Flutter/UI dependencies.
class PaymentCollectionRules {
  PaymentCollectionRules._();

  static String? validateAmount(double amount) {
    if (!amount.isFinite || amount <= 0) {
      return 'أدخل مبلغ تسديد صالحاً';
    }
    // Same rule as central API (MustBeDivisibleBy1000).
    if ((amount % 1000).abs() >= 0.01) {
      return 'يجب أن يكون المبلغ قابلاً للقسمة على 1000';
    }
    return null;
  }

  /// One collection per customer per Baghdad business day; also block if a
  /// prior attempt is still waiting to sync.
  static bool blocksNewCollection({
    required bool alreadyCollectedToday,
    required bool hasUnsyncedPending,
  }) {
    return alreadyCollectedToday || hasUnsyncedPending;
  }

  static String blockMessage({
    required bool alreadyCollectedToday,
    required bool hasUnsyncedPending,
    bool offlineStaleLock = false,
  }) {
    if (hasUnsyncedPending) {
      return 'يوجد تسديد سابق بانتظار الإرسال — اضغط إرسال التسديدات أولاً';
    }
    if (offlineStaleLock) {
      return 'يوجد تسديد محفوظ محلياً لهذا اليوم ولا يوجد إنترنت لتحديث البيانات';
    }
    if (alreadyCollectedToday) {
      return 'تم تسجيل تسديد لهذا الزبون اليوم على السيرفر';
    }
    return 'تعذر التسديد';
  }
}
