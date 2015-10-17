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
using log4net;
using VATRP.App.CustomControls;
using VATRP.App.Model;
using VATRP.App.Services;
using VATRP.App.Views;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(MainWindow));
        private readonly ContactBox _contactBox =  new ContactBox();
        private readonly Dialpad _dialpadBox = new Dialpad();
        private readonly CallProcessingBox _callView = new CallProcessingBox();
        private readonly HistoryView _historyView = new HistoryView();
        private readonly KeyPadCtrl _keypadCtrl = new KeyPadCtrl();
        private CallView _remoteVideoView;
        private SelfView _selfView = new SelfView();
        private readonly SettingsView _settingsView = new SettingsView();
        private readonly CallInfoView _callInfoView = new CallInfoView();
        private readonly LinphoneService _linphoneService;
        private FlashWindowHelper _flashWindowHelper = new FlashWindowHelper();
        #endregion

        #region Properties
        public static LinphoneRegistrationState RegistrationState { get; set; }
        #endregion

        public event HistoryView.MakeCallRequestedDelegate MakeCallRequestedEvent;

        public MainWindow() : base(VATRPWindowType.MAIN_VIEW)
        {
            DataContext = this;
            ServiceManager.Instance.Start();
            _linphoneService = ServiceManager.Instance.LinphoneSipService;
            _linphoneService.RegistrationStateChangedEvent += OnRegistrationChanged;
            _linphoneService.CallStateChangedEvent += OnCallStateChanged;
            _linphoneService.GlobalStateChangedEvent += OnGlobalStateChanged;
            _linphoneService.ErrorEvent += OnErrorEvent;
            ServiceManager.Instance.NewAccountRegisteredEvent += OnNewAccountRegistered;
            InitializeComponent();

        }

        private void btnRecents_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindow(_historyView);
        }

        private void btnContacts_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindow(_contactBox);
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
            }
        }

        private void btnDialpad_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindow(_dialpadBox);
        }

        private void btnVideoMail_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindow(_selfView);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindow(_settingsView);
        }

        private void OnSettingsSaved()
        {
            if (_settingsView.SipSettingsChanged ||
                _settingsView.CodecSettingsChanged ||
                _settingsView.NetworkSettingsChanged ||
                _settingsView.MediaSettingsChanged)
            {
                ServiceManager.Instance.SaveAccountSettings();
                if (_settingsView.SipSettingsChanged)
                    ApplyRegsitrationChanges();
                if (_settingsView.CodecSettingsChanged)
                    ServiceManager.Instance.ApplyCodecChanges();
                if (_settingsView.NetworkSettingsChanged)
                {
                    ServiceManager.Instance.ApplyNetworkingChanges();
                    ServiceManager.Instance.ApplyAVPFChanges();
                }

                if (_settingsView.MediaSettingsChanged)
                {
                    ServiceManager.Instance.ApplyVideoSizeChanges();
                }
            }
        }

        private void ApplyRegsitrationChanges()
        {
            this.registerRequested = true;
            RegUserLabel.Text = string.Format("Account: {0}", App.CurrentAccount.RegistrationUser);
            ServiceManager.Instance.UpdateLinphoneConfig();

            if (_callView.ActiveCall != null)
            {
                var r = MessageBox.Show("The active call will be terminated. Continue?", "ACE",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (r == MessageBoxResult.OK)
                {
                    _linphoneService.TerminateCall(_callView.ActiveCall.NativeCallPtr);
                }
                return;
            }
            if (RegistrationState == LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                _linphoneService.Unregister(false);
            }
            else
            {
                _linphoneService.Register();
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.AllowDestroyWindows = true;
            CloseAllWindows();
            base.Window_Closing(sender, e);
        }

        private void CloseAllWindows()
        {
            
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _linphoneService.RegistrationStateChangedEvent -= OnRegistrationChanged;
            _linphoneService.CallStateChangedEvent -= OnCallStateChanged;
            _linphoneService.GlobalStateChangedEvent -= OnGlobalStateChanged;
            ServiceManager.Instance.NewAccountRegisteredEvent -= OnNewAccountRegistered;

            ServiceManager.Instance.Stop();
            Application.Current.Shutdown();
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

        private void OnErrorEvent(VATRPCall call, string message)
        {
            Console.WriteLine("Error occurred: " + message);
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
            _selfView.IsVisibleChanged += OnChildVisibilityChanged;
            _settingsView.SettingsSavedEvent += OnSettingsSaved;
            _keypadCtrl.KeypadClicked += OnKeypadClicked;
            _dialpadBox.KeypadClicked += OnDialpadClicked;

            _callView.KeypadCtrl = _keypadCtrl;
            _callView.CallInfoCtrl = _callInfoView;

            App.CurrentAccount = ServiceManager.Instance.LoadActiveAccount();
            bool hideNavigation = true;
            if (App.CurrentAccount != null)
            {
                if (!string.IsNullOrEmpty(App.CurrentAccount.ProxyHostname) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationPassword) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationUser) &&
                    App.CurrentAccount.ProxyPort != 0)
                {
                    ServiceSelector.Visibility = Visibility.Collapsed;
                    RegUserLabel.Text = string.Format("Account: {0}", App.CurrentAccount.RegistrationUser);
                    hideNavigation = false;
                }
            }
            
            if (hideNavigation)
            {
                NavPanel.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }
        }

        internal void ResetToggleButton(VATRPWindowType wndType)
        {
            switch (wndType)
            {
                case VATRPWindowType.CALL_VIEW:
                    this.BtnCallView.IsChecked = false;
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
    }
}
