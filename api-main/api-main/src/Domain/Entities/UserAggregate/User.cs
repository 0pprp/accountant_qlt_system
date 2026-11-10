using Domain.Common;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.BranchAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.UserAggregate.Enums;

namespace Domain.Entities.UserAggregate;

public class User : AuditableEntity
{
    public User(string fullName, string motherName, string nationalCode, DateOnly birthDate, string username, string passwordHash,
        string address, string phoneNumber)
    {
        FullName = fullName;
        MotherName = motherName;
        NationalCode = nationalCode;
        BirthDate = birthDate;
        Username = username;
        PasswordHash = passwordHash;
        Address = address;
        PhoneNumber = phoneNumber;
        SecurityStamp = Guid.NewGuid();
        CreationStep = UserCreationStep.PersonalDocuments;
        SalaryDetail = new SalaryDetail();
        UserLogins = new HashSet<UserLogin>();
        UserRoles = new HashSet<UserRole>();
        Attachments = new HashSet<Attachment>();
        OrderListsAsMotaba = new HashSet<OrderList>();
        SoldOrders = new HashSet<Order>();
        UserPermissions = new HashSet<UserPermission>();
        UserPermissionDenials = new HashSet<UserPermissionDenial>();
        UserBranches = new HashSet<UserBranch>();
        SellerCashDeliveryTransactions = new HashSet<SellerCashDeliveryTransaction>();
        PermissionsVersion = 0;
    }

    public string FullName { get; set; }
    public string MotherName { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public Guid SecurityStamp { get; set; }
    public UserCreationStep CreationStep { get; set; }
    public SalaryDetail SalaryDetail { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public double UndeliveredCashAmount { get; set; }
    /// <summary>إصدار تخصيصات الصلاحيات — Concurrency Token.</summary>
    public int PermissionsVersion { get; set; }

    public OrderList? OrderListAsMandob { get; set; }
    public ICollection<OrderList> OrderListsAsMotaba { get; set; }
    public ICollection<UserLogin> UserLogins { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<Attachment> Attachments { get; set; }
    public ICollection<Order> SoldOrders { get; set; }
    public ICollection<UserPermission> UserPermissions { get; set; }
    public ICollection<UserPermissionDenial> UserPermissionDenials { get; set; }
    public ICollection<UserBranch> UserBranches { get; set; }
    public ICollection<SellerCashDeliveryTransaction> SellerCashDeliveryTransactions { get; set; }

    public void Update(string fullName, string motherName, string nationalCode, DateOnly birthDate, string username,
        string address, string phoneNumber, ICollection<UserRole> userRoles, ICollection<UserBranch> userBranches)
    {
        FullName = fullName;
        MotherName = motherName;
        NationalCode = nationalCode;
        BirthDate = birthDate;
        Username = username;
        Address = address;
        PhoneNumber = phoneNumber;
        UserRoles = userRoles;
        UserBranches = userBranches;
    }

    public void InvalidateSessions()
    {
        SecurityStamp = Guid.NewGuid();
    }

    public void BumpPermissionsVersion()
    {
        PermissionsVersion++;
    }
}