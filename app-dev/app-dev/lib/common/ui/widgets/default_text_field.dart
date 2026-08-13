import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/utils/helpers/text_util.dart';
import 'package:vector_graphics/vector_graphics.dart';

class DefaultTextField extends StatefulWidget {
  final String? label;
  final String? defaultValue;
  final int? minLine;
  final int? maxLine;
  final bool isDense;
  final bool showClearIcon;
  final FocusNode? focusNode;
  final TextAlign textAlign;
  final List<TextInputFormatter>? inputFormatters;
  final TextInputType? keyboardType;
  final TextEditingController? textEditingController;
  final EdgeInsets contentPadding;
  final FormFieldValidator<String>? validator;
  final FormFieldSetter<String>? onSaved;
  final TextInputAction? textInputAction;
  final VoidCallback? onTap;
  final bool readOnly;
  final TextDirection initialDirection;
  final TextDirection? textDirection;
  final bool alignLabelWithHint;
  final Function(String)? onChange;
  final int? maxLength;
  final String? errorText;
  final String? hint;
  final Widget? suffix;
  final String? suffixText;
  final Widget? suffixIcon;
  final Widget? prefix;
  final String? prefixText;
  final Widget? prefixIcon;
  final Color fillColor;
  final Color? cursorColor;
  final bool filled;
  final bool enabled;
  final TextStyle textStyle;
  final TextStyle hintStyle;
  final TextStyle? errorStyle;
  final TextStyle? labelStyle;
  final InputBorder? border;
  final InputBorder? focusedBorder;
  final InputBorder? errorBorder;
  final InputCounterWidgetBuilder? buildCounter;
  final bool autoDispose;
  final String? counterText;
  final ValueChanged<String>? onFieldSubmitted;
  final BoxConstraints? prefixIconConstraints;
  final BoxConstraints? suffixIconConstraints;
  final bool obscureText;
  final bool autoFocus;
  final bool useIsEmptyValidator;
  final Color? focusedBorderColor;
  final TextStyle? floatingLabelStyle;

  const DefaultTextField({
    super.key,
    this.label,
    this.defaultValue,
    this.onTap,
    this.floatingLabelStyle,
    this.focusedBorderColor,
    this.readOnly = false,
    this.autoFocus = false,
    this.useIsEmptyValidator = false,
    this.showClearIcon = false,
    this.focusNode,
    this.onFieldSubmitted,
    this.prefixIconConstraints,
    this.inputFormatters,
    this.textAlign = TextAlign.start,
    this.textInputAction,
    this.textDirection,
    this.obscureText = false,
    this.counterText,
    this.contentPadding = const EdgeInsets.all(16),
    this.textStyle = AppTextStyle.bodyMedium,
    this.hintStyle = const TextStyle(color: AppColor.text4, fontSize: 10, fontWeight: FontWeight.w500),
    this.labelStyle,
    this.errorBorder,
    this.textEditingController,
    this.keyboardType,
    this.validator,
    this.onSaved,
    this.buildCounter,
    this.errorStyle = const TextStyle(
      color: AppColor.error,
      fontSize: 12,
      fontWeight: FontWeight.w400,
      fontFamily: 'ElMessiri',
      letterSpacing: 0,
    ),
    this.onChange,
    this.alignLabelWithHint = false,
    this.errorText,
    this.suffix,
    this.enabled = true,
    this.fillColor = AppColor.surface1,
    this.filled = false,
    this.suffixText,
    this.maxLength,
    this.suffixIconConstraints,
    this.suffixIcon,
    this.prefix,
    this.prefixText,
    this.minLine,
    this.maxLine = 1,
    this.prefixIcon,
    this.cursorColor = AppColor.text1,
    this.hint,
    this.border,
    this.focusedBorder,
    this.isDense = false,
    this.autoDispose = true,
    this.initialDirection = TextDirection.rtl,
  });

  @override
  State<DefaultTextField> createState() => _DefaultTextFieldState();
}

class _DefaultTextFieldState extends State<DefaultTextField> {
  late final TextEditingController controller;

