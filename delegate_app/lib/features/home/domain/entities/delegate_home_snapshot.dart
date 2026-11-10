import 'home_announcement.dart';

enum AnnouncementsLoadState {
  loading,
  successEmpty,
  successData,
  forbidden,
  error,
}

class DelegateHomeSnapshot {
  const DelegateHomeSnapshot({
    required this.delegateName,
    required this.listName,
    required this.currentRating,
    required this.previousWeekRating,
    required this.trend,
    required this.installmentSum,
    required this.yesterdayIncome,
    required this.announcements,
    required this.announcementsState,
    this.announcementsError,
    this.avatarUrl,
    this.tutorialVideoUrl,
  });

  final String delegateName;
  final String listName;
  final double currentRating;
  final double previousWeekRating;

  /// `up` | `down` | `flat`
  final String trend;
  final double installmentSum;
  final double yesterdayIncome;
  final List<HomeAnnouncement> announcements;
  final AnnouncementsLoadState announcementsState;
  final String? announcementsError;
  final String? avatarUrl;
  final String? tutorialVideoUrl;
}
