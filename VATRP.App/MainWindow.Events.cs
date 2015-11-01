using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.Views;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace com.vtcsecure.ace.windows
{
    public partial class MainWindow
    {
        private bool registerRequested = false;
        private bool isRegistering = true;
        private bool signOutRequest = false;
        private string videoTitle = string.Empty;
        private void OnCallStateChanged(VATRPCall call)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnCallStateChanged(call)));
                return;
            }
            if (call == null)
            {
                if (_remoteVideoView != null)
                {
                    ServiceManager.Instance.LinphoneService.SetVideoCallWindowHandle(IntPtr.Zero, true);
                    _remoteVideoView.DestroyOnClosing = true; // allow window to be closed
                    _remoteVideoView.Close();
                    _remoteVideoView = null;
                }
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
                    call.RemoteParty = call.To;
                    if (_callView != null)
                       _callView.OnCallStateChanged(call);
                    if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
    Configuration.ConfEntry.USE_RTT, true))
                        _messagingWindow.CreateConversation(call.To.Username);
                    break;
                case VATRPCallState.InProgress:
                    videoTitle = !string.IsNullOrWhiteSpace(call.From.DisplayName)
                        ? string.Format("\"{0}\" {1}", call.From.DisplayName, call.From.Username)
                        : call.From.Username;
                    
                    call.RemoteParty = call.From;
                    ServiceManager.Instance.SoundService.PlayRingTone();
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);

                    _flashWindowHelper.FlashWindow(_callView);
                    break;
                case VATRPCallState.Ringing:
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    call.RemoteParty = call.To;
                    ServiceManager.Instance.SoundService.PlayRingBackTone();
                    break;
                case VATRPCallState.EarlyMedia:
                    break;
                case VATRPCallState.Connected:
                    if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
    Configuration.ConfEntry.USE_RTT, true))
                        _messagingWindow.CreateConversation(call.RemoteParty.Username);
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    _flashWindowHelper.StopFlashing();
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
                                ServiceManager.Instance.LinphoneService.SetVideoPreviewWindowHandle(hWnd);
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
                        if (_remoteVideoView == null)
                        {
                            _remoteVideoView = new CallView();
                        }
                        _remoteVideoView.Title = "Remote Video"; //videoTitle;
                        Window window = GetWindow(_remoteVideoView);
                        if (window != null)
                        {
                            var wih = new WindowInteropHelper(window);

                            IntPtr hWnd = wih.EnsureHandle();
                            ServiceManager.Instance.LinphoneService.SetVideoCallWindowHandle(hWnd);
                        }
                        _remoteVideoView.Show();
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("Main OnCallStateChanged", ex);
                    }
                    break;
                case VATRPCallState.StreamsRunning:
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    break;
                case VATRPCallState.Closed:
                    _flashWindowHelper.StopFlashing();
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    stopPlayback = true;
                    if (_selfView.IsVisible)
                    {
                        _selfView.Hide();
                    }

                    if (_remoteVideoView != null)
                    {
                        _remoteVideoView.Hide();
                    }
                    if (registerRequested || signOutRequest)
                    {
                        _linphoneService.Unregister(false);
                    }
                    break;
                case VATRPCallState.Error:
                    _flashWindowHelper.StopFlashing();
                    if (_callView != null)
                        _callView.OnCallStateChanged(call);
                    stopPlayback = true;
                    if (_remoteVideoView != null)
                    {
                        _remoteVideoView.Hide();
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
                    else if (signOutRequest)
                    {
                        isRegistering = false;
                        WizardPagepanel.Children.Clear();
                        ServiceSelector.Visibility = Visibility.Visible;
                        WizardPagepanel.Visibility = Visibility.Visible;
                        NavPanel.Visibility = Visibility.Collapsed;
                        StatusPanel.Visibility = Visibility.Collapsed;
                        signOutRequest = false;
                        ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.ACCOUNT_IN_USE, string.Empty);
                        ServiceManager.Instance.AccountService.DeleteAccount(App.CurrentAccount);
                        App.CurrentAccount = null;
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

            ServiceManager.Instance.UpdateLoggedinContact();
            
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
                case VATRPWindowType.MESSAGE_VIEW:
                    BtnMessageView.IsChecked = (bool)e.NewValue;
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

        private void OnMakeCallRequested(string called_address)
        {
            MediaActionHandler.MakeVideoCall(called_address);
        }

        private void OnKeypadClicked(object sender, KeyPadEventArgs e)
        {
            _linphoneService.PlayDtmf((char)e.Key, 250);
            _linphoneService.SendDtmf(_callView.ActiveCall, (char)e.Key);
        }

        private void OnDialpadClicked(object sender, KeyPadEventArgs e)
        {
            _linphoneService.PlayDtmf((char)e.Key, 250);
        }
    }
}
