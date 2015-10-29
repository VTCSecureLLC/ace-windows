using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using com.vtcsecure.ace.windows.Interfaces;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for MultimediaSettingsCtrl.xaml
    /// </summary>
    public partial class MultimediaSettingsCtrl : ISettings
    {
        public MultimediaSettingsCtrl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;

            foreach (var item in ResolutionBox.Items)
            {
                var tb = item as TextBlock;
                if (GetVideoID(tb) == App.CurrentAccount.PreferredVideoId)
                {
                    ResolutionBox.SelectedItem = item;
                    break;
                }
            }

            foreach (var item in MediaEncryptionBox.Items)
            {
                var s = item as TextBlock;
                if (s != null && s.Text.Contains(App.CurrentAccount.MediaEncryption))
                {
                    MediaEncryptionBox.SelectedItem = item;
                    break;
                }
            }

        }

        private string GetVideoID(TextBlock tb)
        {
            if (tb == null)
                return string.Empty;

            var index = tb.Text.IndexOf(" (", System.StringComparison.Ordinal);
            return index != -1 ? tb.Text.Substring(0, index).Trim() : string.Empty;
        }

        #region ISettings

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var tb = ResolutionBox.SelectedItem as TextBlock;
            var str = GetVideoID(tb);

            if (!string.IsNullOrWhiteSpace(str) && str != App.CurrentAccount.PreferredVideoId)
                return true;

            var s = MediaEncryptionBox.SelectedItem as TextBlock;
            if (s != null && !s.Text.Contains(App.CurrentAccount.MediaEncryption))
            {
                return true;
            }

            return false;
        }

        public bool Save()
        {
            if (App.CurrentAccount == null)
                return false;

            var tb = ResolutionBox.SelectedItem as TextBlock;
            if (tb != null)
            {
                var str = GetVideoID(tb);
                if (string.IsNullOrWhiteSpace(str))
                    return false;

                App.CurrentAccount.PreferredVideoId = str;
            }

            tb = MediaEncryptionBox.SelectedItem as TextBlock;
            if (tb != null)
            {
                App.CurrentAccount.MediaEncryption = tb.Text;
            }

            ServiceManager.Instance.ConfigurationService.SaveConfig();
            return true;
        }

        #endregion

    }
}
