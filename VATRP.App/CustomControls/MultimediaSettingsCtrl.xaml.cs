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
using VATRP.App.Interfaces;
using VATRP.App.Services;
using VATRP.Core.Model;

namespace VATRP.App.CustomControls
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
        }

        private string GetVideoID(TextBlock tb)
        {
            if (tb == null)
                return string.Empty;

            var index = tb.Text.IndexOf(" (", System.StringComparison.Ordinal);
            if (index != -1)
            {
                var str = tb.Text.Substring(0, index).Trim();
                if (str == App.CurrentAccount.PreferredVideoId)
                {
                    return str;
                }
            }
            return string.Empty;
        }

        #region ISettings

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var tb = ResolutionBox.SelectedItem as TextBlock;
            var str = GetVideoID(tb);

            if (str == App.CurrentAccount.PreferredVideoId)
                return false;

            
            return true;
        }

        public bool Save()
        {
            if (App.CurrentAccount == null)
                return false;

            var tb = ResolutionBox.SelectedItem as TextBlock;
            var str = GetVideoID(tb);
            if (string.IsNullOrWhiteSpace(str))
                return false;

            App.CurrentAccount.PreferredVideoId = str;
            ServiceManager.Instance.ConfigurationService.SaveConfig();
            return true;
        }

        #endregion

    }
}
