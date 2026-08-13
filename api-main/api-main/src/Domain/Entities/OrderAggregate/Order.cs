using Domain.Common;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.OrderAggregate;

public class Order : AuditableEntity
{
    public Order()
    {
    }

    private Order(
        double buyAmount,
        double sellAmount,
        double prepaymentAmount,
        double dailyInstallmentAmount,
        OrderStep step,
        OrderApprovalStatus approvalStatus,
        int customerId,
        int branchId,
        List<OrderItem> orderItems)
    {
        BuyAmount = buyAmount;
        SellAmount = sellAmount;
        PrepaymentAmount = prepaymentAmount;
        DailyInstallmentAmount = dailyInstallmentAmount;
        Step = step;
        ExecutionStatus = OrderExecutionStatus.NotStarted;
        ApprovalStatus = approvalStatus;
        CustomerId = customerId;
        BranchId = branchId;
        Attachments = new HashSet<Attachment>();
        InstallmentPayments = new HashSet<InstallmentPayment>();
        OrderItems = orderItems;
    }

    public static Order CreateByAdmin(
        double buyAmount,
        double sellAmount,
        double prepaymentAmount,
        double dailyInstallmentAmount,
        int customerId,
        int branchId,
        List<OrderItem> orderItems)
    {
        return new Order(
            buyAmount,
            sellAmount,
            prepaymentAmount,
            dailyInstallmentAmount,
            OrderStep.SellerInfo,
            OrderApprovalStatus.Approved,
            customerId,
            branchId,
            orderItems);
    }

    public static Order CreateByAppUser(
        double buyAmount,
        double sellAmount,
        double prepaymentAmount,
        double dailyInstallmentAmount,
        int customerId,
        int branchId,
        int sellerId,
        List<OrderItem> orderItems)
    {
        return new Order(
            buyAmount,
            sellAmount,
            prepaymentAmount,
            dailyInstallmentAmount,
            OrderStep.Attachments,
            OrderApprovalStatus.Pending,
            customerId,
            branchId,
            orderItems)
        {
            SellerId = sellerId
        };
    }

    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double PrepaymentAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public Location? Location { get; set; }
    public string? CreationAddress { get; set; }
    public DateOnly? SaleDate { get; set; }
    public TimeOnly? SaleTime { get; set; }
    public OrderStep Step { get; set; }
    public OrderExecutionStatus ExecutionStatus { get; set; }
    public OrderApprovalStatus ApprovalStatus { get; set; }
    public int? SellerId { get; set; }
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public int? OrderListId { get; set; }

    public User? Seller { get; set; }
    public Customer? Customer { get; set; }
    public Branch? Branch { get; set; }
    public OrderList? OrderList { get; set; }
    public ICollection<Attachment> Attachments { get; set; }
    public ICollection<InstallmentPayment> InstallmentPayments { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }

    public void CompleteAttachmentsStep()
    {
        Step = OrderStep.SellerInfo;
    }

    public void SetSeller(
        int sellerId,
        int orderListId,
        Location? location,
        string creationAddress,
        DateOnly? saleDate,
        TimeOnly? saleTime,
        InstallmentPayment? firstInstallmentPayment = null)
    {
        SellerId = sellerId;
        OrderListId = orderListId;
        Location = location;
        CreationAddress = creationAddress;
        SaleDate = saleDate;
        SaleTime = saleTime;
        Step = OrderStep.Completed;

        if (firstInstallmentPayment is not null)
            InstallmentPayments.Add(firstInstallmentPayment);
    }

    public void Update(
        double buyAmount,
        double sellAmount,
        double prepaymentAmount,
        double dailyInstallmentAmount,
        Location? location,
        string? creationAddress,
        DateOnly? saleDate,
        TimeOnly? saleTime,
        int sellerId,
        int? orderListId)
    {
        BuyAmount = buyAmount;
        SellAmount = sellAmount;
        PrepaymentAmount = prepaymentAmount;
        DailyInstallmentAmount = dailyInstallmentAmount;
        Location = location;
        CreationAddress = creationAddress;
        SaleDate = saleDate;
        SaleTime = saleTime;
        SellerId = sellerId;
        OrderListId = orderListId;
    }

    public void Approve()
    {
        ApprovalStatus = OrderApprovalStatus.Approved;
        ExecutionStatus = OrderExecutionStatus.InProgress;
    }

    public void Reject()
    {
        ApprovalStatus = OrderApprovalStatus.Rejected;
    }
}