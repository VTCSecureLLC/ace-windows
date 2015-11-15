using System;
using System.Windows;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using System.Collections.ObjectModel;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class MainControllerViewModel : ViewModelBase
    {
        private bool _isAccountLogged;
        private string _appTitle = "ACE";
        private bool _isContactDocked;
        private bool _isDialpadDocked;
        private bool _isHistoryDocked;
        private bool _isSettingsDocked;
        private bool _isMessagingDocked;
        private bool _isCallPanelDocked;

        private DialpadViewModel _dialPadViewModel;
        private CallHistoryViewModel _historyViewModel;
        private LocalContactViewModel _contactViewModel;
        private MessagingViewModel _messageViewModel;
        private SettingsViewModel _settingsViewModel;
        private ObservableCollection<CallViewModel> _callsViewModel;
        private CallViewModel _activeCallViewModel;

        public MainControllerViewModel()
        {
            _isAccountLogged = false;
            _isContactDocked = false;
            _isDialpadDocked = false;
            _isHistoryDocked = false;
            _isMessagingDocked = false;
            _isCallPanelDocked = false;
            _isSettingsDocked = false;
            _dialPadViewModel = new DialpadViewModel();
            _historyViewModel = new CallHistoryViewModel(ServiceManager.Instance.HistoryService, _dialPadViewModel);
            _contactViewModel = new LocalContactViewModel(ServiceManager.Instance.ContactService);
            _messageViewModel = new MessagingViewModel(ServiceManager.Instance.ChatService,
            ServiceManager.Instance.ContactService);
            _settingsViewModel = new SettingsViewModel();
            _callsViewModel = new ObservableCollection<CallViewModel>();
        }

        #region Properties

        public bool IsAccountLogged
        {
            get { return _isAccountLogged; }
            set
            {
                _isAccountLogged = value; 
                OnPropertyChanged("IsAccountLogged");
            }
        }

        public string AppTitle
        {
            get { return _appTitle; }
            set
            {
                _appTitle = value; 
                OnPropertyChanged("AppTitle");
            }
        }

        public bool IsContactDocked
        {
            get { return _isContactDocked; }
            set
            {
                _isContactDocked = value;
                OnPropertyChanged("IsContactDocked");
            }
        }

        public bool IsDialpadDocked
        {
            get { return _isDialpadDocked; }
            set
            {
                _isDialpadDocked = value; 
                OnPropertyChanged("IsDialpadDocked");
            }
        }
        public bool IsCallHistoryDocked
        {
            get { return _isHistoryDocked; }
            set
            {
                _isHistoryDocked = value;
                OnPropertyChanged("IsCallHistoryDocked");
            }
        }

        public bool IsSettingsDocked
        {
            get { return _isSettingsDocked; }
            set
            {
                _isSettingsDocked = value;
                OnPropertyChanged("IsSettingsDocked");
            }
        }
        
        public bool IsMessagingDocked
        {
            get { return _isMessagingDocked; }
            set
            {
                _isMessagingDocked = value;
                OnPropertyChanged("IsMessagingDocked");
            }
        }
        public bool IsCallPanelDocked
        {
            get { return _isCallPanelDocked; }
            set
            {
                _isCallPanelDocked = value;
                OnPropertyChanged("IsCallPanelDocked");
            }
        }

        public DialpadViewModel DialpadModel
        {
            get { return _dialPadViewModel; }
        }

        public CallHistoryViewModel HistoryModel
        {
            get { return _historyViewModel; }
        }

        public LocalContactViewModel ContactModel
        {
            get { return _contactViewModel; }
        }

        public MessagingViewModel MessagingModel
        {
            get { return _messageViewModel; }
        }
        public SettingsViewModel SettingsModel
        {
            get { return _settingsViewModel; }
        }

        public CallViewModel ActiveCallModel
        {
            get { return _activeCallViewModel; }
            set
            {
                _activeCallViewModel = value;
                OnPropertyChanged("ActiveCall");
            }
        }
        #endregion

    }
}