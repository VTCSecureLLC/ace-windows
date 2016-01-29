using com.vtcsecure.ace.windows.Utilities;
using com.vtcsecure.ace.windows.Views;
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
        public event UnifiedSettings_EnableSettings ShowSettingsUpdate;

        public UnifiedSettingsSummaryCtrl()
        {
            InitializeComponent();
            Title = "Summary";
        }

        public override void ShowAdvancedOptions(bool show)
        {
            base.ShowAdvancedOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            DebugSettingsPasswordBox.Visibility = visibleSetting;
            ShowDebugSettingsButton.Visibility = visibleSetting;

            SuperSettingsPasswordBox.Visibility = visibleSetting;
            ShowAllSettingsButton.Visibility = visibleSetting;
        }

        // ToDo VATRP-990 - connect these to the correct actions

        private void OnViewTss(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("View TSS Clicked");
            OnContentChanging(UnifiedSettingsContentType.ViewTSS);
        }
        private void OnMailTss(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Mail TSS Clicked");
            string path = TechnicalSupportInfoBuilder.CreateAndGetTechnicalSupportInfoAsTextFile(true);
            var feedbackView = new FeedbackView(path);
            feedbackView.Show();

        }
        private void OnShowAdvanced(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Advanced Clicked");
            string password = AdvancedSettingsPasswordBox.Password;
            if (!string.IsNullOrEmpty(password) && password.Equals("1234"))
            {
                // show advanced settings
                if (ShowSettingsUpdate != null)
                {
                    ShowSettingsUpdate(UnifiedSettings_LevelToShow.Advanced, true);
                }
            }
        }
        private void OnShowDebug(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Debug Clicked");
            string password = DebugSettingsPasswordBox.Password;
            if (!string.IsNullOrEmpty(password) && password.Equals("1234"))
            {
                if (ShowSettingsUpdate != null)
                {
                    ShowSettingsUpdate(UnifiedSettings_LevelToShow.Debug, true);
                }
            }
        }

        private void OnShowAllSettings(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Super Clicked");
            string password = this.SuperSettingsPasswordBox.Password;
            if (!string.IsNullOrEmpty(password) && password.Equals("1234"))
            {
                if (ShowSettingsUpdate != null)
                {
                    ShowSettingsUpdate(UnifiedSettings_LevelToShow.Super, true);
                }
            }
            else if (!string.IsNullOrEmpty(password) && password.Equals("1170"))
            {
                BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview = System.Windows.Visibility.Visible;
                if (ShowSettingsUpdate != null)
                {
                    ShowSettingsUpdate(UnifiedSettings_LevelToShow.Super, true);
                }
            }
        }
    }
}
