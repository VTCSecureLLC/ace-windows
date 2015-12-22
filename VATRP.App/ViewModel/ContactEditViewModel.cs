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

        public ContactEditViewModel(bool addMode, string address)
        {
            _isAddMode = addMode;

            Title = _isAddMode ? "Add new contact" : "Edit contact";
            InfoTitle = "Contact information";
            ContactSipUsername = address;
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
                TrimSipUsername();
                ContactSipUsername = ContactSipUsername + vrsLogoToAddress(_selectedProvider.Label);
                TrimSipUsername();
                OnPropertyChanged("SelectedProvider");

            }
        }

        private bool isValidLabel(string Label)
        {

            if (Label== "@Sorenson.vatrp.net"||Label == "@convo.vatrp.net"||Label == "@purple.vatrp.net"||Label ==  "@caag.vatrp.net"||Label ==  "@bc1.vatrp.net")
                return true;

            return false;
        }
        #endregion
        private string vrsLogoToAddress(string Label) {

            string logoUri ="";
            if (Label == "Sorenson VRS")
                logoUri = "@Sorenson.vatrp.net";
            else if (Label == "Convo Relay")
            {
                logoUri = "@convo.vatrp.net";
            }
            else if (Label == "Purple VRS")
            {
                logoUri = "@purple.vatrp.net";
            }
            else if (Label == "CAAG")
            {
                logoUri = "@caag.vatrp.net";
            }
            else if (Label == "Global VRS")
            {
                logoUri = "@bc1.vatrp.net";
            }

            return logoUri;
        }

        private void LoadProviders()
        {
            var providersList = ServiceManager.Instance.ProviderService.GetProviderList();
            var domain = ValidateAddress(_contactSipUsername) ? "@" + _contactSipUsername.Split('@')[1] : null;
            foreach (var s in providersList)
            {
                var providerModel = new ProviderViewModel {Label = s};
                providerModel.LoadLogo();
                Providers.Add(providerModel);
                if (vrsLogoToAddress(s) ==  domain )
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
            var domain = ValidateAddress(_contactSipUsername) ? "@" + _contactSipUsername.Split('@')[1] : "";
            var name  = isValidLabel(domain.Trim());
            if (name)
                _contactSipUsername = _contactSipUsername.Remove(_contactSipUsername.IndexOf(domain));

            OnPropertyChanged("ContactSipUsername");
        }
    }
}