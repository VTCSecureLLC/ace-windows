using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using VATRP.Core.Extensions;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class ContactEditViewModel : ViewModelBase
    {
        private string _title;
        private bool _isAddMode;
        private string _infoTitle;
        private string _contactName;
        private string _contactSipAddress;
        private string _contactSipUsername;
        private ObservableCollection<ProviderViewModel> _providers;
        private ProviderViewModel _selectedProvider;

        public ContactEditViewModel(bool addMode)
        {
            _isAddMode = addMode;

            Title = _isAddMode ? "Add new contact" : "Edit contact";
            InfoTitle = "Contact information";
            LoadProviders();
        }

        #region Properties

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public string InfoTitle
        {
            get { return _infoTitle; }
            set
            {
                _infoTitle = value;
                OnPropertyChanged("InfoTitle");
            }
        }

        public string ContactName
        {
            get { return _contactName; }
            set
            {
                _contactName = value;
                OnPropertyChanged("ContactName");
            }
        }

        public string ContactSipUsername
        {
            get { return _contactSipUsername; }
            set
            {
                _contactSipUsername = value;
                if (ValidateAddress(_contactSipUsername))
                    _contactSipAddress = _contactSipUsername;
                else
                {
                    ContactSipAddress = _contactSipUsername;
                }
                OnPropertyChanged("ContactSipUsername");
            }
        }

        public string ContactSipAddress
        {
            get { return _contactSipAddress; }
            set
            {
                var host = "unknown.host";
                if (App.CurrentAccount != null)
                {
                    host = App.CurrentAccount.ProxyHostname;
                }
                _contactSipAddress = string.Format("{0}@{1}", value, host);
                OnPropertyChanged("ContactSipAddress");
            }
        }

        public bool IsAddMode
        {
            get { return _isAddMode; }
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
                OnPropertyChanged("SelectedProvider");
            }
        }

        #endregion

        private void LoadProviders()
        {
            var providersList = ServiceManager.Instance.ProviderService.GetProviderList();

            var selectedprovider = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.CURRENT_PROVIDER, "");

            foreach (var s in providersList)
            {
                var providerModel = new ProviderViewModel {Label = s};
                providerModel.LoadLogo();
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

        internal bool ValidateName()
        {
            return !string.IsNullOrWhiteSpace(ContactName);
        }

        internal bool ValidateAddress(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            return ValidateEmailInput(input);
        }

        internal bool ValidateUsername(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            string regex =
               @"^([\w-\.]+)$";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(regex);

            return r.IsMatch(input);
        }

        private bool ValidateEmailInput(string input)
        {
            // Create a new Regex based on the specified regular expression.
            string regex =
                @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(regex);

            return r.IsMatch(input);
        }

        internal void TrimSipUsername()
        {
            var hostToTrim = "@bc1.vatrp.net";
            if (_contactSipUsername.EndsWith(hostToTrim))
                _contactSipUsername = _contactSipUsername.Remove(_contactSipUsername.IndexOf(hostToTrim));
            OnPropertyChanged("ContactSipUsername");
        }
    }
}