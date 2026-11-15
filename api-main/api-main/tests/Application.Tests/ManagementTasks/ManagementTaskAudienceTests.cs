using Application.ManagementTasks.Common;

namespace Application.Tests.ManagementTasks;

public class ManagementTaskAudienceTests
{
    [Theory]
    [InlineData(ManagementTaskAudience.User)]
    [InlineData(ManagementTaskAudience.Delegate)]
    [InlineData(ManagementTaskAudience.Committee)]
    [InlineData(ManagementTaskAudience.SalesEmployee)]
    [InlineData(ManagementTaskAudience.AllFieldApps)]
    public void Audience_Enum_IsDefined(ManagementTaskAudience audience)
    {
        Assert.True(Enum.IsDefined(audience));
    }

    [Fact]
    public void AudienceEmpty_HasStableCode()
    {
        Assert.Equal("Task_Audience_Empty", ManagementTaskErrors.AudienceEmpty.Code);
        Assert.Equal("Task_Audience_TooLarge", ManagementTaskErrors.AudienceTooLarge.Code);
        Assert.Equal("Task_Audience_Invalid", ManagementTaskErrors.InvalidAudience.Code);
    }

    [Fact]
    public void FanoutResult_HoldsCreatedIds()
    {
        var dto = new ManagementTaskFanoutResultDto(3, [11, 12, 13]);
        Assert.Equal(3, dto.CreatedCount);
        Assert.Equal([11, 12, 13], dto.TaskIds);
    }

    [Fact]
    public void PreviewDto_AllowsZeroRecipients()
    {
        var dto = new ManagementTaskAudiencePreviewDto(
            ManagementTaskAudience.Delegate,
            0,
            []);
        Assert.Equal(0, dto.RecipientCount);
        Assert.Empty(dto.Sample);
    }

    [Fact]
    public void MaxRecipients_Is500()
    {
        Assert.Equal(500, ManagementTaskAudienceResolver.MaxRecipients);
    }

    [Fact]
    public void Recipient_Record_KeepsBranch()
    {
        var r = new ManagementTaskAudienceRecipient(9, "أحمد", 4);
        Assert.Equal(9, r.UserId);
        Assert.Equal("أحمد", r.DisplayName);
        Assert.Equal(4, r.BranchId);
    }
}
