using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using VATRP.Core.Extensions;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class ContactEditViewModel : ViewModelBase
    {
        private string _title;
        private readonly bool _isAddMode;
        private string _infoTitle;
        private string _contactName = string.Empty;
        private string _contactSipAddress = string.Empty;
        private string _contactSipUsername = string.Empty;
        private string _avatarPath = string.Empty;
        private readonly string _originAvatarPath = string.Empty;
        private string _newAvatarPath = string.Empty;
        private ImageSource _avatar;
        private ObservableCollection<ProviderViewModel> _providers;
        private ProviderViewModel _selectedProvider;
        private ProviderViewModel _currentProvider;
        private ProviderViewModel _nologoProvider;
        
        public ContactEditViewModel(bool addMode, string address, string avatar)
        {
            _isAddMode = addMode;

            Title = _isAddMode ? "Add new contact" : "Edit contact";
            InfoTitle = "Contact information";
            ContactSipUsername = address;
            LoadProviders();
            if (!_isAddMode)
            {
                _avatarPath = avatar;
                _originAvatarPath = _avatarPath;
            }
            LoadContactAvatar();
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
                if (ValidateAddress(_contactSipUsername) == 0)
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

        public ImageSource Avatar
        {
            get { return _avatar; }
            set
            {
                _avatar = value;
                OnPropertyChanged("Avatar");
            }
        }

        public string NewAvatarPath
        {
            get { return _newAvatarPath; }
        }

        public string OriginAvatarPath
        {
            get { return _originAvatarPath; }
        }

        public bool IsAddMode
        {
            get { return _isAddMode; }
        }
        
        public bool AvatarChanged { get; set; }

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
                UpdateContactAddress(true);
                OnPropertyChanged("SelectedProvider");

            }
        }

        public ProviderViewModel AccountProvider
        {
            get { return _currentProvider; }
        }

        #endregion

        private void LoadProviders()
        {
            var providersList = ServiceManager.Instance.ProviderService.GetProviderListFullInfo();
            providersList.Sort((a, b) => String.Compare(a.Label, b.Label, StringComparison.Ordinal));
            string un, domain;
            int port;
            VATRPCall.ParseSipAddress(_contactSipUsername, out un, out domain, out port);
            ProviderViewModel selectedProvider = null;
            foreach (var s in providersList)
            {
                if (s.Address == "_nologo")
                    continue;
                var providerModel = new ProviderViewModel(s);
                Providers.Add(providerModel);
                if (s.Address == domain && domain.NotBlank())
                    selectedProvider = providerModel;
                if (App.CurrentAccount != null && s.Address == App.CurrentAccount.ProxyHostname)
                    _currentProvider = providerModel;
            }

            var nlp = ServiceManager.Instance.ProviderService.FindProviderLooseSearch("_nologo");
            if (nlp != null)
            {
                _nologoProvider = new ProviderViewModel(nlp);
            }

            if (_isAddMode)
            {
                SelectedProvider = selectedProvider ?? _currentProvider;
            }
            else
                SelectedProvider = selectedProvider ?? _nologoProvider;
        }

        internal bool ValidateName()
        {
            return !string.IsNullOrWhiteSpace(ContactName);
        }

        internal int ValidateAddress(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 1;
            string un, host;
            int port;
            if (!VATRPCall.ParseSipAddress(input, out un, out host, out port))
                return 2;
            if (!ValidateUsername(un))
                return 3;
            if (!ValidateHost(host))
                return 4;
            if (port < 0 || port > 65535)
                return 5;

            return 0;
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

        private bool ValidateHost(string input)
        {
            // Create a new Regex based on the specified regular expression.
            string regex =
                @"^((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(regex);

            return r.IsMatch(input);
        }

        internal void UpdateContactAddress(bool changeProvider)
        {
            string un, host;
            int port;
            if (VATRPCall.ParseSipAddress(_contactSipUsername, out un, out host, out port))
            {
                if (changeProvider)
                {
                    host = _selectedProvider.Provider.Address;
                }
                else
                {
                    if (host != _selectedProvider.Provider.Address && string.IsNullOrEmpty(host))
                        host = _selectedProvider.Provider.Address;
                }

                ContactSipUsername = port == 0
                    ? String.Format("{0}@{1}", un,
                        host)
                    : String.Format("{0}@{1}:{2}", un,
                        host, port);
            }
        }

        private void LoadContactAvatar()
        {
            bool loadCommon = true;
            if (!string.IsNullOrWhiteSpace(_avatarPath))
            {
                try
                {
                    byte[] data = File.ReadAllBytes(_avatarPath);
                    var source = new BitmapImage();
                    source.BeginInit();
                    source.StreamSource = new MemoryStream(data);
                    source.EndInit();
                    Avatar = source;
                    loadCommon = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to load avatar: " + ex.Message);
                }
            }

            if (loadCommon)
            {
                LoadCommonAvatar();
            }
        }

        private void LoadCommonAvatar()
        {
            var avatarUri = "pack://application:,,,/ACE;component/Resources/male.png";
            try
            {
                Avatar = new BitmapImage(new Uri(avatarUri));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load avatar: " + ex.Message);
            }
        }

        internal void SelectAvatar()
        {
            var openDlg = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Image Files (*.jpg, *.jpeg, *.png, *.bmp)| *.jpg;*.jpeg;*.png;*.bmp",
                FilterIndex = 0,

                ShowReadOnly = false,
            };

            if (openDlg.ShowDialog() != true)
                return;
            _avatarPath = openDlg.FileName;
            _newAvatarPath = _avatarPath;
            LoadContactAvatar();
        }
        
    }
}