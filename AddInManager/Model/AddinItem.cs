using System.IO;
using Autodesk.Revit.Attributes;

namespace RevitAddinManager.Model;

public class AddinItem : IAddinNode
{
    public AddinItem(AddinType type)
    {
        AddinType = type;
        m_clientId = Guid.NewGuid();
        ClientIdString = m_clientId.ToString();
        m_assemblyPath = string.Empty;
        AssemblyName = string.Empty;
        FullClassName = string.Empty;
        m_name = string.Empty;
        Save = true;
        VisibilityMode = VisibilityMode.AlwaysVisible;
    }

    public AddinItem(string assemblyPath, Guid clientId, string fullClassName, AddinType type, TransactionMode? transactionMode, RegenerationOption? regenerationOption, JournalingMode? journalingMode)
    {
        TransactionMode = transactionMode;
        RegenerationMode = regenerationOption;
        JournalingMode = journalingMode;
        AddinType = type;
        m_assemblyPath = assemblyPath;
        AssemblyName = Path.GetFileName(m_assemblyPath);
        m_clientId = clientId;
        ClientIdString = clientId.ToString();
        FullClassName = fullClassName;
        var num = fullClassName.LastIndexOf(".");
        m_name = fullClassName.Substring(num + 1);
        Save = true;
        VisibilityMode = VisibilityMode.AlwaysVisible;
    }

    public void SaveToManifest()
    {
        var manifestFile = new ManifestFile(m_name + DefaultSetting.FormatExAddin);
        if (AddinType == AddinType.Application)
        {
            manifestFile.Applications.Add(this);
        }
        else if (AddinType == AddinType.Command)
        {
            manifestFile.Commands.Add(this);
        }
        manifestFile.Save();
    }


    public AddinType AddinType { get; set; }


    public string AssemblyPath
    {
        get => m_assemblyPath;
        set
        {
            m_assemblyPath = value;
            AssemblyName = Path.GetFileName(m_assemblyPath);
        }
    }


    public string AssemblyName { get; set; }


    public Guid ClientId
    {
        get => m_clientId;
        set
        {
            m_clientId = value;
            ClientIdString = m_clientId.ToString();
        }
    }


    protected internal string ClientIdString { get; set; }


    public string FullClassName { get; set; }


    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(m_name))
            {
                return "External Tool";
            }
            return m_name;
        }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                m_name = value;
                return;
            }
            m_name = "External Tool";
        }
    }

    public string Description
    {
        get
        {
            if (string.IsNullOrEmpty(m_description))
            {
                return String.Empty;
            }
            return m_description;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                m_description = String.Empty;
                return;
            }
            m_description = value;
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
        return m_name;
    }


    protected string m_assemblyPath;


    protected Guid m_clientId;


    private string m_name;


    private string m_description;
}