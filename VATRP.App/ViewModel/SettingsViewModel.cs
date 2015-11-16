using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private bool _isSipsettingsEnabled;
        private bool _isCodecSettingsEnabled;
        private bool _isNetworkSettingsEnabled;
        private bool _isMultimediaSettingsEnabled;
        private bool _isTestingSettingsEnabled;

        public SettingsViewModel()
        {
            Reset();
        }

        #region Properties

        public bool SipSettingsChanged { get; set; }
        public bool NetworkSettingsChanged { get; set; }
        public bool CodecSettingsChanged { get; set; }
        public bool CallSettingsChanged { get;  set; }
        public bool MediaSettingsChanged { get; set; }

        public bool IsSipSettingsEnabled
        {
            get { return _isSipsettingsEnabled; }
            set
            {
                _isSipsettingsEnabled = value;
                OnPropertyChanged("IsSipSettingsEnabled");
            }
        }

        public bool IsCodecSettingsEnabled
        {
            get { return _isCodecSettingsEnabled; }
            set
            {
                _isCodecSettingsEnabled = value;
                OnPropertyChanged("IsCodecSettingsEnabled");
            }
        }

        public bool IsNetworkSettingsEnabled
        {
            get { return _isNetworkSettingsEnabled; }
            set
            {
                _isNetworkSettingsEnabled = value;
                OnPropertyChanged("IsNetworkSettingsEnabled");
            }
        }

        public bool IsMultimediaSettingsEnabled
        {
            get { return _isMultimediaSettingsEnabled; }
            set
            {
                _isMultimediaSettingsEnabled = value;
                OnPropertyChanged("IsMultimediaSettingsEnabled");
            }
        }

        public bool IsTestingSettingsEnabled
        {
            get { return _isTestingSettingsEnabled; }
            set
            {
                _isTestingSettingsEnabled = value;
                OnPropertyChanged("IsTestingSettingsEnabled");
            }
        }

        #endregion

        
        internal void Reset()
        {
            CodecSettingsChanged = false;
            NetworkSettingsChanged = false;
            SipSettingsChanged = false;
            CallSettingsChanged = false;
            MediaSettingsChanged = false;

            IsSipSettingsEnabled = false;
            IsCodecSettingsEnabled = false;
            IsNetworkSettingsEnabled = false;
            IsMultimediaSettingsEnabled = false;
            IsTestingSettingsEnabled = false;

        }

        internal void SetActiveSettings(Enums.VATRPSettings settingsType)
        {
            Reset();
            switch (settingsType)
            {
                case VATRPSettings.VATRPSettings_SIP:
                    IsSipSettingsEnabled = true;
                    break;
                case VATRPSettings.VATRPSettings_Codec:
                    IsCodecSettingsEnabled = true;
                    break;
                case VATRPSettings.VATRPSettings_Multimedia:
                    IsMultimediaSettingsEnabled = true;
                    break;
                case VATRPSettings.VATRPSettings_Network:
                    IsNetworkSettingsEnabled = true;
                    break;
                case VATRPSettings.VATRPSettings_Test:
                    IsTestingSettingsEnabled = true;
                    break;
            }
        }
    }
}