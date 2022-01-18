using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace AddInManager.Properties
{
	// Token: 0x02000019 RID: 25
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class Resources
	{
		// Token: 0x060000D8 RID: 216 RVA: 0x00006E4A File Offset: 0x0000504A
		internal Resources()
		{
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000D9 RID: 217 RVA: 0x00006E54 File Offset: 0x00005054
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("AddInManager.Properties.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060000DA RID: 218 RVA: 0x00006E93 File Offset: 0x00005093
		// (set) Token: 0x060000DB RID: 219 RVA: 0x00006E9A File Offset: 0x0000509A
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060000DC RID: 220 RVA: 0x00006EA2 File Offset: 0x000050A2
		internal static string AppName
		{
			get
			{
				return Resources.ResourceManager.GetString("AppName", Resources.resourceCulture);
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060000DD RID: 221 RVA: 0x00006EB8 File Offset: 0x000050B8
		internal static string ClassNotExist
		{
			get
			{
				return Resources.ResourceManager.GetString("ClassNotExist", Resources.resourceCulture);
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000DE RID: 222 RVA: 0x00006ECE File Offset: 0x000050CE
		internal static string DefaultDescription
		{
			get
			{
				return Resources.ResourceManager.GetString("DefaultDescription", Resources.resourceCulture);
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000DF RID: 223 RVA: 0x00006EE4 File Offset: 0x000050E4
		internal static string DependencyNotExist
		{
			get
			{
				return Resources.ResourceManager.GetString("DependencyNotExist", Resources.resourceCulture);
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00006EFA File Offset: 0x000050FA
		internal static string FileNotExist
		{
			get
			{
				return Resources.ResourceManager.GetString("FileNotExist", Resources.resourceCulture);
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060000E1 RID: 225 RVA: 0x00006F10 File Offset: 0x00005110
		internal static string HelpFileWithExt
		{
			get
			{
				return Resources.ResourceManager.GetString("HelpFileWithExt", Resources.resourceCulture);
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x00006F26 File Offset: 0x00005126
		internal static string LoadAnotherFile
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadAnotherFile", Resources.resourceCulture);
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060000E3 RID: 227 RVA: 0x00006F3C File Offset: 0x0000513C
		internal static string LoadCancelled
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadCancelled", Resources.resourceCulture);
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x00006F52 File Offset: 0x00005152
		internal static string LoadFailed
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadFailed", Resources.resourceCulture);
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x00006F68 File Offset: 0x00005168
		internal static string LoadFileFilter
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadFileFilter", Resources.resourceCulture);
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x00006F7E File Offset: 0x0000517E
		internal static string LoadSucceed
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadSucceed", Resources.resourceCulture);
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000E7 RID: 231 RVA: 0x00006F94 File Offset: 0x00005194
		internal static string NoCHMFile
		{
			get
			{
				return Resources.ResourceManager.GetString("NoCHMFile", Resources.resourceCulture);
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000E8 RID: 232 RVA: 0x00006FAA File Offset: 0x000051AA
		internal static string NoIniFile
		{
			get
			{
				return Resources.ResourceManager.GetString("NoIniFile", Resources.resourceCulture);
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000E9 RID: 233 RVA: 0x00006FC0 File Offset: 0x000051C0
		internal static string NoItemsSelected
		{
			get
			{
				return Resources.ResourceManager.GetString("NoItemsSelected", Resources.resourceCulture);
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000EA RID: 234 RVA: 0x00006FD6 File Offset: 0x000051D6
		internal static string NotValidAddin
		{
			get
			{
				return Resources.ResourceManager.GetString("NotValidAddin", Resources.resourceCulture);
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000EB RID: 235 RVA: 0x00006FEC File Offset: 0x000051EC
		internal static string RemoteFile
		{
			get
			{
				return Resources.ResourceManager.GetString("RemoteFile", Resources.resourceCulture);
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000EC RID: 236 RVA: 0x00007002 File Offset: 0x00005202
		internal static string RuinedFile
		{
			get
			{
				return Resources.ResourceManager.GetString("RuinedFile", Resources.resourceCulture);
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000ED RID: 237 RVA: 0x00007018 File Offset: 0x00005218
		internal static string RunFailed
		{
			get
			{
				return Resources.ResourceManager.GetString("RunFailed", Resources.resourceCulture);
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000EE RID: 238 RVA: 0x0000702E File Offset: 0x0000522E
		internal static string SameFile
		{
			get
			{
				return Resources.ResourceManager.GetString("SameFile", Resources.resourceCulture);
			}
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000EF RID: 239 RVA: 0x00007044 File Offset: 0x00005244
		internal static string SameFileLoaded
		{
			get
			{
				return Resources.ResourceManager.GetString("SameFileLoaded", Resources.resourceCulture);
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000F0 RID: 240 RVA: 0x0000705A File Offset: 0x0000525A
		internal static string SaveClicked
		{
			get
			{
				return Resources.ResourceManager.GetString("SaveClicked", Resources.resourceCulture);
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000F1 RID: 241 RVA: 0x00007070 File Offset: 0x00005270
		internal static string SaveIniError
		{
			get
			{
				return Resources.ResourceManager.GetString("SaveIniError", Resources.resourceCulture);
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000F2 RID: 242 RVA: 0x00007086 File Offset: 0x00005286
		internal static string UnloadDualInterface
		{
			get
			{
				return Resources.ResourceManager.GetString("UnloadDualInterface", Resources.resourceCulture);
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000F3 RID: 243 RVA: 0x0000709C File Offset: 0x0000529C
		internal static string VersionTooOld
		{
			get
			{
				return Resources.ResourceManager.GetString("VersionTooOld", Resources.resourceCulture);
			}
		}

		// Token: 0x0400006F RID: 111
		private static ResourceManager resourceMan;

		// Token: 0x04000070 RID: 112
		private static CultureInfo resourceCulture;
	}
}
