using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitElementBipChecker.Model;

namespace RevitElementBipChecker.Viewmodel
{
    public class PropertyViewModel
    {
        public Element Element { get; set; }
        public PropertyViewModel(Autodesk.Revit.DB.Element element)
        {
            Element = element;
        }
        public List<BaseDataCompare> GetPropertyData()
        {
            // invoke the method to get the properties object element
            List<BaseDataCompare> propertyData = new List<BaseDataCompare>();
            Type type = Element.GetType();
            foreach (var property in type.GetProperties())
            {
                if (propertyData.All(data => data.Name != property.Name))
                {
                    BaseDataCompare data = new BaseDataCompare();
                    data.Name = property.Name;
                    data.Type = property.PropertyType;
                    try
                    {
                        data.Value = property.GetValue(Element)?.ToString();
                    }
                    catch (Exception e)
                    {
                        data.Value = e.Message;
                    }
                    propertyData.Add(data);
                }
            }
            return propertyData;
        }
        
    }
 

 
}
