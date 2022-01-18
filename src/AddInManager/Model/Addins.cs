using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Autodesk.Revit.Attributes;

namespace AddInManager.Model
{
	public abstract class Addins
	{
        public SortedDictionary<string, Addin> AddinDict
		{
			get => this.m_addinDict;
            set => this.m_addinDict = value;
        }

		public int Count => this.m_addinDict.Count;

        public Addins()
		{
			this.m_addinDict = new SortedDictionary<string, Addin>();
		}

		public void SortAddin()
		{
			foreach (Addin addin in this.m_addinDict.Values)
			{
				addin.SortAddinItem();
			}
		}

		
		public void AddAddIn(Addin addin)
		{
			string fileName = Path.GetFileName(addin.FilePath);
			if (this.m_addinDict.ContainsKey(fileName))
			{
				this.m_addinDict.Remove(fileName);
			}
			this.m_addinDict[fileName] = addin;
		}

        public bool RemoveAddIn(Addin addin)
		{
			string fileName = Path.GetFileName(addin.FilePath);
			if (this.m_addinDict.ContainsKey(fileName))
			{
				this.m_addinDict.Remove(fileName);
				return true;
			}
			return false;
		}

		public void AddItem(AddinItem item)
		{
            
			string assemblyName = item.AssemblyName;
			if (!this.m_addinDict.ContainsKey(assemblyName))
			{
				this.m_addinDict[assemblyName] = new Addin(item.AssemblyPath);
			}
            this.m_addinDict[assemblyName].ItemList.Add(item);

        }

		public List<AddinItem> LoadItems(Assembly assembly, string fullName, string originalAssemblyFilePath, AddinType type)
		{
			List<AddinItem> list = new List<AddinItem>();
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
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			foreach (Type type2 in array)
			{
				try
				{
					if (!(null == type2) && !type2.IsAbstract)
					{
						Type @interface = type2.GetInterface(fullName);
						if (null != @interface)
						{
							TransactionMode? transactionMode = null;
							RegenerationOption? regenerationOption = null;
							JournalingMode? journalingMode = null;
							if (type != AddinType.Application)
							{
								Attribute[] customAttributes = Attribute.GetCustomAttributes(type2, false);
								foreach (Attribute attribute in customAttributes)
								{
									if (attribute is RegenerationAttribute)
									{
										RegenerationAttribute regenerationAttribute = (RegenerationAttribute)attribute;
										regenerationOption = new RegenerationOption?(regenerationAttribute.Option);
									}
									if (attribute is TransactionAttribute)
									{
										TransactionAttribute transactionAttribute = (TransactionAttribute)attribute;
										transactionMode = new TransactionMode?(transactionAttribute.Mode);
									}
									if (attribute is JournalingAttribute)
									{
										JournalingAttribute journalingAttribute = (JournalingAttribute)attribute;
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
								if (transactionMode != StaticUtil.m_tsactMode)
								{
									list3.Add(type2.Name);
									goto IL_1A7;
								}
							}
							AddinItem item = new AddinItem(originalAssemblyFilePath, Guid.NewGuid(), type2.FullName, type, transactionMode, regenerationOption, journalingMode);
							list.Add(item);
						}
					}
				}
				catch (Exception e)
                {
                    throw new System.ArgumentException(e.ToString());
                }
				IL_1A7:;
			}
			if (list2.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("The following Classes: ");
				foreach (string value in list2)
				{
					stringBuilder.AppendLine(value);
				}
				stringBuilder.Append("implements IExternalCommand but doesn't contain both RegenerationAttribute and TransactionAttribute!");
				StaticUtil.ShowWarning(stringBuilder.ToString());
			}
			if (list3.Count > 0)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendLine("The TransactionMode set to Classes: ");
				foreach (string value2 in list3)
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
