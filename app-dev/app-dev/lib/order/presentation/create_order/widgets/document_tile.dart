import 'dart:io';

import 'package:dotted_border/dotted_border.dart';
import 'package:flutter/material.dart';
import 'package:flutter_image_compress/flutter_image_compress.dart';
import 'package:image_picker/image_picker.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_cached_network_image.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';
import 'package:vector_graphics/vector_graphics_compat.dart';

/// Compresses [file] using the platform's native codecs (fast).
/// Falls back to the original file if compression fails for any reason.
Future<XFile> _compressImage(XFile file) async {
  try {
    final targetPath = '${Directory.systemTemp.path}/compressed_${DateTime.now().millisecondsSinceEpoch}.jpg';
    final result = await FlutterImageCompress.compressAndGetFile(
      file.path,
      targetPath,
      quality: 75,
      minWidth: 1280,
      minHeight: 1280,
      format: CompressFormat.jpeg,
    );
    return result ?? file;
  } catch (_) {
    return file; // compression failed — just use the original
  }
}

class DocumentTile extends StatefulWidget {
  final String icon;
  final String title;
  final String subtitle;
  final String expandedTitle;
  final String expandedSubtitle;
  final AttachmentInformation? attachment;
  final bool isDone;
  final bool isUploading;
  final void Function(XFile image)? imageSelectedCallback;

  const DocumentTile({
    super.key,
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.expandedTitle,
    required this.expandedSubtitle,
    required this.attachment,
    required this.isDone,
    this.isUploading = false,
    this.imageSelectedCallback,
  });

  @override
  State<DocumentTile> createState() => _DocumentTileState();
}

class _DocumentTileState extends State<DocumentTile> with SingleTickerProviderStateMixin {
  late final AnimationController _controller = AnimationController(
    vsync: this,
    duration: const Duration(milliseconds: 350),
  );

  late final Animation<double> _animation = CurvedAnimation(
    parent: _controller,
    curve: Curves.easeInOut,
  );

  bool _expanded = false;
  final _image = ValueNotifier<XFile?>(null);

  void _toggle() {
    setState(() => _expanded = !_expanded);
    _expanded ? _controller.forward() : _controller.reverse();
  }

  Future<void> _pickImage() async {
    final source = await showModalBottomSheet<ImageSource>(
      context: context,
      backgroundColor: Colors.transparent,
      builder: (context) => const _ImageSourceSheet(),
    );
    if (source == null) return;
    final picked = await ImagePicker().pickImage(source: source);
    if (picked == null) return;
    final compressed = await _compressImage(picked);
    _image.value = compressed;
    widget.imageSelectedCallback?.call(compressed);
  }

