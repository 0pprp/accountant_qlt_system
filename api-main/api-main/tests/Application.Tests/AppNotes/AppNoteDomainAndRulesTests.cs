using Application.AppNotes.Common;
using Domain.Entities.AppNoteAggregate;
using Domain.Entities.AppNoteAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.AppNotes;

public class AppNoteDomainAndRulesTests
{
    [Fact]
    public void Create_starts_unread_with_trusted_source()
    {
        var at = DateTimeOffset.Parse("2026-11-09T12:00:00Z");
        var note = new AppNote(
            AppNoteSourceApp.Delegate,
            "manager_note",
            5,
            "مندوب",
            "Mandob",
            3,
            "بغداد",
            "موضوع",
            "نص ملاحظة كافية الطول",
            at);

        Assert.Equal(AppNoteStatus.Unread, note.Status);
        Assert.Equal(AppNoteSourceApp.Delegate, note.SourceApp);
        Assert.Equal(5, note.SenderUserId);
        Assert.Null(note.ReadAtUtc);
        Assert.Equal(0, note.ConcurrencyStamp);
    }

    [Fact]
    public void MarkRead_is_idempotent()
    {
        var note = new AppNote(
            AppNoteSourceApp.Follower,
            null,
            1,
            "متابع",
            "Motaba",
            null,
            null,
            "س",
            "نص ملاحظة كافية الطول",
            DateTimeOffset.UtcNow);

        var t1 = DateTimeOffset.UtcNow;
        Assert.True(note.MarkRead(t1));
        Assert.Equal(AppNoteStatus.Read, note.Status);
        Assert.Equal(t1, note.ReadAtUtc);
        Assert.Equal(1, note.ConcurrencyStamp);

        var t2 = t1.AddMinutes(5);
        Assert.False(note.MarkRead(t2));
        Assert.Equal(t1, note.ReadAtUtc);
        Assert.Equal(1, note.ConcurrencyStamp);
    }

    [Fact]
    public void Create_permission_does_not_grant_read()
    {
        Assert.NotEqual(SeedPermissions.AppNote.Create, SeedPermissions.AppNote.Read);
    }

    [Fact]
    public void Source_labels_are_arabic()
    {
        Assert.Equal("متابع", AppNoteLabels.SourceAppArabic(AppNoteSourceApp.Follower));
        Assert.Equal("مندوب", AppNoteLabels.SourceAppArabic(AppNoteSourceApp.Delegate));
        Assert.Equal("مدير فرع", AppNoteLabels.SourceAppArabic(AppNoteSourceApp.BranchManager));
        Assert.Equal("موظف مبيعات", AppNoteLabels.SourceAppArabic(AppNoteSourceApp.SalesEmployee));
    }

    [Fact]
    public void Committee_task_permissions_are_outside_all()
    {
        Assert.DoesNotContain(SeedPermissions.All, p => p.Name == SeedPermissions.CommitteeTask.Read);
        Assert.Contains(SeedPermissions.CommitteeTask.OutsideAll, p => p.Name == SeedPermissions.CommitteeTask.AppRead);
        Assert.Contains(SeedPermissions.AppNote.OutsideAll, p => p.Name == SeedPermissions.AppNote.Create);
    }
}
