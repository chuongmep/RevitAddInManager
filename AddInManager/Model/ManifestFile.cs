using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace RevitAddinManager.Model;

public class ManifestFile
{
    public ManifestFile()
    {
        _local = false;
        _applications = new List<AddinItem>();
        _commands = new List<AddinItem>();
    }

    public ManifestFile(string fileName) : this()
    {
        _fileName = fileName;
        if (string.IsNullOrEmpty(_filePath))
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AddIn");
            _filePath = Path.Combine(path, _fileName);
        }
    }

    public ManifestFile(bool local) : this()
    {
        _local = local;
    }

    public void Load()
    {
        _xmlDoc = new XmlDocument();
        _xmlDoc.Load(_filePath);
        var documentElement = _xmlDoc.DocumentElement;
        if (!documentElement.Name.Equals(ROOT_NODE))
        {

            throw new ArgumentException(INCORRECT_NODE);
        }
        if (documentElement.ChildNodes.Count == 0)
        {
            throw new ArgumentException(EMPTY_ADDIN);
        }
        _applications.Clear();
        _commands.Clear();
        foreach (var obj in documentElement.ChildNodes)
        {
            var xmlNode = (XmlNode)obj;
            if (!xmlNode.Name.Equals(ADDIN_NODE) || xmlNode.Attributes.Count != 1)
            {
                throw new ArgumentException(INCORRECT_NODE);
            }
            var xmlAttribute = xmlNode.Attributes[0];
            if (xmlAttribute.Value.Equals(APPLICATION_NODE))
            {
                ParseExternalApplications(xmlNode);
            }
            else
            {
                if (!xmlAttribute.Value.Equals(COMMAND_NODE))
                {
                    throw new ArgumentException(INCORRECT_NODE);
                }
                ParseExternalCommands(xmlNode);
            }
        }
    }

    public void Save()
    {
        SaveAs(_filePath);
    }

    public void SaveAs(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentNullException(FILENAME_NULL_OR_EMPTY);
        }
        if (!filePath.ToLower().EndsWith(DefaultSetting.FormatExAddin))
        {
            throw new ArgumentException(FILENAME_INCORRECT_WARNING + filePath);
        }
        var directoryName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        var fileInfo = new FileInfo(filePath);
        _xmlDoc = new XmlDocument();
        CreateXmlForManifest();
        if (File.Exists(filePath))
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
        }
        TextWriter w = new StreamWriter(filePath, false, Encoding.UTF8);
        var xmlTextWriter = new XmlTextWriter(w);
        xmlTextWriter.Formatting = Formatting.Indented;
        _xmlDoc.Save(xmlTextWriter);
        xmlTextWriter.Close();
        _filePath = fileInfo.FullName;
        _fileName = Path.GetFileName(fileInfo.FullName);
    }

    public string FileName
    {
        get => _fileName;
        set => _fileName = value;
    }

    public bool Local
    {
        get => _local;
        set => _local = value;
    }

    private string _vendorDescription;
    public string VendorDescription
    {
        get => _vendorDescription;
        set => _vendorDescription = value;
    }
    public string FilePath
    {
        get
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AddIn");
                _filePath = Path.Combine(path, DefaultSetting.AimInternalName);
            }
            return _filePath;
        }
        set => _filePath = value;
    }


    public List<AddinItem> Applications
    {
        get => _applications;
        set => _applications = value;
    }

    public List<AddinItem> Commands
    {
        get => _commands;
        set => _commands = value;
    }

    private XmlDocument CreateXmlForManifest()
    {
        var xmlNode = _xmlDoc.AppendChild(_xmlDoc.CreateElement(ROOT_NODE));
        foreach (var currentApp in _applications)
        {
            var xmlElement = _xmlDoc.CreateElement(ADDIN_NODE);
            xmlElement.SetAttribute(TYPE_ATTRIBUTE, APPLICATION_NODE);
            xmlNode.AppendChild(xmlElement);
            AddApplicationToXmlElement(xmlElement, currentApp);
            var xmlElement2 = _xmlDoc.CreateElement(VENDORID);
            xmlElement2.InnerText = "ADSK";
            xmlElement.AppendChild(xmlElement2);
            xmlElement2 = _xmlDoc.CreateElement(VENDORDESCRIPTION);
            xmlElement2.InnerText = "Autodesk, www.autodesk.com";
            xmlElement.AppendChild(xmlElement2);
        }
        foreach (var command in _commands)
        {
            var xmlElement3 = _xmlDoc.CreateElement(ADDIN_NODE);
            xmlElement3.SetAttribute(TYPE_ATTRIBUTE, COMMAND_NODE);
            xmlNode.AppendChild(xmlElement3);
            AddCommandToXmlElement(xmlElement3, command);
            var xmlElement4 = _xmlDoc.CreateElement(VENDORID);
            xmlElement4.InnerText = "ADSK";
            xmlElement3.AppendChild(xmlElement4);
            xmlElement4 = _xmlDoc.CreateElement(VENDORDESCRIPTION);
            if(VendorDescription==string.Empty) xmlElement4.InnerText = "Autodesk, www.autodesk.com";
            else xmlElement4.InnerText = VendorDescription;
            xmlElement3.AppendChild(xmlElement4);
        }
        return _xmlDoc;
    }

    private void AddAddInItemToXmlElement(XmlElement xmlEle, AddinItem addinItem)
    {
        if (!string.IsNullOrEmpty(addinItem.AssemblyPath))
        {
            var xmlElement = _xmlDoc.CreateElement(ASSEMBLY);
            if (_local)
            {
                xmlElement.InnerText = addinItem.AssemblyName;
            }
            else
            {
                xmlElement.InnerText = addinItem.AssemblyPath;
            }
            xmlEle.AppendChild(xmlElement);
        }
        if (!string.IsNullOrEmpty(addinItem.ClientIdString))
        {
            var xmlElement2 = _xmlDoc.CreateElement(CLIENTID);
            xmlElement2.InnerText = addinItem.ClientIdString;
            xmlEle.AppendChild(xmlElement2);
        }
        if (!string.IsNullOrEmpty(addinItem.FullClassName))
        {
            var xmlElement3 = _xmlDoc.CreateElement(FULLCLASSNAME);
            xmlElement3.InnerText = addinItem.FullClassName;
            xmlEle.AppendChild(xmlElement3);
        }
    }

    private void AddApplicationToXmlElement(XmlElement appEle, AddinItem currentApp)
    {
        if (!string.IsNullOrEmpty(currentApp.Name))
        {
            var xmlElement = _xmlDoc.CreateElement(NAME_NODE);
            xmlElement.InnerText = currentApp.Name;
            appEle.AppendChild(xmlElement);
        }
        AddAddInItemToXmlElement(appEle, currentApp);
    }

    private void AddCommandToXmlElement(XmlElement commandEle, AddinItem command)
    {
        AddAddInItemToXmlElement(commandEle, command);
        XmlElement xmlElement;
        if (!string.IsNullOrEmpty(command.Name))
        {
            xmlElement = _xmlDoc.CreateElement(TEXT);
            xmlElement.InnerText = command.Name;
            commandEle.AppendChild(xmlElement);
        }
        if (!string.IsNullOrEmpty(command.Description))
        {
            xmlElement = _xmlDoc.CreateElement(DESCRIPTION);
            xmlElement.InnerText = command.Description;
            commandEle.AppendChild(xmlElement);
        }
        var text = command.VisibilityMode.ToString();
        if (!string.IsNullOrEmpty(text))
        {
            text = text.Replace(",", " |");
        }
        xmlElement = _xmlDoc.CreateElement(VISIBILITYMODE);
        xmlElement.InnerText = text;
        commandEle.AppendChild(xmlElement);
    }

    private void ParseExternalApplications(XmlNode nodeApplication)
    {
        var addinItem = new AddinItem(AddinType.Application);
        ParseApplicationItems(addinItem, nodeApplication);
        _applications.Add(addinItem);
    }

    private void ParseExternalCommands(XmlNode nodeCommand)
    {
        var addinItem = new AddinItem(AddinType.Command);
        ParseCommandItems(addinItem, nodeCommand);
        _commands.Add(addinItem);
    }

    private void ParseApplicationItems(AddinItem addinApp, XmlNode nodeAddIn)
    {
        ParseAddInItem(addinApp, nodeAddIn);
        var xmlElement = nodeAddIn[NAME_NODE];
        if (xmlElement != null && !string.IsNullOrEmpty(xmlElement.InnerText))
        {
            addinApp.Name = xmlElement.InnerText;
        }
    }

    private void ParseCommandItems(AddinItem command, XmlNode nodeAddIn)
    {
        ParseAddInItem(command, nodeAddIn);
        var xmlElement = nodeAddIn[TEXT];
        if (xmlElement != null)
        {
            command.Name = xmlElement.InnerText;
        }
        xmlElement = nodeAddIn[DESCRIPTION];
        if (xmlElement != null)
        {
            command.Description = xmlElement.InnerText;
        }
        xmlElement = nodeAddIn[VISIBILITYMODE];
        if (xmlElement != null && !string.IsNullOrEmpty(xmlElement.InnerText))
        {
            command.VisibilityMode = ParseVisibilityMode(xmlElement.InnerText);
        }
    }

    private void ParseAddInItem(AddinItem addinItem, XmlNode nodeAddIn)
    {
        var xmlElement = nodeAddIn[ASSEMBLY];
        if (xmlElement != null)
        {
            if (_local)
            {
                addinItem.AssemblyName = xmlElement.InnerText;
            }
            else
            {
                addinItem.AssemblyPath = xmlElement.InnerText;
            }
        }
        xmlElement = nodeAddIn[CLIENTID];
        if (xmlElement != null)
        {
            try
            {
                if (!string.IsNullOrEmpty(xmlElement.InnerText))
                {
                    addinItem.ClientId = new Guid(xmlElement.InnerText);
                }
                else
                {
                    addinItem.ClientId = Guid.Empty;
                }
            }
            catch (Exception)
            {
                addinItem.ClientId = Guid.Empty;
                addinItem.ClientIdString = xmlElement.InnerText;
            }
        }
        xmlElement = nodeAddIn[FULLCLASSNAME];
        if (xmlElement != null)
        {
            addinItem.FullClassName = xmlElement.InnerText;
        }
    }

    private VisibilityMode ParseVisibilityMode(string visibilityModeString)
    {
        var visibilityMode = VisibilityMode.AlwaysVisible;
        VisibilityMode result;
        try
        {
            var text = "|";
            var separator = text.ToCharArray();
            var array = visibilityModeString.Replace(" | ", "|").Split(separator);
            foreach (var value in array)
            {
                var visibilityMode2 = (VisibilityMode)Enum.Parse(typeof(VisibilityMode), value);
                visibilityMode |= visibilityMode2;
            }
            result = visibilityMode;
        }
        catch (Exception)
        {
            throw new ArgumentException(UNKNOW_VISIBILITYMODE);
        }
        return result;
    }

    private string getFullPath(string fileName)
    {
        FileInfo fileInfo = null;
        try
        {
            fileInfo = new FileInfo(fileName);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(fileName + Environment.NewLine + ex.ToString());
        }
        return fileInfo.FullName;
    }

    private string _fileName;

    private bool _local;

    private string _filePath;

    private List<AddinItem> _applications;

    private List<AddinItem> _commands;

    private string ROOT_NODE = "RevitAddIns";

    private string ADDIN_NODE = "AddIn";

    private string APPLICATION_NODE = "Application";

    private string COMMAND_NODE = "Command";

    private string TYPE_ATTRIBUTE = "Type";

    private string INCORRECT_NODE = "incorrect node in addin file!";

    private string EMPTY_ADDIN = "empty addin file!";

    private string ASSEMBLY = "Assembly";

    private string CLIENTID = "ClientId";

    private string FULLCLASSNAME = "FullClassName";

    private string NAME_NODE = "Name";

    private string TEXT = "Text";

    private string DESCRIPTION = "Description";

    private string VENDORID = "VendorId";

    private string VENDORDESCRIPTION = "VendorDescription";

    private string VISIBILITYMODE = "VisibilityMode";

    private string UNKNOW_VISIBILITYMODE = "Unrecognizable VisibilityMode!";

    private string FILENAME_INCORRECT_WARNING = "File name is incorrect, not .addin file .";

    private string FILENAME_NULL_OR_EMPTY = "File name for RevitAddInManifest is null or empty";

    private XmlDocument _xmlDoc;
}