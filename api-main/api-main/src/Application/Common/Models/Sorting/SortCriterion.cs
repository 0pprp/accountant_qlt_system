namespace Application.Common.Models.Sorting;

public record SortCriterion
{
    public required string Property { get; set; }
    public required SortDirection Direction { get; set; }
}