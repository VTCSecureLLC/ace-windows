using com.vtcsecure.ace.windows.Enums;
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
    /// Interaction logic for AdvancedSettings.xaml
    /// </summary>
    public partial class AdvancedSettings : BaseUnifiedSettingsPanel
    {
        public AdvancedSettings()
        {
            InitializeComponent();
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            AdvancedCtrl.Initialize();
        }

        public override void AddAccountChangedMethod(UnifiedSettings_AccountChange accountChangedHandler)
        {
            AdvancedCtrl.AccountChangeRequested += accountChangedHandler;
        }

        public override void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
            AdvancedCtrl.UpdateForMenuSettingChange(menuSetting);
        }
    }
}
