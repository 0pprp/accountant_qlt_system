import 'package:cached_network_image/cached_network_image.dart';
import 'package:cached_network_image_platform_interface/cached_network_image_platform_interface.dart';
import 'package:flutter/material.dart';
import 'package:team/injections.dart';

class DefaultCachedNetworkImage extends StatelessWidget {
  final String? imageUrl;
  final BoxFit fit;
  final int? maxHeightDiskCache;
  final int? maxWidthDiskCache;
  final double? borderRadius;
  final Widget errorWidget;
  final Widget? placeholder;
  final double? width;
  final double? height;
  final bool isAvatar;
  final Widget Function(BuildContext, ImageProvider)? imageBuilder;

  const DefaultCachedNetworkImage({
    super.key,
    this.fit = BoxFit.cover,
    this.borderRadius,
    this.imageUrl,
    this.isAvatar = false,
    this.maxHeightDiskCache,
    this.maxWidthDiskCache,
    this.errorWidget = const SizedBox(height: 100, width: 100),
    this.placeholder,
    this.width,
    this.height,
    this.imageBuilder,
  });

  @override
  Widget build(BuildContext context) {
    if (imageUrl == null) {
      if (isAvatar) {
        return CircleAvatar(
          radius: height == null ? 62 : height! / 2,
          backgroundColor: Color(0xFF0A6E61),
          child: Icon(Icons.person, color: Colors.white, size: height == null ? 70 : height! / 1.5),
        );
      } else {
        return errorWidget;
      }
    }
    return ClipRRect(
      borderRadius: borderRadius != null ? BorderRadius.circular(borderRadius!) : BorderRadius.zero,
      child: CachedNetworkImage(
        imageUrl: imageUrl!.startsWith('http') ? imageUrl! : Injections.baseUrl + imageUrl!,
        fit: fit,
        imageRenderMethodForWeb: ImageRenderMethodForWeb.HttpGet,
        width: width,
        height: height,
        placeholder: placeholder == null ? null : (context, url) => placeholder!,
      ),
    );
  }
}
