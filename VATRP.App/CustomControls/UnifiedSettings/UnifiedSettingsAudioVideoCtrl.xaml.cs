using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using com.vtcsecure.ace.windows.ViewModel;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{

    /// <summary>
    /// Interaction logic for UnifiedSettingsAudioVideoCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsAudioVideoCtrl : BaseUnifiedSettingsPanel
    {

        public CallViewCtrl CallControl;

        private UnifiedSettingsDeviceCtrl _cameraSelectionCtrl;
        private UnifiedSettingsDeviceCtrl _microphoneSelectionCtrl;
        private UnifiedSettingsDeviceCtrl _speakerSelectionCtrl;

        public UnifiedSettingsAudioVideoCtrl()
        {
            InitializeComponent();
            Title = "Audio/Video";
            this.Loaded += UnifiedSettingsAudioVideoCtrl_Loaded;
            _cameraSelectionCtrl = new UnifiedSettingsDeviceCtrl();
            _cameraSelectionCtrl.OnDeviceSelected += HandleDeviceSelected;

            _microphoneSelectionCtrl = new UnifiedSettingsDeviceCtrl();
            _microphoneSelectionCtrl.OnDeviceSelected += HandleDeviceSelected;

            _speakerSelectionCtrl = new UnifiedSettingsDeviceCtrl();
            _speakerSelectionCtrl.OnDeviceSelected += HandleDeviceSelected;
        }

        // ToDo VATRP987 - Liz E. these need to be hooked into acutal settings. not sure where they live.
        private void UnifiedSettingsAudioVideoCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            base.Initialize();
            if (App.CurrentAccount == null)
                return;
/*            List<VATRPDevice> cameraList = ServiceManager.Instance.GetAvailableCameras();
            string storedCameraId = App.CurrentAccount.SelectedCameraId;
            if (cameraList != null)
            {
                List<string> cameraNames = new List<string>();
                foreach (VATRPDevice device in cameraList)
                {
                    cameraNames.Add(device.displayName);
                    if (!string.IsNullOrEmpty(storedCameraId) && storedCameraId.Equals(device.deviceId))
                    {
                        _selectedCamera = device.displayName;
                    }
                }
                Cameras = cameraNames.ToArray();
            }
*/
            foreach (var item in PreferredVideoSizeComboBox.Items)
            {
                var tb = item as TextBlock;
                if (GetPreferredVideoSizeId(tb).Equals(App.CurrentAccount.PreferredVideoId))
                {
                    PreferredVideoSizeComboBox.SelectedItem = item;
                    break;
                }
            }

            MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
            MuteSpeakerCheckBox.IsChecked = App.CurrentAccount.MuteSpeaker;
            EchoCancelCheckBox.IsChecked = App.CurrentAccount.EchoCancel;
            ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;
            // VATRP-1200 TODO - populate device combo boxes from stored settings.
            List<VATRPDevice> availableCameras = ServiceManager.Instance.GetAvailableCameras();
            string selectedCameraId = App.CurrentAccount.SelectedCameraId;
            if (string.IsNullOrEmpty(selectedCameraId))
            {
                VATRPDevice selectedCamera = ServiceManager.Instance.GetSelectedCamera();
                SelectedCameraLabel.Content = selectedCamera.displayName;
                SelectedCameraLabel.ToolTip = selectedCamera.displayName;
            }
            else
            {
                foreach (VATRPDevice camera in availableCameras)
                {
                    if (!string.IsNullOrEmpty(selectedCameraId) && selectedCameraId.Equals(camera.deviceId))
                    {
                        SelectedCameraLabel.Content = camera.displayName;
                        SelectedCameraLabel.ToolTip = camera.displayName;
                    }
                }
            }

            List<VATRPDevice> availableMicrophones = ServiceManager.Instance.GetAvailableMicrophones();
            string selectedMicrophoneId = App.CurrentAccount.SelectedMicrophoneId;
            if (string.IsNullOrEmpty(selectedMicrophoneId))
            {
                VATRPDevice selectedMicrophone = ServiceManager.Instance.GetSelectedMicrophone();
                SelectedMicrophoneLabel.Content = selectedMicrophone.displayName;
                SelectedMicrophoneLabel.ToolTip = selectedMicrophone.displayName;
            }
            else
            {
                foreach (VATRPDevice microphone in availableMicrophones)
                {
                    if (!string.IsNullOrEmpty(selectedMicrophoneId) && selectedMicrophoneId.Equals(microphone.deviceId))
                    {
                        SelectedMicrophoneLabel.Content = microphone.displayName;
                        SelectedMicrophoneLabel.ToolTip = microphone.displayName;
                    }
                }
            }

            List<VATRPDevice> availableSpeakers = ServiceManager.Instance.GetAvailableSpeakers();
            string selectedSpeakerId = App.CurrentAccount.SelectedSpeakerId;
            if (string.IsNullOrEmpty(selectedSpeakerId))
            {
                VATRPDevice selectedSpeaker = ServiceManager.Instance.GetSelectedSpeakers();
                SelectedSpeakerLabel.Content = selectedSpeaker.displayName;
                SelectedSpeakerLabel.ToolTip = selectedSpeaker.displayName;
            }
            else
            {
                foreach (VATRPDevice speaker in availableSpeakers)
                {
                    if (!string.IsNullOrEmpty(selectedSpeakerId) && selectedSpeakerId.Equals(speaker.deviceId))
                    {
                        SelectedSpeakerLabel.Content = speaker.displayName;
                        SelectedSpeakerLabel.ToolTip = speaker.displayName;
                    }
                }
            }
        }



        public override void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
            if (App.CurrentAccount == null)
                return;

            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.MuteMicrophoneMenu: MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
                    break;
                case ACEMenuSettingsUpdateType.MuteSpeakerMenu: MuteSpeakerCheckBox.IsChecked = App.CurrentAccount.MuteSpeaker;
                    break;
                case ACEMenuSettingsUpdateType.ShowSelfViewMenu: ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;
                    break;
                default:
                    break;
            }
        }

        public override void ShowSuperOptions(bool show)
        {
            base.ShowSuperOptions(show);
            // 1170-ready: specified for android. is implemented here if we want to enable it.
            EchoCancelCheckBox.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview;
            EchoCancelLabel.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview;

            // 1170-ready: specified for ios. Is connected and implemented for windows.
            ShowSelfViewLabel.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview;
            ShowSelfViewCheckBox.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview;
        }


        private void OnMuteMicrophone(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Mute Microphone Clicked");
            bool enabled = MuteMicrophoneCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteMicrophone)
            {
                App.CurrentAccount.MuteMicrophone = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                if ((CallControl != null) && CallControl.IsLoaded)
                {
                    CallControl.UpdateMuteSettingsIfOpen();
                }
            }
        }
        private void OnMuteSpeaker(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Mute Speaker Clicked");
            bool enabled = MuteSpeakerCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteSpeaker)
            {
                App.CurrentAccount.MuteSpeaker = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                if ((CallControl != null) && CallControl.IsLoaded)
                {
                    CallControl.UpdateMuteSettingsIfOpen();
                }
            }
        }
        private void OnEchoCancel(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Echo Cancel Call Clicked");
            bool enabled = this.EchoCancelCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EchoCancel)
            {
                App.CurrentAccount.EchoCancel = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }
        private void OnShowSelfView(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
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

        // VATRP-1200 TODO - store settings, update Linphone
        #region Device selection
        private void OnShowCameraOptions(object sender, RoutedEventArgs e)
        {
            List<VATRPDevice> availableCameras = ServiceManager.Instance.GetAvailableCameras();
            _cameraSelectionCtrl.deviceList = availableCameras;
            _cameraSelectionCtrl.Initialize();
            this.ContentPanel.Content = _cameraSelectionCtrl;
        }

        private void OnShowMicrophoneOptions(object sender, RoutedEventArgs e)
        {
            List<VATRPDevice> availableMicrophones = ServiceManager.Instance.GetAvailableMicrophones();
            _microphoneSelectionCtrl.deviceList = availableMicrophones;
            _microphoneSelectionCtrl.Initialize();
            this.ContentPanel.Content = _microphoneSelectionCtrl;
        }

        private void OnShowSpeakerOptions(object sender, RoutedEventArgs e)
        {
            List<VATRPDevice> availableSpeakers = ServiceManager.Instance.GetAvailableSpeakers();
            _speakerSelectionCtrl.deviceList = availableSpeakers;
            _speakerSelectionCtrl.Initialize();
            this.ContentPanel.Content = _speakerSelectionCtrl;
        }

        private void HandleDeviceSelected(VATRPDevice device)
        {
            this.ContentPanel.Content = null;
            if (device == null)
                return;

            switch (device.deviceType)
            {
                case VATRPDeviceType.CAMERA: HandleCameraSelected(device);
                    break;
                case VATRPDeviceType.MICROPHONE: HandleMicrophoneSelected(device);
                    break;
                case VATRPDeviceType.SPEAKER: HandleSpeakerSelected(device);
                    break;
                default: break;
            }
        }


        private void OnSelectCamera(object sender, RoutedEventArgs e)
        {
/*            Console.WriteLine("Camera Selected");
            if (App.CurrentAccount == null)
                return;

            string str = SelectCameraComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(str))
            {
                List<VATRPDevice> cameraList = ServiceManager.Instance.GetAvailableCameras();
                string selectedCameraId = App.CurrentAccount.SelectedCameraId;
                VATRPDevice selectedDevice = new VATRPDevice(selectedCameraId, VATRPDeviceType.CAMERA);
                foreach(VATRPDevice camera in cameraList)
                {
                    if (camera.displayName.Equals(str))
                    {
                        selectedDevice = camera;
                    }
                }
                if (!selectedDevice.deviceId.Equals(selectedCameraId))
                {
                    App.CurrentAccount.SelectedCameraId = selectedDevice.deviceId;
                    ServiceManager.Instance.ApplyMediaSettingsChanges();
                    ServiceManager.Instance.SaveAccountSettings();
                }
            }
*/        }
        public void HandleCameraSelected(VATRPDevice device)
        {
            if ((App.CurrentAccount == null) || (device == null))
                return;
            SelectedCameraLabel.Content = device.displayName;
            string selectedCameraId = App.CurrentAccount.SelectedCameraId;
            if (!device.deviceId.Equals(selectedCameraId))
            {
                App.CurrentAccount.SelectedCameraId = device.deviceId;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        public void HandleMicrophoneSelected(VATRPDevice device)
        {
            if ((App.CurrentAccount == null) || (device == null))
                return;

            SelectedMicrophoneLabel.Content = device.displayName;
            string selectedDeviceId = App.CurrentAccount.SelectedMicrophoneId;
            if (!device.deviceId.Equals(selectedDeviceId))
            {
                App.CurrentAccount.SelectedMicrophoneId = device.deviceId;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        public void HandleSpeakerSelected(VATRPDevice device)
        {
            if ((App.CurrentAccount == null) || (device == null))
                return;

            SelectedSpeakerLabel.Content = device.displayName;
            string selectedDeviceId = App.CurrentAccount.SelectedSpeakerId;
            if (!device.deviceId.Equals(selectedDeviceId))
            {
                App.CurrentAccount.SelectedSpeakerId = device.deviceId;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }


        private void OnSelectMicrophone(object sender, RoutedEventArgs e)
        {
/*            Console.WriteLine("Microphone Selected");
            if (App.CurrentAccount == null)
                return;
            string str = SelectMicrophoneComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(str))
            {
                List<VATRPDevice> microphoneList = ServiceManager.Instance.GetAvailableMicrophones();
                string selectedMicrophoneId = App.CurrentAccount.SelectedMicrophoneId;
                VATRPDevice selectedDevice = new VATRPDevice(selectedMicrophoneId, VATRPDeviceType.MICROPHONE);
                foreach (VATRPDevice camera in microphoneList)
                {
                    if (camera.displayName.Equals(str))
                    {
                        selectedDevice = camera;
                    }
                }
                if (!selectedDevice.deviceId.Equals(selectedMicrophoneId))
                {
                    App.CurrentAccount.SelectedMicrophoneId = selectedDevice.deviceId;
                    ServiceManager.Instance.ApplyMediaSettingsChanges();
                    ServiceManager.Instance.SaveAccountSettings();
                }
            }
*/        }
        private void OnSelectSpeaker(object sender, RoutedEventArgs e)
        {
/*            Console.WriteLine("Speaker Selected");
            if (App.CurrentAccount == null)
                return;
            string str = SelectSpeakerComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(str))
            {
                List<VATRPDevice> speakerList = ServiceManager.Instance.GetAvailableSpeakers();
                string selectedSpeakerId = App.CurrentAccount.SelectedSpeakerId;
                VATRPDevice selectedDevice = new VATRPDevice(selectedSpeakerId, VATRPDeviceType.SPEAKER);
                foreach (VATRPDevice camera in speakerList)
                {
                    if (camera.displayName.Equals(str))
                    {
                        selectedDevice = camera;
                    }
                }
                if (!selectedDevice.deviceId.Equals(selectedSpeakerId))
                {
                    App.CurrentAccount.SelectedSpeakerId = selectedDevice.deviceId;
                    ServiceManager.Instance.ApplyMediaSettingsChanges();
                    ServiceManager.Instance.SaveAccountSettings();
                }
            }
*/        }
        #endregion

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

        private string GetPreferredVideoSizeId(TextBlock tb)
        {
            if (tb == null)
                return string.Empty;

            var index = tb.Text.IndexOf(" (", System.StringComparison.Ordinal);
            return index != -1 ? tb.Text.Substring(0, index).Trim() : string.Empty;
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
    }
}
