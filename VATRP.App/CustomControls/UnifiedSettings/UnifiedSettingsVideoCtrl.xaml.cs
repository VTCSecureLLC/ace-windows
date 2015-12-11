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

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsVideoCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsVideoCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsVideoCtrl()
        {
            InitializeComponent();
            //            VideoCodecsListView.Items.Clear();
            // foreach (var item in App.CurrentAccount.VideoCodecsList)
            // {
            //     VideoCodecsListView.Items.Add(item);
            // }
            this.Loaded += UnifiedSettingsVideoCtrl_Loaded;
        }


        void UnifiedSettingsVideoCtrl_Loaded(object sender, RoutedEventArgs e)
        {

            if (App.CurrentAccount == null)
                return;
            // ToDo: VATRP-1170 enable video
            AutomaticallyStartCheckBox.IsChecked = App.CurrentAccount.VideoAutomaticallyStart;
            AutomaticallyStartCheckBox.IsEnabled = false;
            // ToDo: VATRP-1170 accept video
            AutomaticallyAcceptCheckBox.IsChecked = App.CurrentAccount.VideoAutomaticallyAccept;
            AutomaticallyAcceptCheckBox.IsEnabled = false;

            ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;

            string accountPreset = App.CurrentAccount.VideoPreset;
            if (string.IsNullOrWhiteSpace(accountPreset))
            {
                accountPreset = "default";
            }
            foreach (var item in VideoPresetComboBox.Items)
            {
                var tb = item as TextBlock;
                string itemString = tb.Text;
                if (itemString.Equals(accountPreset))
                {
                    VideoPresetComboBox.SelectedItem = item;
                    break;
                }
            }

            foreach (var item in PreferredVideoSizeComboBox.Items)
            {
                var tb = item as TextBlock;
                if (GetPreferredVideoSizeId(tb) == App.CurrentAccount.PreferredVideoId)
                {
                    PreferredVideoSizeComboBox.SelectedItem = item;
                    break;
                }
            }

            VideoCodecsListView.Items.Clear();
            foreach (var item in App.CurrentAccount.VideoCodecsList)
            {
                VideoCodecsListView.Items.Add(item);
            }
        }

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            if (IsVideoPresetChanged())
            {
                return true;
            }

            if (IsPreferredVideoSizeChanged())
            {
                return true;
            }


            // video codecs
            foreach (var item in VideoCodecsListView.Items)
            {
                var cfgCodec = item as VATRPCodec;
                if (cfgCodec != null)
                {
                    foreach (var accountCodec in App.CurrentAccount.VideoCodecsList)
                    {
                        if (accountCodec.CodecName == cfgCodec.CodecName && accountCodec.Channels == cfgCodec.Channels &&
                            accountCodec.Rate == cfgCodec.Rate)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsVideoPresetChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var videoPresetText = VideoPresetComboBox.SelectedItem as TextBlock;
            string videoPresetString = GetVideoPresetId(videoPresetText);
            if ((string.IsNullOrWhiteSpace(videoPresetString) && !string.IsNullOrWhiteSpace(App.CurrentAccount.VideoPreset)) ||
                (!string.IsNullOrWhiteSpace(videoPresetString) && string.IsNullOrWhiteSpace(App.CurrentAccount.VideoPreset)))
                return true;
            if ((!string.IsNullOrWhiteSpace(videoPresetString) && !string.IsNullOrWhiteSpace(App.CurrentAccount.VideoPreset)) &&
                (!videoPresetString.Equals(App.CurrentAccount.VideoPreset)))
                return true;

            return false;
        }

        private bool IsPreferredVideoSizeChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var tb = PreferredVideoSizeComboBox.SelectedItem as TextBlock;
            string str = GetPreferredVideoSizeId(tb);
            if ((string.IsNullOrWhiteSpace(str) && !string.IsNullOrWhiteSpace(App.CurrentAccount.PreferredVideoId)) ||
                (!string.IsNullOrWhiteSpace(str) && string.IsNullOrWhiteSpace(App.CurrentAccount.PreferredVideoId)))
                return true;
            if ((!string.IsNullOrWhiteSpace(str) && !string.IsNullOrWhiteSpace(App.CurrentAccount.PreferredVideoId)) &&
                (!str.Equals(App.CurrentAccount.PreferredVideoId)))
                return true;
            return false;
        }



        #region HelperMethods
        private string GetVideoPresetId(TextBlock tb)
        {
            if (tb == null)
                return string.Empty;

            string value = tb.Text.Trim();
            if (value.Equals("default"))
            {
                return null;
            }
            return value;
        }

        private string GetPreferredVideoSizeId(TextBlock tb)
        {
            if (tb == null)
                return string.Empty;

            var index = tb.Text.IndexOf(" (", System.StringComparison.Ordinal);
            return index != -1 ? tb.Text.Substring(0, index).Trim() : string.Empty;
        }

        #endregion

        #region General Video Settings
        private void OnAutomaticallyStart(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Automatically Start Video Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enable = AutomaticallyStartCheckBox.IsChecked ?? false;
            if (enable != App.CurrentAccount.VideoAutomaticallyStart)
            {
                App.CurrentAccount.VideoAutomaticallyStart = enable;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        private void OnAutomaticallyAccept(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Automatically Accept Video Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enable = AutomaticallyAcceptCheckBox.IsChecked ?? false;
            if (enable != App.CurrentAccount.VideoAutomaticallyAccept)
            {
                App.CurrentAccount.VideoAutomaticallyAccept = enable;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }
        private void OnShowSelfView(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Self View Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enable = this.ShowSelfViewCheckBox.IsChecked ?? true;
            if (enable != App.CurrentAccount.ShowSelfView)
            {
                App.CurrentAccount.ShowSelfView = enable;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        private void OnVideoPreset(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Video Preset Clicked");
            if (App.CurrentAccount == null)
                return;
            if (!IsVideoPresetChanged())
                return;

            var tb = VideoPresetComboBox.SelectedItem as TextBlock;
            if (tb != null)
            {
                var str = tb.Text;
                if (string.IsNullOrWhiteSpace(str))
                    return;
                if (str.Equals("default"))
                {
                    str = null;
                }
                
                App.CurrentAccount.VideoPreset = str;
            }
            ServiceManager.Instance.ApplyMediaSettingsChanges();
            ServiceManager.Instance.SaveAccountSettings();
        }

        private void OnPreferredVideoSize(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Preferred Video Size Clicked");
            if (App.CurrentAccount == null)
                return;
            if (!IsPreferredVideoSizeChanged())
            {
                return;
            }

            var tb = PreferredVideoSizeComboBox.SelectedItem as TextBlock;
            if (tb != null)
            {
                string str = GetPreferredVideoSizeId(tb);
                if (string.IsNullOrWhiteSpace(str))
                    return;
                // check to see if the value changed
                App.CurrentAccount.PreferredVideoId = str;
            }
            ServiceManager.Instance.ApplyMediaSettingsChanges();
            ServiceManager.Instance.SaveAccountSettings();
        }

        #endregion

        #region videoCodecs
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
                SaveVideoCodecsSettings();
            }
        }

        private void SaveVideoCodecsSettings()
        {
            if (App.CurrentAccount == null)
                return;

            foreach (var item in VideoCodecsListView.Items)
            {
                var cfgCodec = item as VATRPCodec;
                if (cfgCodec != null)
                {
                    foreach (var accountCodec in App.CurrentAccount.VideoCodecsList)
                    {
                        if (accountCodec.CodecName == cfgCodec.CodecName && accountCodec.Channels == cfgCodec.Channels &&
                            accountCodec.Rate == cfgCodec.Rate)
                        {
                            accountCodec.Status = cfgCodec.Status;
                        }
                    }
                }
            }
            ServiceManager.Instance.ApplyCodecChanges();
            ServiceManager.Instance.SaveAccountSettings();

        }

        private void VideoCodecsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        #endregion
    }
}
