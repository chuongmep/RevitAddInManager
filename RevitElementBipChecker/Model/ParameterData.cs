using Autodesk.Revit.DB;

namespace RevitElementBipChecker.Model
{
   public class ParameterData
    {
        public ParameterData(Parameter parameter,Document doc,bool isinstance=true)
        {
            this.Parameter = parameter;
            this.BuiltInParameter = (parameter.Definition as InternalDefinition).BuiltInParameter.ToString();
            this.ParameterName = parameter.Definition.Name;
            this.Id = parameter.Id.ToString();
            this.ParameterGroup = parameter.Definition.ParameterGroup.ToString();
            this.ParameterType = parameter.GetParameterType();
            this.GroupName = LabelUtils.GetLabelFor(parameter.Definition.ParameterGroup);
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

        public Autodesk.Revit.DB.Parameter Parameter { get; set; }
        public string ParameterName { get; set; }
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
