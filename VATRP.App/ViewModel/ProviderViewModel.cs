using System;
using System.ComponentModel;
using System.Diagnostics;
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
        private ImageSource _grayed_image;
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

        public ImageSource GrayedLogo
        {
            get { return _grayed_image; }
            set
            {
                _grayed_image = value;
                OnPropertyChanged("GrayedLogo");
            }
        }
        public ProviderViewModel()
        {
            _label = string.Empty;
            _isSelected = false;
        }

        internal void LoadLogo(bool grayed)
        {
            var imgSuffix = grayed ? "_gr" : string.Empty;
            var logoUri = string.Format( "pack://application:,,,/ACE;component/Resources/zvrs{0}.png",  imgSuffix);
            if ( Label == "Sorenson VRS")
                logoUri = string.Format( "pack://application:,,,/ACE;component/Resources/sorensonvrs{0}.png", imgSuffix);
            else if ( Label == "Convo Relay")
            {
                logoUri = string.Format( "pack://application:,,,/ACE;component/Resources/convovrs{0}.png", imgSuffix);
            }
            else if ( Label == "Purple VRS")
            {
                logoUri = string.Format( "pack://application:,,,/ACE;component/Resources/purplevrs{0}.png", imgSuffix);
            }
            else if (Label == "CAAG")
            {
                logoUri = string.Format( "pack://application:,,,/ACE;component/Resources/caag{0}.png", imgSuffix);
            }
            else if (Label == "Global VRS")
            {
                logoUri = string.Format( "pack://application:,,,/ACE;component/Resources/globalvrs{0}.png", imgSuffix);
            }

            try
            {
                if (grayed)
                {
                    GrayedLogo = new BitmapImage(new Uri(logoUri));
                }
                else
                {
                    Logo = new BitmapImage(new Uri(logoUri));
                }
                // use public setter
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in load logo: " + ex.Message);
            }
        }
    }
}

