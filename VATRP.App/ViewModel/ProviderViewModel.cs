using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VATRP.Core.Enums;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class ProviderViewModel : ViewModelBase
    {
        private bool _isSelected;
        private SolidColorBrush backColor;
        private ImageSource _image;
        private string _label;
        private string _proxyHost;

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

        public string ProxyHost
        {
            get { return _proxyHost; }
            set
            {
                _proxyHost = value;
                OnPropertyChanged("ProxyHost");
            }
        }

        public ImageSource Logo
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Logo");
            }
        }

        public ProviderViewModel()
        {
            _label = string.Empty;
            _isSelected = false;
        }

        internal void LoadLogo()
        {
            var logoUri = "pack://application:,,,/ACE;component/Resources/zvrs.png";
            if ( Label == "Sorenson VRS")
                logoUri = "pack://application:,,,/ACE;component/Resources/sorensonvrs.png";
            else if ( Label == "Convo Relay")
            {
                logoUri = "pack://application:,,,/ACE;component/Resources/convovrs.png";
            }
            else if ( Label == "Purple VRS")
            {
                logoUri = "pack://application:,,,/ACE;component/Resources/purplevrs.png";
            }
            else if (Label == "CAAG")
            {
                logoUri = "pack://application:,,,/ACE;component/Resources/caag.png";
            }
            else if (Label == "Global VRS")
            {
                logoUri = "pack://application:,,,/ACE;component/Resources/globalvrs.png";
            }

            try
            {
                Logo = new BitmapImage(new Uri(logoUri));
                // use public setter
            }
            catch (Exception ex)
            {

            }
        }
    }
}

