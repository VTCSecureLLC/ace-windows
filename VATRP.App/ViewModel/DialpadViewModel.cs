using System;
using System.Windows;

namespace VATRP.App.ViewModel
{
    public class DialpadViewModel : ViewModelBase
    {
        private string _remotePartyNumber = string.Empty;
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
                OnPropertyChanged("RemotePartyNumber");
            }
        }

        #endregion

    }
}