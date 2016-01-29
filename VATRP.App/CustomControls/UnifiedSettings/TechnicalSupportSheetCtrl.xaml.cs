using com.vtcsecure.ace.windows.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for TechnicalSupportSheetCtrl.xaml
    /// </summary>
    public partial class TechnicalSupportSheetCtrl : BaseUnifiedSettingsPanel
    {
        public TechnicalSupportSheetCtrl()
        {
            InitializeComponent();
            Initialize();
        }
        public override void Initialize()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            this.ACEVersionLabel.Content = assembly.GetName().Name + " Version: ";
            this.ACEVersionInfoLabel.Content = string.Format("Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);
            OperatingSystem os = Environment.OSVersion;

            this.OperatingSystemInfoLabel.Text = FriendlyName() + " " + os.ServicePack;// Environment.OSVersion.VersionString;

            this.TechnicalSupprtInfoTextBlock.Text = TechnicalSupportInfoBuilder.GetStringForTechnicalSupprtString(false);
        }
        public string HKLM_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
                if (rk == null) return "";
                return (string)rk.GetValue(key);
            }
            catch { return ""; }
        }

        public string FriendlyName()
        {
            string ProductName = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
            string CSDVersion = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CSDVersion");
            if (ProductName != "")
            {
                return (ProductName.StartsWith("Microsoft") ? "" : "Microsoft ") + ProductName +
                            (CSDVersion != "" ? " " + CSDVersion : "");
            }
            return "";
        }
    }
}
