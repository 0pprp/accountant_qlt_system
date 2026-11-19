using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.SafeAggregate.Enums;

public enum SafeMovementKind : byte
{
    [Display(Name = "رصيد افتتاحي")]
    Opening = 0,

    [Display(Name = "إضافة")]
    Deposit = 1,

    [Display(Name = "سحب")]
    Withdrawal = 2,

    [Display(Name = "نقل داخل")]
    TransferIn = 3,

    [Display(Name = "نقل خارج")]
    TransferOut = 4,

    [Display(Name = "تحصيل")]
    Collection = 5,

    [Display(Name = "مقدمة")]
    Prepayment = 6,

    [Display(Name = "مصروف")]
    ExpenseOut = 7,

    [Display(Name = "مشترى")]
    PurchaseOut = 8,

    [Display(Name = "إرجاع")]
    Refund = 9
}
