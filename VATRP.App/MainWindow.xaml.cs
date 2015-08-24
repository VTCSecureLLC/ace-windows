using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using VATRP.App.Services;
using VATRP.App.Views;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ContactBox _contactBox =  new ContactBox();
        private Dialpad _dialpadBox = new Dialpad();
        private VideoBox _videoBox = new VideoBox();
        private SelfView _selfView = new SelfView();
        private LinphoneService _linphoneService;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnRecents_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindow(_selfView);
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
            ToggleWindow(_videoBox);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.AllowDestroyWindows = true;
            CloseAllWindows();
        }

        private void CloseAllWindows()
        {
            if (_contactBox != null && _contactBox.IsActivated)
            {
                _contactBox.Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
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

            ServiceSelector.Visibility = Visibility.Hidden;
            WizardPagepanel.Visibility = Visibility.Visible;
        }

        private void onIPRelaySelect(object sender, RoutedEventArgs e)
        {
            
        }

        private void onIPCTSSelect(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _linphoneService = ServiceManager.Instance.LinphoneSipService;
            _linphoneService.RegistrationStateChangedEvent += OnRegistrationChanged;
            _linphoneService.CallStateChangedEvent += OnCallStateChanged;
            _linphoneService.GlobalStateChangedEvent += OnGlobalStateChanged;
            ServiceManager.Instance.Start();

        }

        private void OnGlobalStateChanged(LinphoneGlobalState state)
        {
            Console.WriteLine("Global State changed: " + state);
        }

        private void OnCallStateChanged(VATRPCall call)
        {
            
        }

        private void OnRegistrationChanged(LinphoneWrapper.Enums.LinphoneRegistrationState state)
        {
            Console.WriteLine("Registration State changed: " + state);
        }
    }
}
