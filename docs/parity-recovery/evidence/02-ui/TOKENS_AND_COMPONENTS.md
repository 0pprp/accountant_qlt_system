# UI tokens & component map (دفعة 02)

## Tokens
| Token | Value | Source |
|---|---|---|
| primary | #006A64 | theme.css / AppColors.primary |
| ink | #00201E | theme.css gray-900 / AppColors.foreground |
| card | #FFFFFF | AppColors.card |
| search/field bg | #F8F8F8 | theme --color-background |
| Flutter page bg | #F4F7F7 | AppColors.background (لا تُستبدل) |
| font | ElMessiri | typography.css / AppTextStyles |

## Components
| Component | Vue | Flutter |
|---|---|---|
| PageHeader | Core/components/ui/PageHeader.vue | (AppBar / preview title) |
| FilterBar | FilterBar.vue | — |
| StatusTabs | StatusTabs.vue | — |
| EntityCard | EntityCard.vue | core/widgets/ui/entity_card.dart |
| DataField | DataField.vue | data_field.dart |
| StatusBadge | StatusBadge.vue | status_badge.dart |
| ActionRow | ActionRow.vue | action_row.dart |
| ConfirmReasonDialog | ConfirmReasonDialog.vue | centered_dialog (موجود) |
| DataState | DataState.vue | data_state.dart |
| Pagination | Pagination.vue | — |

## Samples applied
- RequestCard.vue → EntityCard/DataField/ActionRow (حقول SourceParity محفوظة)
- MatchingSummary.vue → شبكة 2×2 / 4 (حالات loading/failed/insufficient بلا صفر وهمي)
- SalesRequestCard → EntityCard kit
- committee HomeTab list card → EntityCard kit

## Preview
- Panel DEV: /dev/ui-kit (مسجّل فقط في import.meta.env.DEV؛ publicSession؛ بلا endpoints)
- Flutter: UiKitPreviewPage (debug assert)

## Proof class
- Unit: sales ui_kit_preview_test + sales suite 39+2؛ committee 19؛ vue-tsc OK
- Browser-live: لقطات evidence/02-ui (360/768/1440) لصفحة المعاينة — ليست دورة بزنس حية
- Device: غير مثبت في هذه الدفعة
