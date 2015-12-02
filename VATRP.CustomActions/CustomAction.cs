using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

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
            return ActionResult.Success;
        }
    }
}
