using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for UnifiedSettingsAdvancedCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsAdvancedCtrl : BaseUnifiedSettingsPanel
    {
        public CallViewCtrl CallControl;
        private CollectionView _codecsView;

        public UnifiedSettingsAdvancedCtrl()
        {
            InitializeComponent();
        }

        #region Initialization
        public override void Initialize()
        {
            base.Initialize();
            if (App.CurrentAccount != null)
            {
                // Audio
                InitializeAudio();
                // Video
                InitializeVideo();

                //Network
                InitializeNetwork();
            }

            // Debug
            foreach (TextBlock textBlock in LoggingComboBox.Items)
            {
                if (textBlock.Text.Equals(App.CurrentAccount.Logging))
                {
                    LoggingComboBox.SelectedItem = textBlock;
                }
            }

            if (LoggingComboBox.SelectedItem == null)
                LoggingComboBox.SelectedIndex = 0;


        }

        private void InitializeAudio()
        {
            AudioCodecsListView.ItemsSource = App.CurrentAccount.AudioCodecsList;
            _codecsView = (CollectionView)CollectionViewSource.GetDefaultView(AudioCodecsListView.ItemsSource);
            if (_codecsView != null)
            {
                _codecsView.SortDescriptions.Add(new SortDescription("Priority", ListSortDirection.Ascending));
                _codecsView.Refresh();
            }
        }

        private void InitializeVideo()
        {
            AutomaticallyStartCheckBox.IsChecked = App.CurrentAccount.VideoAutomaticallyStart;
            AutomaticallyAcceptCheckBox.IsChecked = App.CurrentAccount.VideoAutomaticallyAccept;

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

            float preferredFPS = App.CurrentAccount.PreferredFPS;
            PreferredFPSTextBox.Text = Convert.ToString(preferredFPS);

            VideoCodecsListView.ItemsSource = App.CurrentAccount.VideoCodecsList;
            _codecsView = (CollectionView)CollectionViewSource.GetDefaultView(VideoCodecsListView.ItemsSource);
            if (_codecsView != null)
            {
                _codecsView.SortDescriptions.Add(new SortDescription("Priority", ListSortDirection.Ascending));
                _codecsView.Refresh();
            }
            /*
            New option name: RTCP feedback
Option Location: Advanced > Video
Options: Implicit, Explicit, Off
Default: "Implicit"

AVPF shall be off by default and  "rtcp_fb_implicit_rtcp_fb" on =1 call that combination "RTCP feedback (AVPF)"   (Implicit)
and then a setting with both AVPF off and  "rtcp_fb_implicit_rtcp_fb" off , and call that  setting  "RTCP feedback (AVPF)"   (Off)
And a setting with both AVPF on and  "rtcp_fb_implicit_rtcp_fb" on , and call that  setting  "RTCP feedback (AVPF)"   (Explicit).
             * The function can be activated/deactivated via an integer parameter called "rtcp_fb_implicit_rtcp_fb" inside "rtp" section. Values are 0 or 1.
This parameter can be set dynamically also via the traditional wrappers to get/set parameters (lp_config_set_int for C API).
Default value = 1
             * */
            string rtcpFeedbackString = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.RTCP_FEEDBACK, "Implicit");

            foreach (var item in RtcpFeedbackComboBox.Items)
            {
                var tb = item as TextBlock;
                if (tb.Text.Equals(rtcpFeedbackString))
                {
                    RtcpFeedbackComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void InitializeNetwork()
        {
            if (App.CurrentAccount.EnableICE && App.CurrentAccount.EnableSTUN)
                App.CurrentAccount.EnableICE = false; // normalization

            UseStunServerCheckbox.IsChecked = App.CurrentAccount.EnableSTUN;
            StunServerTextBox.Text = App.CurrentAccount.STUNAddress;
            UseIceServerCheckbox.IsChecked = App.CurrentAccount.EnableICE;

            foreach (TextBlock textBlock in MediaEncryptionComboBox.Items)
            {
                if (textBlock.Text.Equals(App.CurrentAccount.MediaEncryption))
                {
                    MediaEncryptionComboBox.SelectedItem = textBlock;
                }
            }

            AdaptiveRateCheckbox.IsChecked = App.CurrentAccount.EnableAdaptiveRate;

            foreach (TextBlock textBlock in AlgorithmComboBox.Items)
            {
                if (textBlock.Text.Equals(App.CurrentAccount.AdaptiveRateAlgorithm))
                {
                    AlgorithmComboBox.SelectedItem = textBlock;
                }
            }

            if (AlgorithmComboBox.SelectedItem == null)
                AlgorithmComboBox.SelectedIndex = 0;

            UploadBandwidthTextBox.Text = App.CurrentAccount.UploadBandwidth.ToString();
            DownloadBandwidthTextBox.Text = App.CurrentAccount.DownloadBandwidth.ToString();
            QoSCheckbox.IsChecked = App.CurrentAccount.EnableQualityOfService;
            IPv6Checkbox.IsChecked = App.CurrentAccount.EnableIPv6;
        }
        #endregion

        #region Save
        public override void SaveData() // called when we leave this page
        {
           
            if (App.CurrentAccount == null)
                return;

            if (!IsChanged())
            {
                return;
            }

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

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            bool changed = false;

            if (!changed)
                changed = IsAudioChanged();
            if (!changed)
                changed = IsVideoChanged();

            return changed;
        }
        private bool IsAudioChanged()
        {
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
        private bool IsVideoChanged()
        {
            if (IsVideoPresetChanged())
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

        private bool IsRtcpFeedbackChanged()
        {
            var rtcpFeedbackTextBlock = RtcpFeedbackComboBox.SelectedItem as TextBlock;
            string rtcpFeedbackString = rtcpFeedbackTextBlock.Text;
            string oldRtcpFeedbackString = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.RTCP_FEEDBACK, "Off");

            if ((string.IsNullOrWhiteSpace(rtcpFeedbackString) && !string.IsNullOrWhiteSpace(oldRtcpFeedbackString)) ||
                (!string.IsNullOrWhiteSpace(rtcpFeedbackString) && string.IsNullOrWhiteSpace(oldRtcpFeedbackString)))
                return true;
            if ((!string.IsNullOrWhiteSpace(rtcpFeedbackString) && !string.IsNullOrWhiteSpace(oldRtcpFeedbackString)) &&
                (!rtcpFeedbackString.Equals(oldRtcpFeedbackString)))
                return true;

            return false;
        }
        #endregion

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


        #endregion

        public override void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
            if (App.CurrentAccount == null)
                return;

            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.ShowSelfViewMenu: ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;
                    break;
                default:
                    break;
            }
        }


        //==================== Audio Settings
        #region Audio Codecs

        private void AudioCodecsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void AudioCodecCheckBox_Click(object sender, RoutedEventArgs e)
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

        //==================== Video Settings
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
                ServiceManager.Instance.SaveAccountSettings();

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.VideoPolicyChanged);
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
                ServiceManager.Instance.SaveAccountSettings();

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.VideoPolicyChanged);
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

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.ShowSelfViewChanged);
            }
        }

        public void OnPreferredFPS(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            // then get and set the preferred fps.
            string stringValue = PreferredFPSTextBox.Text;
            if (!string.IsNullOrEmpty(stringValue))
            {
                float floatValue = float.Parse(stringValue);
                App.CurrentAccount.PreferredFPS = floatValue;
                // set the preferred fps in linphone
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

        #endregion

        #region videoCodecs
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

        #region RtcpFeedback
        private void OnRtcpFeedback(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("RTCP Feedback Selected");
            if (App.CurrentAccount == null)
                return;
            if (!IsRtcpFeedbackChanged())
                return;

            var tb = RtcpFeedbackComboBox.SelectedItem as TextBlock;
            if (tb != null)
            {
                var str = tb.Text;
                if (string.IsNullOrWhiteSpace(str))
                    return;

                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                     Configuration.ConfEntry.RTCP_FEEDBACK, str);
                if (str.Equals("Explicit"))
                {
                    ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                         Configuration.ConfEntry.AVPF_ON, true);
                }
                else
                {
                    ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                         Configuration.ConfEntry.AVPF_ON, false);
                }
                ServiceManager.Instance.ConfigurationService.SaveConfig();
                // RTCP Feedback and AVPF are related
                ServiceManager.Instance.ApplyAVPFChanges();
            }
        }
        #endregion


        private void OnDebugMode(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }

        private void OnClearLogs(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }
        private void OnSendLogs(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }

        private void OnPersistentNotifier(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }
        private void OnEnableAnimations(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnSharingServerURLChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnRemoteProvisioningChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnSIPExpireChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnFileSharingServerURLChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        private void OnLoggingChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock valueA = (TextBlock)LoggingComboBox.SelectedItem;
            string value = valueA.Text;
            if (App.CurrentAccount != null)
            {
                App.CurrentAccount.Logging = value;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.AdvancedSettingsChanged);
            }
        }

        #region
        private void OnAdaptiveRateChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = AdaptiveRateCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableAdaptiveRate)
            {
                App.CurrentAccount.EnableAdaptiveRate = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        private void OnAdaptiveRateAlgorithmChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock valueA = (TextBlock)AlgorithmComboBox.SelectedItem;
            string value = valueA.Text;
            if (App.CurrentAccount != null)
            {
                bool needsUpdate = false;
                if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(App.CurrentAccount.AdaptiveRateAlgorithm))
                    needsUpdate = true;
                else if (!string.IsNullOrEmpty(value) && !App.CurrentAccount.AdaptiveRateAlgorithm.Equals(value))
                    needsUpdate = true;
                // do not update if we do nto need it.
                if (needsUpdate)
                {
                    App.CurrentAccount.AdaptiveRateAlgorithm = value;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnStunServerChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = UseStunServerCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableSTUN)
            {
                App.CurrentAccount.EnableSTUN = enabled;
                if (enabled)
                {
                    App.CurrentAccount.EnableICE = false;
                    UseIceServerCheckbox.IsChecked = App.CurrentAccount.EnableICE;
                }

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        public void OnStunServerChanged(Object sender, RoutedEventArgs args)
        {
            string newStunServer = StunServerTextBox.Text;
            // VATRP-1949: removed check for empty stun server. However - maybe we want a test here so that if the user has
            //  Stun Server checkbox enabled we prompt the user if the value does not look like a valid address?
            if (App.CurrentAccount != null)
            {
                bool updateStunServer = false;
                if (!string.IsNullOrEmpty(newStunServer) && string.IsNullOrEmpty(App.CurrentAccount.STUNAddress))
                    updateStunServer = true;
                else if (!string.IsNullOrEmpty(newStunServer) && !newStunServer.Equals(App.CurrentAccount.STUNAddress))
                    updateStunServer = true;
                if (updateStunServer)
                {
                    App.CurrentAccount.STUNAddress = newStunServer;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnIceServerChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = UseIceServerCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableICE)
            {
                App.CurrentAccount.EnableICE = enabled;
                if (enabled)
                {
                    App.CurrentAccount.EnableSTUN = false;
                    UseStunServerCheckbox.IsChecked = App.CurrentAccount.EnableSTUN;
                }
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        public void OnUploadBandwidthChanged(Object sender, RoutedEventArgs args)
        {
            string newBandwidth = UploadBandwidthTextBox.Text;
            if (!string.IsNullOrEmpty(newBandwidth))
            {
                int bandwidth = 0;
                if (int.TryParse(newBandwidth, out bandwidth))
                {
                    App.CurrentAccount.UploadBandwidth = bandwidth;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }
        public void OnDownloadBandwidthChanged(Object sender, RoutedEventArgs args)
        {
            string newBandwidth = DownloadBandwidthTextBox.Text;
            if (!string.IsNullOrEmpty(newBandwidth))
            {
                int bandwidth = 0;
                if (int.TryParse(newBandwidth, out bandwidth))
                {
                    App.CurrentAccount.DownloadBandwidth = bandwidth;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnQoSChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = QoSCheckbox.IsChecked ?? false;

            SipDscpTextBox.IsEnabled = enabled;
            AudioDscpTextBox.IsEnabled = enabled;
            VideoDscpTextBox.IsEnabled = enabled;

            if (enabled)
            {
                SipDscpTextBox.Text = App.CurrentAccount.SipDscpValue.ToString();
                AudioDscpTextBox.Text = App.CurrentAccount.AudioDscpValue.ToString();
                VideoDscpTextBox.Text = App.CurrentAccount.VideoDscpValue.ToString();
            }
            else
            {
                SipDscpTextBox.Text = "0";
                AudioDscpTextBox.Text = "0";
                VideoDscpTextBox.Text = "0";
            }

            if (enabled != App.CurrentAccount.EnableQualityOfService)
            {
                App.CurrentAccount.EnableQualityOfService = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        private void OnSIPDscpChanged(object sender, RoutedEventArgs e)
        {
            if (!SipDscpTextBox.IsEnabled)
                return;

            string newDscp = SipDscpTextBox.Text;
            if (!string.IsNullOrEmpty(newDscp))
            {
                int val = 0;
                if (int.TryParse(newDscp, out val))
                {
                    App.CurrentAccount.SipDscpValue = val;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnAudioDscpChanged(object sender, RoutedEventArgs e)
        {
            if (!AudioDscpTextBox.IsEnabled)
                return;

            string newDscp = AudioDscpTextBox.Text;
            if (!string.IsNullOrEmpty(newDscp))
            {
                int val = 0;
                if (int.TryParse(newDscp, out val))
                {
                    App.CurrentAccount.AudioDscpValue = val;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnVideoDscpChanged(object sender, RoutedEventArgs e)
        {
            if (!VideoDscpTextBox.IsEnabled)
                return;

            string newDscp = VideoDscpTextBox.Text;
            if (!string.IsNullOrEmpty(newDscp))
            {
                int val = 0;
                if (int.TryParse(newDscp, out val))
                {
                    App.CurrentAccount.VideoDscpValue = val;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnIPv6(object sender, RoutedEventArgs e)
        {
            bool enabled = IPv6Checkbox.IsChecked ?? false;

            if (enabled != App.CurrentAccount.EnableIPv6)
            {
                App.CurrentAccount.EnableIPv6 = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }
        #endregion

        #region NotYetSpecifiedForWindows
        private void OnEdgeOptimization(object sender, RoutedEventArgs e)
        {
            bool enabled = EdgeOptimizationCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }

        private void OnWifiOnly(object sender, RoutedEventArgs e)
        {
            bool enabled = WifiOnlyCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }

        private void OnRandomPort(object sender, RoutedEventArgs e)
        {
            bool enabled = RandomPortCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }

        public void OnAudioPortsChanged(Object sender, RoutedEventArgs args)
        {
            string newAudioPorts = AudioPortsTextBox.Text;
            // Placeholder - not yet indicated for Windows
            //            if (string.IsNullOrEmpty(newAudioPorts))
            //            {
            //                string oldAudioPorts = App.CurrentAccount.Username;
            //                AudioPortsTextBox.Text = oldAudioPorts;
            //            }
        }

        public void OnVideoPortsChanged(Object sender, RoutedEventArgs args)
        {
            string newVideoPorts = VideoPortsTextBox.Text;
            // Placeholder - not yet indicated for Windows

            //            if (string.IsNullOrEmpty(newVideoPorts))
            //            {
            //                string oldVideoPorts = App.CurrentAccount.Username;
            //                VideoPortsTextBox.Text = oldVideoPorts;
            //            }
        }

        private void OnMediaEncryptionChanged(object sender, RoutedEventArgs e)
        {
            TextBlock valueTB = (TextBlock)MediaEncryptionComboBox.SelectedItem;
            string value = valueTB.Text;
            if (App.CurrentAccount != null)
            {
                App.CurrentAccount.MediaEncryption = value;
                // update media settings.
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }

        }
        private void OnPushNotifications(object sender, RoutedEventArgs e)
        {
            bool enabled = PushNotificationsCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }


        #endregion
    }
}
