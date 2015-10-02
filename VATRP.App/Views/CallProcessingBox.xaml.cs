using System;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using log4net;
using VATRP.App.Model;
using VATRP.App.Services;
using VATRP.Core.Model;
using VATRP.Core.Services;
using Timer = System.Timers.Timer;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for CallProcessingBox.xaml
    /// </summary>
    public partial class CallProcessingBox
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(CallProcessingBox));
        private LinphoneService _linphoneService;
        private VATRPCall _currentCall = null;
        private readonly Timer timerCall;
        private readonly Timer autoAnswerTimer;
        private int secondsInCall;
        private int secondsToAutoAnswer = 0;

        public CallProcessingBox() : base(VATRPWindowType.CALL_VIEW)
        {
            InitializeComponent();
            _linphoneService = ServiceManager.Instance.LinphoneSipService;
            timerCall = new Timer
            {
                Interval = 1000,
                AutoReset = true
            };
            timerCall.Elapsed += OnUpdatecallTimer;
#if DEBUG
            autoAnswerTimer = new Timer
            {
                Interval = 1000,
                AutoReset = true
            };
            autoAnswerTimer.Elapsed += OnAutoAnswerTimer;
#endif
        }


        internal void OnCallStateChanged(VATRPCall call)
        {
            _currentCall = call;

            switch (call.CallState)
            {
                case VATRPCallState.Trying:
                    CallerDisplayNameBox.Text = _currentCall.To.DisplayName;
                    CallerNumberBox.Text = _currentCall.To.Username;
                    secondsInCall = 0;
                    CallDurationBox.Visibility = Visibility.Hidden;
                    CallerDisplayNameBox.Visibility = Visibility.Visible;
                    CallerNumberBox.Visibility = Visibility.Visible;
                    IncomingPanel.Visibility = Visibility.Hidden;
                    InCallPanel.Visibility = Visibility.Visible;
                    Show();

                    CallStateBox.Text = "Trying";
                    if (App.ActiveCallHistoryEvent == null)
                    {
                        App.ActiveCallHistoryEvent = new VATRPCallEvent(App.CurrentAccount.RegistrationUser, call.To.Username)
                        {
                            DisplayName = call.To.DisplayName,
                            Status = VATRPHistoryEvent.StatusType.Outgoing
                        };
                    }
                    break;
                case VATRPCallState.InProgress:
                    Show();
                    secondsInCall = 0;
                    CallerDisplayNameBox.Text = _currentCall.From.DisplayName;
                    CallerNumberBox.Text = _currentCall.From.Username;
                    if (App.ActiveCallHistoryEvent == null)
                    {
                        App.ActiveCallHistoryEvent = new VATRPCallEvent(App.CurrentAccount.RegistrationUser, call.From.Username)
                        {
                            DisplayName = call.From.DisplayName,
                            Status = VATRPHistoryEvent.StatusType.Incoming
                        };
                    }
                    ReceiveCall(call);
                    break;
                case VATRPCallState.Ringing:
                    CallerDisplayNameBox.Visibility = Visibility.Visible;
                    CallerNumberBox.Visibility = Visibility.Visible;
                    IncomingPanel.Visibility = Visibility.Hidden;
                    InCallPanel.Visibility = Visibility.Visible;

                    Show();
                    secondsInCall = 0;
                    CallStateBox.Text = "Ringing";
                    break;
                case VATRPCallState.EarlyMedia:
                    CallStateBox.Text = "Processing";
                    break;
                case VATRPCallState.Connected:
                    CallStateBox.Text = "Connected";
                    CallDurationBox.Visibility = Visibility.Visible;
                    IncomingPanel.Visibility = Visibility.Collapsed;
                    InCallPanel.Visibility = Visibility.Visible;
                    timerCall.Start();
                    _currentCall.CallEstablishTime = DateTime.Now;
                    BtnMute.Content = _linphoneService.IsCallMuted() ? "UnMute" : "Mute";
                    break;
                case VATRPCallState.Closed:
                    CallStateBox.Text = "Terminated";
                    InCallPanel.Visibility = System.Windows.Visibility.Collapsed;
                    Hide();
#if DEBUG
                    if (autoAnswerTimer.Enabled)
                    {
                        secondsToAutoAnswer = 100;
                        autoAnswerTimer.Stop();
                    }
#endif
                    if (App.ActiveCallHistoryEvent != null)
                    {
                        if (secondsInCall == 0)
                        {
                            if (App.ActiveCallHistoryEvent.Status == VATRPHistoryEvent.StatusType.Incoming)
                                App.ActiveCallHistoryEvent.Status = VATRPHistoryEvent.StatusType.Missed;
                        }
                        else
                        {
                            App.ActiveCallHistoryEvent.EndTime = DateTime.Now;
                        }
                        ServiceManager.Instance.HistoryService.AddCallEvent(App.ActiveCallHistoryEvent);
                        App.ActiveCallHistoryEvent = null;
                    }
                    ActiveCall = null;
                    secondsInCall = 0;
                    break;
                case VATRPCallState.Error:
                    Hide();
                    CallStateBox.Text = "Error occurred";
                    if (App.ActiveCallHistoryEvent != null)
                    {
                        if (secondsInCall == 0)
                        {
                            if (App.ActiveCallHistoryEvent.Status == VATRPHistoryEvent.StatusType.Incoming)
                                App.ActiveCallHistoryEvent.Status = VATRPHistoryEvent.StatusType.Missed;
                        }
                        else
                        {
                            App.ActiveCallHistoryEvent.EndTime = DateTime.Now;
                        }
                        ServiceManager.Instance.HistoryService.AddCallEvent(App.ActiveCallHistoryEvent);
                        App.ActiveCallHistoryEvent = null;
                    }
                    ActiveCall = null;
                    secondsInCall = 0;
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

        private void OnAutoAnswerTimer(object sender, ElapsedEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ElapsedEventArgs>(OnAutoAnswerTimer), sender, new object[] {e});
                return;
            }

            secondsToAutoAnswer--;
            CallStateBox.Text = string.Format("AutoAnswer after {0} sec", secondsToAutoAnswer);

            if (secondsToAutoAnswer <= 0)
            {
                autoAnswerTimer.Stop();
                _linphoneService.AcceptCall(_currentCall);
            }
        }

        private void OnUpdatecallTimer(object sender, ElapsedEventArgs e)
        {
            if (_currentCall == null)
            {
                if (timerCall != null)
                    timerCall.Stop();
                return;
            }

            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ElapsedEventArgs>(OnUpdatecallTimer), sender, new object[] { e });
                return;
            }

            TimeSpan duration = TimeSpan.FromSeconds(++secondsInCall);

            string callTime = duration.Hours + ":" + duration.Minutes.ToString("00") + ":" + duration.Seconds.ToString("00");

            CallDurationBox.Text = callTime;
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
            CallDurationBox.Visibility = Visibility.Collapsed;
            
            CallerDisplayNameBox.Text = call.DisplayName;
            CallerNumberBox.Text = call.From.Username;
            IncomingPanel.Visibility = Visibility.Visible;
            InCallPanel.Visibility = Visibility.Collapsed;
