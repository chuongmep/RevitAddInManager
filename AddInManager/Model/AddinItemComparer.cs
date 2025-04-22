namespace RevitAddinManager.Model;

/// <summary>
/// Compare Sort By Full Class Name Method
/// </summary>
public class AddinItemComparer : IComparer<AddinItem>
{
    public int Compare(AddinItem x, AddinItem y)
    {
        return string.Compare(x.FullClassName, y.FullClassName, StringComparison.Ordinal);
    }
}