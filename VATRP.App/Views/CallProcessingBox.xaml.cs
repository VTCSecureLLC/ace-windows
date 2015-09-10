using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows;
using VATRP.App.Model;
using VATRP.App.Services;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;
using Timer = System.Timers.Timer;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for CallProcessingBox.xaml
    /// </summary>
    public partial class CallProcessingBox
    {
        private LinphoneService _linphoneService;
        private VATRPCall _currentCall = null;
        private readonly Timer timerCall;
        private int secondsInCall;

        public CallProcessingBox() : base(VATRPWindowType.CALL_VIEW)
        {
            InitializeComponent();
            _linphoneService = ServiceManager.Instance.LinphoneSipService;
            _linphoneService.CallStateChangedEvent += OnCallStateChanged;
            timerCall = new Timer()
            {
                Interval = 1000,
                AutoReset = true
            };
            timerCall.Elapsed += OnUpdatecallTimer;
        }

        private void OnCallStateChanged(Core.Model.VATRPCall call)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnCallStateChanged(call)));
                return;
            }

            _currentCall = call;
            var callStatusString = "";
            switch (call.CallState)
            {
                case VATRPCallState.Trying:
                    CallerDisplayNameBox.Text = _currentCall.To.DisplayName;
                    CallerNumberBox.Text = _currentCall.To.Username;
                    secondsInCall = 0;
                    this.CallDurationBox.Visibility = Visibility.Collapsed;
                    this.CallStateBox.Text = "Trying";
                    break;
                case VATRPCallState.InProgress:
                    secondsInCall = 0;
                    CallerDisplayNameBox.Text = _currentCall.From.DisplayName;
                    CallerNumberBox.Text = _currentCall.From.Username;
                    ReceiveCall(call);
                    break;
                case VATRPCallState.Ringing:
                    secondsInCall = 0;
                    callStatusString = "It is now ringing remotely !";
                    this.CallStateBox.Text = "Ringing";
                    break;
                case VATRPCallState.EarlyMedia:
                    callStatusString = "Receiving some early media";
                    this.CallStateBox.Text = "Processing";
                    break;
                case VATRPCallState.Connected:
                    this.CallStateBox.Text = "Connected";
                    this.CallDurationBox.Visibility = Visibility.Visible;
                    this.IncomingPanel.Visibility = Visibility.Collapsed;
                    this.InCallPanel.Visibility = Visibility.Visible;
                    this.timerCall.Start();
                    BtnMute.Content = _linphoneService.IsCallMuted() ? "UnMute" : "Mute";
                    break;
                case VATRPCallState.Closed:
                    callStatusString = "Call is terminated.";
                    this.CallStateBox.Text = "Terminated";
                    this.Hide();
                    ActiveCall = null;
                    break;
                case VATRPCallState.Error:
                    callStatusString = "Call failure !";
                    this.CallStateBox.Text = "Error occurred";
                    break;
                default:
                    break;
            }
        }


        private void OnMute(object sender, RoutedEventArgs e)
        {
            _linphoneService.ToggleMute();
            BtnMute.Content = _linphoneService.IsCallMuted() ? "UnMute" : "Mute";
        }

        private void OnEndCall(object sender, RoutedEventArgs e)
        {
            _linphoneService.TerminateCall(_currentCall);
        }

        private void OnUpdatecallTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this._currentCall == null)
            {
                if (timerCall != null)
                    timerCall.Stop();
                return;
            }

            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new EventHandler<ElapsedEventArgs>(this.OnUpdatecallTimer), sender, new object[] { e });
                return;
            }

            TimeSpan duration = TimeSpan.FromSeconds(++secondsInCall);

            var callTime = string.Empty;

            callTime = duration.Hours.ToString() + ":" + duration.Minutes.ToString("00") + ":" + duration.Seconds.ToString("00");

            this.CallDurationBox.Text = callTime;
        }


        public VATRPCall ActiveCall
        {
            get { return _currentCall; }
            set { _currentCall = value; }
        }

        private void OnSwitchVideo(object sender, RoutedEventArgs e)
        {
            _linphoneService.SwitchSelfVideo();
        }

        internal void ReceiveCall(VATRPCall call)
        {
            _currentCall = call;
            this.CallDurationBox.Visibility = Visibility.Collapsed;
            this.CallStateBox.Text = "Incoming call";
            this.CallerDisplayNameBox.Text = call.DisplayName;
            this.CallerNumberBox.Text = call.From.Username;
            this.IncomingPanel.Visibility = Visibility.Visible;
            this.InCallPanel.Visibility = Visibility.Collapsed;
            this.Show();
        }

        private void AcceptCall(object sender, RoutedEventArgs e)
        {
            try
            {
                _linphoneService.AcceptCall(_currentCall);
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("AcceptCall", ex);
            }
        }

        private void DeclineCall(object sender, RoutedEventArgs e)
        {
            try
            {
                _linphoneService.TerminateCall(_currentCall);
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("DeclineCall", ex);
            }
        }
    }
}
