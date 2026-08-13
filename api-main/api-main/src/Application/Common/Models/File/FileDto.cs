namespace Application.Common.Models.File;

public record FileDto
{
    public required string Extension { get; set; }
    public required string Name { get; set; }
    public required string RelativePath { get; set; }
}