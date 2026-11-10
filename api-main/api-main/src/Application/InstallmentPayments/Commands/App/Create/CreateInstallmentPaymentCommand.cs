using Application.InstallmentPayments.Common;

namespace Application.InstallmentPayments.Commands.App.Create;

public record CreateInstallmentPaymentCommand : IRequest<Result<InstallmentPaymentWriteResultDto>>
{
    public int OrderId { get; set; }
    public double Amount { get; set; }
    public int UserId { get; set; }
    /// <summary>مفتاح جهاز ثابت — إعادة الإرسال بنفس القيمة والبصمة = إقرار دون قبض ثانٍ.</summary>
    public Guid? ClientPaymentId { get; set; }
    /// <summary>تاريخ التحصيل المعتمد (بغداد). إن غاب يُحسب من ساعة السيرفر — يجب أن يطابق Sync.</summary>
    public DateOnly? Date { get; set; }
}

public record CreateInstallmentPaymentDto(double Amount, Guid? ClientPaymentId = null, DateOnly? Date = null);
