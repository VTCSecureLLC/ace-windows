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
using VATRP.App.Interfaces;
using VATRP.App.Services;
using VATRP.Core.Model;

namespace VATRP.App.CustomControls
{
    /// <summary>
    /// Interaction logic for NetworkSettingsCtrl.xaml
    /// </summary>
    public partial class NetworkSettingsCtrl : ISettings
    {
        public NetworkSettingsCtrl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            EnableStunCheckBox.IsChecked = App.CurrentAccount.EnubleSTUN;
            StunHostnameBox.Text = App.CurrentAccount.STUNAddress;
            StunHostPortBox.Text = App.CurrentAccount.STUNPort.ToString();
        }

        #region ISettings

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var enabled = EnableStunCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnubleSTUN)
                return true;

            if (StunHostnameBox.Text != App.CurrentAccount.STUNAddress)
                return true;
            ushort port = 0;
            ushort.TryParse(StunHostPortBox.Text, out port);

            if (port != App.CurrentAccount.STUNPort && port != 0)
                return false;

            return true;
        }

        public bool Save()
        {
            if (App.CurrentAccount == null)
                return false;
            App.CurrentAccount.EnubleSTUN = EnableStunCheckBox.IsChecked ?? false;
            App.CurrentAccount.STUNAddress = StunHostnameBox.Text;
            ushort port = 0;

            ushort.TryParse(StunHostPortBox.Text, out port);
            App.CurrentAccount.STUNPort = port;
            return true;
        }

        #endregion
    }
}
