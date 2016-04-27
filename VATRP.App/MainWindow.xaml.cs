﻿using System;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.ViewModel;
using log4net;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.Views;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;
using HockeyApp;
using com.vtcsecure.ace.windows.CustomControls.Resources;
using System.IO;
using VATRP.Core.Events;
using com.vtcsecure.ace.windows.CustomControls.UnifiedSettings;

namespace com.vtcsecure.ace.windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        #region Members
        private static readonly log4net.ILog LOG = LogManager.GetLogger(typeof(MainWindow));
        private readonly ContactBox _contactBox =  new ContactBox();
        private readonly Dialpad _dialpadBox;
        private readonly CallProcessingBox _callView = new CallProcessingBox();
        private readonly HistoryView _historyView = new HistoryView();
        private readonly KeyPadCtrl _keypadCtrl = new KeyPadCtrl();
        private readonly MediaTextWindow _messagingWindow;
        private CallView _remoteVideoView;
        private SelfView _selfView = new SelfView();
        private readonly SettingsView _settingsView = new SettingsView();
        private SettingsWindow _settingsWindow;
        private readonly CallInfoView _callInfoView = new CallInfoView();
        private readonly CallOverlayView _callOverlayView = new CallOverlayView();
        private readonly ILinphoneService _linphoneService;
        private FlashWindowHelper _flashWindowHelper = new FlashWindowHelper();
        private readonly MainControllerViewModel _mainViewModel;
        private Size CombinedUICallViewSize = new Size(700, 700);
        private Point _lastWindowPosition;
        private bool _playRegisterNotify = true;
        private bool _playRegistrationFailureNotify = true;
        private readonly DispatcherTimer deferredHideTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(2000),
        };
        private readonly DispatcherTimer deferredShowPreviewTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(500),
        };
        #endregion

        #region Properties
        public static LinphoneRegistrationState RegistrationState { get; set; }
        public static LinphoneReason RegistrationFailReason { get; set; }
        public bool IsSlidingDialpad { get; set; }

        public bool IsSlidingMenu { get; set; }

        #endregion


        public MainWindow() : base(VATRPWindowType.MAIN_VIEW)
        {
            _mainViewModel = new MainControllerViewModel();
            _mainViewModel.ActivateWizardPage = true;
            _mainViewModel.OfferServiceSelection = false;

            _linphoneService = ServiceManager.Instance.LinphoneService;
            _linphoneService.RegistrationStateChangedEvent += OnRegistrationChanged;
            _linphoneService.CallStateChangedEvent += OnCallStateChanged;
            _linphoneService.GlobalStateChangedEvent += OnGlobalStateChanged;
            _linphoneService.NetworkReachableEvent += OnLinphoneConnectivityChanged;

            ServiceManager.Instance.NewAccountRegisteredEvent += OnNewAccountRegistered;
            ServiceManager.Instance.LinphoneCoreStartedEvent += OnLinphoneCoreStarted;
            ServiceManager.Instance.LinphoneCoreStoppedEvent += OnLinphoneCoreStopped;
            InitializeComponent();
            DataContext = _mainViewModel;
            ctrlHistory.SetDataContext(_mainViewModel.HistoryModel);
            ctrlContacts.SetDataContext(_mainViewModel.ContactsModel);
            _dialpadBox = new Dialpad(_mainViewModel.DialpadModel);
            _messagingWindow = new MediaTextWindow(_mainViewModel.SipSimpleMessagingModel);
            ctrlDialpad.SetViewModel(_mainViewModel.DialpadModel);
            ctrlLocalContact.SetDataContext(_mainViewModel.ContactModel);
            ctrlCall.ParentViewModel =_mainViewModel;
            ctrlMoreMenu.SetDataContext(_mainViewModel.MoreMenuModel);
            //_settingsView.SetSettingsModel(_mainViewModel.SettingsModel);
            
            EnterFullScreenCheckBox.IsEnabled = false;

//            _settingsWindow = new SettingsWindow(ctrlCall, OnAccountChangeRequested);
//            ctrlSettings.SetCallControl(ctrlCall);
//            ctrlCall.SettingsControl = ctrlSettings;
            deferredHideTimer.Tick += DeferedHideOnError;
            deferredShowPreviewTimer.Tick += DeferredShowPreview;
            CombinedUICallViewSize.Width = 700;
            CombinedUICallViewSize.Height = 700;
        }

        private void btnRecents_Click(object sender, RoutedEventArgs e)
        {
            CloseMeunAnimated();
            bool isChecked = BtnRecents.IsChecked ?? false;
            if (isChecked)
            {
                CloseDialpadAnimated();
                _mainViewModel.IsContactDocked = false;
                _mainViewModel.IsSettingsDocked = false;
                _mainViewModel.IsResourceDocked = false;
                _mainViewModel.IsMenuDocked = false;
                _mainViewModel.HistoryModel.ResetLastMissedCallTime();
                _mainViewModel.UIMissedCallsCount = 0;
            }
            _mainViewModel.IsCallHistoryDocked = isChecked;
        }

        private void btnContacts_Click(object sender, RoutedEventArgs e)
        {
            CloseMeunAnimated();
            bool isChecked = BtnContacts.IsChecked ?? false;
            if (isChecked)
            {
                CloseDialpadAnimated();
                _mainViewModel.IsCallHistoryDocked = false;
                _mainViewModel.IsSettingsDocked = false;
                _mainViewModel.IsResourceDocked = false;
                _mainViewModel.IsMenuDocked = false;
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

        private void OnVideoMailClicked(object sender, EventArgs e)
        {
            CloseMeunAnimated();
            if (App.CurrentAccount != null)
            {
                App.CurrentAccount.VideoMailCount = 0;
                _mainViewModel.ShowVideomailIndicator = false;
                if (_mainViewModel.ContactModel != null)
                    _mainViewModel.ContactModel.VideoMailCount = App.CurrentAccount.VideoMailCount;
                if (_mainViewModel.MoreMenuModel != null)
                    _mainViewModel.MoreMenuModel.VideoMailCount = App.CurrentAccount.VideoMailCount;

                ServiceManager.Instance.AccountService.Save();
            }
        }

        private void OnSelfViewClicked(object sender, EventArgs e)
        {
            CloseMeunAnimated();
            if (_selfView != null)
            {
                bool enabled = _mainViewModel.MoreMenuModel.IsSelfViewActive;
                ShowSelfPreview(enabled);
                ShowSelfPreviewItem.IsChecked = enabled;
            }
            else
            {
                this.ShowSelfPreviewItem.IsChecked = false;
                _mainViewModel.MoreMenuModel.IsSelfViewActive = false;
            }
        }

        private void OnShowSettings(object sender, EventArgs e)
        {
            CloseMeunAnimated();
//            CloseDialpadAnimated();
//            if (_mainViewModel.IsSettingsDocked)
//                return;
//            _mainViewModel.IsCallHistoryDocked = false;
//            _mainViewModel.IsContactDocked = false;
//            _mainViewModel.IsResourceDocked = false;

//           _mainViewModel.IsMenuDocked = true;
//            _mainViewModel.IsSettingsDocked = true;
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow(ctrlCall, OnAccountChangeRequested);
            }
            _settingsWindow.Show();
        }

        private void btnDialpad_Click(object sender, RoutedEventArgs e)
        {
            //ToggleWindow(_dialpadBox);
            _mainViewModel.IsDialpadDocked = BtnDialpad.IsChecked ?? false;
            if (_mainViewModel.IsDialpadDocked)
                OpenDialpadAnimated();
            else
                CloseDialpadAnimated();
        }

        private void btnShowResources(object sender, EventArgs e)
        {
            CloseDialpadAnimated();
            CloseMeunAnimated();
            _mainViewModel.IsCallHistoryDocked = false;
            _mainViewModel.IsContactDocked = false;
            _mainViewModel.IsSettingsDocked = false;
            ctrlResource.ActivateDeafHohResource();
            _mainViewModel.IsResourceDocked = true;
            _mainViewModel.IsMenuDocked = true;
        }

        private void btnShowChatClicked(object sender, RoutedEventArgs e)
        {
            if (_messagingWindow != null)
            {
                bool enabled = BtnChatView.IsChecked ?? false;
                ActivateChatWindow(enabled);
                ShowMessagingViewItem.IsChecked = enabled;
            }
            else
            {
                this.ShowMessagingViewItem.IsChecked = false;
                _mainViewModel.IsChatViewEnabled = false;
            }
        }

        private void btnMoreMenuClicked(object sender, RoutedEventArgs e)
        {
            OpenMenuAnimated();
            _mainViewModel.ShowVideomailIndicator = false;
            if (_mainViewModel.IsSettingsDocked || _mainViewModel.IsResourceDocked)
                _mainViewModel.IsMenuDocked = true;
        }

        private void OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType changeType)
        {
            switch (changeType)
            {
                case Enums.ACEMenuSettingsUpdateType.ClearSettings: ClearSettingsAndUpdateUI();
                    break;
                case Enums.ACEMenuSettingsUpdateType.Logout:
                    break;
                case Enums.ACEMenuSettingsUpdateType.RunWizard: RunWizard();
                    break;
                case Enums.ACEMenuSettingsUpdateType.UserNameChanged: UpdateUIForUserNameChange();
                    break;
                case Enums.ACEMenuSettingsUpdateType.VideoPolicyChanged: UpdateVideoPolicy();
                    break;
                case Enums.ACEMenuSettingsUpdateType.RegistrationChanged: HandleRegistrationSettingsChange();
                    break;
                case Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged: HandleNetworkSettingsChange();
                    break;
                case Enums.ACEMenuSettingsUpdateType.ShowSelfViewChanged: HandleShowSelfViewChanged();
                    break;
                case Enums.ACEMenuSettingsUpdateType.AdvancedSettingsChanged: HandleAdvancedSettingsChange();
                    break;
                case Enums.ACEMenuSettingsUpdateType.CardDavConfigChanged: HandleCardDAVSettingsChange();
                    break;
                default:
                    break;
            }
        }

        private void HandleShowSelfViewChanged()
        {
            if (App.CurrentAccount != null)
            {
                SelfViewItem.IsChecked = App.CurrentAccount.ShowSelfView;
            }
        }
        private void UpdateUIForUserNameChange()
        {
        }

        private void HandleNetworkSettingsChange()
        {
            ServiceManager.Instance.SaveAccountSettings();
            ServiceManager.Instance.ApplyNetworkingChanges();
        }

        private void HandleAdvancedSettingsChange()
        {
            ServiceManager.Instance.SaveAccountSettings();
            ServiceManager.Instance.AdvancedSettings();
        }
        
        private void HandleRegistrationSettingsChange()
        {
            ServiceManager.Instance.SaveAccountSettings();
            ApplyRegistrationChanges();
        }

        private void HandleCardDAVSettingsChange()
        {
            ServiceManager.Instance.SaveAccountSettings();
            ApplyCardDAVChanges();
        }

        private void UpdateVideoPolicy()
        {
            if (App.CurrentAccount != null)
            {
                _linphoneService.EnableVideo(App.CurrentAccount.EnableVideo, App.CurrentAccount.VideoAutomaticallyStart, App.CurrentAccount.VideoAutomaticallyAccept);
            }
        }
        private void RunWizard()
        {
            signOutRequest = true;
            ServiceManager.Instance.ClearAccountInformation();
        }

        private void ClearSettingsAndUpdateUI()
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
                    ServiceManager.Instance.ApplyDtmfInbandChanges();
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
                    _linphoneService.TerminateCall(_mainViewModel.ActiveCallModel.ActiveCall.NativeCallPtr, "Call ended");
                }
                return;
            }

            if (RegistrationState == LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                _linphoneService.Unregister(false);
            }
            else
            {
                // Liz E. - We do want this else here to prevent registration from being called twice. 
                //  If we call unregister above, then after the app has finished unregistering, it will use the 
                //  register requested flag to call Register. Otherwise, go on and call Register here. This
                //  mechanism was previously put in place as a lock to make sure that we move through the states properly
                //  before calling register.
                _linphoneService.Register();
            }
        }

        private void ApplyCardDAVChanges()
        {
            ServiceManager.Instance.UpdateLinphoneConfig();
            ServiceManager.Instance.LinphoneService.CardDAVSync();
        }

        private void UpdateMenuSettingsForRegistrationState()
        {
            if (RegistrationState != LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MuteMicrophoneCheckbox.IsChecked = false;
                MuteMicrophoneCheckbox.IsEnabled = false;
                SelfViewItem.IsEnabled = false;
                SelfViewItem.IsChecked = false;
            }
            else
            {
                if (RegistrationState != LinphoneRegistrationState.LinphoneRegistrationCleared)
                {
                    MyAccountMenuItem.IsEnabled = true;
                }
                MuteMicrophoneCheckbox.IsEnabled = true;
                SelfViewItem.IsEnabled = true;
                if (App.CurrentAccount != null)
                {
                    MuteMicrophoneCheckbox.IsChecked = App.CurrentAccount.MuteMicrophone;
                    SelfViewItem.IsChecked = App.CurrentAccount.ShowSelfView;
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
            _mainViewModel.RttMessagingModel.StopInputProcessor();
            ServiceManager.Instance.LinphoneCoreStoppedEvent -= OnLinphoneCoreStopped;
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
                ServiceManager.Instance.AccountService.Save();
                wizardPage.InitializeToAccount(currentAccount);
                ChangeWizardPage(wizardPage);
            } // else let it go to the front by default to set up a new account with new service selection
        }

        private void OnVideoRelaySelect(object sender, RoutedEventArgs e)
        {
            var wizardPage = new ProviderLoginScreen(this);
            var newAccount = new VATRPAccount {AccountType = VATRPAccountType.VideoRelayService};
            App.CurrentAccount = newAccount;
            
            ChangeWizardPage(wizardPage);
        }

        private void ChangeWizardPage(UserControl wizardPage)
        {
            if (wizardPage == null)
            {
                _mainViewModel.ActivateWizardPage = false;
                _mainViewModel.OfferServiceSelection = true;
                return;
            }
            WizardPagepanel.Children.Clear();

            DockPanel.SetDock(wizardPage, Dock.Top);
            wizardPage.Height = double.NaN;
            wizardPage.Width = double.NaN;

            WizardPagepanel.Children.Add(wizardPage);
            WizardPagepanel.LastChildFill = true;

            _mainViewModel.ActivateWizardPage = true;
            _mainViewModel.OfferServiceSelection = false;
        }

        private void onIPRelaySelect(object sender, RoutedEventArgs e)
        {
            var wizardPage = new ProviderLoginScreen(this);
            var newAccount = new VATRPAccount { AccountType = VATRPAccountType.IP_Relay };
            App.CurrentAccount = newAccount;
            
            ChangeWizardPage(wizardPage);
        }

        private void onIPCTSSelect(object sender, RoutedEventArgs e)
        {
            var wizardPage = new ProviderLoginScreen(this);
            var newAccount = new VATRPAccount { AccountType = VATRPAccountType.IP_CTS };
            App.CurrentAccount = newAccount;
            ChangeWizardPage(wizardPage);
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            base.Window_Initialized(sender, e);
        }

        public void InitializeMainWindow()
        {
            // this method does not take into account the two checkboxes that allow the user to select whether or not to
            //   remember the password and whether or not to automatically login.

            ServiceManager.Instance.UpdateLoggedinContact();

            if (App.CurrentAccount == null || !App.CurrentAccount.Username.NotBlank())
            {
                if (_mainViewModel.ActivateWizardPage)
                    OnVideoRelaySelect(this, null);
            }
            else
            {
                bool autoLogin = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.AUTO_LOGIN, false);
                // if autologin && account exists, try to load the password. 
                if (autoLogin && (App.CurrentAccount != null))
                {
                    App.CurrentAccount.ReadPassword(ServiceManager.Instance.GetPWFile());
                }
               
                if (!autoLogin || string.IsNullOrEmpty(App.CurrentAccount.Password))
                {
                    var wizardPage = new ProviderLoginScreen(this);
                    wizardPage.InitializeToAccount(App.CurrentAccount);

                    ChangeWizardPage(wizardPage);
                }
                else
                {
                    // if the user has selected not to log in automatically, make sure that we move to the 
                    ServiceManager.Instance.StartupLinphoneCore();
                }
            }
        }

        private void OnGlobalStateChanged(LinphoneGlobalState state)
        {
            Console.WriteLine("Global State changed: " + state);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // VATRP-901
            EventManager.RegisterClassHandler(typeof(Window),
     Keyboard.KeyUpEvent, new KeyEventHandler(OnAppKeyUp), true);

            _historyView.IsVisibleChanged += OnChildVisibilityChanged;
            _historyView.MakeCallRequested += OnMakeCallRequested;
            _contactBox.IsVisibleChanged += OnChildVisibilityChanged;
            _dialpadBox.IsVisibleChanged += OnChildVisibilityChanged;
            _messagingWindow.MakeCallRequested += OnMakeCallRequested;

            //_settingsView.IsVisibleChanged += OnChildVisibilityChanged;
            _messagingWindow.IsVisibleChanged += OnChildVisibilityChanged;
            _selfView.IsVisibleChanged += OnChildVisibilityChanged;
            //_settingsView.SettingsSavedEvent += OnSettingsSaved;
            _keypadCtrl.KeypadClicked += OnKeypadClicked;
            _dialpadBox.KeypadClicked += OnDialpadClicked;
            _callInfoView.IsVisibleChanged += OnCallInfoVisibilityChanged;

            ctrlMoreMenu.ResourceClicked += btnShowResources;
            ctrlMoreMenu.SettingsClicked += OnShowSettings;
            ctrlMoreMenu.SelfViewClicked += OnSelfViewClicked;
            ctrlMoreMenu.VideoMailClicked += OnVideoMailClicked;
            ctrlLocalContact.VideomailCountReset += OnVideoMailClicked;

            ctrlCall.KeypadClicked += OnKeypadClicked;
            ctrlCall.RttToggled += OnRttToggled;
            ctrlCall.FullScreenOnToggled += OnFullScreenToggled;
            ctrlCall.SwitchHoldCallsRequested += OnSwitchHoldCallsRequested;
            ctrlCall.VideoOnToggled += OnCameraSwitched;
            ctrlCall.HideDeclineMessageRequested += OnHideDeclineMessage;

            _callOverlayView.CallManagerView = _callView;
            ctrlHistory.MakeCallRequested += OnMakeCallRequested;
            ctrlContacts.MakeCallRequested += OnMakeCallRequested;
            ctrlDialpad.MakeCallRequested += OnMakeCallRequested;
            ctrlCall.KeypadCtrl = _keypadCtrl;
            ctrlDialpad.KeypadPressed += OnDialpadClicked;
            _mainViewModel.DialpadHeight = ctrlDialpad.ActualHeight;

            _mainViewModel.RttMessagingModel.RttReceived += OnRttReceived;
            _mainViewModel.SipSimpleMessagingModel.DeclineMessageReceived += OnDeclineMessageReceived;
            ServiceManager.Instance.LinphoneService.OnMWIReceivedEvent += OnVideoMailCountChanged;

            // Liz E. - ToDo unified Settings
//            ctrlSettings.AccountChangeRequested += OnAccountChangeRequested;
            //ctrlSettings.SipSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.CodecSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.MultimediaSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.NetworkSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.CallSettingsChangeClicked += OnSettingsChangeRequired;

            ctrlResource.CallResourceRequested += OnCallResourceRequested;
            ctrlLocalContact.CallResourceRequested += OnCallResourceRequested;

            // reset provider selection in dialpad
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.CURRENT_PROVIDER, "");
            VATRPAccount account = App.CurrentAccount;
            bool autoLogin = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.AUTO_LOGIN, false);
            if (autoLogin && (App.CurrentAccount != null))
            {
                App.CurrentAccount.ReadPassword(ServiceManager.Instance.GetPWFile());
            }
            if ((App.CurrentAccount != null) && autoLogin && !string.IsNullOrEmpty(App.CurrentAccount.Password))
            {
                if (!string.IsNullOrEmpty(App.CurrentAccount.ProxyHostname) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationPassword) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationUser) &&
                    App.CurrentAccount.ProxyPort != 0)
                {
                    _mainViewModel.OfferServiceSelection = false;
                    _mainViewModel.IsAccountLogged = true;
                    // VATRP-1899: This is a quick and dirty solution for POC. It will be functional, but not the end implementation we will want.
                    if (!App.CurrentAccount.UserNeedsAgentView)
                    {
                        OpenDialpadAnimated();
                        UpdateVideomailCount();
                        _mainViewModel.IsCallHistoryDocked = true;
                        _mainViewModel.DialpadModel.UpdateProvider();
                        SetToUserAgentView(false);
                        RearrangeUICallView(GetCallViewSize());
                    }
                    else
                    {
                        SetToUserAgentView(true);
                    }
                }
            }
        }

        private Size GetCallViewSize()
        {
            Size dimensions = new Size(700,700);
            if (_mainViewModel.IsInCallFullScreen)
            {
                System.Windows.Forms.Screen currentScreen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                dimensions.Width = currentScreen.Bounds.Width;
                dimensions.Height = currentScreen.Bounds.Height;
            }
            return dimensions;
        }

        private void OnFullScreenToggled(bool switch_on)
        {
            if (_mainViewModel.IsInCallFullScreen == switch_on)
                return;

            _mainViewModel.IsInCallFullScreen = switch_on;
            WindowState = WindowState.Normal;

            Size szDimensions = GetCallViewSize();
            if (switch_on)
            {
                System.Windows.Forms.Screen currentScreen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                _lastWindowPosition.X = this.Left;
                _lastWindowPosition.Y = this.Top;
              
                this.CallViewBorder.BorderThickness = new Thickness(0);

                this.ResizeMode = System.Windows.ResizeMode.NoResize;
                this.SizeToContent = SizeToContent.Manual;
                this.WindowStyle = WindowStyle.None;
                this.MaxHeight = szDimensions.Height;
                this.MaxWidth = szDimensions.Width;
                this.Height = szDimensions.Height;
                this.Width = szDimensions.Width;
                this.Left = currentScreen.Bounds.Left;
                this.Top = currentScreen.Bounds.Top;

                CombinedUICallViewSize.Height = this.Height;
                CombinedUICallViewSize.Width = this.Width - (_mainViewModel.IsMessagingDocked ? ctrlRTT.ActualWidth : 0);
                this.MessageViewBorder.BorderThickness = _mainViewModel.IsMessagingDocked
                    ? new Thickness(1, 0, 0, 0)
                    : new Thickness(0);
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.MaxHeight = 700;
                this.MaxWidth = Double.PositiveInfinity;
                
                CombinedUICallViewSize.Width = szDimensions.Width;
                CombinedUICallViewSize.Height = szDimensions.Height;
                this.SizeToContent = SizeToContent.WidthAndHeight;
                WindowState = System.Windows.WindowState.Normal;
                this.ResizeMode = System.Windows.ResizeMode.CanMinimize;
                this.Left = _lastWindowPosition.X;
                this.Top = _lastWindowPosition.Y;
                this.CallViewBorder.BorderThickness = new Thickness(1, 0, 1, 0);
                if (_mainViewModel.IsMessagingDocked)
                {
                    this.MessageViewBorder.BorderThickness = new Thickness(1, 0, 0, 0);
                }
            }
            RearrangeUICallView(szDimensions);
            Activate();
        }

        private void RearrangeUICallView(Size callViewDimensions)
        {
            Point topleftInScreen = new Point(0, 0);
            try
            {
                int offset = 0;
                if (!_mainViewModel.IsInCallFullScreen)
                {
                    topleftInScreen = ctrlCall.PointToScreen(new Point(0, 0));
                    topleftInScreen = this.PointFromScreen(topleftInScreen);
                    topleftInScreen.Y += SystemParameters.CaptionHeight;
                    offset = 8;
                }

                if (_mainViewModel.IsMessagingDocked && _mainViewModel.IsInCallFullScreen)
                {
                    callViewDimensions.Width -= ctrlRTT.ActualWidth;
                }

                CombinedUICallViewSize = callViewDimensions;

                ctrlCall.Width = callViewDimensions.Width;

                ctrlCall.ctrlOverlay.Width = callViewDimensions.Width;
                ctrlCall.ctrlOverlay.Height = callViewDimensions.Height;

                if (_mainViewModel.ActiveCallModel != null)
                {
                    _mainViewModel.ActiveCallModel.VideoWidth = (int) CombinedUICallViewSize.Width;
                    _mainViewModel.ActiveCallModel.VideoHeight = (int) ctrlCall.ActualHeight;
                }

                ctrlCall.ctrlOverlay.CommandWindowLeftMargin = topleftInScreen.X +
                                                               (callViewDimensions.Width -
                                                                ctrlCall.ctrlOverlay.CommandOverlayWidth)/2 + offset;
                ctrlCall.ctrlOverlay.CommandWindowTopMargin = topleftInScreen.Y +
                                                              (ctrlCall.ActualHeight - 40 -
                                                               ctrlCall.ctrlOverlay.CommandOverlayHeight);

                ctrlCall.ctrlOverlay.NumpadWindowLeftMargin = topleftInScreen.X +
                                                              (callViewDimensions.Width -
                                                               ctrlCall.ctrlOverlay.NumpadOverlayWidth)/2 + offset;
                ctrlCall.ctrlOverlay.NumpadWindowTopMargin = ctrlCall.ctrlOverlay.CommandWindowTopMargin -
                                                             ctrlCall.ctrlOverlay.NumpadOverlayHeight;

                ctrlCall.ctrlOverlay.CallInfoOverlayWidth = (int) callViewDimensions.Width - 30;
                ctrlCall.ctrlOverlay.CallInfoWindowLeftMargin = topleftInScreen.X +
                                                                (callViewDimensions.Width -
                                                                 ctrlCall.ctrlOverlay.CallInfoOverlayWidth)/2 + offset;
                ctrlCall.ctrlOverlay.CallInfoWindowTopMargin = topleftInScreen.Y + 40;

                ctrlCall.ctrlOverlay.CallsSwitchWindowLeftMargin = topleftInScreen.X + 10;
                ctrlCall.ctrlOverlay.CallsSwitchWindowTopMargin = topleftInScreen.Y + 10;

                ctrlCall.ctrlOverlay.NewCallAcceptWindowLeftMargin = topleftInScreen.X +
                                                                     (callViewDimensions.Width -
                                                                      ctrlCall.ctrlOverlay.NewCallAcceptOverlayWidth)/2 +
                                                                     offset;
                ctrlCall.ctrlOverlay.NewCallAcceptWindowTopMargin = topleftInScreen.Y +
                                                                    (ctrlCall.ActualHeight -
                                                                     ctrlCall.ctrlOverlay.NewCallAcceptOverlayHeight)/2;

                ctrlCall.ctrlOverlay.OnHoldOverlayWidth = 100; // (int)callViewDimensions.Width - 30;
                ctrlCall.ctrlOverlay.OnHoldWindowLeftMargin = topleftInScreen.X +
                                                              (callViewDimensions.Width -
                                                               ctrlCall.ctrlOverlay.OnHoldOverlayWidth)/2 + offset;
                ctrlCall.ctrlOverlay.OnHoldWindowTopMargin = ctrlCall.ctrlOverlay.CallInfoOverlayHeight +
                                                             ctrlCall.ctrlOverlay.CallInfoWindowTopMargin + 40;
                // topleftInScreen.Y + 40;

                ctrlCall.ctrlOverlay.QualityIndicatorWindowLeftMargin = topleftInScreen.X + offset + 20;
                ctrlCall.ctrlOverlay.QualityIndicatorWindowTopMargin = topleftInScreen.Y +
                                                                       (ctrlCall.ActualHeight - 20 -
                                                                        ctrlCall.ctrlOverlay
                                                                            .QualityIndicatorOverlayHeight);

                ctrlCall.ctrlOverlay.ShowQualityIndicatorWindow(false);
                ctrlCall.ctrlOverlay.Refresh();
                if (_mainViewModel.ActiveCallModel != null &&
                    _mainViewModel.ActiveCallModel.CallState != VATRPCallState.Closed)
                    ctrlCall.ctrlOverlay.ShowQualityIndicatorWindow(this.WindowState != WindowState.Minimized);

                ctrlCall.ctrlOverlay.EncryptionIndicatorWindowLeftMargin = topleftInScreen.X +
                                                                callViewDimensions.Width - 40 + offset;
                ctrlCall.ctrlOverlay.EncryptionIndicatorWindowTopMargin = topleftInScreen.Y + 20;

                ctrlCall.ctrlOverlay.ShowEncryptionIndicatorWindow(false);
                ctrlCall.ctrlOverlay.Refresh();

                if (_mainViewModel.ActiveCallModel == null) return;

                if (_mainViewModel.ActiveCallModel.CallState != VATRPCallState.Closed &&
                    _mainViewModel.ActiveCallModel.CallState != VATRPCallState.Declined &&
                    _mainViewModel.ActiveCallModel.CallState != VATRPCallState.Error)
                {
                    ctrlCall.ctrlOverlay.ShowEncryptionIndicatorWindow(this.WindowState != WindowState.Minimized);

                    // ToDo VATRP - 3878
                    //if (_mainViewModel.ActiveCallModel.ShowInfoMessage)
                    //{
                    //    ctrlCall.ctrlOverlay.InfoMsgWindowLeftMargin = topleftInScreen.X +
                    //                          (callViewDimensions.Width -
                    //                           ctrlCall.ctrlOverlay.InfoMsgOverlayWidth) / 2 + offset;
                    //    ctrlCall.ctrlOverlay.InfoMsgWindowTopMargin = ctrlCall.ctrlOverlay.CommandWindowTopMargin -
                    //                                                 ctrlCall.ctrlOverlay.InfoMsgOverlayHeight - 30;

                    //    ctrlCall.ctrlOverlay.ShowInfoMsgWindow(this.WindowState != WindowState.Minimized);
                    //}
                    ctrlCall.ctrlOverlay.Refresh();
                }
            }
            catch (Exception ex)
            {
                LOG.Error("RearrangeUICallView", ex);
            }
        }

        private void OnCameraSwitched(bool switch_on)
        {
            if (_mainViewModel.ActiveCallModel != null && _mainViewModel.ActiveCallModel.ActiveCall != null)
                ServiceManager.Instance.LinphoneService.SendCameraSwtichAsInfo(_mainViewModel.ActiveCallModel.ActiveCall.NativeCallPtr, switch_on);
        }

        private void OnRttToggled(bool switch_on)
        {
            _mainViewModel.IsMessagingDocked = switch_on;
            ShowRTTView.IsChecked = switch_on;
            this.MessageViewBorder.BorderThickness = _mainViewModel.IsMessagingDocked
                ? new Thickness(1, 0, 0, 0)
                : new Thickness(0);
            if (_mainViewModel.IsInCallFullScreen)
            {
                RearrangeUICallView(GetCallViewSize());
            }
        }

        private void OnRttReceived(object sender, EventArgs e)
        {
            IntPtr callPtr = (IntPtr)sender;

            if (!_mainViewModel.IsMessagingDocked)
            {
                if (_mainViewModel.ActiveCallModel != null &&
                    _mainViewModel.ActiveCallModel.ActiveCall != null &&
                     callPtr == _mainViewModel.ActiveCallModel.ActiveCall.NativeCallPtr)
                {
                    ctrlCall.CheckRttButton();
                    OnRttToggled(true);
                }
            }
        }
        
        private void OnVideoMailCountChanged(MWIEventArgs args)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.OnVideoMailCountChanged(args)));
                return;
            }

            if (App.CurrentAccount != null)
            {
                if (args != null) 
                    App.CurrentAccount.VideoMailCount += args.MwiCount;
                UpdateVideomailCount();
                ServiceManager.Instance.AccountService.Save();
            }
        }

        private void OnSignOutRequested(object sender, RoutedEventArgs e)
        {
            if (signOutRequest || RegistrationState == LinphoneRegistrationState.LinphoneRegistrationProgress)
            {
                MessageBox.Show("Account registration is in progress. Please wait.", "ACE",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (App.CurrentAccount == null)
                return;

            if (_mainViewModel.ActiveCallModel != null && _mainViewModel.ActiveCallModel.ActiveCall != null)
            {
                var r = MessageBox.Show("The active call will be terminated. Continue?", "ACE",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (r == MessageBoxResult.OK)
                {
                    _linphoneService.TerminateCall(_mainViewModel.ActiveCallModel.ActiveCall.NativeCallPtr, "Call ended");
                }
            }

            _mainViewModel.ContactModel.RegistrationState = LinphoneRegistrationState.LinphoneRegistrationFailed;

            signOutRequest = true;
            // remove the password file - the user is manually signing out, do not remember the password despite autologin in this case
            string pwFile = ServiceManager.Instance.GetPWFile();
            try
            {
                if (!string.IsNullOrEmpty(pwFile))
                {
                    if (File.Exists(pwFile))
                    {
                        File.Delete(pwFile);
                    }
                }
            }
            catch (Exception ex)
            {
                // I think it is ok for this to be silent, but log it
                LOG.Info("MainWindow.OnSignOutRequested: unabled to check file existance or remove file - filename:" + pwFile + " Details: " + ex.Message);
            }
            switch (RegistrationState)
            {
                case LinphoneRegistrationState.LinphoneRegistrationProgress:
                    MessageBox.Show("Account registration is in progress. Please wait.", "ACE", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    break;
                case LinphoneRegistrationState.LinphoneRegistrationOk:
                {
                    _linphoneService.Unregister(false);
                }
                    break;
                case LinphoneRegistrationState.LinphoneRegistrationCleared:
                case LinphoneRegistrationState.LinphoneRegistrationFailed:
                case LinphoneRegistrationState.LinphoneRegistrationNone:
                {
                    signOutRequest = false;
                    WizardPagepanel.Children.Clear();
                    _mainViewModel.OfferServiceSelection = false;
                    _mainViewModel.ActivateWizardPage = true;

                    _mainViewModel.IsAccountLogged = false;
                    CloseDialpadAnimated();
                    _mainViewModel.IsCallHistoryDocked = false;
                    _mainViewModel.IsContactDocked = false;
                    _mainViewModel.IsMessagingDocked = false;
                    
                    this.Wizard_HandleLogout();
                }
                    break;
            }
        }

        #region Menu Handlers
        private void OnAboutClicked(object sender, RoutedEventArgs e)
        {
            AboutView aboutView = new AboutView();
            aboutView.Show();
        }

        private void OnMyAccount(object sender, RoutedEventArgs e)
        {
//            CloseDialpadAnimated();
//            _mainViewModel.IsCallHistoryDocked = false;
//            _mainViewModel.IsContactDocked = false;
//            _mainViewModel.IsResourceDocked = false;
//            _mainViewModel.IsSettingsDocked = true;
//            com.vtcsecure.ace.windows.CustomControls.UnifiedSettings.SettingsWindow settingsWindow = new CustomControls.UnifiedSettings.SettingsWindow();
//            settingsWindow.Show();
        }
        private void OnGoToSupport(object sender, RoutedEventArgs e)
        {
            var feedbackView = new FeedbackView();
            feedbackView.Show();
        }

        private void OnProvideFeedback(object sender, RoutedEventArgs e)
        {
            var feedbackView = new FeedbackView();
            feedbackView.Show();
        }

        private async void OnCheckForUpdates(object sender, RoutedEventArgs e)
        {
            // Liz E. - not entirely certain this check works - putting it into the build to test it, but I believe it should already be being called
            //   on launch. This gives us a place to manually click to check. If it is not working for Windows, then we can make use of this API to 
            //   create out own check:
            // http://support.hockeyapp.net/kb/api/api-versions
            // ToDo VATRP-1057: When we have a publishable version, that is where we should be checking for updates, not hockeyapp.s
            //check for updates on the HockeyApp server
            try
            {
                await HockeyClient.Current.CheckForUpdatesAsync(true, () =>
                {
                    if (Application.Current.MainWindow != null)
                    {
                        Application.Current.MainWindow.Close();
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                LOG.Error("OnCheckUpdates", ex);
            }
        }

        private void OnSyncContacts(object sender, RoutedEventArgs e)
        {
            // T.M  VATRP - 3664 Moved from resources page
            ServiceManager.Instance.LinphoneService.CardDAVSync();
        }

        // View Menu
        private void OnMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Video Menu
        private void OnHideWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnShowSelfView(object sender, RoutedEventArgs e)
        {
            bool enabled = this.SelfViewItem.IsChecked;
            if (enabled != App.CurrentAccount.ShowSelfView)
            {
                ShowSelfView(enabled);
            }
        }

        private void ShowSelfView(bool enabled)
        {
            App.CurrentAccount.ShowSelfView = enabled;
            ServiceManager.Instance.ApplyMediaSettingsChanges();
            ServiceManager.Instance.SaveAccountSettings();
            if (_settingsWindow != null)
            {
                _settingsWindow.RespondToMenuUpdate(Enums.ACEMenuSettingsUpdateType.ShowSelfViewMenu);
            }
//            if (ctrlSettings != null)
//            {
//                ctrlSettings.RespondToMenuUpdate(Enums.ACEMenuSettingsUpdateType.ShowSelfViewMenu);
//            }
        }

        private void OnShowPreviewWindow(object sender, RoutedEventArgs e)
        {
            if (_selfView != null)
            {
                bool enabled = this.ShowSelfPreviewItem.IsChecked;
                ShowSelfPreview(enabled);
                _mainViewModel.MoreMenuModel.IsSelfViewActive = enabled;
            }
            else
            {
                this.ShowSelfPreviewItem.IsChecked = false;
                _mainViewModel.MoreMenuModel.IsSelfViewActive = false;
            }
        }

        private void ShowSelfPreview(bool enabled)
        {
            try
            {
                if (!enabled)
                {
                    _selfView.Hide();
                }
                else if (!_selfView.IsVisible)
                {
                    _selfView.Show();
                    _selfView.Activate();
                }
            }
            catch
            {
                
            }
        }

        private void OnShowMessagingWindow(object sender, RoutedEventArgs e)
        {
            if (_messagingWindow != null)
            {
                bool enabled = this.ShowMessagingViewItem.IsChecked;
                ActivateChatWindow(enabled);
                _mainViewModel.IsChatViewEnabled = enabled;
            }
            else
            {
                this.ShowMessagingViewItem.IsChecked = false;
                _mainViewModel.IsChatViewEnabled = false;
            }
        }

        private void ActivateChatWindow(bool enabled)
        {
            try
            {
                if (!enabled)
                {
                    _messagingWindow.Hide();
                }
                else if (!_messagingWindow.IsVisible)
                {
                    _messagingWindow.Show();
                    _messagingWindow.Activate();
                }
            }
            catch
            {
                
            }
        }

        private void OnShowRTTView(object sender, RoutedEventArgs e)
        {
            bool enabled = this.ShowRTTView.IsChecked;
            if (enabled != ctrlCall.IsRTTViewShown())
            {
                ctrlCall.UpdateRTTToggle(enabled);
                _mainViewModel.IsMessagingDocked = enabled;
            }
        }

        private void OnEnterFullScreen(object sender, RoutedEventArgs e)
        {
            // I think what we want to do here is to resize the video to full screen
            if (EnterFullScreenCheckBox.IsChecked)
            {
                this.ResizeMode = System.Windows.ResizeMode.CanResize;
                this.MaxHeight = SystemParameters.WorkArea.Height;
                CombinedUICallViewSize.Width = SystemParameters.WorkArea.Width;
                CombinedUICallViewSize.Height = SystemParameters.WorkArea.Height;
                WindowState = WindowState.Maximized;
            }
            else
            {
                this.MaxHeight = 700;
                CombinedUICallViewSize.Width = 700;
                CombinedUICallViewSize.Height = 700;

                WindowState = System.Windows.WindowState.Normal;
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
            }
        }

        // Audio Menu
        private void OnAudioMenuItemOpened(object sender, RoutedEventArgs e)
        {
            if (RegistrationState == LinphoneRegistrationState.LinphoneRegistrationOk)
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
            if (App.CurrentAccount == null)
                return;
            bool enabled = MuteMicrophoneCheckbox.IsChecked;
            if (enabled != App.CurrentAccount.MuteMicrophone)
            {
                App.CurrentAccount.MuteMicrophone = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

//                if (ctrlSettings != null)
                if (_settingsWindow != null)
                {
                    _settingsWindow.RespondToMenuUpdate(Enums.ACEMenuSettingsUpdateType.MuteMicrophoneMenu);
                }
                if ((ctrlCall != null) && ctrlCall.IsLoaded)
                {
                    ctrlCall.UpdateMuteSettingsIfOpen();
                }
            }
        }

        #endregion

        private void OnCallResourceRequested(ResourceInfo resourceInfo)
        {
            if ((resourceInfo != null) && !string.IsNullOrEmpty(resourceInfo.address))
            {
                MediaActionHandler.MakeVideoCall(resourceInfo.address);
            }
        }

        private void OnAppKeyUp(object sender, KeyEventArgs e)
        {
            if (_mainViewModel == null)
                return;
            if (_linphoneService != null && ( _mainViewModel.ActiveCallModel != null && 
                                             _mainViewModel.ActiveCallModel.CallState == VATRPCallState.InProgress ))
            {
                if (e.Key == Key.Enter && !e.IsRepeat)
                {
                    if (_linphoneService.GetActiveCallsCount == 1)
                    {
                        // Accept incoming call
                        ctrlCall.AcceptCall(this, null);
                    }
                    else if (_linphoneService.GetActiveCallsCount == 2)
                    {
                        // Hold/Accept incoming call
                        ctrlCall.HoldAndAcceptCall(this, null);
                    }
                }
            }
            
            if (e.Key == Key.Escape &&  _mainViewModel.IsInCallFullScreen)
            {
               ctrlCall.ExitFullScreen();
            }
        }

        private void SlideDownCompleted(object sender, EventArgs e)
        {
            IsSlidingDialpad = false;
            _mainViewModel.DialpadHeight = 1;
        }

        private void SlideUpCompleted(object sender, EventArgs e)
        {
            IsSlidingDialpad = false;
            _mainViewModel.DialpadHeight = ctrlDialpad.ActualHeight;
        }

        public void OpenDialpadAnimated()
        {
            if (IsSlidingDialpad )
                return;
            CloseMeunAnimated();
            IsSlidingDialpad = true;
            _mainViewModel.DialpadHeight = 1;
            _mainViewModel.IsDialpadDocked = true;
            var s = (Storyboard)Resources["SlideUpAnimation"];

            if (s != null)
            {
                s.Begin();
            }
        }

        public void CloseDialpadAnimated()
        {
            if (IsSlidingDialpad )
                return;
            IsSlidingDialpad = true;
            _mainViewModel.DialpadHeight = 1;
            _mainViewModel.IsDialpadDocked = false;
            var s = (Storyboard)Resources["SlideDownAnimation"];
            if (s != null) s.Begin();
        }

        #region Menu Animation
        public void OpenMenuAnimated()
        {
            CloseDialpadAnimated();
            if (IsSlidingMenu)
                return;
            IsSlidingMenu = true;
            var s = (Storyboard)Resources["SlideMenuUpAnimation"];

            if (s != null)
            {
                s.Begin();
            }
        }

        public void CloseMeunAnimated()
        {
            if (IsSlidingMenu)
                return;
            IsSlidingMenu = true;
            var s = (Storyboard)Resources["SlideMenuDownAnimation"];
            if (s != null) s.Begin();
        }

        private void SlideMenuUpCompleted(object sender, EventArgs e)
        {
            IsSlidingMenu = false;
        }

        private void SlideMenuDownCompleted(object sender, EventArgs e)
        {
            IsSlidingMenu = false;
        }
        
        #endregion
    }
}
