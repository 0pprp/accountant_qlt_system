import 'package:collection/collection.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/order/presentation/create_order/bloc/create_order_bloc.dart';
import 'package:team/order/presentation/create_order/widgets/document_tile.dart';

class CustomerDocumentsStepView extends StatelessWidget {
  const CustomerDocumentsStepView({super.key});

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 24),
      children: const [
        _DualSideCustomerDocumentTile(
          attachmentType: AttachmentType.nationalCard,
          icon: 'trust_receipt_icon.svg',
          title: 'مسح الهوية الوطنية',
          subtitle: 'يجب التقاط الصورة بوضوح تام',
          expandedTitle: 'يرجى مسح هويتك الوطنية',
          expandedSubtitle: 'تأكد من ظهور الاسم والرقم الوطني بوضوح كامل',
        ),
        _DualSideCustomerDocumentTile(
          attachmentType: AttachmentType.residenceCard,
          icon: 'residence_card_icon.svg',
          title: 'مسح بطاقة السكن*',
          subtitle: 'يجب التقاط الصورة بجودة عالية',
          expandedTitle: 'يرجى مسح بطاقة السكن',
          expandedSubtitle: 'يجب أن تكون الصورة حديثة وجميع البيانات مقروءة',
        ),
        _CustomerDocumentTile(
          attachmentType: AttachmentType.rationCard,
          icon: 'ration_card_icon.svg',
          title: 'مسح بطاقة التموينيه',
          subtitle: 'يوصى بالتقاط الصورة بأعلى جودة',
          expandedTitle: 'ارفع صورة بطاقة التموين',
          expandedSubtitle: 'تأكد من وضوح الاسم ورقم البطاقة التموينية',
        ),
        _CustomerDocumentTile(
          attachmentType: AttachmentType.personalPicture,
          icon: 'face_scan_icon.svg',
          title: 'مسح الوجه',
          subtitle: 'يجب وضع الوجه ضمن حدود الصوره',
          expandedTitle: 'التقط صورة سيلفي واضحة للوجه',
          expandedSubtitle: 'يجب أن يكون الوجه في مركز الإطار مع إضاءة كافية',
        ),
        SizedBox(height: 60),
      ],
    );
  }
}

class _CustomerDocumentTile extends StatelessWidget {
  const _CustomerDocumentTile({
    required this.attachmentType,
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.expandedTitle,
    required this.expandedSubtitle,
  });

  final AttachmentType attachmentType;
  final String icon;
  final String title;
  final String subtitle;
  final String expandedTitle;
  final String expandedSubtitle;

  @override
  Widget build(BuildContext context) {
    final bloc = context.read<CreateOrderBloc>();

    return DefaultBuilder<CreateOrderBloc>(
      buildWhen:
          (previous, state) =>
              state.event is UploadOrEditAttachmentEvent &&
              (state.event as UploadOrEditAttachmentEvent).attachmentType == attachmentType,
      builder: (context, state) {
        final attachment = bloc.selectedCustomer?.attachments.firstWhereOrNull(
          (e) => e.type == attachmentType,
        );
        return DocumentTile(
          icon: icon,
          title: title,
          subtitle: subtitle,
          expandedTitle: expandedTitle,
          expandedSubtitle: expandedSubtitle,
          attachment: attachment,
          isDone: attachment?.relativePath != null,
          isUploading:
              state.event is UploadOrEditAttachmentEvent &&
              state is LoadingState &&
              (state.event as UploadOrEditAttachmentEvent).attachmentType == attachmentType,
          imageSelectedCallback: (file) {
            bloc.add(
              UploadOrEditAttachmentEvent(
                xFile: file,
                attachmentType: attachmentType,
                attachmentId: attachment?.id,
              ),
            );
          },
        );
      },
    );
  }
}

class _DualSideCustomerDocumentTile extends StatelessWidget {
  const _DualSideCustomerDocumentTile({
    required this.attachmentType,
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.expandedTitle,
    required this.expandedSubtitle,
  });

  final AttachmentType attachmentType;
  final String icon;
  final String title;
  final String subtitle;
  final String expandedTitle;
  final String expandedSubtitle;

  @override
  Widget build(BuildContext context) {
    final bloc = context.read<CreateOrderBloc>();

    return DefaultBuilder<CreateOrderBloc>(
      buildWhen:
          (previous, state) =>
              state.event is UploadOrEditAttachmentEvent &&
              (state.event as UploadOrEditAttachmentEvent).attachmentType == attachmentType,
      builder: (context, state) {
        final typed = bloc.selectedCustomer?.attachments.where((e) => e.type == attachmentType).toList() ?? [];
        final frontAttachment = typed.elementAtOrNull(0);
        final backAttachment = typed.elementAtOrNull(1);

        final isUploading =
            state.event is UploadOrEditAttachmentEvent &&
            state is LoadingState &&
            (state.event as UploadOrEditAttachmentEvent).attachmentType == attachmentType;

        return DualSideDocumentTile(
          icon: icon,
          title: title,
          subtitle: subtitle,
          expandedTitle: expandedTitle,
          expandedSubtitle: expandedSubtitle,
          frontAttachment: frontAttachment,
          backAttachment: backAttachment,
          isDone: frontAttachment?.relativePath != null && backAttachment?.relativePath != null,
          isUploading: isUploading,
          frontImageSelectedCallback: (file) {
            bloc.add(
              UploadOrEditAttachmentEvent(
                xFile: file,
                attachmentType: attachmentType,
                attachmentId: frontAttachment?.id,
              ),
            );
          },
          backImageSelectedCallback: (file) {
            bloc.add(
              UploadOrEditAttachmentEvent(
                xFile: file,
                attachmentType: attachmentType,
                attachmentId: backAttachment?.id,
              ),
            );
          },
        );
      },
    );
  }
}
