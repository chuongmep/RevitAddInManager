using System.Collections.Generic;
using System.Linq;

namespace RevitElementBipChecker.Model;

public class MetadataCompareDiff
{
    public const string NotExistValue = "<Not Exist>";
    public const string EmptyValue = "<empty>";

    public static List<ComparisonResult> CompareDifferences(List<BaseDataCompare> list1, List<BaseDataCompare> list2)
    {
        List<ComparisonResult> results = new List<ComparisonResult>();

        // Create dictionaries for quick lookup based on Name
        Dictionary<string, BaseDataCompare> dict1 = list1.ToDictionary(item => item.Name);
        Dictionary<string, BaseDataCompare> dict2 = list2.ToDictionary(item => item.Name);

        foreach (var item1 in list1)
        {
            if (dict2.TryGetValue(item1.Name, out var item2))
            {
                // Item with the same name exists in both lists
                if (item1.Value == item2.Value)
                {
                    results.Add(new ComparisonResult
                    {
                        Name = item1.Name,
                        Type = item1.Type,
                        Value1 = item1.Value,
                        Value2 = item2.Value,
                        StateCompare = StateCompare.SameValue,
                        Similarity = 1,
                    });
                }
                else
                {
                    results.Add(new ComparisonResult
                    {
                        Name = item1.Name,
                        Type = item1.Type,
                        Value1 = item1.Value,
                        Value2 = item2.Value,
                        StateCompare = StateCompare.DifferentValue,
                        Similarity = CosineSimilarity.CalculateCosineSimilarity(item1.Value, item2.Value)
                    });
                }
            }
            else
            {
                // Item with this name exists in list1 but not in list2
                results.Add(new ComparisonResult
                {
                    Name = item1.Name,
                    Type = item1.Type,
                    Value1 = item1.Value,
                    Value2 = NotExistValue,
                    StateCompare = StateCompare.InList1Only,
                    Similarity = -1,
                });
            }
        }

        foreach (var item2 in list2)
        {
            if (!dict1.ContainsKey(item2.Name))
            {
                // Item with this name exists in list2 but not in list1
                results.Add(new ComparisonResult
                {
                    Name = item2.Name,
                    Type = item2.Type,
                    Value1 = NotExistValue,
                    Value2 = item2.Value,
                    StateCompare = StateCompare.InList2Only,
                    Similarity = -1
                });
            }
        }

        return results;
    }
}