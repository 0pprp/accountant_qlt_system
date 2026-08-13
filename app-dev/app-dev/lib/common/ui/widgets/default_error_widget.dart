import 'package:flutter/material.dart'
    show
        BuildContext,
        Center,
        Color,
        Colors,
        Column,
        EdgeInsets,
        Icon,
        IconButton,
        Icons,
        MainAxisAlignment,
        MediaQuery,
        Padding,
        SizedBox,
        StatelessWidget,
        Text,
        TextAlign,
        TextOverflow,
        TextStyle,
        VoidCallback,
        Widget,
        kToolbarHeight;

class DefaultErrorWidget extends StatelessWidget {
  const DefaultErrorWidget({super.key, required this.error, this.onRetry, this.height, this.textColor});

  final String? error;
  final VoidCallback? onRetry;
  final double? height;
  final Color? textColor;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: height ?? MediaQuery.of(context).size.height - (kToolbarHeight * 5),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: Text(
                error!,
                style: TextStyle(color: textColor ?? Colors.grey[600]),
                maxLines: 1,
                textAlign: TextAlign.center,
                overflow: TextOverflow.ellipsis,
              ),
            ),
            SizedBox(height: 16),
            IconButton(
              icon: Icon(Icons.refresh, color: textColor ?? Colors.grey[400]),
              onPressed: () {
                onRetry?.call();
              },
            ),
          ],
        ),
      ),
    );
  }
}
