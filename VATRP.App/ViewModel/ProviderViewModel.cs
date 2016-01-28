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

        private VATRPServiceProvider _provider;
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

        public VATRPServiceProvider Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }

        public ProviderViewModel(VATRPServiceProvider provider)
        {
            this._provider = provider;
            _isSelected = false;
        }
    }
}

