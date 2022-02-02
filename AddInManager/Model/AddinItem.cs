using System.IO;
using Autodesk.Revit.Attributes;

namespace RevitAddinManager.Model;

public class AddinItem : IAddinNode
{
    public AddinItem(AddinType type)
    {
        AddinType = type;
        clientId = Guid.NewGuid();
        ClientIdString = clientId.ToString();
        _assemblyPath = string.Empty;
        AssemblyName = string.Empty;
        FullClassName = string.Empty;
        _name = string.Empty;
        Save = true;
        VisibilityMode = VisibilityMode.AlwaysVisible;
    }

    public AddinItem(string assemblyPath, Guid clientId, string fullClassName, AddinType type, TransactionMode? transactionMode, RegenerationOption? regenerationOption, JournalingMode? journalingMode)
    {
        TransactionMode = transactionMode;
        RegenerationMode = regenerationOption;
        JournalingMode = journalingMode;
        AddinType = type;
        _assemblyPath = assemblyPath;
        AssemblyName = Path.GetFileName(_assemblyPath);
        this.clientId = clientId;
        ClientIdString = clientId.ToString();
        FullClassName = fullClassName;
        var num = fullClassName.LastIndexOf(".", StringComparison.Ordinal);
        _name = fullClassName.Substring(num + 1);
        Save = true;
        VisibilityMode = VisibilityMode.AlwaysVisible;
    }

    public void SaveToManifest()
    {
        var manifestFile = new ManifestFile(_name + DefaultSetting.FormatExAddin);
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
        get => _assemblyPath;
        set
        {
            _assemblyPath = value;
            AssemblyName = Path.GetFileName(_assemblyPath);
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
            if (string.IsNullOrEmpty(_name))
            {
                return "External Tool";
            }
            return _name;
        }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                _name = value;
                return;
            }
            _name = "External Tool";
        }
    }

    public string Description
    {
        get
        {
            if (string.IsNullOrEmpty(_description))
            {
                return string.Empty;
            }
            return _description;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                _description = string.Empty;
                return;
            }
            _description = value;
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
        return _name;
    }


    protected string _assemblyPath;


    protected Guid clientId;


    private string _name;


    private string _description;
}