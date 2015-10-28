using System;
using System.Windows;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class CallViewModel : ViewModelBase
    {
        private bool _visualizeRing;
        private bool _visualizeIncoming;
        private int _ringCount;
        private bool _showInfo;
        private VATRPCallState _callState;
        private string _displayName;
        private string _remoteNumber;
        private int _duration;
        private int _autoAnswer;

        public CallViewModel()
        {
            _visualizeRing = false;
            _visualizeIncoming = false;
        }

        #region Properties

        public bool VisualizeIncoming
        {
            get { return _visualizeIncoming; }
            set
            {
                _visualizeIncoming = value;
                OnPropertyChanged("VisualizeIncoming");
            }
        }

        public bool VisualizeRinging
        {
            get { return _visualizeRing; }
            set
            {
                _visualizeRing = value;
                OnPropertyChanged("VisualizeRinging");
            }
        }

        public int RingCount
        {
            get { return _ringCount; }
            set
            {
                _ringCount = value;
                OnPropertyChanged("RingCount");
            }
        }

        public bool ShowInfo
        {
            get
            {
                return _showInfo;
            }
            set
            {
                if (_showInfo != value)
                {
                    _showInfo = value;
                    OnPropertyChanged("ShowInfo");
                }
            }
        }

        public VATRPCallState CallState
        {
            get
            {
                return _callState;
            }
            set
            {
                _callState = value;
                OnPropertyChanged("CallState");
                switch (_callState)
                {
                    case VATRPCallState.StreamsRunning:
                        ShowInfo = true;
                        break;
                    default:
                        ShowInfo = false;
                        break;
                }
            }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        public string RemoteNumber
        {
            get { return _remoteNumber; }
            set
            {
                _remoteNumber = value;
                OnPropertyChanged("RemoteNumber");
            }
        }

        public int Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged("Duration");
                }
            }
        }

        public int AutoAnswer
        {
            get { return _autoAnswer; }
            set
            {
                _autoAnswer = value; 
                OnPropertyChanged("AutoAnswer");
            }
        }

        #endregion

    }
}