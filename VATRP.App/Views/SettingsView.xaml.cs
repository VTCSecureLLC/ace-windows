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
using System.Windows.Shapes;
using VATRP.App.CustomControls;
using VATRP.App.Model;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView
    {
        public bool SipSettingsChanged { get; private set; }
        public bool NetworkSettingsChanged { get; private set; }
        public bool CodecSettingsChanged { get; private set; }
        public bool CallSettingsChanged { get; private set; }

        #region Event
        public delegate void SettingsSavedDelegate();
        public event SettingsSavedDelegate SettingsSavedEvent;
        #endregion

        public SettingsView()
            : base(VATRPWindowType.SETTINGS_VIEW)
        {
            SipSettingsChanged = false;
            NetworkSettingsChanged = false;
            CodecSettingsChanged = false;
            CallSettingsChanged = false;
            InitializeComponent();
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (SipSettingsPage != null && SipSettingsPage.IsChanged())
            {
                if (!SipSettingsPage.Save())
                {
                    SettingsTab.SelectedIndex = 0;
                    return;
                }
                SipSettingsChanged = true;
            }

            if (NetworkSettingsPage != null && NetworkSettingsPage.IsChanged())
            {
                if (!NetworkSettingsPage.Save())
                {
                    SettingsTab.SelectedIndex = 1;
                    return;
                }
                NetworkSettingsChanged = true;
            }

            if (CodecSettingsPage != null && CodecSettingsPage.IsChanged())
            {
                if (!CodecSettingsPage.Save())
                {
                    SettingsTab.SelectedIndex = 2;
                    return;
                }
                CodecSettingsChanged = true;
            }

            if (CallSettingsPage != null && CallSettingsPage.IsChanged())
            {
                if (!CallSettingsPage.Save())
                {
                    SettingsTab.SelectedIndex = 3;
                    return;
                }
                CallSettingsChanged = true;
            }
            Close();
            if (SettingsSavedEvent != null)
            {
                SettingsSavedEvent();
                CodecSettingsChanged = false;
                NetworkSettingsChanged = false;
                SipSettingsChanged = false;
                CallSettingsChanged = false;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
#if !DEBUG
            TestingTabItem.Visibility = Visibility.Collapsed;
#endif

        }
    }
}
