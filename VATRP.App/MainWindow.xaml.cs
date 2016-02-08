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
        private readonly CallInfoView _callInfoView = new CallInfoView();
        private readonly CallOverlayView _callOverlayView = new CallOverlayView();
        private readonly ILinphoneService _linphoneService;
        private FlashWindowHelper _flashWindowHelper = new FlashWindowHelper();
        private readonly MainControllerViewModel _mainViewModel;
        private Size CombinedUICallViewSize = new Size(700, 700);
        private Point _lastWindowPosition;
        private bool _playRegisterNotify = true;
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
            ServiceManager.Instance.NewAccountRegisteredEvent += OnNewAccountRegistered;
            ServiceManager.Instance.LinphoneCoreStartedEvent += OnLinphoneCoreStarted;
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
            deferredHideTimer.Tick += DeferedHideOnError;
            deferredShowPreviewTimer.Tick += DeferredShowPreview;
            CombinedUICallViewSize.Width = 700;
            CombinedUICallViewSize.Height = 700;
        }

        private void btnRecents_Click(object sender, RoutedEventArgs e)
        {
            //ToggleWindow(_historyView);
            bool isChecked = BtnRecents.IsChecked ?? false;
            if (isChecked)
            {
                CloseAnimated();
                _mainViewModel.IsContactDocked = false;
                _mainViewModel.IsSettingsDocked = false;
                _mainViewModel.IsResourceDocked = false;
                _mainViewModel.HistoryModel.ResetLastMissedCallTime();
                _mainViewModel.UIMissedCallsCount = 0;
            }
            _mainViewModel.IsCallHistoryDocked = isChecked;
        }

        private void btnContacts_Click(object sender, RoutedEventArgs e)
        {
           // ToggleWindow(_contactBox);
            bool isChecked = BtnContacts.IsChecked ?? false;
            if (isChecked)
            {
                CloseAnimated();
                _mainViewModel.IsCallHistoryDocked = false;
                _mainViewModel.IsSettingsDocked = false;
                _mainViewModel.IsResourceDocked = false;
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
            if (_mainViewModel.IsDialpadDocked)
                OpenAnimated();
            else
                CloseAnimated();
        }

        private void btnShowResources(object sender, RoutedEventArgs e)
        {
            bool isChecked = BtnResourcesView.IsChecked ?? false;
            if (isChecked)
            {
                CloseAnimated();
                _mainViewModel.IsCallHistoryDocked = false;
                _mainViewModel.IsContactDocked = false;
                _mainViewModel.IsSettingsDocked = false;
            }
            _mainViewModel.IsResourceDocked = isChecked;// BtnSettings.IsChecked ?? false;
            // VATRP 856
            // ToggleWindow(_messagingWindow);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = BtnSettings.IsChecked ?? false;
            if (isChecked)
            {
                CloseAnimated();
                _mainViewModel.IsCallHistoryDocked = false;
                _mainViewModel.IsContactDocked = false;
                _mainViewModel.IsResourceDocked = false;
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
                case Enums.ACEMenuSettingsUpdateType.VideoPolicyChanged: UpdateVideoPolicy();
                    break;
                case Enums.ACEMenuSettingsUpdateType.RegistrationChanged: HandleRegistrationSettingsChange();
                    break;
                case Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged: HandleNetworkSettingsChange();
                    break;
                case Enums.ACEMenuSettingsUpdateType.ShowSelfViewChanged: HandleShowSelfViewChanged();
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
        private void HandleRegistrationSettingsChange()
        {
            ServiceManager.Instance.SaveAccountSettings();
            ApplyRegistrationChanges();
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
            else
            {
                // Liz E. - We do want this else here to prevent registration from being called twice. 
                //  If we call unregister above, then after the app has finished unregistering, it will use the 
                //  register requested flag to call Register. Otherwise, go on and call Register here. This
                //  mechanism was previously put in place as a lock to make sure that we move through the states properly
                //  befor calling register.
                _linphoneService.Register();
            }
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
                if (!App.CurrentAccount.AutoLogin || string.IsNullOrEmpty(App.CurrentAccount.Password))
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
            _settingsView.IsVisibleChanged += OnChildVisibilityChanged;
            _messagingWindow.IsVisibleChanged += OnChildVisibilityChanged;
            _selfView.IsVisibleChanged += OnChildVisibilityChanged;
            _settingsView.SettingsSavedEvent += OnSettingsSaved;
            _keypadCtrl.KeypadClicked += OnKeypadClicked;
            _dialpadBox.KeypadClicked += OnDialpadClicked;
            _callInfoView.IsVisibleChanged += OnCallInfoVisibilityChanged;

            ctrlCall.KeypadClicked += OnKeypadClicked;
            ctrlCall.RttToggled += OnRttToggled;
            ctrlCall.FullScreenOnToggled += OnFullScreenToggled;
            ctrlCall.SwitchHoldCallsRequested += OnSwitchHoldCallsRequested;
            ctrlCall.VideoOnToggled += OnCameraSwitched;

            _callOverlayView.CallManagerView = _callView;
            ctrlHistory.MakeCallRequested += OnMakeCallRequested;
            ctrlContacts.MakeCallRequested += OnMakeCallRequested;
            ctrlCall.KeypadCtrl = _keypadCtrl;
            ctrlDialpad.KeypadPressed += OnDialpadClicked;
            _mainViewModel.DialpadHeight = ctrlDialpad.ActualHeight;

            _mainViewModel.MessagingModel.RttReceived += OnRttReceived;

            // Liz E. - ToDo unified Settings
            ctrlSettings.AccountChangeRequested += OnAccountChangeRequested;
            //ctrlSettings.SipSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.CodecSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.MultimediaSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.NetworkSettingsChangeClicked += OnSettingsChangeRequired;
            //ctrlSettings.CallSettingsChangeClicked += OnSettingsChangeRequired;

            ctrlResource.CallResourceRequested += OnCallResourceRequested;
            ctrlLocalContact.CallResourceRequested += OnCallResourceRequested;

            if ((App.CurrentAccount != null) && App.CurrentAccount.AutoLogin && App.CurrentAccount.Password.NotBlank())
            {
                if (!string.IsNullOrEmpty(App.CurrentAccount.ProxyHostname) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationPassword) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationUser) &&
                    App.CurrentAccount.ProxyPort != 0)
                {
                    _mainViewModel.OfferServiceSelection = false;
                    _mainViewModel.IsAccountLogged = true;
                    _mainViewModel.ContactModel.VideoMailCount = App.CurrentAccount.VideoMailCount;
                    // VATRP-1899: This is a quick and dirty solution for POC. It will be functional, but not the end implementation we will want.
                    if (!App.CurrentAccount.UserNeedsAgentView)
                    {
                        OpenAnimated();
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
        }

        private void RearrangeUICallView(Size callViewDimensions)
        {
            Point topleftInScreen = new Point(0, 0);

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
                _mainViewModel.ActiveCallModel.VideoWidth = (int)CombinedUICallViewSize.Width;
                _mainViewModel.ActiveCallModel.VideoHeight = (int)ctrlCall.ActualHeight;
            }

            ctrlCall.ctrlOverlay.CommandWindowLeftMargin = topleftInScreen.X + (callViewDimensions.Width - ctrlCall.ctrlOverlay.CommandOverlayWidth) / 2 + offset;
            ctrlCall.ctrlOverlay.CommandWindowTopMargin = topleftInScreen.Y + (ctrlCall.ActualHeight - 40 - ctrlCall.ctrlOverlay.CommandOverlayHeight);

            ctrlCall.ctrlOverlay.NumpadWindowLeftMargin = topleftInScreen.X + (callViewDimensions.Width - ctrlCall.ctrlOverlay.NumpadOverlayWidth) / 2 + offset;
            ctrlCall.ctrlOverlay.NumpadWindowTopMargin = ctrlCall.ctrlOverlay.CommandWindowTopMargin - ctrlCall.ctrlOverlay.NumpadOverlayHeight;

            ctrlCall.ctrlOverlay.CallInfoOverlayWidth = (int)callViewDimensions.Width - 30;
            ctrlCall.ctrlOverlay.CallInfoWindowLeftMargin = topleftInScreen.X + (callViewDimensions.Width - ctrlCall.ctrlOverlay.CallInfoOverlayWidth) / 2 + offset;
            ctrlCall.ctrlOverlay.CallInfoWindowTopMargin = topleftInScreen.Y + 40;

            ctrlCall.ctrlOverlay.CallsSwitchWindowLeftMargin = topleftInScreen.X + 10;
            ctrlCall.ctrlOverlay.CallsSwitchWindowTopMargin = topleftInScreen.Y + 10;

            ctrlCall.ctrlOverlay.NewCallAcceptWindowLeftMargin = topleftInScreen.X + (callViewDimensions.Width - ctrlCall.ctrlOverlay.NewCallAcceptOverlayWidth) / 2 + offset;
            ctrlCall.ctrlOverlay.NewCallAcceptWindowTopMargin = topleftInScreen.Y + (ctrlCall.ActualHeight - ctrlCall.ctrlOverlay.NewCallAcceptOverlayHeight) / 2;

            ctrlCall.ctrlOverlay.OnHoldOverlayWidth = 100;// (int)callViewDimensions.Width - 30;
            ctrlCall.ctrlOverlay.OnHoldWindowLeftMargin = topleftInScreen.X + (callViewDimensions.Width - ctrlCall.ctrlOverlay.OnHoldOverlayWidth) / 2 + offset;
            ctrlCall.ctrlOverlay.OnHoldWindowTopMargin = ctrlCall.ctrlOverlay.CallInfoOverlayHeight + ctrlCall.ctrlOverlay.CallInfoWindowTopMargin + 40;// topleftInScreen.Y + 40;

            ctrlCall.ctrlOverlay.Refresh();
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
            if (!_mainViewModel.IsMessagingDocked)
            {
                ctrlCall.CheckRttButton();
                OnRttToggled(true);
            }
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
            if (signOutRequest)
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
                    _linphoneService.TerminateCall(_mainViewModel.ActiveCallModel.ActiveCall.NativeCallPtr);
                }
            }

            signOutRequest = true;

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
                {
                    signOutRequest = false;
                    WizardPagepanel.Children.Clear();
                    _mainViewModel.OfferServiceSelection = false;
                    _mainViewModel.ActivateWizardPage = true;

                    _mainViewModel.IsAccountLogged = false;
                    CloseAnimated();
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
            CloseAnimated();
            _mainViewModel.IsCallHistoryDocked = false;
            _mainViewModel.IsContactDocked = false;
            _mainViewModel.IsResourceDocked = false;
            _mainViewModel.IsSettingsDocked = true;

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
            await HockeyClient.Current.CheckForUpdatesAsync(true, () =>
            {
                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                }
                return true;
            }); 
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
                App.CurrentAccount.ShowSelfView = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                if (ctrlSettings != null)
                {
                    ctrlSettings.RespondToMenuUpdate(Enums.ACEMenuSettingsUpdateType.ShowSelfViewMenu);
                }
            }
        }

        private void OnShowPreviewWindow(object sender, RoutedEventArgs e)
        {
            if (_selfView != null)
            {
                bool enabled = this.ShowSelfPreviewItem.IsChecked;
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
            else
            {
                this.ShowSelfPreviewItem.IsChecked = false;
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
            if (App.CurrentAccount == null)
                return;
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

        private void OnCallResourceRequested(ResourceInfo resourceInfo)
        {
            if ((resourceInfo != null) && !string.IsNullOrEmpty(resourceInfo.address))
            {
                MediaActionHandler.MakeVideoCall(resourceInfo.address);
            }
        }

        private void OnAppKeyUp(object sender, KeyEventArgs e)
        {
            if (_linphoneService != null && (_mainViewModel != null && _mainViewModel.ActiveCallModel != null && 
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
        }

        public bool IsSliding { get; set; }
		
        private void SlideDownCompleted(object sender, EventArgs e)
        {
            IsSliding = false;
            _mainViewModel.DialpadHeight = 1;
        }

        private void SlideUpCompleted(object sender, EventArgs e)
        {
            IsSliding = false;
            _mainViewModel.DialpadHeight = ctrlDialpad.ActualHeight;
        }

        public void OpenAnimated()
        {
            if (IsSliding )
                return;
            IsSliding = true;
            _mainViewModel.DialpadHeight = 1;
            _mainViewModel.IsDialpadDocked = true;
            var s = (Storyboard)Resources["SlideUpAnimation"];

            if (s != null)
            {
                s.Begin();
            }
        }

        public void CloseAnimated()
        {
            if (IsSliding )
                return;
            IsSliding = true;
            _mainViewModel.DialpadHeight = 1;
            _mainViewModel.IsDialpadDocked = false;
            var s = (Storyboard)Resources["SlideDownAnimation"];
            if (s != null) s.Begin();
        }
    }
}
