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
        private int _selectedPage;

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

        public int SelectedPage
        {
            get { return _selectedPage; }
            set
            {
                _selectedPage = value;
                OnPropertyChanged("SelectedPage");
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
        }

        internal void SetActiveSettings(Enums.VATRPSettings settingsType)
        {
            Reset();
            switch (settingsType)
            {
                case VATRPSettings.VATRPSettings_SIP:
                    SelectedPage = 0;
                    break;
                case VATRPSettings.VATRPSettings_Codec:
                    SelectedPage = 2;
                    break;
                case VATRPSettings.VATRPSettings_Multimedia:
                    SelectedPage = 3;
                    break;
                case VATRPSettings.VATRPSettings_Network:
                    SelectedPage = 1;
                    break;
                case VATRPSettings.VATRPSettings_Test:
                    SelectedPage = 4;
                    break;
                default:
                    SelectedPage = -1;
                    break;
            }
        }
    }
}