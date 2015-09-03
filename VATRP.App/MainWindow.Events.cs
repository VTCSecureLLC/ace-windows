using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
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
                    ToggleWindow(_videoBox);
                    break;
                case VATRPCallState.InProgress:
                case VATRPCallState.Ringing:
                    if (_videoBox.Visibility != Visibility.Visible)
                        _videoBox.Show();
                    callStatusString = "It is now ringing remotely !";
                    break;
                case VATRPCallState.EarlyMedia:
                    callStatusString = "Receiving some early media";
                    break;
                case VATRPCallState.Connected:
                    callStatusString = "We are connected !";
                    break;
                case VATRPCallState.Closed:
                    callStatusString = "Call is terminated.";
                    break;
                case VATRPCallState.Error:
                    callStatusString = "Call failure !";
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
                    break;
                case LinphoneRegistrationState.LinphoneRegistrationFailed:
                    RegStatusLabel.Foreground = Brushes.Red;
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
