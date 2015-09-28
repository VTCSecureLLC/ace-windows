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
using VATRP.App.Views;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.App
{
    public partial class MainWindow
    {
        private bool registerRequested = false;
        private bool isRegistering = true;
        private string videoTitle = string.Empty;
        private void OnCallStateChanged(VATRPCall call)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnCallStateChanged(call)));
                return;
            }

            LOG.Info(string.Format("CallStateChanged: State - {0}. Call: {1}", call.CallState, call.NativeCallPtr ));
            var stopPlayback = false;
            switch (call.CallState)
            {
                case VATRPCallState.Trying:
                    // call started, 
                    videoTitle = call.To.DisplayName;
                    videoTitle = !string.IsNullOrWhiteSpace(call.To.DisplayName)
                        ? string.Format("\"{0}\" {1}", call.To.DisplayName, call.To.Username)
                        : call.To.Username;
                    if (_callView != null)
                       _callView.OnCallStateChanged(call);
                    break;
                case VATRPCallState.InProgress:
                    videoTitle = !string.IsNullOrWhiteSpace(call.From.DisplayName)
                        ? string.Format("\"{0}\" {1}", call.From.DisplayName, call.From.Username)
                        : call.From.Username;
                    
                    ServiceManager.Instance.SoundService.PlayRingTone();
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    break;
                case VATRPCallState.Ringing:
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    ServiceManager.Instance.SoundService.PlayRingBackTone();
                    break;
                case VATRPCallState.EarlyMedia:
                    break;
                case VATRPCallState.Connected:
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    stopPlayback = true;
                    try
                    {
                        if (_selfView.IsVisible)
                        {
                            Window window = Window.GetWindow(_selfView);
                            if (window != null)
                            {
                                var wih = new WindowInteropHelper(window);
                                IntPtr hWnd = wih.EnsureHandle();
                                ServiceManager.Instance.LinphoneSipService.SetVideoPreviewWindowHandle(hWnd);
                            }
                        }
                        else
                        {
                            _selfView.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("Show Self preview", ex);
                        RecreateSelfView();
                    }
                    try
                    {
                        if (ServiceManager.Instance.LinphoneSipService.IsVideoEnabled(call))
                        {
                            if (_remoteVideoView == null)
                            {
                                _remoteVideoView = new CallView();
                            }
                            _remoteVideoView.Title = "Remote Video";//videoTitle;
                            Window window = GetWindow(_remoteVideoView);
                            if (window != null)
                            {
                                var wih = new WindowInteropHelper(window);

                                IntPtr hWnd = wih.EnsureHandle();
                                ServiceManager.Instance.LinphoneSipService.SetVideoCallWindowHandle(hWnd);
                            }
                            _remoteVideoView.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("Main OnCallStateChanged", ex);
                    }
                    break;
                case VATRPCallState.Closed:
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    stopPlayback = true;
                    if (_remoteVideoView != null)
                    {
                        _remoteVideoView.Close();
                        _remoteVideoView = null;
                    }
                    if (registerRequested)
                    {
                        _linphoneService.Unregister();
                    }
                    break;
                case VATRPCallState.Error:
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    stopPlayback = true;
                    if (_remoteVideoView != null)
                    {
                        _remoteVideoView.Close();
                        _remoteVideoView = null;
                    }
                    break;
                default:
                    break;
            }

            if (stopPlayback)
            {
                ServiceManager.Instance.SoundService.StopRingBackTone();
                ServiceManager.Instance.SoundService.StopRingTone();
            }
        }

        private void RecreateSelfView()
        {
            LOG.Info("Recreate Self view");
            try
            {
                _selfView = new SelfView();
                _selfView.Show();
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("RecreateSelfView", ex);
            }
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
            LOG.Info(String.Format("Registration state changed. Current - {0}", state));
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
            LOG.Info(string.Format( "New account registered. Useaname -{0}. Host - {1} Port - {2}",
                App.CurrentAccount.RegistrationUser,
                App.CurrentAccount.ProxyHostname,
                App.CurrentAccount.ProxyPort));
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
