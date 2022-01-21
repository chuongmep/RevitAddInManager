using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace AddinManager.Model
{
    public class ManifestFile
    {
        public ManifestFile()
        {
            this.m_local = false;
            this.m_applications = new List<AddinItem>();
            this.m_commands = new List<AddinItem>();
        }

        public ManifestFile(string fileName) : this()
        {
            this.m_fileName = fileName;
            if (string.IsNullOrEmpty(this.m_filePath))
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AddIn");
                this.m_filePath = Path.Combine(path, this.m_fileName);
            }
        }

        public ManifestFile(bool local) : this()
        {
            this.m_local = local;
        }

        public void Load()
        {
            this.m_xmlDoc = new XmlDocument();
            this.m_xmlDoc.Load(this.m_filePath);
            XmlElement documentElement = this.m_xmlDoc.DocumentElement;
            if (!documentElement.Name.Equals(this.ROOT_NODE))
            {

                throw new System.ArgumentException(this.INCORRECT_NODE);
            }
            if (documentElement.ChildNodes.Count == 0)
            {
                throw new System.ArgumentException(this.EMPTY_ADDIN);
            }
            this.m_applications.Clear();
            this.m_commands.Clear();
            foreach (object obj in documentElement.ChildNodes)
            {
                XmlNode xmlNode = (XmlNode)obj;
                if (!xmlNode.Name.Equals(this.ADDIN_NODE) || xmlNode.Attributes.Count != 1)
                {
                    throw new System.ArgumentException(this.INCORRECT_NODE);
                }
                XmlAttribute xmlAttribute = xmlNode.Attributes[0];
                if (xmlAttribute.Value.Equals(this.APPLICATION_NODE))
                {
                    this.ParseExternalApplications(xmlNode);
                }
                else
                {
                    if (!xmlAttribute.Value.Equals(this.COMMAND_NODE))
                    {
                        throw new System.ArgumentException(this.INCORRECT_NODE);
                    }
                    this.ParseExternalCommands(xmlNode);
                }
            }
        }

        public void Save()
        {
            this.SaveAs(this.m_filePath);
        }

        public void SaveAs(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new System.ArgumentNullException(this.FILENAME_NULL_OR_EMPTY);
            }
            if (!filePath.ToLower().EndsWith(DefaultSetting.FormatExAddin))
            {
                throw new System.ArgumentException(this.FILENAME_INCORRECT_WARNING + filePath);
            }
            string directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            FileInfo fileInfo = new FileInfo(filePath);
            this.m_xmlDoc = new XmlDocument();
            this.CreateXmlForManifest();
            if (File.Exists(filePath))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }
            TextWriter w = new StreamWriter(filePath, false, Encoding.UTF8);
            XmlTextWriter xmlTextWriter = new XmlTextWriter(w);
            xmlTextWriter.Formatting = Formatting.Indented;
            this.m_xmlDoc.Save(xmlTextWriter);
            xmlTextWriter.Close();
            this.m_filePath = fileInfo.FullName;
            this.m_fileName = Path.GetFileName(fileInfo.FullName);
        }

        public string FileName
        {
            get => this.m_fileName;
            set => this.m_fileName = value;
        }

        public bool Local
        {
            get => this.m_local;
            set => this.m_local = value;
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
                if (string.IsNullOrEmpty(this.m_filePath))
                {
                    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AddIn");
                    this.m_filePath = Path.Combine(path, DefaultSetting.AimInternalName);
                }
                return this.m_filePath;
            }
            set => this.m_filePath = value;
        }


        public List<AddinItem> Applications
        {
            get => this.m_applications;
            set => this.m_applications = value;
        }

        public List<AddinItem> Commands
        {
            get => this.m_commands;
            set => this.m_commands = value;
        }

        private XmlDocument CreateXmlForManifest()
        {
            XmlNode xmlNode = this.m_xmlDoc.AppendChild(this.m_xmlDoc.CreateElement(this.ROOT_NODE));
            foreach (AddinItem currentApp in this.m_applications)
            {
                XmlElement xmlElement = this.m_xmlDoc.CreateElement(this.ADDIN_NODE);
                xmlElement.SetAttribute(this.TYPE_ATTRIBUTE, this.APPLICATION_NODE);
                xmlNode.AppendChild(xmlElement);
                this.AddApplicationToXmlElement(xmlElement, currentApp);
                XmlElement xmlElement2 = this.m_xmlDoc.CreateElement(this.VENDORID);
                xmlElement2.InnerText = "ADSK";
                xmlElement.AppendChild(xmlElement2);
                xmlElement2 = this.m_xmlDoc.CreateElement(this.VENDORDESCRIPTION);
                xmlElement2.InnerText = "Autodesk, www.autodesk.com";
                xmlElement.AppendChild(xmlElement2);
            }
            foreach (AddinItem command in this.m_commands)
            {
                XmlElement xmlElement3 = this.m_xmlDoc.CreateElement(this.ADDIN_NODE);
                xmlElement3.SetAttribute(this.TYPE_ATTRIBUTE, this.COMMAND_NODE);
                xmlNode.AppendChild(xmlElement3);
                this.AddCommandToXmlElement(xmlElement3, command);
                XmlElement xmlElement4 = this.m_xmlDoc.CreateElement(this.VENDORID);
                xmlElement4.InnerText = "ADSK";
                xmlElement3.AppendChild(xmlElement4);
                xmlElement4 = this.m_xmlDoc.CreateElement(this.VENDORDESCRIPTION);
                if(VendorDescription==string.Empty) xmlElement4.InnerText = "Autodesk, www.autodesk.com";
                else xmlElement4.InnerText = VendorDescription;
                xmlElement3.AppendChild(xmlElement4);
            }
            return this.m_xmlDoc;
        }

        private void AddAddInItemToXmlElement(XmlElement xmlEle, AddinItem addinItem)
        {
            if (!string.IsNullOrEmpty(addinItem.AssemblyPath))
            {
                XmlElement xmlElement = this.m_xmlDoc.CreateElement(this.ASSEMBLY);
                if (this.m_local)
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
                XmlElement xmlElement2 = this.m_xmlDoc.CreateElement(this.CLIENTID);
                xmlElement2.InnerText = addinItem.ClientIdString;
                xmlEle.AppendChild(xmlElement2);
            }
            if (!string.IsNullOrEmpty(addinItem.FullClassName))
            {
                XmlElement xmlElement3 = this.m_xmlDoc.CreateElement(this.FULLCLASSNAME);
                xmlElement3.InnerText = addinItem.FullClassName;
                xmlEle.AppendChild(xmlElement3);
            }
        }

        private void AddApplicationToXmlElement(XmlElement appEle, AddinItem currentApp)
        {
            if (!string.IsNullOrEmpty(currentApp.Name))
            {
                XmlElement xmlElement = this.m_xmlDoc.CreateElement(this.NAME_NODE);
                xmlElement.InnerText = currentApp.Name;
                appEle.AppendChild(xmlElement);
            }
            this.AddAddInItemToXmlElement(appEle, currentApp);
        }

        private void AddCommandToXmlElement(XmlElement commandEle, AddinItem command)
        {
            this.AddAddInItemToXmlElement(commandEle, command);
            XmlElement xmlElement;
            if (!string.IsNullOrEmpty(command.Name))
            {
                xmlElement = this.m_xmlDoc.CreateElement(this.TEXT);
                xmlElement.InnerText = command.Name;
                commandEle.AppendChild(xmlElement);
            }
            if (!string.IsNullOrEmpty(command.Description))
            {
                xmlElement = this.m_xmlDoc.CreateElement(this.DESCRIPTION);
                xmlElement.InnerText = command.Description;
                commandEle.AppendChild(xmlElement);
            }
            string text = command.VisibilityMode.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace(",", " |");
            }
            xmlElement = this.m_xmlDoc.CreateElement(this.VISIBILITYMODE);
            xmlElement.InnerText = text;
            commandEle.AppendChild(xmlElement);
        }

        private void ParseExternalApplications(XmlNode nodeApplication)
        {
            AddinItem addinItem = new AddinItem(AddinType.Application);
            this.ParseApplicationItems(addinItem, nodeApplication);
            this.m_applications.Add(addinItem);
        }

        private void ParseExternalCommands(XmlNode nodeCommand)
        {
            AddinItem addinItem = new AddinItem(AddinType.Command);
            this.ParseCommandItems(addinItem, nodeCommand);
            this.m_commands.Add(addinItem);
        }

        private void ParseApplicationItems(AddinItem addinApp, XmlNode nodeAddIn)
        {
            this.ParseAddInItem(addinApp, nodeAddIn);
            XmlElement xmlElement = nodeAddIn[this.NAME_NODE];
            if (xmlElement != null && !string.IsNullOrEmpty(xmlElement.InnerText))
            {
                addinApp.Name = xmlElement.InnerText;
            }
        }

        private void ParseCommandItems(AddinItem command, XmlNode nodeAddIn)
        {
            this.ParseAddInItem(command, nodeAddIn);
            XmlElement xmlElement = nodeAddIn[this.TEXT];
            if (xmlElement != null)
            {
                command.Name = xmlElement.InnerText;
            }
            xmlElement = nodeAddIn[this.DESCRIPTION];
            if (xmlElement != null)
            {
                command.Description = xmlElement.InnerText;
            }
            xmlElement = nodeAddIn[this.VISIBILITYMODE];
            if (xmlElement != null && !string.IsNullOrEmpty(xmlElement.InnerText))
            {
                command.VisibilityMode = this.ParseVisibilityMode(xmlElement.InnerText);
            }
        }

        private void ParseAddInItem(AddinItem addinItem, XmlNode nodeAddIn)
        {
            XmlElement xmlElement = nodeAddIn[this.ASSEMBLY];
            if (xmlElement != null)
            {
                if (this.m_local)
                {
                    addinItem.AssemblyName = xmlElement.InnerText;
                }
                else
                {
                    addinItem.AssemblyPath = xmlElement.InnerText;
                }
            }
            xmlElement = nodeAddIn[this.CLIENTID];
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
            xmlElement = nodeAddIn[this.FULLCLASSNAME];
            if (xmlElement != null)
            {
                addinItem.FullClassName = xmlElement.InnerText;
            }
        }

        private VisibilityMode ParseVisibilityMode(string visibilityModeString)
        {
            VisibilityMode visibilityMode = VisibilityMode.AlwaysVisible;
            VisibilityMode result;
            try
            {
                string text = "|";
                char[] separator = text.ToCharArray();
                string[] array = visibilityModeString.Replace(" | ", "|").Split(separator);
                foreach (string value in array)
                {
                    VisibilityMode visibilityMode2 = (VisibilityMode)Enum.Parse(typeof(VisibilityMode), value);
                    visibilityMode |= visibilityMode2;
                }
                result = visibilityMode;
            }
            catch (Exception)
            {
                throw new System.ArgumentException(this.UNKNOW_VISIBILITYMODE);
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
                throw new System.ArgumentException(fileName + Environment.NewLine + ex.ToString());
            }
            return fileInfo.FullName;
        }

        private string m_fileName;

        private bool m_local;

        private string m_filePath;

        private List<AddinItem> m_applications;

        private List<AddinItem> m_commands;

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

        private XmlDocument m_xmlDoc;
    }
}
