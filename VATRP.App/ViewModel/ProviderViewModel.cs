using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;
using VATRP.Core.Enums;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class ProviderViewModel : ViewModelBase
    {
        private bool _isSelected;
        private SolidColorBrush backColor;
        private Bitmap _image;
        private string _label;

        public SolidColorBrush ProviderBackBrush
        {
            get { return backColor; }
            set
            {
                backColor = value;
                OnPropertyChanged("ProviderBackBrush");
            }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public string Label
        {
            get { return _label; }
            set
            {
                _label = value; 
                OnPropertyChanged("Label");
            }
        }

        public System.Drawing.Bitmap Image
        {
            get { return _image; }
            set
            {
                _image = value; 
                OnPropertyChanged("Image");
            }
        }

        public ProviderViewModel()
        {
            _label = string.Empty;
            _isSelected = false;
        }
    }
}

