using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.ViewModel;
using log4net;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.Views;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(MainWindow));
        private readonly ContactBox _contactBox =  new ContactBox();
        private readonly Dialpad _dialpadBox;
        private readonly CallProcessingBox _callView = new CallProcessingBox();
        private readonly HistoryView _historyView = new HistoryView();
        private readonly KeyPadCtrl _keypadCtrl = new KeyPadCtrl();
        private readonly MediaTextWindow _messagingWindow;
        private CallView _remoteVideoView;
        private SelfView _selfView = new SelfView();
        private readonly SettingsView _settingsView = new SettingsView();
        private readonly CallInfoView _callInfoView = new CallInfoView();
        private readonly CallOverlayView _callOverlayView = new CallOverlayView();
        private readonly ILinphoneService _linphoneService;
        private FlashWindowHelper _flashWindowHelper = new FlashWindowHelper();
        private readonly MainControllerViewModel _mainViewModel;
        private int CombinedUICallViewSize = 700;
        #endregion

        #region Properties
        public static LinphoneRegistrationState RegistrationState { get; set; }
        
        #endregion


        public MainWindow() : base(VATRPWindowType.MAIN_VIEW)
        {
            _mainViewModel = new MainControllerViewModel();

            ServiceManager.Instance.Start();
            _linphoneService = ServiceManager.Instance.LinphoneService;
            _linphoneService.RegistrationStateChangedEvent += OnRegistrationChanged;
            _linphoneService.CallStateChangedEvent += OnCallStateChanged;
            _linphoneService.GlobalStateChangedEvent += OnGlobalStateChanged;
            ServiceManager.Instance.NewAccountRegisteredEvent += OnNewAccountRegistered;
            InitializeComponent();
            DataContext = _mainViewModel;
            ctrlHistory.SetDataContext(_mainViewModel.HistoryModel);
            ctrlContacts.SetDataContext(_mainViewModel.ContactsModel);
            _dialpadBox = new Dialpad(_mainViewModel.DialpadModel);
            _messagingWindow = new MediaTextWindow(_mainViewModel.MessagingModel);
            ctrlDialpad.SetViewModel(_mainViewModel.DialpadModel);
            ctrlLocalContact.SetDataContext(_mainViewModel.ContactModel);
            ctrlCall.ParentViewModel =_mainViewModel;
            _settingsView.SetSettingsModel(_mainViewModel.SettingsModel);
            EnterFullScreenCheckBox.IsEnabled = false;

            ctrlSettings.SetCallControl(ctrlCall);
            ctrlCall.SettingsControl = ctrlSettings;
        }

        private void btnRecents_Click(object sender, RoutedEventArgs e)
        {
            //ToggleWindow(_historyView);
            bool isChecked = BtnRecents.IsChecked ?? false;
            if (isChecked)
            {
                _mainViewModel.IsDialpadDocked = false;
                _mainViewModel.IsContactDocked = false;
                _mainViewModel.IsSettingsDocked = false;
            }
            _mainViewModel.IsCallHistoryDocked = isChecked;
        }

        private void btnContacts_Click(object sender, RoutedEventArgs e)
        {
           // ToggleWindow(_contactBox);
            bool isChecked = BtnContacts.IsChecked ?? false;
            if (isChecked)
            {
                _mainViewModel.IsDialpadDocked = false;
                _mainViewModel.IsCallHistoryDocked = false;
                _mainViewModel.IsSettingsDocked = false;
            }
            _mainViewModel.IsContactDocked = isChecked;
        }

        private void ToggleWindow(VATRPWindow window)
        {
            if (window == null)
                return;
            if ( window.IsVisible)
            {
                window.Hide();
            }
            else
            {
                window.Show();
                window.Activate();
            }
        }

        private void btnDialpad_Click(object sender, RoutedEventArgs e)
        {
            //ToggleWindow(_dialpadBox);
            _mainViewModel.IsDialpadDocked = BtnDialpad.IsChecked ?? false;
        }

        private void btnShowResources(object sender, RoutedEventArgs e)
        {
            ToggleWindow(_messagingWindow);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = BtnSettings.IsChecked ?? false;
            if (isChecked)
            {
                _mainViewModel.IsDialpadDocked = false;
                _mainViewModel.IsCallHistoryDocked = false;
                _mainViewModel.IsContactDocked = false;
            }
            _mainViewModel.IsSettingsDocked = BtnSettings.IsChecked ?? false;
            if (_mainViewModel.IsSettingsDocked)
            {
                ctrlSettings.Initialize();
            }
        }

        private void OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType changeType)
        {
            switch (changeType)
            {
                case Enums.ACEMenuSettingsUpdateType.ClearAccount: ClearAccountAndUpdateUI();
                    break;
                case Enums.ACEMenuSettingsUpdateType.Logout:
                    break;
                case Enums.ACEMenuSettingsUpdateType.RunWizard: RunWizard();
                    break;
                case Enums.ACEMenuSettingsUpdateType.UserNameChanged: UpdateUIForUserNameChange();
                    break;
                case Enums.ACEMenuSettingsUpdateType.RegistrationChanged: HandleRegistrationSettingsChange();
                    break;
                default:
                    break;
            }
        }

        private void UpdateUIForUserNameChange()
        {
        }
        private void HandleRegistrationSettingsChange()
        {
            ServiceManager.Instance.SaveAccountSettings();
            ApplyRegistrationChanges();
        }

        private void RunWizard()
        {
            //            ctrlSettings.Visibility = System.Windows.Visibility.Hidden;            
            //            ServiceSelector.Visibility = System.Windows.Visibility.Visible;
            signOutRequest = true;
            ServiceManager.Instance.ClearAccountInformation();
        }

        private void ClearAccountAndUpdateUI()
        {
            OnResetToDefaultConfiguration();
        }

        private void OnSettingsSaved()
        {
            if (_mainViewModel.SettingsModel.SipSettingsChanged ||
                _mainViewModel.SettingsModel.CodecSettingsChanged ||
                _mainViewModel.SettingsModel.NetworkSettingsChanged ||
                _mainViewModel.SettingsModel.CallSettingsChanged ||
                _mainViewModel.SettingsModel.MediaSettingsChanged)
            {
                ServiceManager.Instance.SaveAccountSettings();
                if (_mainViewModel.SettingsModel.SipSettingsChanged)
                    ApplyRegistrationChanges();
                if (_mainViewModel.SettingsModel.CodecSettingsChanged)
                    ServiceManager.Instance.ApplyCodecChanges();
                if (_mainViewModel.SettingsModel.NetworkSettingsChanged)
                {
                    ServiceManager.Instance.ApplyNetworkingChanges();
                }

                if (_mainViewModel.SettingsModel.CallSettingsChanged)
                {
                    ServiceManager.Instance.ApplyAVPFChanges();
                    ServiceManager.Instance.ApplyDtmfOnSIPInfoChanges();
                }

                if (_mainViewModel.SettingsModel.MediaSettingsChanged)
                {
                    ServiceManager.Instance.ApplyMediaSettingsChanges();
                }
            }
        }

        private void ApplyRegistrationChanges()
        {
            this.registerRequested = true;
            ServiceManager.Instance.UpdateLinphoneConfig();

            if (_mainViewModel.ActiveCallModel != null && _mainViewModel.ActiveCallModel.ActiveCall != null)
            {
                var r = MessageBox.Show("The active call will be terminated. Continue?", "ACE",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (r == MessageBoxResult.OK)
                {
                    _linphoneService.TerminateCall(_mainViewModel.ActiveCallModel.ActiveCall.NativeCallPtr);
                }
                return;
            }

            if (RegistrationState == LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                _linphoneService.Unregister(false);
            }
            _linphoneService.Register();
        }

        private void UpdateMenuSettingsForRegistrationState()
        {
            if (RegistrationState == LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MuteMicrophoneCheckbox.IsChecked = false;
                MuteMicrophoneCheckbox.IsEnabled = false;
            }
            else
            {
                MuteMicrophoneCheckbox.IsEnabled = true;
                if (App.CurrentAccount != null)
                {
                    MuteMicrophoneCheckbox.IsEnabled = App.CurrentAccount.MuteMicrophone;
                }
            }

        }


        private void MQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.AllowDestroyWindows = true;
            registerRequested = false;
            base.Window_Closing(sender, e);
            _mainViewModel.MessagingModel.StopInputProcessor();
            ServiceManager.Instance.Stop();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            ServiceManager.Instance.WaitForServiceCompletion(5);

            _linphoneService.RegistrationStateChangedEvent -= OnRegistrationChanged;
            _linphoneService.CallStateChangedEvent -= OnCallStateChanged;
            _linphoneService.GlobalStateChangedEvent -= OnGlobalStateChanged;
            ServiceManager.Instance.NewAccountRegisteredEvent -= OnNewAccountRegistered;
            
            Application.Current.Shutdown();
        }

        private void Wizard_HandleLogout()
        {
            // in this case we want to show the login page of the wizard without clearing the account
            VATRPAccount currentAccount = App.CurrentAccount;
            if (currentAccount != null)
            {
                ProviderLoginScreen wizardPage = new ProviderLoginScreen(this);
                currentAccount.Password = ""; // clear password for logout
                wizardPage.InitializeToAccount(currentAccount);
                ChangeWizardPage(wizardPage);
            } // else let it go to the front by default to set up a new account with new service selection
        }

        private void OnVideoRelaySelect(object sender, RoutedEventArgs e)
        {
            var wizardPage = new ProviderLoginScreen(this);
            var newAccount = new VATRPAccount {AccountType = VATRPAccountType.VideoRelayService};
            App.CurrentAccount = newAccount;
            ServiceManager.Instance.AccountService.AddAccount(newAccount);
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.ACCOUNT_IN_USE, App.CurrentAccount.AccountID);
            ChangeWizardPage(wizardPage);
        }

        private void ChangeWizardPage(UserControl wizardPage)
        {
            if (wizardPage == null)
            {
                WizardPagepanel.Visibility = Visibility.Collapsed;
                return;
            }
            WizardPagepanel.Children.Clear();

            DockPanel.SetDock(wizardPage, Dock.Top);
            wizardPage.Height = double.NaN;
            wizardPage.Width = double.NaN;

            WizardPagepanel.Children.Add(wizardPage);
            WizardPagepanel.LastChildFill = true;

            ServiceSelector.Visibility = Visibility.Collapsed;
            WizardPagepanel.Visibility = Visibility.Visible;
        }

        private void onIPRelaySelect(object sender, RoutedEventArgs e)
        {
            var wizardPage = new ProviderLoginScreen(this);
            var newAccount = new VATRPAccount { AccountType = VATRPAccountType.IP_Relay };
            App.CurrentAccount = newAccount;
            ServiceManager.Instance.AccountService.AddAccount(newAccount);
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.ACCOUNT_IN_USE, App.CurrentAccount.AccountID);
            ChangeWizardPage(wizardPage);
        }

        private void onIPCTSSelect(object sender, RoutedEventArgs e)
        {
            var wizardPage = new ProviderLoginScreen(this);
            var newAccount = new VATRPAccount { AccountType = VATRPAccountType.IP_CTS };
            App.CurrentAccount = newAccount;
            ServiceManager.Instance.AccountService.AddAccount(newAccount);
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.ACCOUNT_IN_USE, App.CurrentAccount.AccountID);
            ChangeWizardPage(wizardPage);
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            base.Window_Initialized(sender, e);
            if (ServiceManager.Instance.UpdateLinphoneConfig())
            {
                if (ServiceManager.Instance.StartLinphoneService())
                    ServiceManager.Instance.Register();
            }
        }

       private void OnGlobalStateChanged(LinphoneGlobalState state)
        {
            Console.WriteLine("Global State changed: " + state);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _historyView.IsVisibleChanged += OnChildVisibilityChanged;
            _historyView.MakeCallRequested += OnMakeCallRequested;
            _contactBox.IsVisibleChanged += OnChildVisibilityChanged;
            _dialpadBox.IsVisibleChanged += OnChildVisibilityChanged;
            _settingsView.IsVisibleChanged += OnChildVisibilityChanged;
            _messagingWindow.IsVisibleChanged += OnChildVisibilityChanged;
            _settingsView.SettingsSavedEvent += OnSettingsSaved;
            _keypadCtrl.KeypadClicked += OnKeypadClicked;
            _dialpadBox.KeypadClicked += OnDialpadClicked;
            _callInfoView.IsVisibleChanged += OnCallInfoVisibilityChanged;

            ctrlCall.KeypadClicked += OnKeypadClicked;
            ctrlCall.RttToggled += OnRttToggled;
            ctrlCall.SwitchHoldCallsRequested += OnSwitchHoldCallsRequested;

            _callOverlayView.CallManagerView = _callView;
            ctrlHistory.MakeCallRequested += OnMakeCallRequested;
            ctrlContacts.MakeCallRequested += OnMakeCallRequested;
            ctrlCall.KeypadCtrl = _keypadCtrl;
            ctrlDialpad.KeypadPressed += OnDialpadClicked;

            // Liz E. - ToDo unified Settings
            ctrlSettings.AccountChangeRequested += OnAccountChangeRequested;
            //ctrlSettings.SipSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.CodecSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.MultimediaSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.NetworkSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.CallSettingsChangeClicked += OnSettingsChangeRequired;

            if (App.CurrentAccount != null)
            {
                if (!string.IsNullOrEmpty(App.CurrentAccount.ProxyHostname) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationPassword) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationUser) &&
                    App.CurrentAccount.ProxyPort != 0)
                {
                    ServiceSelector.Visibility = Visibility.Collapsed;
                    _mainViewModel.IsAccountLogged = true;
                    _mainViewModel.IsDialpadDocked = true;
                    _mainViewModel.IsCallHistoryDocked = true;
                }
            }
            
            ServiceManager.Instance.UpdateLoggedinContact();
        }

        private void OnRttToggled(bool switch_on)
        {
            _mainViewModel.IsMessagingDocked = switch_on;
        }

        internal void ResetToggleButton(VATRPWindowType wndType)
        {
            switch (wndType)
            {
                case VATRPWindowType.MESSAGE_VIEW:
                    this.BtnResourcesView.IsChecked = false;
                    break;
                case VATRPWindowType.CONTACT_VIEW:
                    this.BtnContacts.IsChecked = false;
                    break;
                case VATRPWindowType.DIALPAD_VIEW:
                    this.BtnDialpad.IsChecked = false;
                    break;
                case VATRPWindowType.RECENTS_VIEW:
                    BtnRecents.IsChecked = false;
                    break;
                default:
                    break;
            }
        }

        private void OnSignOutRequested(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null || signOutRequest)
                return;
            this.signOutRequest = true;
            if (_mainViewModel.ActiveCallModel != null && _mainViewModel.ActiveCallModel.ActiveCall != null)
            {
                var r = MessageBox.Show("The active call will be terminated. Continue?", "ACE",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (r == MessageBoxResult.OK)
                {
                    _linphoneService.TerminateCall(_mainViewModel.ActiveCallModel.ActiveCall.NativeCallPtr);
                }
                return;
            }

            if (RegistrationState == LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                _linphoneService.Unregister(false);
            }
            
		}

        #region Menu Handlers
        private void OnAboutClicked(object sender, RoutedEventArgs e)
        {
            AboutView aboutView = new AboutView();
            aboutView.Show();
        }

        private void OnProvideFeedback(object sender, RoutedEventArgs e)
        {
            var feedbackView = new FeedbackView();
            feedbackView.Show();
        }

        // Video Menu
        private void OnHideWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnEnterFullScreen(object sender, RoutedEventArgs e)
        {
            // I think what we want to do here is to resize the video to full screen
            if (EnterFullScreenCheckBox.IsChecked)
            {
                this.ResizeMode = System.Windows.ResizeMode.CanResize;
                this.MaxHeight = SystemParameters.WorkArea.Height;
                CombinedUICallViewSize = (int)SystemParameters.WorkArea.Width;
                WindowState = WindowState.Maximized;
            }
            else
            {
                this.MaxHeight = 700;
                CombinedUICallViewSize = 700;
                WindowState = System.Windows.WindowState.Normal;
//                this.Height = 700;
//                this.Width = 316;
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
            }
        }

        // Audio Menu
        private void OnAudioMenuItemOpened(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount != null)
            {
                MuteMicrophoneCheckbox.IsEnabled = true;
                MuteMicrophoneCheckbox.IsChecked = App.CurrentAccount.MuteMicrophone;
            }
            else
            {
                MuteMicrophoneCheckbox.IsEnabled = false;
                MuteMicrophoneCheckbox.IsChecked = false;
            }

        }

        private void OnMuteMicrophone(object sender, RoutedEventArgs e)
        {
            bool enabled = MuteMicrophoneCheckbox.IsChecked;
            if (enabled != App.CurrentAccount.MuteMicrophone)
            {
                App.CurrentAccount.MuteMicrophone = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                if (ctrlSettings != null)
                {
                    ctrlSettings.RespondToMenuUpdate(Enums.ACEMenuSettingsUpdateType.MuteMicrophoneMenu);
                }
                if ((ctrlCall != null) && ctrlCall.IsLoaded)
                {
                    ctrlCall.UpdateMuteSettingsIfOpen();
                }
            }
        }

        #endregion
    }
}
