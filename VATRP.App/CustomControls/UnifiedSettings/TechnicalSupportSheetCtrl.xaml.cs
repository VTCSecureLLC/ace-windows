using com.vtcsecure.ace.windows.Utilities;
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

            this.OperatingSystemInfoLabel.Text = Environment.OSVersion.VersionString;

            this.TechnicalSupprtInfoTextBlock.Text = TechnicalSupportInfoBuilder.GetStringForTechnicalSupprtString(false);
        }
    }
}
