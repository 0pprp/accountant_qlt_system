import 'package:collection/collection.dart';
import 'package:flutter/material.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_loading_widget.dart';

class ExpansionOption {
  final int id;
  final String name;

  ExpansionOption({required this.id, required this.name});
}

class DefaultExpansionTile extends StatefulWidget {
  const DefaultExpansionTile({
    super.key,
    required this.valueNotifier,
    this.children,
    required this.controller,
    this.isNumber = false,
    this.onItemChanged,
    this.onExpanded,
  });

  final List<ExpansionOption>? children;
  final ValueNotifier<ExpansionOption?> valueNotifier;
  final ExpansionTileController controller;
  final Function? onExpanded;
  final Function(ExpansionOption)? onItemChanged;
  final bool isNumber;

  @override
  State<DefaultExpansionTile> createState() => _DefaultExpansionTileState();
}

class _DefaultExpansionTileState extends State<DefaultExpansionTile> with SingleTickerProviderStateMixin {
  bool isExpanded = false;
  late AnimationController _animationController;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(duration: const Duration(milliseconds: 300), vsync: this);
    _animation = CurvedAnimation(parent: _animationController, curve: Curves.easeInOut);
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ValueListenableBuilder(
      valueListenable: widget.valueNotifier,
      builder:
          (context, value, child) => Column(
            children: [
              ExpansionTile(
                onExpansionChanged: (value) {
                  setState(() {
                    isExpanded = value;
                  });
                  if (value) {
                    _animationController.forward();
                    widget.onExpanded?.call();
                  } else {
                    _animationController.reverse();
                  }
                },
                collapsedShape: const ContinuousRectangleBorder(
                  borderRadius: BorderRadius.all(Radius.circular(16)),
                  side: BorderSide(color: AppColor.stroke, width: 2),
                ),
                shape: const ContinuousRectangleBorder(
                  borderRadius: BorderRadius.all(Radius.circular(16)),
                  side: BorderSide(color: AppColor.stroke, width: 2),
                ),
                controller: widget.controller,
                title: Text(
                  widget.valueNotifier.value?.name ?? '',
                  style: widget.isNumber ? AppTextStyle.numberMedium : AppTextStyle.bodyMedium,
                ),
                // Remove default children to prevent them from appearing inside the ExpansionTile
                children: const [],
              ),
              // Add spacing and separate container for expanded content with animation
              AnimatedBuilder(
                animation: _animation,
                builder: (context, child) {
                  return SizeTransition(
                    sizeFactor: _animation,
                    child: Column(
                      children: [
                        const SizedBox(height: 8), // Spacing between collapsed and expanded parts
                        FadeTransition(
                          opacity: _animation,
                          child: Container(
                            decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(16),
                              border: Border.all(color: AppColor.stroke, width: 2),
                            ),
                            child:
                                widget.children == null
                                    ? Padding(padding: const EdgeInsets.all(8.0), child: DefaultLoadingWidget())
                                    : widget.children!.length > 4
                                    ? SizedBox(
                                      height: 4 * 56.0, // 4 items * approximate ListTile height
                                      child: ListView.separated(
                                        padding: EdgeInsets.zero,
                                        itemCount: widget.children!.length,
                                        separatorBuilder:
                                            (context, index) => Container(
                                              margin: EdgeInsets.symmetric(horizontal: 16),
                                              height: 1,
                                              color: AppColor.stroke,
                                            ),
                                        itemBuilder: (context, i) {
                                          final e = widget.children![i];
                                          return ListTile(
                                            title: Text(
                                              e.name,
                                              style: (widget.isNumber
                                                      ? AppTextStyle.numberMedium
                                                      : AppTextStyle.bodyMedium)
                                                  .withColor(
                                                    e == widget.valueNotifier.value
                                                        ? AppColor.primary
                                                        : Color(0xff808189),
                                                  ),
                                            ),
                                            onTap: () {
                                              widget.valueNotifier.value = e;
                                              widget.onItemChanged?.call(e);
                                              widget.controller.collapse();
                                            },
                                          );
                                        },
                                      ),
                                    )
                                    : Column(
                                      children:
                                          widget.children!
                                              .mapIndexed(
                                                (i, e) => Column(
                                                  children: [
                                                    ListTile(
                                                      title: Text(
                                                        e.name,
                                                        style: (widget.isNumber
                                                                ? AppTextStyle.numberMedium
                                                                : AppTextStyle.bodyMedium)
                                                            .withColor(
                                                              e == widget.valueNotifier.value
                                                                  ? AppColor.primary
                                                                  : Color(0xff808189),
                                                            ),
                                                      ),
                                                      onTap: () {
                                                        widget.valueNotifier.value = e;
                                                        widget.onItemChanged?.call(e);
                                                        widget.controller.collapse();
                                                      },
                                                    ),
                                                    if (i != widget.children!.length - 1)
                                                      Container(
                                                        margin: EdgeInsets.symmetric(horizontal: 16),
                                                        height: 1,
                                                        color: AppColor.stroke,
                                                      ),
                                                  ],
                                                ),
                                              )
                                              .toList(),
                                    ),
                          ),
                        ),
                      ],
                    ),
                  );
                },
              ),
            ],
          ),
    );
  }
}