#if DEBUG
            if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, 
                Configuration.ConfEntry.AUTO_ANSWER, false))
            {
                if (autoAnswerTimer != null)
                {
                    secondsToAutoAnswer = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, 
                        Configuration.ConfEntry.AUTO_ANSWER_AFTER, 2);
                    if (secondsToAutoAnswer > 60)
                        secondsToAutoAnswer = 60;
                    else if (secondsToAutoAnswer < 0)
                        secondsToAutoAnswer = 0;

                    CallStateBox.Text = string.Format("AutoAnswer after {0} sec", secondsToAutoAnswer);
                    if (secondsToAutoAnswer > 0)
                    {
                        LOG.Info(string.Format("Activating AutoAnswer after {0}  sec.", secondsToAutoAnswer));
                        autoAnswerTimer.Start();
                    }
                }
            }
            else
            {
                CallStateBox.Text = "Incoming call";
            }
#else
            CallStateBox.Text = "Incoming call";
#endif
            Show();
        }

        private void AcceptCall(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (autoAnswerTimer.Enabled)
            {
                secondsToAutoAnswer = 100;
                autoAnswerTimer.Stop();
            } 
#endif

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
#if DEBUG
            if (autoAnswerTimer.Enabled)
            {
                secondsToAutoAnswer = 100;
                autoAnswerTimer.Stop();
            }
#endif
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