  @override
  void initState() {
    controller = widget.textEditingController ?? TextEditingController();
    if (widget.defaultValue != null) controller.text = widget.defaultValue!;
    controller.addListener(_listener);
    super.initState();
  }

  @override
  void dispose() {
    controller.removeListener(_listener);
    // if (widget.autoDispose) controller.dispose();
    super.dispose();
  }

  String? _isEmptyValidator(String? value) {
    if (value == null || value.isEmpty) {
      return 'لطفا اطلاعات خواسته شده را وارد کنید';
    }
    return null;
  }

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      controller: controller,
      textAlignVertical: TextAlignVertical.center,
      onFieldSubmitted: widget.onFieldSubmitted,
      obscureText: widget.obscureText,
      // onTapOutside: (event) {
      //   FocusManager.instance.primaryFocus?.unfocus();
      // },
      decoration: InputDecoration(
        filled: widget.filled,
        fillColor: widget.fillColor,
        enabled: widget.enabled,
        labelStyle: widget.labelStyle ?? AppTextStyle.bodyMedium,
        floatingLabelStyle: widget.floatingLabelStyle ?? AppTextStyle.labelMedium,
        suffixIconConstraints: widget.suffixIconConstraints,
        hintStyle: widget.hintStyle,
        errorStyle: widget.errorStyle,
        errorBorder:
            widget.errorBorder ??
            OutlineInputBorder(
              borderSide: const BorderSide(color: AppColor.error, width: 2),
              borderRadius: BorderRadius.circular(12),
            ),
        border:
            widget.border ??
            OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(width: 1.5, color: controller.text.isEmpty ? AppColor.stroke : AppColor.text2),
            ),
        focusedBorder:
            widget.focusedBorder ??
            OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(width: 1.5, color: widget.focusedBorderColor ?? AppColor.primary),
            ),
        enabledBorder:
            widget.border ??
            OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(width: 1.5, color: controller.text.isEmpty ? AppColor.stroke : AppColor.text2),
            ),
        disabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(width: 1.5, color: AppColor.stroke),
        ),
        contentPadding: widget.contentPadding,
        suffix: widget.suffix,
        suffixText: widget.suffixText,
        suffixIcon:
            widget.showClearIcon
                ? IconButton(
                  onPressed: () {
                    controller.clear();
                    widget.onChange?.call('');
                  },
                  icon: VectorGraphic(loader: AssetBytesLoader('assets/svg/close_icon.svg')),
                )
                : widget.suffixIcon,
        prefix: widget.prefix,
        prefixText: widget.prefixText,
        prefixIconConstraints: widget.prefixIconConstraints,
        prefixIcon: widget.prefixIcon,
        hintText: widget.hint,
        labelText: widget.label,
        alignLabelWithHint: widget.alignLabelWithHint,
        errorText: widget.errorText,
        isDense: widget.isDense,
        counterText: widget.counterText,
      ),
      textInputAction: widget.textInputAction,
      keyboardType: widget.keyboardType,
      onSaved: widget.onSaved,
      cursorColor: widget.cursorColor,
      validator: widget.useIsEmptyValidator && widget.validator == null ? _isEmptyValidator : widget.validator,
      readOnly: widget.readOnly,
      onChanged: widget.onChange,
      maxLength: widget.maxLength,
      style: widget.textStyle,
      minLines: widget.minLine,
      maxLines: widget.maxLine,
      focusNode: widget.focusNode,
      autofocus: widget.autoFocus,
      textAlign: widget.textAlign,
      inputFormatters: widget.inputFormatters,
      buildCounter: widget.buildCounter,
      textDirection:
          widget.textDirection ?? TextUtil.getDirection(controller.text, defaultDirection: widget.initialDirection),
      onTap: _onTap,
    );
  }

  void _onTap() {
    // if (controller.selection == TextSelection.fromPosition(TextPosition(offset: controller.text.length - 1))) {
    //   controller.selection = TextSelection.fromPosition(
    //     TextPosition(offset: controller.text.length),
    //   );
    // }
    if (widget.onTap != null) {
      widget.onTap!();
    }
  }

  void _listener() {
    if (controller.text.length < 2) {
      if (mounted) setState(() {});
    }
  }
}
