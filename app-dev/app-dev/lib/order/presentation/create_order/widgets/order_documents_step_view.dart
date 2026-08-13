import 'package:collection/collection.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:team/account/domain/user/user.dart';
import 'package:team/common/utils/bloc/default_bloc.dart';
import 'package:team/order/presentation/create_order/bloc/create_order_bloc.dart';
import 'package:team/order/presentation/create_order/widgets/document_tile.dart';

class OrderDocumentsStepView extends StatelessWidget {
  const OrderDocumentsStepView({super.key});

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 24),
      children: const [
        _OrderDocumentTile(
          attachmentType: AttachmentType.purchaseReceipt,
          icon: 'purchase_receipt_icon.svg',
          title: 'وصل الشراء',
          subtitle: 'تصوير وصل شراء المبيع بوضوح',
          expandedTitle: 'ارفع صورة وصل الشراء',
          expandedSubtitle: 'تأكد من وضوح جميع تفاصيل وصل الشراء قبل الرفع',
        ),
        _OrderDocumentTile(
          attachmentType: AttachmentType.trustReceipt,
          icon: 'trust_receipt_icon.svg',
          title: 'وصل أمانة',
          subtitle: 'يرجى التقط صورة لوصل الأمانة',
          expandedTitle: 'ارفع صورة وصل الأمانة',
          expandedSubtitle: 'تأكد من وضوح جميع تفاصيل وصل الأمانة قبل الرفع',
        ),
        _OrderDocumentTile(
          attachmentType: AttachmentType.saleContract,
          icon: 'sale_contract_icon.svg',
          title: 'عقد البيع',
          subtitle: 'التقط الصورة بأعلى جودة للعقد',
          expandedTitle: 'ارفع صورة عقد البيع',
          expandedSubtitle: 'تأكد من وضوح جميع بنود العقد قبل الرفع',
        ),
        SizedBox(height: 60),
      ],
    );
  }
}

class _OrderDocumentTile extends StatelessWidget {
  const _OrderDocumentTile({
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
        final attachment = bloc.orderAttachments.firstWhereOrNull(
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
