namespace Application.Users.Commands.Admin.PatchTabAccess;

public sealed class PatchUserTabAccessCommand : IRequest<Result>
{
    public int UserId { get; init; }
    public int ExpectedVersion { get; init; }
    public List<PatchUserTabAccessChange> Changes { get; init; } = [];
}

public sealed class PatchUserTabAccessChange
{
    public string TabKey { get; init; } = string.Empty;
    /// <summary>true فتح، false قفل، null إزالة التخصيص واعتماد الدور.</summary>
    public bool? Enabled { get; init; }
}

public sealed class PatchUserTabAccessDto
{
    public int ExpectedVersion { get; init; }
    public List<PatchUserTabAccessChangeDto>? Changes { get; init; }
}

public sealed class PatchUserTabAccessChangeDto
{
    public string TabKey { get; init; } = string.Empty;
    public bool? Enabled { get; init; }
}
