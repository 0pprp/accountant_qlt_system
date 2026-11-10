using Domain.Services.SalesMatching;

namespace Application.SalesRequests.Common;

public sealed class SalesMatchCandidate
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Phone { get; init; }
    public int BranchId { get; init; }
}

public static class SalesRequestMatchEvaluator
{
    public static SalesMatchCardDto EvaluateCard(
        string criterion,
        string? queryName,
        string? queryPhone,
        IReadOnlyList<SalesMatchCandidate> candidates)
    {
        var label = SalesMatchCardLabels.For(criterion);
        try
        {
            return criterion switch
            {
                SalesMatchCriteria.TripleName => EvaluateNameCard(
                    criterion, label, queryName,
                    SalesRequestNameSimilarity.HasEnoughTripleName,
                    SalesRequestNameSimilarity.IsTripleNameMatch,
                    candidates),
                SalesMatchCriteria.FatherGrandfather => EvaluateNameCard(
                    criterion, label, queryName,
                    SalesRequestNameSimilarity.HasEnoughFatherGrandfather,
                    SalesRequestNameSimilarity.IsFatherOrGrandfatherMatch,
                    candidates),
                SalesMatchCriteria.SonFather => EvaluateNameCard(
                    criterion, label, queryName,
                    n => SalesRequestNameSimilarity.MatchingTokens(n).Count >= 2,
                    SalesRequestNameSimilarity.IsSonFatherMatch,
                    candidates),
                SalesMatchCriteria.Phone => EvaluatePhoneCard(label, queryPhone, candidates),
                _ => new SalesMatchCardDto
                {
                    Key = criterion,
                    Label = label,
                    State = SalesMatchCardLabels.StateName(SalesMatchCardState.Failed),
                    Count = null
                }
            };
        }
        catch
        {
            return new SalesMatchCardDto
            {
                Key = criterion,
                Label = label,
                State = SalesMatchCardLabels.StateName(SalesMatchCardState.Failed),
                Count = null
            };
        }
    }

    public static IEnumerable<SalesMatchCandidate> FilterHits(
        string criterion,
        string? queryName,
        string? queryPhone,
        IEnumerable<SalesMatchCandidate> candidates)
    {
        return criterion switch
        {
            SalesMatchCriteria.TripleName when SalesRequestNameSimilarity.HasEnoughTripleName(queryName)
                => candidates.Where(c => SalesRequestNameSimilarity.IsTripleNameMatch(queryName, c.Name)),
            SalesMatchCriteria.FatherGrandfather when SalesRequestNameSimilarity.HasEnoughFatherGrandfather(queryName)
                => candidates.Where(c => SalesRequestNameSimilarity.IsFatherOrGrandfatherMatch(queryName, c.Name)),
            SalesMatchCriteria.SonFather when SalesRequestNameSimilarity.MatchingTokens(queryName).Count >= 2
                => candidates.Where(c => SalesRequestNameSimilarity.IsSonFatherMatch(queryName, c.Name)),
            SalesMatchCriteria.Phone when SalesPhoneNormalizer.HasEnoughPhoneData(queryPhone)
                => candidates.Where(c => SalesPhoneNormalizer.Matches(queryPhone, c.Phone)),
            _ => Enumerable.Empty<SalesMatchCandidate>()
        };
    }

    public static string BuildMatchReason(string criterion, string? queryName, string candidateName)
    {
        var (q0, q1, q2) = SalesRequestNameSimilarity.TripleParts(queryName);
        var (c0, c1, c2) = SalesRequestNameSimilarity.TripleParts(candidateName);
        return criterion switch
        {
            SalesMatchCriteria.TripleName => $"تطابق الاسم الثلاثي: {c0} {c1} {c2}",
            SalesMatchCriteria.FatherGrandfather => $"تطابق الأب والجد: {c1} {c2}",
            SalesMatchCriteria.SonFather => $"تطابق ابن/أب: {q0}/{q1} ↔ {c0}/{c1}",
            SalesMatchCriteria.Phone => "تطابق رقم الهاتف (لا يعني بالضرورة نفس الشخص)",
            _ => criterion
        };
    }

    public static bool HasAnyBlockingMatch(
        string? queryName,
        string? queryPhone,
        IReadOnlyList<SalesMatchCandidate> customers,
        IReadOnlyList<SalesMatchCandidate> peerRequests)
    {
        foreach (var key in SalesMatchCriteria.All)
        {
            var customerCard = EvaluateCard(key, queryName, queryPhone, customers);
            if (customerCard.State == SalesMatchCardLabels.StateName(SalesMatchCardState.Results)
                && customerCard.Count is > 0)
                return true;

            var peerCard = EvaluateCard(key, queryName, queryPhone, peerRequests);
            if (peerCard.State == SalesMatchCardLabels.StateName(SalesMatchCardState.Results)
                && peerCard.Count is > 0)
                return true;
        }

        return false;
    }

    private static SalesMatchCardDto EvaluateNameCard(
        string key,
        string label,
        string? queryName,
        Func<string?, bool> hasEnough,
        Func<string?, string?, bool> matches,
        IReadOnlyList<SalesMatchCandidate> candidates)
    {
        if (!hasEnough(queryName))
        {
            return new SalesMatchCardDto
            {
                Key = key,
                Label = label,
                State = SalesMatchCardLabels.StateName(SalesMatchCardState.InsufficientData),
                Count = null
            };
        }

        var count = candidates.Count(c => matches(queryName, c.Name));
        return new SalesMatchCardDto
        {
            Key = key,
            Label = label,
            State = SalesMatchCardLabels.StateName(
                count > 0 ? SalesMatchCardState.Results : SalesMatchCardState.NoResults),
            Count = count
        };
    }

    private static SalesMatchCardDto EvaluatePhoneCard(
        string label,
        string? queryPhone,
        IReadOnlyList<SalesMatchCandidate> candidates)
    {
        if (!SalesPhoneNormalizer.HasEnoughPhoneData(queryPhone))
        {
            return new SalesMatchCardDto
            {
                Key = SalesMatchCriteria.Phone,
                Label = label,
                State = SalesMatchCardLabels.StateName(SalesMatchCardState.InsufficientData),
                Count = null
            };
        }

        var count = candidates.Count(c => SalesPhoneNormalizer.Matches(queryPhone, c.Phone));
        return new SalesMatchCardDto
        {
            Key = SalesMatchCriteria.Phone,
            Label = label,
            State = SalesMatchCardLabels.StateName(
                count > 0 ? SalesMatchCardState.Results : SalesMatchCardState.NoResults),
            Count = count
        };
    }
}
