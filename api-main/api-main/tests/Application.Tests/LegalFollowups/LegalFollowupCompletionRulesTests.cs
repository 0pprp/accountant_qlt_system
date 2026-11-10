using Application.LegalFollowups.Common;

namespace Application.Tests.LegalFollowups;

public class LegalFollowupCompletionRulesTests
{
    [Fact]
    public void Ready_requires_gps_and_four_docs()
    {
        var err = LegalFollowupCompletionRules.ValidateReady(null, null, ["NationalIdFront"]);
        Assert.Equal("يجب تحديد موقع عنوان المحل أولاً", err);

        err = LegalFollowupCompletionRules.ValidateReady(33.3, 44.4, ["NationalIdFront", "NationalIdBack"]);
        Assert.Contains("المستمسكات", err);

        err = LegalFollowupCompletionRules.ValidateReady(
            33.3,
            44.4,
            [
                "NationalIdFront",
                "NationalIdBack",
                "ResidenceCardFront",
                "ResidenceCardBack"
            ]);
        Assert.Null(err);
    }

    [Fact]
    public void Legacy_residence_card_covers_sides()
    {
        var err = LegalFollowupCompletionRules.ValidateReady(
            33.3,
            44.4,
            ["NationalIdFront", "NationalIdBack", "ResidenceCard"]);
        Assert.Null(err);
    }
}
