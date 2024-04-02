using Autodesk.Revit.DB;

namespace RevitElementBipChecker.Model
{
   public class ParameterData : ViewmodeBase
    {
        // public ParameterData()
        // {
        //     
        // }
        /// <summary>
        ///  Init Parameter data with type or instance
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="doc"></param>
        /// <param name="isinstance"></param>
        public ParameterData(Parameter parameter,Document doc,bool isinstance=true)
        {
            this.Parameter = parameter;
            this.BuiltInParameter = (parameter.Definition as InternalDefinition)?.BuiltInParameter.ToString();
            this.Name = parameter.Definition.Name;
            this.Id = parameter.Id.ToString();
#if R19 || R20 || R21 || R22  || R23 || R24
            this.ParameterGroup = parameter.Definition.ParameterGroup.ToString();
             this.GroupName = LabelUtils.GetLabelFor(parameter.Definition.ParameterGroup);
#else
            this.ParameterGroup = parameter.Definition.GetGroupTypeId().ToString();
            this.GroupName = LabelUtils.GetLabelForGroup(parameter.Definition.GetGroupTypeId());
#endif
            this.ParameterType = parameter.GetParameterType();

            this.Type = parameter.GetParameterType();
            this.Unit = parameter.GetParameterUnit();
            this.ReadWrite = parameter.IsReadWrite();
            this.Value = parameter.GetValue();
            this.StringValue = parameter.AsValueString() == null
                ? parameter.AsString()
                : parameter.AsValueString() ;
            this.Shared = parameter.Shared();
            this.GUID = parameter.Guid();
            this.TypeOrInstance = isinstance?"Instance":"Type";
            this.AssGlobalPara = parameter.GetAssGlobalParameter(doc);
            this.AssGlobalParaValue = parameter.GetAssGlobalParameterValue(doc);
        }

        public ParameterData(Autodesk.Revit.DB.Element element, Parameter parameter, Document doc,
            bool isinstance = true) : this(parameter, doc, isinstance)
        {
            this.ElementId = element.Id.ToString();
            this.Category = element.Category?.Name;
        }

        public string ElementId { get; set; }
        public string Category { get; set; }
        public Autodesk.Revit.DB.Parameter Parameter { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string TypeOrInstance { get; set; }
        public string BuiltInParameter { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string ReadWrite { get; set; }
        public string Value { get; set; }
        public string StringValue { get; set; }
        public string ParameterGroup { get; set; }
        public string ParameterType { get; set; }
        public string GroupName { get; set; }
        public string Shared { get; set; }
        public string GUID { get; set; }

        public string AssGlobalPara { get; set; }
        public string AssGlobalParaValue { get; set; }
        
    }
}
