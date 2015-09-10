using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using VATRP.App.Services;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.App
{
    public partial class MainWindow
    {
        private void OnCallStateChanged(VATRPCall call)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnCallStateChanged(call)));
                return;
            }

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
                    ServiceManager.Instance.SoundService.StopRingBackTone();
                    ServiceManager.Instance.SoundService.StopRingTone();
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
                    ServiceManager.Instance.SoundService.StopRingBackTone();
                    ServiceManager.Instance.SoundService.StopRingTone();
                    _remoteVideoView.Hide();
                    break;
                case VATRPCallState.Error:
                    callStatusString = "Call failure !";
                    ServiceManager.Instance.SoundService.StopRingBackTone();
                    ServiceManager.Instance.SoundService.StopRingTone();
                    _remoteVideoView.Hide();
                    break;
                default:
                    callStatusString = call.CallState.ToString();
                    break;
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

            switch (state)
            {
                case LinphoneRegistrationState.LinphoneRegistrationProgress:
                    RegStatusLabel.Foreground = Brushes.Black;
                    statusString = "In progress";
                    break;
                case LinphoneRegistrationState.LinphoneRegistrationOk:
                    statusString = "Registered";
                    RegStatusLabel.Foreground = Brushes.Green;
                    ServiceManager.Instance.SoundService.PlayConnectionChanged(true);
                    break;
                case LinphoneRegistrationState.LinphoneRegistrationFailed:
                    RegStatusLabel.Foreground = Brushes.Red;
                    ServiceManager.Instance.SoundService.PlayConnectionChanged(false);
                    break;
                default:
                    RegStatusLabel.Foreground = Brushes.Black;
                    break;
            }
            var regString = string.Format("Registration status: {0}", statusString);
            RegStatusLabel.Content = regString;
            Console.WriteLine("Registration State changed: " + state);
            if (RegistrationState == LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                // hide panels
                ServiceSelector.Visibility = Visibility.Collapsed;
                WizardPagepanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
