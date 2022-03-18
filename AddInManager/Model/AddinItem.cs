using Autodesk.Revit.Attributes;
using System.IO;

namespace RevitAddinManager.Model;

public class AddinItem : IAddinNode
{
    public AddinItem(AddinType type)
    {
        AddinType = type;
        clientId = Guid.NewGuid();
        ClientIdString = clientId.ToString();
        assemblyPath = string.Empty;
        AssemblyName = string.Empty;
        FullClassName = string.Empty;
        name = string.Empty;
        Save = true;
        VisibilityMode = VisibilityMode.AlwaysVisible;
    }

    public AddinItem(string assemblyPath, Guid clientId, string fullClassName, AddinType type, TransactionMode? transactionMode, RegenerationOption? regenerationOption, JournalingMode? journalingMode)
    {
        TransactionMode = transactionMode;
        RegenerationMode = regenerationOption;
        JournalingMode = journalingMode;
        AddinType = type;
        this.assemblyPath = assemblyPath;
        AssemblyName = Path.GetFileName(this.assemblyPath);
        this.clientId = clientId;
        ClientIdString = clientId.ToString();
        FullClassName = fullClassName;
        var num = fullClassName.LastIndexOf(".", StringComparison.Ordinal);
        name = fullClassName.Substring(num + 1);
        Save = true;
        VisibilityMode = VisibilityMode.AlwaysVisible;
    }

    public void SaveToManifest()
    {
        var manifestFile = new ManifestFile(name + DefaultSetting.FormatExAddin);
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
        get => assemblyPath;
        set
        {
            assemblyPath = value;
            AssemblyName = Path.GetFileName(assemblyPath);
        }
    }

    public string AssemblyName { get; set; }

    public Guid ClientId
    {
        get => clientId;
        set
        {
            clientId = value;
            ClientIdString = clientId.ToString();
        }
    }

    protected internal string ClientIdString { get; set; }

    public string FullClassName { get; set; }

    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(name))
            {
                return "External Tool";
            }
            return name;
        }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                name = value;
                return;
            }
            name = "External Tool";
        }
    }

    public string Description
    {
        get
        {
            if (string.IsNullOrEmpty(description))
            {
                return string.Empty;
            }
            return description;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                description = string.Empty;
                return;
            }
            description = value;
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
        return name;
    }

    protected string assemblyPath;

    protected Guid clientId;

    private string name;

    private string description;
}