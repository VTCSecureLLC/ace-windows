using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.Model;
using VATRP.Core.Model;
using com.vtcsecure.ace.windows.ViewModel;
using NAudio.Wave;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{

    /// <summary>
    /// Interaction logic for UnifiedSettingsAudioVideoCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsAudioVideoCtrl : BaseUnifiedSettingsPanel
    {

        public CallViewCtrl CallControl;

//        private UnifiedSettingsDeviceCtrl _cameraSelectionCtrl;
//        private UnifiedSettingsDeviceCtrl _microphoneSelectionCtrl;
//        private UnifiedSettingsDeviceCtrl _speakerSelectionCtrl;

        public ObservableCollection<VATRPDevice> CameraList { get; private set; }
        public ObservableCollection<VATRPDevice> SpeakerList { get; private set; }
        public ObservableCollection<VATRPDevice> MicrophoneList { get; private set; }

        private readonly AudioRecorder recorder;
        private float lastPeak;
        private bool _isInitializing;
        public UnifiedSettingsAudioVideoCtrl()
        {
            InitializeComponent();
            Title = "Audio/Video";
            this.Loaded += UnifiedSettingsAudioVideoCtrl_Loaded;
            this.Unloaded += UnifiedSettingsAudioVideoCtrl_Unloaded;
/*            _cameraSelectionCtrl = new UnifiedSettingsDeviceCtrl();
            _cameraSelectionCtrl.OnDeviceSelected += HandleDeviceSelected;

            _microphoneSelectionCtrl = new UnifiedSettingsDeviceCtrl();
            _microphoneSelectionCtrl.OnDeviceSelected += HandleDeviceSelected;

            _speakerSelectionCtrl = new UnifiedSettingsDeviceCtrl();
            _speakerSelectionCtrl.OnDeviceSelected += HandleDeviceSelected;
*/
            CameraList = new ObservableCollection<VATRPDevice>();
            MicrophoneList = new ObservableCollection<VATRPDevice>();
            SpeakerList = new ObservableCollection<VATRPDevice>();
            recorder = new AudioRecorder();
            this.DataContext = this;

        }

        
        // ToDo VATRP987 - Liz E. these need to be hooked into acutal settings. not sure where they live.
        private void UnifiedSettingsAudioVideoCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void UnifiedSettingsAudioVideoCtrl_Unloaded(object sender, RoutedEventArgs e)
        {
            Uninitialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            this.recorder.Stopped += OnRecorderStopped;
            recorder.SampleAggregator.MaximumCalculated += OnRecorderPeekCalculated;

            MicLevelMeter.Value = 0;
            if (App.CurrentAccount == null)
                return;
            _isInitializing = true;
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

            List<VATRPDevice> availableCameras = ServiceManager.Instance.GetAvailableCameras();
            VATRPDevice selectedCamera = ServiceManager.Instance.GetSelectedCamera();
            CameraList.Clear();
            foreach (VATRPDevice camera in availableCameras)
            {
                CameraList.Add(camera);
                if ((selectedCamera != null) && selectedCamera.deviceId.Trim().Equals(camera.deviceId.Trim()))
                {
                    SelectCameraComboBox.SelectedItem = camera;
                }

            }

            List<VATRPDevice> availableMicrophones = ServiceManager.Instance.GetAvailableMicrophones();
            VATRPDevice selectedMicrophone = ServiceManager.Instance.GetSelectedMicrophone();
            MicrophoneList.Clear();
            foreach (VATRPDevice microphone in availableMicrophones)
            {
                MicrophoneList.Add(microphone);
                if ((selectedMicrophone != null) && selectedMicrophone.deviceId.Trim().Equals(microphone.deviceId.Trim()))
                {
                    SelectMicrophoneComboBox.SelectedItem = microphone;
                }

            }
            List<VATRPDevice> availableSpeakers = ServiceManager.Instance.GetAvailableSpeakers();
            VATRPDevice selectedSpeaker = ServiceManager.Instance.GetSelectedSpeakers();
            SpeakerList.Clear();
            foreach (VATRPDevice speaker in availableSpeakers)
            {
                SpeakerList.Add(speaker);
                if ((selectedSpeaker != null) && selectedSpeaker.deviceId.Trim().Equals(speaker.deviceId.Trim()))
                {
                    SelectSpeakerComboBox.SelectedItem = speaker;
                }

            }

            if (selectedMicrophone != null)
            {
                int recordingDevId = -1;

                for (int n = 0; n < WaveIn.DeviceCount; n++)
                {
                    if (WaveIn.GetCapabilities(n).ProductName.Contains(selectedMicrophone.displayName))
                    {
                        recordingDevId = n;
                        break;
                    }
                }
                if (recordingDevId != -1)
                {
                    recorder.MicrophoneLevel = 100;
                    recorder.BeginMonitoring(recordingDevId);
                }
            }

            SelfPreviewCtrl.VideoWidth = 200;
            SelfPreviewCtrl.VideoHeight = 200;

            if (selectedCamera != null && !_firstTimeInitialize)
            {
                SelfPreviewCtrl.Initialize(selectedCamera.deviceId);
            }

            _firstTimeInitialize = false;
            _isInitializing = false;

        }


        public override void Uninitialize()
        {
            SelfPreviewCtrl.Uninitialize();
            recorder.Stop();
            this.recorder.Stopped -= OnRecorderStopped;
            recorder.SampleAggregator.MaximumCalculated -= OnRecorderPeekCalculated;
        }


        // VATRP-1200 TODO - store settings, update Linphone
        #region Device selection

        private void OnSelectCamera(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null || !_initialized || _isInitializing) return;
            Console.WriteLine("Camera Selected");

            bool updateData = false;
            VATRPDevice selectedCamera = (VATRPDevice)SelectCameraComboBox.SelectedItem;
            string selectedCameraId = App.CurrentAccount.SelectedCameraId;
            if (string.IsNullOrEmpty(selectedCameraId))
            {
                if (SelectCameraComboBox.Items.Count > 0)
                {
                    var device = SelectCameraComboBox.Items.GetItemAt(0) as VATRPDevice;
                    if (device != null)
                    {
                        updateData = true;
                        App.CurrentAccount.SelectedCameraId = device.deviceId;
                        selectedCameraId = device.deviceId;
                    }
                }
            }
            if ((selectedCamera != null && selectedCamera.deviceId != selectedCameraId) || updateData)
            {
                if (selectedCamera != null) 
                    App.CurrentAccount.SelectedCameraId = selectedCamera.deviceId;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        private void OnSelectMicrophone(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null || !_initialized || _isInitializing) return;
            Console.WriteLine("Microphone Selected");
            bool updateData = false;
            VATRPDevice selectedMicrophone = (VATRPDevice)SelectMicrophoneComboBox.SelectedItem;
            string selectedMicrophoneId = App.CurrentAccount.SelectedMicrophoneId;

            if (string.IsNullOrEmpty(selectedMicrophoneId))
            {
                if (SelectMicrophoneComboBox.Items.Count > 0)
                {
                    var device = SelectMicrophoneComboBox.Items.GetItemAt(0) as VATRPDevice;
                    if (device != null)
                    {
                        updateData = true;
                        App.CurrentAccount.SelectedMicrophoneId = device.deviceId;
                        selectedMicrophoneId = device.deviceId;
                    }
                }
            }

            if ((selectedMicrophone != null && selectedMicrophone.deviceId != selectedMicrophoneId) || updateData )
            {
                if (selectedMicrophone != null) 
                    App.CurrentAccount.SelectedMicrophoneId = selectedMicrophone.deviceId;
                recorder.Stop();
                MicLevelMeter.Value = 0;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
                if (selectedMicrophone != null)
                {
                    int recordingDevId = -1;
                    
                    for (int n = 0; n < WaveIn.DeviceCount; n++)
                    {
                        if (WaveIn.GetCapabilities(n).ProductName.Contains(selectedMicrophone.displayName))
                        {
                            recordingDevId = n;
                            break;
                        }
                    }
                    if (recordingDevId != -1)
                    {
                        recorder.MicrophoneLevel = 100;
                        recorder.BeginMonitoring(recordingDevId);
                    }
                }
            }
        }

        private void OnSelectSpeaker(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null || !_initialized || _isInitializing) return;
            Console.WriteLine("Speaker Selected");
            bool updateData = false;
            VATRPDevice selectedSpeaker = (VATRPDevice)SelectSpeakerComboBox.SelectedItem;
            string selectedSpeakerId = App.CurrentAccount.SelectedSpeakerId;
            
            if (string.IsNullOrEmpty(selectedSpeakerId))
            {
                if (SelectSpeakerComboBox.Items.Count > 0)
                {
                    var device = SelectSpeakerComboBox.Items.GetItemAt(0) as VATRPDevice;
                    if (device != null)
                    {
                        updateData = true;
                        App.CurrentAccount.SelectedSpeakerId = device.deviceId;
                        selectedSpeakerId = device.deviceId;
                    }
                }
            }

            if ((selectedSpeaker != null && selectedSpeaker.deviceId != selectedSpeakerId) || updateData)
            {
                if (selectedSpeaker != null) 
                    App.CurrentAccount.SelectedSpeakerId = selectedSpeaker.deviceId;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        #endregion

        private bool IsPreferredVideoSizeChanged()
        {
            if (App.CurrentAccount == null || _isInitializing)
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
            if (App.CurrentAccount == null || _isInitializing)
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
        
        private void OnRecorderPeekCalculated(object sender, MaxSampleEventArgs e)
        {
            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
            this.MicLevelMeter.Value = lastPeak * 100;
        }

        private void OnRecorderStopped(object sender, EventArgs e)
        {
            this.MicLevelMeter.Value = 0;
        }
    }
}
