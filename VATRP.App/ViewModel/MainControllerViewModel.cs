using System;
using System.Windows;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using System.Collections.ObjectModel;
using log4net;
using VATRP.Core.Interfaces;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class MainControllerViewModel : ViewModelBase
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (MainControllerViewModel));
        private bool _isAccountLogged;
        private string _appTitle = "ACE";
        private bool _isContactDocked;
        private bool _isDialpadDocked;
        private bool _isHistoryDocked;
        private bool _isSettingsDocked;
        private bool _isResourceDocked;
        private bool _isMessagingDocked;
        private bool _isCallPanelDocked;
        private bool _isIncallFullScreen;
        private bool _offerServiceSelection;
        private bool _activateWizardPage;
        private double _dialpadHeight;

        private DialpadViewModel _dialPadViewModel;
        private CallHistoryViewModel _historyViewModel;
        private LocalContactViewModel _contactViewModel;
        private MessagingViewModel _messageViewModel;
        private SettingsViewModel _settingsViewModel;
        private ObservableCollection<CallViewModel> _callsViewModelList;
        private CallViewModel _activeCallViewModel;
        private ContactsViewModel _contactsViewModel;
        private ILinphoneService _linphoneService;
        private int _uiMissedCallsCount;


        public MainControllerViewModel()
        {
            _isAccountLogged = false;
            _isContactDocked = false;
            _isDialpadDocked = false;
            _isHistoryDocked = false;
            _isMessagingDocked = false;
            _isCallPanelDocked = false;
            _isSettingsDocked = false;
            _isResourceDocked = false;
            _offerServiceSelection = false;
            _activateWizardPage = false; 
            _dialPadViewModel = new DialpadViewModel();
            _historyViewModel = new CallHistoryViewModel(ServiceManager.Instance.HistoryService, _dialPadViewModel);
            _contactsViewModel = new ContactsViewModel(ServiceManager.Instance.ContactService, _dialPadViewModel);
            _contactViewModel = new LocalContactViewModel(ServiceManager.Instance.ContactService);
            _messageViewModel = new MessagingViewModel(ServiceManager.Instance.ChatService,
                ServiceManager.Instance.ContactService);
            _settingsViewModel = new SettingsViewModel();
            _historyViewModel.MissedCallsCountChanged += OnMissedCallsCountChanged;
            _callsViewModelList = new ObservableCollection<CallViewModel>();
            _linphoneService = ServiceManager.Instance.LinphoneService;
            _dialpadHeight = 350;
        }

        private void OnMissedCallsCountChanged(object callEvent, EventArgs args)
        {
            if (_historyViewModel != null)
            {
                if (IsCallHistoryDocked)
                {
                    _historyViewModel.ResetLastMissedCallTime();
                }
                UIMissedCallsCount = _historyViewModel.UnseenMissedCallsCount;
            }
        }

        #region Properties

        public bool IsAccountLogged
        {
            get { return _isAccountLogged; }
            set
            {
                _isAccountLogged = value;
                OnPropertyChanged("IsAccountLogged");
                OnPropertyChanged("IsDashboardDocked");
            }
        }

        public bool IsInCallFullScreen
        {
            get { return _isIncallFullScreen; }
            set
            {
                _isIncallFullScreen = value;
                OnPropertyChanged("IsInCallFullScreen");
                OnPropertyChanged("IsDashboardDocked");
                OnPropertyChanged("IsCallPanelBorderVisible");
            }
        }

        public bool IsDashboardDocked
        {
            get { return _isAccountLogged && !_isIncallFullScreen; }
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

        public bool IsResourceDocked
        {
            get { return _isResourceDocked; }
            set
            {
                _isResourceDocked = value;
                OnPropertyChanged("IsResourceDocked");
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

        public bool OfferServiceSelection
        {
            get { return _offerServiceSelection; }
            set
            {
                _offerServiceSelection = value;
                OnPropertyChanged("OfferServiceSelection");
            }
        }

        public bool ActivateWizardPage
        {
            get
            {
                return _activateWizardPage; 
            }
            set
            {
                _activateWizardPage = value;
                OnPropertyChanged("ActivateWizardPage");
            }
        }

        public int UIMissedCallsCount
        {
            get { return _uiMissedCallsCount; }
            set
            {
                _uiMissedCallsCount = value;
                OnPropertyChanged("UIMissedCallsCount");
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

        public ContactsViewModel ContactsModel
        {
            get { return _contactsViewModel; }
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
                if (_activeCallViewModel != null)
                {
                    if (_activeCallViewModel.ActiveCall != null)
                        ServiceManager.Instance.ActiveCallPtr = _activeCallViewModel.ActiveCall.NativeCallPtr;
                    else
                        ServiceManager.Instance.ActiveCallPtr = IntPtr.Zero;
                }
                else
                    ServiceManager.Instance.ActiveCallPtr = IntPtr.Zero;
                OnPropertyChanged("ActiveCall");
            }
        }

        public ObservableCollection<CallViewModel> CallsViewModelList
        {
            get { return _callsViewModelList ?? (_callsViewModelList = new ObservableCollection<CallViewModel>()); }
            set
            {
                _callsViewModelList = value;
                OnPropertyChanged("CallsViewModelList");
            }
        }

        public double DialpadHeight
        {
            get { return _dialpadHeight; }
            set
            {
                _dialpadHeight = value; 
                OnPropertyChanged("DialpadHeight");
            }
        }

        #endregion

        #region Calls management
        
        internal CallViewModel FindCallViewModel(VATRPCall call)
        {
            if (call == null)
                return null;
            lock (CallsViewModelList)
            {
                foreach (var callVM in CallsViewModelList)
                {
                    if (callVM.Equals(call))
                        return callVM;
                }
            }
            return null;
        }

        internal void AddCalViewModel(CallViewModel callViewModel)
        {
            if (FindCallViewModel(callViewModel))
                return;

            lock (CallsViewModelList)
            {
                CallsViewModelList.Add(callViewModel);
                OnPropertyChanged("CallsViewModelList");
            }
        }

        internal int RemoveCalViewModel(CallViewModel callViewModel)
        {
            lock (CallsViewModelList)
            {
                CallsViewModelList.Remove(callViewModel);
                OnPropertyChanged("CallsViewModelList");
                return CallsViewModelList.Count;
            }
        }

        internal CallViewModel GetNextViewModel(CallViewModel skipVM)
        {
            lock (CallsViewModelList)
            {
                foreach (var callVM in CallsViewModelList)
                {
                    if (!callVM.Equals(skipVM))
                        return callVM;
                }
            }
            return null;
        }

        internal bool FindCallViewModel(CallViewModel callViewModel)
        {
            lock (CallsViewModelList)
            {
                foreach (var callVM in CallsViewModelList)
                {
                    if (callVM.Equals(callViewModel))
                        return true;
                }
            }
            return false;
        }

        internal void TerminateCall(CallViewModel viewModel)
        {
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    LOG.Info(String.Format("Terminating call call for {0}. {1}", viewModel.CallerInfo,
                        viewModel.ActiveCall.NativeCallPtr));

                    try
                    {
                        _linphoneService.TerminateCall(viewModel.ActiveCall.NativeCallPtr);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("TerminateCall", ex);
                    }
                }
            }
        }

        internal void AcceptCall(CallViewModel viewModel)
        {
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    LOG.Info(String.Format("Accepting call call for {0}. {1}", viewModel.CallerInfo,
                        viewModel.ActiveCall.NativeCallPtr));

                    viewModel.AcceptCall();
                    bool muteMicrophone = false;
                    bool muteSpeaker = false;
                    bool enableVideo = true;
                    if (App.CurrentAccount != null)
                    {
                        muteMicrophone = App.CurrentAccount.MuteMicrophone;
                        muteSpeaker = App.CurrentAccount.MuteSpeaker;
                        enableVideo = App.CurrentAccount.EnableVideo;
                    }
                    try
                    {
                        _linphoneService.AcceptCall(viewModel.ActiveCall.NativeCallPtr,
                            ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                                Configuration.ConfEntry.USE_RTT, true), muteMicrophone, muteSpeaker, enableVideo);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("AcceptCall", ex);
                    }
                }
            }
        }

        internal void DeclineCall(CallViewModel viewModel)
        {
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    LOG.Info(String.Format("Declining call for {0}. {1}", viewModel.CallerInfo,
                        viewModel.ActiveCall.NativeCallPtr));

                    viewModel.DeclineCall(false);
                    try
                    {
                        _linphoneService.DeclineCall(viewModel.ActiveCall.NativeCallPtr);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("DeclineCall", ex);
                    }
                }
            }
        }

        internal void EndAndAcceptCall(CallViewModel viewModel)
        {
            lock (CallsViewModelList)
            {
                CallViewModel nextCall = GetNextViewModel(viewModel);
                if (nextCall != null)
                {
                    TerminateCall(nextCall);

                    AcceptCall(viewModel);
                }
            }
        }

        #endregion

        internal void ResumeCall(CallViewModel viewModel)
        {
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    LOG.Info(String.Format("Resuming call for {0}. {1}", viewModel.CallerInfo,
                        viewModel.ActiveCall.NativeCallPtr));

                    viewModel.ResumeCall();
                }
            }
        }

        internal void PauseCall(CallViewModel viewModel)
        {
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    LOG.Info(String.Format("Pausing call for {0}. {1}", viewModel.CallerInfo,
                        viewModel.ActiveCall.NativeCallPtr));
                    viewModel.PauseCall();
                }
            }
        }

        internal bool SwitchCall(CallViewModel pausedCallViewModel, CallViewModel activeCallViewModel)
        {
            LOG.Info(String.Format("Switching call. Main call {0}. {1}. Secondary call {2} {3}",
                activeCallViewModel.CallerInfo,
                activeCallViewModel.ActiveCall.NativeCallPtr, pausedCallViewModel.CallerInfo,
                pausedCallViewModel.ActiveCall.NativeCallPtr));

            if (activeCallViewModel.CallState == VATRPCallState.LocalPaused && activeCallViewModel.PauseRequest)
            {
                if (pausedCallViewModel.CallState == VATRPCallState.LocalPaused && pausedCallViewModel.PauseRequest)
                    return false;
				ResumeCall(pausedCallViewModel);
            }
            else
            {
                PauseCall(activeCallViewModel);
            }
            return true;
        }
    }
}