import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import '../../injection.dart';
import '../network/dio_client.dart';

class CustomerPhotoButton extends StatefulWidget {
  const CustomerPhotoButton({
    super.key,
    required this.listId,
    required this.customerId,
    this.hasPhoto = false,
    this.onBusy,
    this.onSaved,
  });
  final int listId;
  final int customerId;
  final bool hasPhoto;
  final ValueChanged<bool>? onBusy;
  final VoidCallback? onSaved;

  @override
  State<CustomerPhotoButton> createState() => _CustomerPhotoButtonState();
}

class _CustomerPhotoButtonState extends State<CustomerPhotoButton> {
  bool busy = false;
  late bool saved = widget.hasPhoto;

  @override
  void didUpdateWidget(covariant CustomerPhotoButton oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.hasPhoto && !saved) saved = true;
  }

  Future<void> pick() async {
    if (busy) return;
    setState(() => busy = true);
    widget.onBusy?.call(true);
    try {
      final source = await showModalBottomSheet<ImageSource>(context: context, builder: (sheet) => SafeArea(
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          ListTile(leading: const Icon(Icons.photo_camera), title: const Text('التقاط بالكاميرا'),
            onTap: () => Navigator.pop(sheet, ImageSource.camera)),
          ListTile(leading: const Icon(Icons.photo_library), title: const Text('اختيار من المعرض'),
            onTap: () => Navigator.pop(sheet, ImageSource.gallery)),
        ])));
      if (source == null || !mounted) return;
      final image = await ImagePicker().pickImage(source: source, maxWidth: 1600, imageQuality: 80);
      if (image == null || !mounted) return;
      if (await image.length() > 2000000) {
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('اختر صورة أصغر من 2 ميغابايت')));
        return;
      }
      if (!mounted) return;
      await Injection.committee.uploadCustomerPhoto(widget.listId, widget.customerId, image);
      if (mounted) setState(() => saved = true);
      widget.onSaved?.call();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(DioClient.mapError(e))));
    } finally {
      if (mounted) {
        setState(() => busy = false);
        widget.onBusy?.call(false);
      }
    }
  }
  @override
  Widget build(BuildContext context) => OutlinedButton.icon(onPressed: busy ? null : pick,
    icon: Icon(saved ? Icons.check_circle : Icons.add_a_photo_outlined),
    label: Text(busy ? 'جارٍ إرفاق الصورة…' : saved ? 'الصورة محفوظة · استبدال' : 'التقاط صورة'));
}
