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

            bool changed = false;

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
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }

        public bool Save()
        {
            if (App.CurrentAccount == null)
                return false;

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
            return true;
        }

        public override void SaveData()
        {
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
        }

        #region General Video Settings
        private void OnAutomaticallyStart(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Automatically Start Video Clicked");
            bool enabled = AutomaticallyStartCheckBox.IsChecked ?? false;
            // ToDo 1199
        }

        private void OnAutomaticallyAccept(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Automatically Accept Video Clicked");
            bool enabled = AutomaticallyAcceptCheckBox.IsChecked ?? false;
            // ToDo 1199
        }
        private void OnShowSelfView(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Self View Clicked");
            bool enabled = ShowSelfViewCheckBox.IsChecked ?? false;
            // ToDo 1199
        }
        private void OnVideoPreset(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Video Preset Clicked");
            // ToDo 1199
        }
        private void OnPreferredVideoSize(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Preferred Video Size Clicked");
            // ToDo 1199
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
                ServiceManager.Instance.ApplyCodecChanges();

            }
        }

        private void VideoCodecsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        #endregion
    }
}
