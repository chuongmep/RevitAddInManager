using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Autodesk.Revit.DB;

namespace RevitElementBipChecker.Model;

public class CompareParameter
{
    public const string NotExistValue = "<Not Exist>";
    public const string EmptyValue = "<empty>";
    public static List<ParameterDifference> CompareParameters(List<ParameterDifference> list1, List<ParameterDifference> list2)
    {
        var differences = new List<ParameterDifference>();
        // Find parameters in list1 that are not in list2
        foreach (var parameter1 in list1)
        {
            var parameter2 = list2.FirstOrDefault(p => p.Name == parameter1.Name);

            if (parameter2 == null)
            {
                parameter1.Value1 = parameter1.StringValue;
                parameter1.Value2 = NotExistValue;
                parameter1.State = StateParameter.NotExistIn2;
                differences.Add(parameter1);
            }
            else if (parameter1.Type != parameter2.Type || parameter1.Value != parameter2.Value)
            {
                parameter1.Value1 = parameter1.StringValue;
                parameter1.Value2 = parameter2.StringValue;
                parameter1.Similarity = CosineSimilarity.CalculateCosineSimilarity(parameter1.Value1, parameter1.Value2);
                parameter1.State = StateParameter.DifferentValue;
                differences.Add(parameter1);
            }
            else
            {
                parameter1.Value1 = parameter1.StringValue;
                parameter1.Value2 = parameter2.StringValue;
                parameter1.Similarity = CosineSimilarity.CalculateCosineSimilarity(parameter1.Value1, parameter1.Value2);
                parameter1.State = StateParameter.SameValue;
                differences.Add(parameter1);
            }
        }

        // Find parameters in list2 that are not in list1
        foreach (var parameter2 in list2)
        {
            var parameter1 = list1.FirstOrDefault(p => p.Name == parameter2.Name);

            if (parameter1 == null)
            {
                parameter2.Value1 = NotExistValue;
                parameter2.Value2 = parameter2.StringValue;
                parameter2.State = StateParameter.NotExistIn1;
                differences.Add(parameter2);
            }
        }
        return differences;
    }
}

public class ParameterDifference: ParameterData
{
    public StateParameter State { get; set; } = StateParameter.Unknown;
    /// <summary>
    /// Value parameter of Element1
    /// </summary>
    public string Value1 { get; set; }
    /// <summary>
    /// Value parameter of Element2
    /// </summary>
    public string Value2 { get; set; }

    public double Similarity { get; set; } = -1;
    private Brush _rowColor = Brushes.White;
    public Brush RowColor
    {
        get { return _rowColor; }
        set
        {
            _rowColor = value;
            OnPropertyChanged(nameof(RowColor));
        }
    }
    public ParameterDifference(Parameter parameter, Document doc, bool isinstance = true) : base(parameter, doc, isinstance)
    {
        
    }

    public ParameterDifference(Element element, Parameter parameter, Document doc, bool isinstance = true) : base(element, parameter, doc, isinstance)
    {
    }
}

public enum StateParameter
{
    SameValue,
    DifferentValue,
    NotExistIn1,
    NotExistIn2,
    Unknown
}