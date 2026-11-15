namespace Application.ManagementSalesMap.Common;

/// <summary>حروف قوائم ثابتة داخل المحافظة حسب OrderList.Id تصاعديًا.</summary>
public static class SalesMapListLetters
{
    private static readonly string[] Singles =
    [
        "أ", "ب", "ت", "ث", "ج", "ح", "خ", "د", "ذ", "ر", "ز", "س", "ش", "ص",
        "ض", "ط", "ظ", "ع", "غ", "ف", "ق", "ك", "ل", "م", "ن", "ه", "و", "ي"
    ];

    public static IReadOnlyDictionary<int, string> Assign(IEnumerable<int> listIdsOrdered)
    {
        var map = new Dictionary<int, string>();
        var i = 0;
        foreach (var id in listIdsOrdered.Distinct())
        {
            map[id] = LetterAt(i);
            i++;
        }
        return map;
    }

    public static string LetterAt(int index)
    {
        if (index < 0)
            index = 0;
        if (index < Singles.Length)
            return Singles[index];

        var rem = index - Singles.Length;
        var a = rem / Singles.Length;
        var b = rem % Singles.Length;
        // ثنائي: أب، أت… دون تكرار الحرف الأول مع نفسه عند الحاجة
        return Singles[Math.Min(a, Singles.Length - 1)] + Singles[b];
    }
}
