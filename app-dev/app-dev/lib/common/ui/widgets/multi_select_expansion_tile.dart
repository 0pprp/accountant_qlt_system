import 'package:flutter/material.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/deafult_expansion_tile.dart';
import 'package:team/order/domain/order_status.dart';

/// Multi-select expansion dropdown matching [DefaultExpansionTile] styling.
class MultiSelectExpansionTile<T> extends StatefulWidget {
  const MultiSelectExpansionTile({
    super.key,
    required this.options,
    required this.selectedValues,
    required this.labelBuilder,
    required this.controller,
    this.hint = '',
    this.onChanged,
    this.onExpanded,
  });

  final List<T> options;
  final ValueNotifier<List<T>> selectedValues;
  final String Function(T value) labelBuilder;
  final ExpansionTileController controller;
  final String hint;
  final ValueChanged<List<T>>? onChanged;
  final VoidCallback? onExpanded;

  @override
  State<MultiSelectExpansionTile<T>> createState() => _MultiSelectExpansionTileState<T>();
}

class _MultiSelectExpansionTileState<T> extends State<MultiSelectExpansionTile<T>> with SingleTickerProviderStateMixin {
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

  String _title(List<T> selected) {
    if (selected.isEmpty) return widget.hint;
    return selected.map(widget.labelBuilder).join('، ');
  }

  void _toggle(T option) {
    final next = List<T>.from(widget.selectedValues.value);
    if (next.contains(option)) {
      next.remove(option);
    } else {
      next.add(option);
    }
    widget.selectedValues.value = next;
    widget.onChanged?.call(next);
  }

  @override
  Widget build(BuildContext context) {
    return ValueListenableBuilder<List<T>>(
      valueListenable: widget.selectedValues,
      builder: (context, selected, child) {
        return Column(
          children: [
            ExpansionTile(
              onExpansionChanged: (value) {
                setState(() => isExpanded = value);
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
                _title(selected),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: AppTextStyle.bodyMedium,
              ),
              children: const [],
            ),
            AnimatedBuilder(
              animation: _animation,
              builder: (context, child) {
                return SizeTransition(
                  sizeFactor: _animation,
                  child: Column(
                    children: [
                      const SizedBox(height: 8),
                      FadeTransition(
                        opacity: _animation,
                        child: Container(
                          decoration: BoxDecoration(
                            borderRadius: BorderRadius.circular(16),
                            border: Border.all(color: AppColor.stroke, width: 2),
                          ),
                          child: Column(
                            children: [
                              for (var i = 0; i < widget.options.length; i++) ...[
                                ListTile(
                                  onTap: () => _toggle(widget.options[i]),
                                  title: Text(
                                    widget.labelBuilder(widget.options[i]),
                                    style: AppTextStyle.bodyMedium.withColor(
                                      selected.contains(widget.options[i]) ? AppColor.primary : const Color(0xff808189),
                                    ),
                                  ),
                                  trailing: Icon(
                                    selected.contains(widget.options[i])
                                        ? Icons.check_box_rounded
                                        : Icons.check_box_outline_blank_rounded,
                                    color: selected.contains(widget.options[i]) ? AppColor.primary : AppColor.text2,
                                  ),
                                ),
                                if (i != widget.options.length - 1)
                                  Container(
                                    margin: const EdgeInsets.symmetric(horizontal: 16),
                                    height: 1,
                                    color: AppColor.stroke,
                                  ),
                              ],
                            ],
                          ),
                        ),
                      ),
                    ],
                  ),
                );
              },
            ),
          ],
        );
      },
    );
  }
}

/// Convenience wrapper for [ExecutionStatus] multi-select.
class ExecutionStatusMultiSelectTile extends StatelessWidget {
  const ExecutionStatusMultiSelectTile({
    super.key,
    required this.selectedValues,
    required this.controller,
    this.onChanged,
    this.onExpanded,
  });

  final ValueNotifier<List<ExecutionStatus>> selectedValues;
  final ExpansionTileController controller;
  final ValueChanged<List<ExecutionStatus>>? onChanged;
  final VoidCallback? onExpanded;

  @override
  Widget build(BuildContext context) {
    return MultiSelectExpansionTile<ExecutionStatus>(
      options: ExecutionStatus.values,
      selectedValues: selectedValues,
      labelBuilder: (status) => status.arabicLabel,
      controller: controller,
      hint: 'حالة التنفيذ',
      onChanged: onChanged,
      onExpanded: onExpanded,
    );
  }
}
