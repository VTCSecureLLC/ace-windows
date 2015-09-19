using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using VATRP.App.Model;
using VATRP.App.Services;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.App
{
    public partial class MainWindow
    {
        private bool registerRequested = false;
        private bool isRegistering = true;

        private void OnCallStateChanged(VATRPCall call)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnCallStateChanged(call)));
                return;
            }

            var stopPlayback = false;
            var callStatusString = "";
            switch (call.CallState)
            {
                case VATRPCallState.Trying:
                    // call started, 
                    if (_callView.Visibility != Visibility.Visible)
                        _callView.Show();
                    _remoteVideoView.Title = call.To.DisplayName;
                    break;
                case VATRPCallState.InProgress:
                    ServiceManager.Instance.SoundService.PlayRingTone();
                    _remoteVideoView.Title = call.From.DisplayName;
                    break;
                case VATRPCallState.Ringing:
                    if (_callView.Visibility != Visibility.Visible)
                        _callView.Show();
                    callStatusString = "It is now ringing remotely !";
                    ServiceManager.Instance.SoundService.PlayRingBackTone();
                    break;
                case VATRPCallState.EarlyMedia:
                    callStatusString = "Receiving some early media";
                    break;
                case VATRPCallState.Connected:
                    callStatusString = "We are connected !";
                    stopPlayback = true;
                    if (ServiceManager.Instance.LinphoneSipService.IsVideoEnabled(call))
                    {
                        _remoteVideoView.Show();
                    }
                    Window window = Window.GetWindow(_remoteVideoView);
                    if (window != null)
                    {
                        var wih = new WindowInteropHelper(window);
                        IntPtr hWnd = wih.Handle;
                        ServiceManager.Instance.LinphoneSipService.SetVideoCallWindowHandle(hWnd);
                    }
                    break;
                case VATRPCallState.Closed:
                    callStatusString = "Call is terminated.";
                    stopPlayback = true;
                    _remoteVideoView.Hide();
                    if (registerRequested)
                    {
                        _linphoneService.Unregister();
                    }
                    break;
                case VATRPCallState.Error:
                    callStatusString = "Call failure !";
                    stopPlayback = true;
                    _remoteVideoView.Hide();
                    break;
                default:
                    callStatusString = call.CallState.ToString();
                    break;
            }

            if (stopPlayback)
            {
                ServiceManager.Instance.SoundService.StopRingBackTone();
                ServiceManager.Instance.SoundService.StopRingTone();
            }
            Console.WriteLine("Call StateChanged " + call.NativeCallPtr + " State: " + callStatusString);
        }

        private void OnRegistrationChanged(LinphoneRegistrationState state)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnRegistrationChanged(state)));
                return;
            }

            RegistrationState = state;
            var statusString = "Unregistered";
            this.BtnSettings.IsEnabled = true;

            switch (state)
            {
                case LinphoneRegistrationState.LinphoneRegistrationProgress:
                    RegStatusLabel.Foreground = Brushes.Black;
                    statusString = isRegistering ? "Registering..." : "Unregistering...";
                    this.BtnSettings.IsEnabled = false;
                    break;
                case LinphoneRegistrationState.LinphoneRegistrationOk:
                    statusString = "Registered";
                    RegStatusLabel.Foreground = Brushes.Green;
                    ServiceManager.Instance.SoundService.PlayConnectionChanged(true);
                    isRegistering = false;
                    break;
                case LinphoneRegistrationState.LinphoneRegistrationFailed:
                    RegStatusLabel.Foreground = Brushes.Red;
                    ServiceManager.Instance.SoundService.PlayConnectionChanged(false);
                    break;
                case LinphoneRegistrationState.LinphoneRegistrationCleared:
                    RegStatusLabel.Foreground = Brushes.Red;
                    statusString = "Unregistered";
                    isRegistering = true;
                    ServiceManager.Instance.SoundService.PlayConnectionChanged(false);
                    if (registerRequested)
                    {
                        registerRequested = false;
                        _linphoneService.Register();
                    }
                    break;
                default:
                    RegStatusLabel.Foreground = Brushes.Black;
                    break;
            }
            var regString = string.Format("{0}", statusString);
            RegStatusLabel.Content = regString;
        }

        private void OnNewAccountRegistered(string accountId)
        {
            ServiceSelector.Visibility = Visibility.Collapsed;
            WizardPagepanel.Visibility = Visibility.Collapsed;
            RegUserLabel.Text = string.Format( "Account: {0}", App.CurrentAccount.RegistrationUser);
            NavPanel.Visibility = Visibility.Visible;
            StatusPanel.Visibility = Visibility.Visible;

            if (ServiceManager.Instance.UpdateLinphoneConfig())
            {
                if (ServiceManager.Instance.StartLinphoneService())
                    ServiceManager.Instance.Register();
            }
        }
        private void OnChildVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (App.AllowDestroyWindows)
                return;
            var window = sender as VATRPWindow;
            if (window == null)
                return;
            switch (window.WindowType)
            {
                case VATRPWindowType.CONTACT_VIEW:
                    BtnContacts.IsChecked = (bool) e.NewValue;
                    break;
                case VATRPWindowType.SELF_VIEW:
                    BtnCallView.IsChecked = (bool)e.NewValue;
                    break;
                case VATRPWindowType.RECENTS_VIEW:
                    BtnRecents.IsChecked = (bool)e.NewValue;
                    break;
                case VATRPWindowType.DIALPAD_VIEW:
                    BtnDialpad.IsChecked = (bool)e.NewValue;
                    break;
                case VATRPWindowType.SETTINGS_VIEW:
                    BtnSettings.IsChecked = (bool)e.NewValue;
                    break;
            }
        }
    }
}
