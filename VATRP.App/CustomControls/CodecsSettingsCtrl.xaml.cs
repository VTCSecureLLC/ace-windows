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
    /// Interaction logic for CodecsSettingsCtrl.xaml
    /// </summary>
    public partial class CodecsSettingsCtrl : ISettings
    {
        public CodecsSettingsCtrl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
             AudioCodecsListView.Items.Clear();
            foreach (var item in App.CurrentAccount.AudioCodecsList)
            {
                AudioCodecsListView.Items.Add(item);
            }

            VideoCodecsListView.Items.Clear();
            foreach (var item in App.CurrentAccount.VideoCodecsList)
            {
                VideoCodecsListView.Items.Add(item);
            }
        }

        #region ISettings

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

           
            return true;
        }

        public bool Save()
        {
            if (App.CurrentAccount == null)
                return false;
            
            return true;
        }

        #endregion
    }
}
