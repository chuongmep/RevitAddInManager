﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace RevitElementBipChecker.Model;

public class ParameterComparer
{
    public static List<ParameterDifference> CompareParameters(List<ParameterBase> list1, List<ParameterBase> list2)
    {
        var differences = new List<ParameterDifference>();

        // Find parameters in list1 that are not in list2
        foreach (var parameter1 in list1)
        {
            var parameter2 = list2.FirstOrDefault(p => p.Name == parameter1.Name);

            if (parameter2 == null)
            {
                differences.Add(new ParameterDifference
                {
                    Name = parameter1.Name,
                    Type = parameter1.Type,
                    Value1 = parameter1.Value,
                    Value2 = null // Not found in list2
                });
            }
            else if (parameter1.Type != parameter2.Type || parameter1.Value != parameter2.Value)
            {
                differences.Add(new ParameterDifference
                {
                    Name = parameter1.Name,
                    Type = parameter1.Type,
                    Value1 = parameter1.Value,
                    Value2 = parameter2.Value
                });
            }
        }

        // Find parameters in list2 that are not in list1
        foreach (var parameter2 in list2)
        {
            var parameter1 = list1.FirstOrDefault(p => p.Name == parameter2.Name);

            if (parameter1 == null)
            {
                differences.Add(new ParameterDifference
                {
                    Name = parameter2.Name,
                    Type = parameter2.Type,
                    Value1 = null, // Not found in list1
                    Value2 = parameter2.Value
                });
            }
        }
        return differences;
    }
}
public class ParameterBase
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Type { get; set; }
}
public class ParameterDifference: ViewmodeBase
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Value1 { get; set; }
    public string Value2 { get; set; }
    private Brush _rowColor = Brushes.Transparent;
    public Brush RowColor
    {
        get { return _rowColor; }
        set
        {
            _rowColor = value;
            OnPropertyChanged(nameof(RowColor));
        }
    }
}