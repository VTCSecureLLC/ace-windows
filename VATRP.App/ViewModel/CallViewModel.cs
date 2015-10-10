using System;
using System.Windows;

namespace VATRP.App.ViewModel
{
    public class CallViewModel : ViewModelBase
    {
        private bool _visualizeRing;
        private bool _visualizeIncoming;
        private int _ringCount;

        public CallViewModel()
        {

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

        #endregion

    }
}