  @override
  void dispose() {
    _controller.dispose();
    _image.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: AnimatedBuilder(
        animation: _animation,
        builder:
            (context, _) => Container(
              decoration: BoxDecoration(
                color: AppColor.surface1,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(
                  color: _expanded ? AppColor.primary : AppColor.stroke,
                  width: 1.5,
                ),
              ),
              child: Column(
                children: [
                  // ── Header ────────────────────────────────────────────────────
                  InkWell(
                    onTap: _toggle,
                    borderRadius: BorderRadius.circular(16),
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
                      child: Row(
                        children: [
                          Container(
                            width: 44,
                            height: 44,
                            decoration: BoxDecoration(
                              color: AppColor.surface2,
                              borderRadius: BorderRadius.circular(16),
                            ),
                            child: Center(
                              child: VectorGraphic(
                                loader: AssetBytesLoader('assets/svg/${widget.icon}'),
                              ),
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              spacing: 2,
                              children: [
                                Text(
                                  widget.title,
                                  style: AppTextStyle.headlineMedium.withColor(AppColor.text1),
                                ),
                                Text(
                                  widget.subtitle,
                                  style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                                ),
                              ],
                            ),
                          ),
                          if (!widget.isUploading)
                            AnimatedContainer(
                              duration: const Duration(milliseconds: 300),
                              width: 20,
                              height: 20,
                              decoration: BoxDecoration(
                                shape: BoxShape.circle,
                                color: (widget.isDone ? AppColor.success : AppColor.surface3).withValues(alpha: 0.5),
                              ),
                              child: Icon(
                                Icons.check,
                                size: 14,
                                color: widget.isDone ? AppColor.success : AppColor.surface3,
                              ),
                            ),
                          if (widget.isUploading)
                            SizedBox(
                              width: 20,
                              height: 20,
                              child: DefaultLoadingWidget(
                                color: AppColor.primary,
                              ),
                            ),
                          const SizedBox(width: 8),
                          RotationTransition(
                            turns: Tween(begin: 0.0, end: 0.5).animate(_animation),
                            child: VectorGraphic(
                              loader: AssetBytesLoader('assets/svg/arrow_down_icon.svg'),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),

                  // ── Expanded content ──────────────────────────────────────────
                  SizeTransition(
                    sizeFactor: _animation,
                    alignment: Alignment(-1, 0),
                    child: Padding(
                      padding: const EdgeInsets.only(left: 16, right: 16, bottom: 16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Divider(color: AppColor.stroke),
                          Padding(
                            padding: const EdgeInsets.only(top: 12, bottom: 8),
                            child: Text(
                              widget.expandedTitle,
                              style: AppTextStyle.bodyMedium.withColor(AppColor.text1),
                            ),
                          ),
                          Text(
                            widget.expandedSubtitle,
                            style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                          ),
                          const SizedBox(height: 16),
                          ValueListenableBuilder<XFile?>(
                            valueListenable: _image,
                            builder:
                                (context, image, _) => _DocumentImageUpload(
                                  image: image,
                                  attachment: widget.attachment,
                                  onPickImage: _pickImage,
                                ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
      ),
    );
  }
}

// ── Dual-side document tile (front + back) ────────────────────────────────────

class DualSideDocumentTile extends StatefulWidget {
  final String icon;
  final String title;
  final String subtitle;
  final String expandedTitle;
  final String expandedSubtitle;
  final AttachmentInformation? frontAttachment;
  final AttachmentInformation? backAttachment;
  final bool isDone;
  final bool isUploading;
  final void Function(XFile image)? frontImageSelectedCallback;
  final void Function(XFile image)? backImageSelectedCallback;

  const DualSideDocumentTile({
    super.key,
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.expandedTitle,
    required this.expandedSubtitle,
    required this.frontAttachment,
    required this.backAttachment,
    required this.isDone,
    this.isUploading = false,
    this.frontImageSelectedCallback,
    this.backImageSelectedCallback,
  });

  @override
  State<DualSideDocumentTile> createState() => _DualSideDocumentTileState();
}

class _DualSideDocumentTileState extends State<DualSideDocumentTile> with SingleTickerProviderStateMixin {
  late final AnimationController _controller = AnimationController(
    vsync: this,
    duration: const Duration(milliseconds: 350),
  );

  late final Animation<double> _animation = CurvedAnimation(
    parent: _controller,
    curve: Curves.easeInOut,
  );

  bool _expanded = false;
  int _selectedSide = 0; // 0 = front, 1 = back
  final _frontImage = ValueNotifier<XFile?>(null);
  final _backImage = ValueNotifier<XFile?>(null);

  void _toggle() {
    setState(() => _expanded = !_expanded);
    _expanded ? _controller.forward() : _controller.reverse();
  }

  Future<void> _pickImage() async {
    final source = await showModalBottomSheet<ImageSource>(
      context: context,
      backgroundColor: Colors.transparent,
      builder: (context) => const _ImageSourceSheet(),
    );
    if (source == null) return;
    // capture the side now — the user could switch tabs while the picker is open
    final side = _selectedSide;
    final picked = await ImagePicker().pickImage(source: source);
    if (picked == null) return;
    final compressed = await _compressImage(picked);
    if (side == 0) {
      _frontImage.value = compressed;
      widget.frontImageSelectedCallback?.call(compressed);
    } else {
      _backImage.value = compressed;
      widget.backImageSelectedCallback?.call(compressed);
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    _frontImage.dispose();
    _backImage.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: AnimatedBuilder(
        animation: _animation,
        builder:
            (context, _) => Container(
              decoration: BoxDecoration(
                color: AppColor.surface1,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(
                  color: _expanded ? AppColor.primary : AppColor.stroke,
                  width: 1.5,
                ),
              ),
              child: Column(
                children: [
                  // ── Header ──────────────────────────────────────────────────────
                  InkWell(
                    onTap: _toggle,
                    borderRadius: BorderRadius.circular(16),
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
                      child: Row(
                        children: [
                          Container(
                            width: 44,
                            height: 44,
                            decoration: BoxDecoration(
                              color: AppColor.surface2,
                              borderRadius: BorderRadius.circular(16),
                            ),
                            child: Center(
                              child: VectorGraphic(
                                loader: AssetBytesLoader('assets/svg/${widget.icon}'),
                              ),
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              spacing: 2,
                              children: [
                                Text(
                                  widget.title,
                                  style: AppTextStyle.headlineMedium.withColor(AppColor.text1),
                                ),
                                Text(
                                  widget.subtitle,
                                  style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                                ),
                              ],
                            ),
                          ),
                          if (!widget.isUploading)
                            AnimatedContainer(
                              duration: const Duration(milliseconds: 300),
                              width: 20,
                              height: 20,
                              decoration: BoxDecoration(
                                shape: BoxShape.circle,
                                color: (widget.isDone ? AppColor.success : AppColor.surface3).withValues(alpha: 0.5),
                              ),
                              child: Icon(
                                Icons.check,
                                size: 14,
                                color: widget.isDone ? AppColor.success : AppColor.surface3,
                              ),
                            ),
                          if (widget.isUploading)
                            SizedBox(
                              width: 20,
                              height: 20,
                              child: DefaultLoadingWidget(color: AppColor.primary),
                            ),
                          const SizedBox(width: 8),
                          RotationTransition(
                            turns: Tween(begin: 0.0, end: 0.5).animate(_animation),
                            child: VectorGraphic(
                              loader: AssetBytesLoader('assets/svg/arrow_down_icon.svg'),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),

                  // ── Expanded content ─────────────────────────────────────────────
                  SizeTransition(
                    sizeFactor: _animation,
                    alignment: const Alignment(-1, 0),
                    child: Padding(
                      padding: const EdgeInsets.only(left: 16, right: 16, bottom: 16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Divider(color: AppColor.stroke),
                          Padding(
                            padding: const EdgeInsets.only(top: 12, bottom: 8),
                            child: Text(
                              '${widget.expandedTitle}: '
                              '${_selectedSide == 0 ? 'الواجهة الأمامية' : 'الواجهة الخلفية'}',
                              style: AppTextStyle.bodyMedium.withColor(AppColor.text1),
                            ),
                          ),
                          Text(
                            widget.expandedSubtitle,
                            style: AppTextStyle.bodySmall.withColor(AppColor.text2),
                          ),

                          // Side selector
                          const SizedBox(height: 43),
                          Stack(
                            fit: StackFit.passthrough,
                            clipBehavior: Clip.none,
                            children: [
                              ValueListenableBuilder<XFile?>(
                                valueListenable: _selectedSide == 0 ? _frontImage : _backImage,
                                builder:
                                    (context, image, _) => _DocumentImageUpload(
                                      image: image,
                                      attachment: _selectedSide == 0 ? widget.frontAttachment : widget.backAttachment,
                                      onPickImage: _pickImage,
                                    ),
                              ),
                              Positioned(
                                top: -27,
                                right: 8,
                                left: 8,
                                child: Container(
                                  padding: EdgeInsets.all(8),
                                  decoration: BoxDecoration(
                                    color: AppColor.white,
                                    border: Border.all(color: AppColor.stroke, width: 1.5),
                                    borderRadius: BorderRadius.circular(18),
                                  ),
                                  child: Row(
                                    spacing: 8,
                                    children: [
                                      Expanded(
                                        child: _SideTab(
                                          label: 'الواجهة الأمامية',
                                          icon: 'card_front_icon.svg',
                                          isSelected: _selectedSide == 0,
                                          onTap: () => setState(() => _selectedSide = 0),
                                        ),
                                      ),
                                      Expanded(
                                        child: _SideTab(
                                          label: 'الواجهة الخلفية',
                                          icon: 'card_back_icon.svg',
                                          isSelected: _selectedSide == 1,
                                          onTap: () => setState(() => _selectedSide = 1),
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
      ),
    );
  }
}

class _SideTab extends StatelessWidget {
  final String label;
  final String icon;
  final bool isSelected;
  final VoidCallback onTap;

  const _SideTab({
    required this.label,
    required this.icon,
    required this.isSelected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 4),
        decoration: BoxDecoration(
          color: isSelected ? AppColor.surface4 : Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected ? AppColor.primary : AppColor.white,
            width: 1.5,
          ),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          spacing: 6,
          children: [
            VectorGraphic(
              loader: AssetBytesLoader('assets/svg/$icon'),
              colorFilter: ColorFilter.mode(
                AppColor.black,
                BlendMode.srcIn,
              ),
            ),
            Text(
              label,
              style: AppTextStyle.bodySmall.withColor(
                AppColor.black,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ── Image upload area ─────────────────────────────────────────────────────────

class _DocumentImageUpload extends StatelessWidget {
  final XFile? image;
  final AttachmentInformation? attachment;
  final VoidCallback onPickImage;

  const _DocumentImageUpload({
    required this.image,
    required this.attachment,
    required this.onPickImage,
  });

  bool get _hasImage => image != null || attachment?.relativePath != null;

  @override
  Widget build(BuildContext context) {
    return DottedBorder(
      options: RoundedRectDottedBorderOptions(
        radius: const Radius.circular(20),
        dashPattern: const [6, 4],
        color: _hasImage ? AppColor.surface1 : AppColor.text4,
        strokeWidth: 2,
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(20),
        child: SizedBox(
          height: 250,
          width: double.infinity,
          child: Material(
            color: Colors.transparent,
            child:
                _hasImage
                    ? Stack(
                      children: [
                        Positioned.fill(
                          child:
                              image != null
                                  ? Image.file(File(image!.path), fit: BoxFit.contain)
                                  : DefaultCachedNetworkImage(
                                    imageUrl: attachment!.relativePath,
                                    fit: BoxFit.contain,
                                  ),
                        ),
                        Positioned.fill(
                          child: ColoredBox(color: AppColor.black.withValues(alpha: 0.1)),
                        ),
                        Center(
                          child: FilledButton(
                            onPressed: onPickImage,
                            style: FilledButton.styleFrom(
                              backgroundColor: AppColor.surface1,
                              foregroundColor: AppColor.primary,
                              padding: const EdgeInsets.symmetric(vertical: 0, horizontal: 40),
                            ),
                            child: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Text(
                                  'تعديل ',
                                  style: AppTextStyle.headlineMedium.withColor(AppColor.primary),
                                ),
                                VectorGraphic(
                                  loader: AssetBytesLoader('assets/svg/edit_pen_icon.svg'),
                                  colorFilter: ColorFilter.mode(AppColor.primary, BlendMode.srcIn),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ],
                    )
                    : InkWell(
                      onTap: onPickImage,
                      child: Center(
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            VectorGraphic(
                              loader: AssetBytesLoader('assets/svg/images_icon.svg'),
                            ),
                            Padding(
                              padding: const EdgeInsets.only(top: 16, bottom: 8),
                              child: Text(
                                'أضغط للرفع',
                                style: AppTextStyle.headlineMedium.withColor(AppColor.primary),
                              ),
                            ),
                            Text(
                              'JPG, JPEG, PNG',
                              style: AppTextStyle.bodyMedium.copyWith(
                                color: AppColor.text4,
                                fontFamily: '',
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
          ),
        ),
      ),
    );
  }
}

// ── Image source bottom sheet ─────────────────────────────────────────────────

class _ImageSourceSheet extends StatelessWidget {
  const _ImageSourceSheet();

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: AppColor.surface1, // replace with your sheet background
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'رفع صورة',
            style: AppTextStyle.headlineMedium.withColor(AppColor.text2),
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: _SourceCard(
                  icon: 'assets/svg/gallery_icon.svg',
                  label: 'معرض الصور',
                  onTap: () => Navigator.pop(context, ImageSource.gallery),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _SourceCard(
                  // replace these svg paths with your actual icons
                  icon: 'assets/svg/camera_icon.svg',
                  label: 'آلة تصوير',
                  onTap: () => Navigator.pop(context, ImageSource.camera),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Align(
            alignment: Alignment.center,
            child: Container(
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: AppColor.stroke,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _SourceCard extends StatelessWidget {
  final String icon;
  final String label;
  final VoidCallback onTap;

  const _SourceCard({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 24),
        decoration: BoxDecoration(
          color: AppColor.surface2,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 56,
              height: 56,
              decoration: BoxDecoration(
                color: AppColor.primary.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(16),
              ),
              child: Center(
                child: VectorGraphic(
                  loader: AssetBytesLoader(icon),
                  colorFilter: ColorFilter.mode(AppColor.primary, BlendMode.srcIn),
                ),
              ),
            ),
            const SizedBox(height: 12),
            Text(label, style: AppTextStyle.bodyMedium.withColor(AppColor.text1)),
          ],
        ),
      ),
    );
  }
}
