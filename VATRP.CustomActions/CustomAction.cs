using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace VATRP.CustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult RemoveUserData(Session session)
        {
            session.Log("RemoveUserData");
            try
            {
                var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VATRP");

                var dirInfo = new DirectoryInfo(applicationDataPath);

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    dir.Delete();
                }

                Directory.Delete(applicationDataPath, false);
            }
            catch (Exception ex)
            {
                session.Log("exception on RemoveUserData: " + ex.Message);
            }

            // Remove auto start key from registry if set
            try
            {
                using (
                    RegistryKey key =
                        Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.DeleteValue("ACE", false);
                }
            }
            catch (Exception)
            {
                
            }
            return ActionResult.Success;
        }
    }
}
