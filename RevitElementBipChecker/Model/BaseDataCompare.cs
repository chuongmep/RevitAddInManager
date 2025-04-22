using System;
using System.Windows.Media;

namespace RevitElementBipChecker.Model;

public class BaseDataCompare : ViewmodeBase
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public string Value { get; set; }
}

/// <summary>
/// The result of comparing two objects
/// </summary>
public class ComparisonResult : BaseDataCompare
{
    public double Similarity { get; set; }
    public StateCompare StateCompare{ get; set; }
    public string Value1 { get; set; }
    public string Value2 { get; set; }
    private System.Windows.Media.Brush _rowColor = Brushes.White;
    public System.Windows.Media.Brush RowColor
    {
        get { return _rowColor; }
        set
        {
            _rowColor = value;
            OnPropertyChanged(nameof(RowColor));
        }
    }
}

public enum StateCompare
{
    SameValue,
    DifferentValue,
    InList1Only,
    InList2Only
}