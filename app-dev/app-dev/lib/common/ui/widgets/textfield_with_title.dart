import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:team/common/ui/theme/app_color.dart';
import 'package:team/common/ui/theme/app_text_styles.dart';
import 'package:team/common/ui/widgets/default_text_field.dart';

class TextFieldWIthTitle extends StatelessWidget {
  final TextEditingController _controller;

  TextFieldWIthTitle({
    super.key,
    required TextEditingController controller,
    required String title,
    String? hint,
    bool isOptional = false,
    bool isEnabled = true,
    bool isEditable = true,
    Widget? suffix,
    bool isNumber = false,
    bool isPercent = false,
    VoidCallback? onTap,
    FormFieldValidator<String>? validator,
  }) : _controller = controller,
       _title = title,
       _hint = hint,
       _onTap = onTap,
       _isEditable = isEditable,
       _isEnabled = isEnabled,
       _isNumber = isNumber,
       _isPercent = isPercent,
       _suffix = suffix,
       _isOptional = isOptional,
       _validator = validator;
  final String _title;
  final String? _hint;
  final bool _isOptional;
  final bool _isEnabled;
  final bool _isEditable;
  final bool _isNumber;
  final bool _isPercent;
  final Widget? _suffix;
  final VoidCallback? _onTap;
  final FormFieldValidator<String>? _validator;
  final FocusNode _focusNode = FocusNode();

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(padding: const EdgeInsets.only(bottom: 6), child: Text(_title, style: AppTextStyle.labelMedium)),
          GestureDetector(
            onTap:
                !_isEnabled
                    ? () {
                      _onTap?.call();
                      _focusNode.requestFocus();
                    }
                    : null,
            child: AbsorbPointer(
              absorbing: !_isEnabled,
              child: DefaultTextField(
                focusNode: _focusNode,
                readOnly: !_isEditable,
                textEditingController: _controller,
                keyboardType: _isNumber ? TextInputType.number : TextInputType.text,
                hint: _hint ?? '$_title خود را وارد کنید',
                suffix: _suffix,
                validator: _validator,
                filled: !_isEnabled && _isEditable,
                useIsEmptyValidator: !_isOptional,
                enabled: _isEnabled,
                inputFormatters:
                    _isNumber
                        ? [
                          FilteringTextInputFormatter.allow(RegExp('[0-9۰-۹]')),
                          if (_isPercent) LengthLimitingTextInputFormatter(2),
                        ]
                        : null,
                textInputAction: TextInputAction.next,
                textStyle: AppTextStyle.labelMedium.copyWith(
                  color: _isEnabled || !_isEditable ? AppColor.text1 : AppColor.text1,
                ),
                fillColor: _isEnabled ? AppColor.surface1 : AppColor.surface3,
                onTap: _onTap,
                autoDispose: true,
                focusedBorderColor: AppColor.text1,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
