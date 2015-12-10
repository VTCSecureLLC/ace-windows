using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for UnifiedSettingsSummaryCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsSummaryCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsSummaryCtrl()
        {
            InitializeComponent();
            Title = "Summary";
        }


        // ToDo VATRP-990 - connect these to the correct actions

        private void OnViewTss(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("View TSS Clicked");
        }
        private void OnMailTss(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Mail TSS Clicked");
        }
        private void OnShowAdvanced(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Advanced Clicked");
        }
        private void OnShowDebug(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Debug Clicked");
        }

    }
}
