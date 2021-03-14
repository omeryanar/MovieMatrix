using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace Setup
{
    public class Utilities
    {
        public static void CreateShortcut(string setupLocation, string productName, string applicationTitle, string applicationDescription)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string shortcutPath = Path.Combine(desktopPath, applicationTitle + ".lnk");

            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = Path.Combine(setupLocation, productName + ".exe");
            shortcut.Description = applicationDescription;
            shortcut.Save();
        }

        public static string GetPreviousSetupLocation(string applicationTitle)
        {
            RegistryKey entry = Registry.CurrentUser.CreateSubKey(String.Format(@"Software\{0}", applicationTitle));
            return entry.GetValue("Location", String.Empty).ToString();
        }

        public static void SetCurrentSetupLocation(string setupLocation, string applicationTitle)
        {
            RegistryKey entry = Registry.CurrentUser.CreateSubKey(String.Format(@"Software\{0}", applicationTitle));
            entry.SetValue("Location", setupLocation);
        }

        public static bool CheckRunningApplication(string productName)
        {
            return GetRunningApplication(productName) != null;
        }

        public static bool CloseRunningApplication(string productName)
        {
            try
            {
                Process process = GetRunningApplication(productName);
                if (process != null)
                    return process.CloseMainWindow();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Process GetRunningApplication(string productName)
        {
            Process currentProcess = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(productName))
            {
                if (process.Id != currentProcess.Id)
                    return process;
            }

            return null;
        }
    }
}
