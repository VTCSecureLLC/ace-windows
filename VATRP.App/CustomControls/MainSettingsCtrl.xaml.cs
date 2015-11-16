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
using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Interfaces;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for MainSettingsCtrl.xaml
    /// </summary>
    public partial class MainSettingsCtrl
    {

        #region Events
        public delegate void SettingsChangeDelegate(VATRPSettings settingsType);
        public event SettingsChangeDelegate SipSettingsChangeClicked;
        public event SettingsChangeDelegate CodecSettingsChangeClicked;
        public event SettingsChangeDelegate MultimediaSettingsChangeClicked;
        public event SettingsChangeDelegate NetworkSettingsChangeClicked;
        public event SettingsChangeDelegate CallSettingsChangeClicked;
        #endregion

        public MainSettingsCtrl()
        {
            InitializeComponent();
        }

        private void OnSipSettings(object sender, RoutedEventArgs e)
        {
            if (SipSettingsChangeClicked != null)
                SipSettingsChangeClicked(VATRPSettings.VATRPSettings_SIP);
        }

        private void OnCodecSettings(object sender, RoutedEventArgs e)
        {
            if (CodecSettingsChangeClicked != null)
                CodecSettingsChangeClicked(VATRPSettings.VATRPSettings_Codec);
        }

        private void OnMultimediaSettings(object sender, RoutedEventArgs e)
        {
            if (MultimediaSettingsChangeClicked != null) 
                MultimediaSettingsChangeClicked(VATRPSettings.VATRPSettings_Multimedia);
        }

        private void OnNetworkSettings(object sender, RoutedEventArgs e)
        {
            if (NetworkSettingsChangeClicked != null) 
                NetworkSettingsChangeClicked(VATRPSettings.VATRPSettings_Network);
        }

        private void OnCallSettings(object sender, RoutedEventArgs e)
        {
            if (CallSettingsChangeClicked != null)
                CallSettingsChangeClicked(VATRPSettings.VATRPSettings_Test);
        }

    }
}
