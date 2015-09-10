using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private ContactBox _contactBox =  new ContactBox();
        private Dialpad _dialpadBox = new Dialpad();
        private CallProcessingBox _callView = new CallProcessingBox();
        private HistoryView _historyView = new HistoryView();
        private CallView _remoteVideoView = new CallView();
        private SelfView _selfView = new SelfView();
        private LinphoneService _linphoneService;
        private ServiceManager _serviceManager;

        #endregion

        #region Properties
        public static LinphoneRegistrationState RegistrationState { get; set; }
        #endregion
        public MainWindow() : base(VATRPWindowType.MAIN_VIEW)
        {
            DataContext = this;
            _serviceManager = ServiceManager.Instance;
            if (_serviceManager != null)
                _serviceManager.Start();
            _linphoneService = ServiceManager.Instance.LinphoneSipService;
            _linphoneService.RegistrationStateChangedEvent += OnRegistrationChanged;
            _linphoneService.CallStateChangedEvent += OnCallStateChanged;
            _linphoneService.GlobalStateChangedEvent += OnGlobalStateChanged;
            _linphoneService.ErrorEvent += OnErrorEvent;
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

        private void ToggleWindow(Window window)
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
            if (_callView.ActiveCall == null)
            {
                ToggleWindow(_selfView);
            }
            else
            {
                ToggleWindow(_callView);
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            
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
            if (_contactBox != null && _contactBox.IsActivated)
            {
                _contactBox.Close();
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _linphoneService.RegistrationStateChangedEvent -= OnRegistrationChanged;
            _linphoneService.CallStateChangedEvent -= OnCallStateChanged;
            _linphoneService.GlobalStateChangedEvent -= OnGlobalStateChanged;

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
            if (_serviceManager.UpdateLinphoneConfig())
            {
                if (_linphoneService.Start(true))
                    _linphoneService.Register();
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
            IConfigurationService confService = ServiceManager.Instance.ConfigurationService;

            App.CurrentAccount = ServiceManager.Instance.LoadActiveAccount();
            if (App.CurrentAccount != null)
            {
                if (!string.IsNullOrEmpty(App.CurrentAccount.ProxyHostname) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationPassword) &&
                    !string.IsNullOrEmpty(App.CurrentAccount.RegistrationUser) &&
                    App.CurrentAccount.ProxyPort != 0)
                {
                    ServiceSelector.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                Debug.WriteLine("Current AccountService is ");
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
