using Domain.Common;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.BranchAggregate;
using Domain.Entities.OrderAggregate;

namespace Domain.Entities.CustomerAggregate;

public class Customer : AuditableEntity
{
    private Customer()
    {
    }
    
    public Customer(
        string fullName,
        string motherName,
        string nationalCode,
        DateOnly birthDate,
        string phoneNumber,
        string whatsAppPhoneNumber,
        Business business,
        int branchId)
    {
        FullName = fullName;
        MotherName = motherName;
        NationalCode = nationalCode;
        BirthDate = birthDate;
        PhoneNumber = phoneNumber;
        WhatsAppPhoneNumber = whatsAppPhoneNumber;
        Business = business;
        BranchId = branchId;
        Attachments = new HashSet<Attachment>();
        Orders = new HashSet<Order>();
    }

    public string FullName { get; set; }
    public string MotherName { get; set; }
    public string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public string PhoneNumber { get; set; }
    public string WhatsAppPhoneNumber { get; set; }
    public Business Business { get; set; }
    public int BranchId { get; set; }

    /// <summary>
    /// حالة القانونية — مصدر العمل: Customers.IsLegal في النظام القديم.
    /// true بعد اعتماد التحويل القانوني؛ لا تُستنتج من SalesRequest أو الملاحظات.
    /// </summary>
    public bool IsLegal { get; set; }

    /// <summary>بيع وهمي — مصدر: Customers.IsFakeSale عند قرار «وهمي».</summary>
    public bool IsFakeSale { get; set; }

    public void MarkLegal() => IsLegal = true;

    public void MarkFakeSale() => IsFakeSale = true;

    public Branch? Branch { get; set; }
    public ICollection<Attachment> Attachments { get; set; }
    public ICollection<Order> Orders { get; set; }

    public void Update(
        string fullName,
        string motherName,
        string nationalCode,
        DateOnly birthDate,
        string phoneNumber,
        string whatsAppPhoneNumber,
        Business business,
        int branchId)
    {
        FullName = fullName;
        MotherName = motherName;
        NationalCode = nationalCode;
        BirthDate = birthDate;
        PhoneNumber = phoneNumber;
        WhatsAppPhoneNumber = whatsAppPhoneNumber;
        Business = business;
        BranchId = branchId;
    }
}