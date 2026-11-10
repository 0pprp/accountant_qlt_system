class HomeAnnouncement {
  const HomeAnnouncement({
    required this.id,
    required this.title,
    this.imageUrl,
    this.body,
    this.linkUrl,
  });

  final String id;
  final String title;
  final String? imageUrl;
  final String? body;
  final String? linkUrl;
}
