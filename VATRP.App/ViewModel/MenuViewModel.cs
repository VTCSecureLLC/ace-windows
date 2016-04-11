using System;
using System.Windows;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class MenuViewModel : ViewModelBase
    {
        private bool _isSelfViewActive;
        private int _videoMailCount;

        public MenuViewModel()
        {
            _videoMailCount = 0;
        }

        public bool IsSelfViewActive
        {
            get { return _isSelfViewActive; }
            set
            {
                _isSelfViewActive = value;
                OnPropertyChanged("IsSelfViewActive");
            }
        }

        public int VideoMailCount
        {
            get { return _videoMailCount; }
            set
            {
                _videoMailCount = value;
                OnPropertyChanged("VideoMailCount");
            }
        }
    }
}