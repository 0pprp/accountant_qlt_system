import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:team/common/services/navigation/route_names.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/order/presentation/order_details/bloc/order_details_bloc.dart';
import 'package:vector_graphics/vector_graphics.dart';

class OrderHeaderButtons extends StatelessWidget {
  const OrderHeaderButtons({super.key});

  @override
  Widget build(BuildContext context) {
    final bloc = context.read<OrderDetailsBloc>();
    return Row(
      children: [
        Expanded(
          child: FilledButton(
            onPressed:
                () =>
                    bloc.user?.id != null
                        ? Navigator.pushNamed(context, RouteNames.customerOrders, arguments: {'userId': bloc.user?.id})
                        : null,
            style: FilledButton.styleFrom(
              backgroundColor: Colors.white,
              foregroundColor: AppColor.primary,
              padding: EdgeInsets.zero,
            ),
            child: Stack(
              alignment: Alignment.center,
              fit: StackFit.expand,
              children: [
                ClipRRect(
                  borderRadius: BorderRadius.circular(16),
                  child: Transform.flip(
                    flipX: true,
                    child: VectorGraphic(
                      loader: AssetBytesLoader('assets/svg/button_background.svg'),
                      fit: BoxFit.fill,
                    ),
                  ),
                ),
                Center(child: Text('مبيعات العميل', style: AppTextStyle.bodySmall.withColor(AppColor.primary))),
              ],
            ),
          ),
        ),
        SizedBox(width: 24),
        Expanded(
          child: FilledButton(
            onPressed:
                () =>
                    bloc.user?.id != null
                        ? Navigator.pushNamed(context, RouteNames.customerPayments, arguments: bloc.user?.id)
                        : null,
            style: FilledButton.styleFrom(
              backgroundColor: Colors.white,
              foregroundColor: AppColor.primary,
              padding: EdgeInsets.zero,
            ),
            child: Stack(
              alignment: Alignment.center,
              fit: StackFit.expand,
              children: [
                ClipRRect(
                  borderRadius: BorderRadius.circular(16),
                  child: VectorGraphic(loader: AssetBytesLoader('assets/svg/button_background.svg'), fit: BoxFit.fill),
                ),
                Center(child: Text('تسديدات العميل', style: AppTextStyle.bodySmall.withColor(AppColor.primary))),
              ],
            ),
          ),
        ),
      ],
    );
  }
}
