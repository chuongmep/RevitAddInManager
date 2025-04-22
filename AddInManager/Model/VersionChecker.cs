using Microsoft.Win32;

namespace RevitAddinManager.Model;

public static class VersionChecker
{
    /// <summary>
    /// Return Current Version Installed In Computer
    /// </summary>
    public static string CurrentVersion
    {
        get
        {
            string displayName;
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(DefaultSetting.Application))
                    {
                        return subkey.GetValue("DisplayVersion") as string;
                    }
                }
                key.Close();
            }

            registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(DefaultSetting.Application))
                    {
                        return subkey.GetValue("DisplayVersion") as string;
                    }
                }
                key.Close();
            }
            return string.Empty;
        }

    }
}