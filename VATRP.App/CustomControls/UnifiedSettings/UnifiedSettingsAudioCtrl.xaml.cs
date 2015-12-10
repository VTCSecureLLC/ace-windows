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
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.Enums;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsAudioCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsAudioCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsAudioCtrl()
        {
            InitializeComponent();
            this.Loaded += UnifiedSettingsAudioCtrl_Loaded;
        }

        void UnifiedSettingsAudioCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;

            MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
            MuteSpeakerCheckBox.IsChecked = App.CurrentAccount.MuteSpeaker;
            MuteSpeakerCheckBox.IsEnabled = false;

            AudioCodecsListView.Items.Clear();
            foreach (var item in App.CurrentAccount.AudioCodecsList)
            {
                AudioCodecsListView.Items.Add(item);
            }
        }

        public override void UpdateForMenuSettingChange(ACEMenuSettings menuSetting)
        {
            if (App.CurrentAccount == null)
                return;

            switch (menuSetting)
            {
                case ACEMenuSettings.MuteMicrophoneMenu: MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
                    break;
                default:
                    break;
            }
        }

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            bool changed = false;
            // check audio codecs
            foreach (var item in AudioCodecsListView.Items)
            {
                var cfgCodec = item as VATRPCodec;
                if (cfgCodec != null)
                {
                    foreach (var accountCodec in App.CurrentAccount.AudioCodecsList)
                    {
                        if (accountCodec.CodecName == cfgCodec.CodecName && accountCodec.Channels == cfgCodec.Channels &&
                            accountCodec.Rate == cfgCodec.Rate && accountCodec.Status != cfgCodec.Status)
                        {
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

        public override void SaveData()
        {
            if (App.CurrentAccount == null)
                return;

            foreach (var item in AudioCodecsListView.Items)
            {
                var cfgCodec = item as VATRPCodec;
                if (cfgCodec != null)
                {
                    foreach (var accountCodec in App.CurrentAccount.AudioCodecsList)
                    {
                        if (accountCodec.CodecName == cfgCodec.CodecName && accountCodec.Channels == cfgCodec.Channels &&
                            accountCodec.Rate == cfgCodec.Rate && accountCodec.Status != cfgCodec.Status)
                        {
                            accountCodec.Status = cfgCodec.Status;
                        }
                    }
                }
            }
            ServiceManager.Instance.ApplyCodecChanges();
            ServiceManager.Instance.SaveAccountSettings();
        }

        #region Audio Settings (in call)
        private void OnMuteMicrophone(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Mute Microphone Clicked");
            bool enabled = MuteMicrophoneCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteMicrophone)
            {
                App.CurrentAccount.MuteMicrophone = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }
        private void OnMuteSpeaker(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Mute Speaker Clicked");
            bool enabled = MuteSpeakerCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteSpeaker)
            {
                App.CurrentAccount.MuteSpeaker = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        #endregion

        #region Audio Codecs

        private void AudioCodecsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public static T FindAncestorOrSelf<T>(DependencyObject obj)
        where T : DependencyObject
        {
            while (obj != null)
            {
                T objTest = obj as T;

                if (objTest != null)
                    return objTest;

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var lvItem = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);
            var listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;
            if (listView != null)
            {
                listView.SelectedItem = null;
                var index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);
                listView.SelectedIndex = index;
                SaveData();
            }
        }
        #endregion


    }
}
