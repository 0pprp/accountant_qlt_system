class City {
  const City({
    required this.name,
    required this.linkDelegate,
  });

  final String name;
  /// Normalized HTTPS base ending with `/`.
  final String linkDelegate;
}
