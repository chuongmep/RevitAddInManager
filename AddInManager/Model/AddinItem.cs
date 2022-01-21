using System.IO;
using Autodesk.Revit.Attributes;

namespace AddinManager.Model
{
    public class AddinItem : IAddinNode
    {
        public AddinItem(AddinType type)
        {
            this.AddinType = type;
            this.m_clientId = Guid.NewGuid();
            this.ClientIdString = this.m_clientId.ToString();
            this.m_assemblyPath = string.Empty;
            this.AssemblyName = string.Empty;
            this.FullClassName = string.Empty;
            this.m_name = string.Empty;
            this.Save = true;
            this.VisibilityMode = VisibilityMode.AlwaysVisible;
        }

        public AddinItem(string assemblyPath, Guid clientId, string fullClassName, AddinType type, TransactionMode? transactionMode, RegenerationOption? regenerationOption, JournalingMode? journalingMode)
        {
            this.TransactionMode = transactionMode;
            this.RegenerationMode = regenerationOption;
            this.JournalingMode = journalingMode;
            this.AddinType = type;
            this.m_assemblyPath = assemblyPath;
            this.AssemblyName = Path.GetFileName(this.m_assemblyPath);
            this.m_clientId = clientId;
            this.ClientIdString = clientId.ToString();
            this.FullClassName = fullClassName;
            int num = fullClassName.LastIndexOf(".");
            this.m_name = fullClassName.Substring(num + 1);
            this.Save = true;
            this.VisibilityMode = VisibilityMode.AlwaysVisible;
        }

        public void SaveToManifest()
        {
            ManifestFile manifestFile = new ManifestFile(this.m_name + DefaultSetting.FormatExAddin);
            if (this.AddinType == AddinType.Application)
            {
                manifestFile.Applications.Add(this);
            }
            else if (this.AddinType == AddinType.Command)
            {
                manifestFile.Commands.Add(this);
            }
            manifestFile.Save();
        }


        public AddinType AddinType { get; set; }


        public string AssemblyPath
        {
            get => this.m_assemblyPath;
            set
            {
                this.m_assemblyPath = value;
                this.AssemblyName = Path.GetFileName(this.m_assemblyPath);
            }
        }


        public string AssemblyName { get; set; }


        public Guid ClientId
        {
            get => this.m_clientId;
            set
            {
                this.m_clientId = value;
                this.ClientIdString = this.m_clientId.ToString();
            }
        }


        protected internal string ClientIdString { get; set; }


        public string FullClassName { get; set; }


        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_name))
                {
                    return "External Tool";
                }
                return this.m_name;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.m_name = value;
                    return;
                }
                this.m_name = "External Tool";
            }
        }

        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_description))
                {
                    return "\"\"";
                }
                return this.m_description;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.m_description = "\"\"";
                    return;
                }
                this.m_description = value;
            }
        }


        public VisibilityMode VisibilityMode { get; set; }

        public bool Save { get; set; }


        public bool Hidden { get; set; }


        public TransactionMode? TransactionMode { get; set; }


        public RegenerationOption? RegenerationMode { get; set; }


        public JournalingMode? JournalingMode { get; set; }


        public override string ToString()
        {
            return this.m_name;
        }


        protected string m_assemblyPath;


        protected Guid m_clientId;


        private string m_name;


        private string m_description;
    }
}
