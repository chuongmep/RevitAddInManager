using System.IO;
using System.Reflection;
using System.Text;
using Autodesk.Revit.Attributes;

namespace RevitAddinManager.Model
{
    public abstract class Addins
    {
        public SortedDictionary<string, Addin> AddinDict
        {
            get => m_addinDict;
            set => m_addinDict = value;
        }

        public int Count => m_addinDict.Count;

        public Addins()
        {
            m_addinDict = new SortedDictionary<string, Addin>();
        }

        public void SortAddin()
        {
            foreach (var addin in m_addinDict.Values)
            {
                addin.SortAddinItem();
            }
        }


        public void AddAddIn(Addin addin)
        {
            var fileName = Path.GetFileName(addin.FilePath);
            if (m_addinDict.ContainsKey(fileName))
            {
                m_addinDict.Remove(fileName);
            }
            m_addinDict[fileName] = addin;
        }

        public bool RemoveAddIn(Addin addin)
        {
            var fileName = Path.GetFileName(addin.FilePath);
            if (m_addinDict.ContainsKey(fileName))
            {
                m_addinDict.Remove(fileName);
                return true;
            }
            return false;
        }

        public void AddItem(AddinItem item)
        {

            var assemblyName = item.AssemblyName;
            if (!m_addinDict.ContainsKey(assemblyName))
            {
                m_addinDict[assemblyName] = new Addin(item.AssemblyPath);
            }
            m_addinDict[assemblyName].ItemList.Add(item);

        }

        public List<AddinItem> LoadItems(Assembly assembly, string fullName, string originalAssemblyFilePath, AddinType type)
        {
            var list = new List<AddinItem>();
            Type[] array = null;
            try
            {
                array = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                array = ex.Types;
                if (array == null)
                {
                    return list;
                }
            }
            var list2 = new List<string>();
            var list3 = new List<string>();
            foreach (var type2 in array)
            {
                try
                {
                    if (!(null == type2) && !type2.IsAbstract)
                    {
                        var @interface = type2.GetInterface(fullName);
                        if (null != @interface)
                        {
                            TransactionMode? transactionMode = null;
                            RegenerationOption? regenerationOption = null;
                            JournalingMode? journalingMode = null;
                            if (type != AddinType.Application)
                            {
                                var customAttributes = Attribute.GetCustomAttributes(type2, false);
                                foreach (var attribute in customAttributes)
                                {
                                    if (attribute is RegenerationAttribute)
                                    {
                                        var regenerationAttribute = (RegenerationAttribute)attribute;
                                        regenerationOption = new RegenerationOption?(regenerationAttribute.Option);
                                    }
                                    if (attribute is TransactionAttribute)
                                    {
                                        var transactionAttribute = (TransactionAttribute)attribute;
                                        transactionMode = new TransactionMode?(transactionAttribute.Mode);
                                    }
                                    if (attribute is JournalingAttribute)
                                    {
                                        var journalingAttribute = (JournalingAttribute)attribute;
                                        journalingMode = new JournalingMode?(journalingAttribute.Mode);
                                    }
                                    if (transactionMode != null && regenerationOption != null)
                                    {
                                        break;
                                    }
                                }
                                if (transactionMode == null)
                                {
                                    list2.Add(type2.Name);
                                    goto IL_1A7;
                                }
                                if (transactionMode != StaticUtil.TransactMode)
                                {
                                    list3.Add(type2.Name);
                                    goto IL_1A7;
                                }
                            }
                            var item = new AddinItem(originalAssemblyFilePath, Guid.NewGuid(), type2.FullName, type, transactionMode, regenerationOption, journalingMode);
                            list.Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException(e.ToString());
                }
                IL_1A7:;
            }
            if (list2.Count > 0)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("The following Classes: ");
                foreach (var value in list2)
                {
                    stringBuilder.AppendLine(value);
                }
                stringBuilder.Append("implements IExternalCommand but doesn't contain both RegenerationAttribute and TransactionAttribute!");
                StaticUtil.ShowWarning(stringBuilder.ToString());
            }
            if (list3.Count > 0)
            {
                var stringBuilder2 = new StringBuilder();
                stringBuilder2.AppendLine("The TransactionMode set to Classes: ");
                foreach (var value2 in list3)
                {
                    stringBuilder2.AppendLine(value2);
                }
                stringBuilder2.Append(" are not the same as the mode set to Add-In Manager!");
                StaticUtil.ShowWarning(stringBuilder2.ToString());
            }
            return list;
        }

        protected SortedDictionary<string, Addin> m_addinDict;

        protected int m_maxCount = 100;

        protected int m_count;
    }
}
