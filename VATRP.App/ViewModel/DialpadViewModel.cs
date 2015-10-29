using System;
using System.Windows;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class DialpadViewModel : ViewModelBase
    {
        private string _remotePartyNumber = string.Empty;
        private bool _allowAudioCall = false;
        private bool _allowVideoCall = false;
        private int _remotePartyDigitLimit = 10;

        public DialpadViewModel()
        {

        }

        #region Properties

        public String RemotePartyNumber
        {
            get { return _remotePartyNumber; }
            set
            {

                if (_remotePartyNumber.Length > _remotePartyDigitLimit)
                    return;

                _remotePartyNumber = value;
                AllowAudioCall = !string.IsNullOrWhiteSpace(_remotePartyNumber);
                AllowVideoCall = !string.IsNullOrWhiteSpace(_remotePartyNumber);
                OnPropertyChanged("RemotePartyNumber");
            }
        }

        public bool AllowAudioCall
        {
            get { return _allowAudioCall; }
            set
            {
                _allowAudioCall = value;
                OnPropertyChanged("AllowAudioCall");
            }
        }
        public bool AllowVideoCall
        {
            get { return _allowVideoCall && App.CanMakeVideoCall; }
            set
            {
                _allowVideoCall = value;
                OnPropertyChanged("AllowVideoCall");
            }
        }
        #endregion

    }
}