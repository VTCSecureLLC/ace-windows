using com.vtcsecure.ace.windows.Services;
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
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsTextCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsTextCtrl : BaseUnifiedSettingsPanel
    {
        public CallViewCtrl CallControl;

        public UnifiedSettingsTextCtrl()
        {
            InitializeComponent();
            Title = "Text";
            this.Loaded += UnifiedSettingsTextCtrl_Loaded;
        }

        void UnifiedSettingsTextCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            base.Initialize();
            this.EnableRealTimeTextCheckbox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, true);

            var textSendMode = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.TEXT_SEND_MODE, "Real Time Text");

            foreach (var item in TextSendModeComboBox.Items)
            {
                var tb = item as TextBlock;
                if (tb != null)
                {
                    string itemString = tb.Text;
                    if (itemString.Equals(textSendMode))
                    {
                        TextSendModeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            var rttFontName = string.Empty;

            var defaultFont = Fonts.SystemFontFamilies.FirstOrDefault();
            if (defaultFont != null)
                rttFontName = defaultFont.Source;
            
            if (App.CurrentAccount != null)
            {
                rttFontName = App.CurrentAccount.RTTFontFamily;
            }

            foreach (var fontItem in TextFontFamilyComboBox.Items)
            {
                var ff = fontItem as FontFamily;
                if (ff != null)
                {
                    if (ff.Source.Equals(rttFontName))
                    {
                        TextFontFamilyComboBox.SelectedItem = ff;
                        break;
                    }
                }
            }
        }

        private void OnEnableRealTimeText(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Enable Real Time Text Call Clicked");
            bool enabled = EnableRealTimeTextCheckbox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();

            if (CallControl != null && CallControl.IsLoaded)
                CallControl.EnableRTTButton(enabled);
        }

        private void OnTextsendMode(object sender, SelectionChangedEventArgs e)
        {
            var textSendModeLabel = TextSendModeComboBox.SelectedItem as TextBlock;
            if (textSendModeLabel != null)
            {
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.TEXT_SEND_MODE, textSendModeLabel.Text);

                ServiceManager.Instance.ConfigurationService.SaveConfig();
            }
        }

        private void OnTextFontChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized)
                return;
            var ff = TextFontFamilyComboBox.SelectedItem as FontFamily;
            if (ff != null)
            {
                if (App.CurrentAccount.RTTFontFamily == ff.Source)
                    return;
                App.CurrentAccount.RTTFontFamily = ff.Source;
                ServiceManager.Instance.AccountService.Save();
                ServiceManager.Instance.ChatService.UpdateRTTFontFamily(ff.Source);
            }
        }
    }
}
