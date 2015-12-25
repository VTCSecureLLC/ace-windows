using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Windows;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class DialpadViewModel : ViewModelBase
    {
        private string _remotePartyNumber = string.Empty;
        private bool _allowAudioCall = false;
        private bool _allowVideoCall = false;
        private int _remotePartyDigitLimit = 1000;
        private VATRPCallState _callState;
        private ObservableCollection<ProviderViewModel> _providers;
        private ProviderViewModel _selectedProvider;

        public DialpadViewModel()
        {
            _callState = VATRPCallState.Closed;
            ServiceManager.Instance.ProviderService.ServiceStarted += OnProvidersListLoaded;
        }


        private void OnProvidersListLoaded(object sender, EventArgs args)
        {
            var providersList = ServiceManager.Instance.ProviderService.GetProviderList();

            var selectedprovider =  ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,  Configuration.ConfEntry.CURRENT_PROVIDER, "");

            foreach (var s in providersList)
            {
                var providerModel = new ProviderViewModel {Label = s};
                providerModel.LoadLogo(true);
                providerModel.LoadLogo(false);
                Providers.Add(providerModel);
                if (s == selectedprovider)
                    _selectedProvider = providerModel;
            }

            if (_selectedProvider == null)
                if (Providers != null && Providers.Count > 0)
                {
                    _selectedProvider = Providers[0];
                }
            
        }

        #region Properties

        public String RemotePartyNumber
        {
            get { return _remotePartyNumber; }
            set
            {

                if (_remotePartyNumber == value)
                    return;

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

        public VATRPCallState CallState
        {
            get { return _callState; }
            set
            {
                _callState = value; 
                OnPropertyChanged("CallState");
            }
        }

        public ObservableCollection<ProviderViewModel> Providers
        {
            get { return _providers ?? (_providers = new ObservableCollection<ProviderViewModel>()); }
            set
            {
                _providers = value; 
                OnPropertyChanged("Providers");
            }
        }

        public ProviderViewModel SelectedProvider
        {
            get { return _selectedProvider; }
            set
            {
                _selectedProvider = value;
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.CURRENT_PROVIDER, value.Label);
                OnPropertyChanged("SelectedProvider");
            }
        }

        #endregion

    }
}