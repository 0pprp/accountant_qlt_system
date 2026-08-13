namespace Application.Safes.Queries.Admin.GetByBranch;

public record GetSafeByBranchResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public double RemainingCashAmount { get; set; }
    public double NetBalance { get; set; }
    public double TotalAmount => RemainingCashAmount + NetBalance + (TotalUndeliveredCashAmount ?? 0) + (TotalBranchesRemainingCashAmount ?? 0);
    public double? TotalUndeliveredCashAmount { get; set; }
    public double? TotalBranchesRemainingCashAmount { get; set; }
    public int? BranchId { get; set; }
}