using System;
using System.Windows;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class VideoViewModel : ViewModelBase
    {
        private double _videoWidth;
        private double _videoHeight;

        public VideoViewModel()
        {
        }

        public double VideoWidth
        {
            get { return _videoWidth; }
            set
            {
                _videoWidth = value; 
                OnPropertyChanged("VideoWidth");
            }
        }

        public double VideoHeight
        {
            get { return _videoHeight; }
            set
            {
                _videoHeight = value; 
                OnPropertyChanged("VideoHeight");
            }
        }
    }
}