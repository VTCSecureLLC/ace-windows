using System;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Structs;
using Timer = System.Timers.Timer;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for CallProcessingBox.xaml
    /// </summary>
    public partial class CallProcessingBox
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(CallProcessingBox));
        private ILinphoneService _linphoneService;
        private VATRPCall _currentCall = null;
        private readonly Timer ringTimer;
        private readonly Timer autoAnswerTimer;
        private bool subscribedForStats;
        private Timer timerCall;
        #endregion

        #region Properties

        public KeyPadCtrl KeypadCtrl { get; set; }
        public CallInfoView CallInfoCtrl { get; set; }

        #endregion
        public CallProcessingBox() : base(VATRPWindowType.CALL_VIEW)
        {
            DataContext = new CallViewModel();
            InitializeComponent();
            _linphoneService = ServiceManager.Instance.LinphoneService;
            timerCall = new Timer
            {
                Interval = 1000,
                AutoReset = true
            };
            timerCall.Elapsed += OnUpdatecallTimer;

            ringTimer = new Timer
                        {
                            Interval = 1800,
                            AutoReset = true
                        };
            ringTimer.Elapsed += OnUpdateRingCounter;

#if DEBUG
            autoAnswerTimer = new Timer
            {
                Interval = 1000,
                AutoReset = true
            };
            autoAnswerTimer.Elapsed += OnAutoAnswerTimer;
#endif
        }

        private void OnUpdatecallTimer(object sender, ElapsedEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ElapsedEventArgs>(OnUpdatecallTimer), sender, new object[] { e });
                return;
            }
            ((CallViewModel) DataContext).Duration++;
            timerCall.Start();
        }

        private void OnUpdateRingCounter(object sender, ElapsedEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ElapsedEventArgs>(OnUpdateRingCounter), sender, new object[] { e });
                return;
            }
            ((CallViewModel) DataContext).RingCount += 1;
            ringTimer.Start();
        }

        internal void OnCallStateChanged(VATRPCall call)
        {
            bool stopAnimation = false;
            _currentCall = call;
            ServiceManager.Instance.ActiveCallPtr = call.NativeCallPtr;

            var model = DataContext as CallViewModel;
            if (model == null)
                return;

            switch (call.CallState)
            {
                case VATRPCallState.Trying:
                    model.DisplayName = _currentCall.To.DisplayName;
                    model.RemoteNumber = _currentCall.To.Username;
                    model.Duration = 0;
                    model.AutoAnswer = 0;
                    IncomingPanel.Visibility = Visibility.Hidden;
                    InCallPanel.Visibility = Visibility.Visible;
                    model.CallState = VATRPCallState.Trying;
                    Show();

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
                {
                    model.CallState = VATRPCallState.InProgress;
                    Show();
                    model.Duration = 0;
                    model.AutoAnswer = 0;
                    model.DisplayName = _currentCall.From.DisplayName;
                    model.RemoteNumber = _currentCall.From.Username;
                    if (App.ActiveCallHistoryEvent == null)
                    {
                        App.ActiveCallHistoryEvent = new VATRPCallEvent(App.CurrentAccount.RegistrationUser,
                            call.From.Username)
                        {
                            DisplayName = call.From.DisplayName,
                            Status = VATRPHistoryEvent.StatusType.Incoming
                        };
                    }
                    ReceiveCall(call);

                    model.VisualizeIncoming = true;
                    if (!model.VisualizeRinging)
                    {
                        RingCounterBox.Foreground = new SolidColorBrush(Colors.White);
                        model.VisualizeRinging = true;
                        model.RingCount = 1;
                        if (ringTimer != null)
                        {
                            if (ringTimer.Enabled)
                                ringTimer.Stop();
                            ringTimer.Interval = 1800;
                            ringTimer.Start();
                        }
                    }
                }
                    break;
                case VATRPCallState.Ringing:
                {
                    IncomingPanel.Visibility = Visibility.Hidden;
                    InCallPanel.Visibility = Visibility.Visible;
                    model.CallState = VATRPCallState.Ringing;
                    Show();
                    model.Duration = 0;
                    model.AutoAnswer = 0;
                    model.VisualizeIncoming = false;
                    if (!model.VisualizeRinging)
                    {
                        RingCounterBox.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0x1C, 0x1C));
                        model.VisualizeRinging = true;
                        model.RingCount = 1;
                        if (ringTimer != null)
                        {
                            if (ringTimer.Enabled)
                                ringTimer.Stop();
                            ringTimer.Interval = 4000;
                            ringTimer.Start();
                        }
                    }
                }
                    break;
                case VATRPCallState.EarlyMedia:
                    model.CallState = VATRPCallState.EarlyMedia;
                    break;
                case VATRPCallState.Connected:
                {
                    stopAnimation = true;
                    model.CallState = VATRPCallState.Connected;
                    timerCall.Start();
                    IncomingPanel.Visibility = Visibility.Collapsed;
                    InCallPanel.Visibility = Visibility.Visible;
                    _currentCall.CallEstablishTime = DateTime.Now;
                    BtnMute.Content = _linphoneService.IsCallMuted() ? "UnMute" : "Mute";
                }
                    break;
                case VATRPCallState.StreamsRunning:
                    SubscribeCallStatistics();
                    model.CallState = VATRPCallState.StreamsRunning;
                    break;
                case VATRPCallState.Closed:
                    stopAnimation = true; 
                    model.CallState = VATRPCallState.Closed;
                    OnCallClosed(model);
                    break;
                case VATRPCallState.Error:
                    stopAnimation = true;
                    model.CallState = VATRPCallState.Error;
                    OnCallClosed(model);
                    break;
            }

            if (stopAnimation)
            {
                StopAnimation();
            }
        }

        private void OnCallClosed(CallViewModel model)
        {
            InCallPanel.Visibility = System.Windows.Visibility.Collapsed;
            KeypadCtrl.Hide();
            UnsubscribeCallStaistics();
            if (CallInfoCtrl != null)
            {
                CallInfoCtrl.Hide();
            }

            if (timerCall.Enabled)
                timerCall.Stop();
#if DEBUG
            if (autoAnswerTimer.Enabled)
                autoAnswerTimer.Stop();
#endif

            Hide();
            if (App.ActiveCallHistoryEvent != null)
            {
                if (model.Duration == 0)
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
            ServiceManager.Instance.ActiveCallPtr = IntPtr.Zero;
        }

        private void StopAnimation()
        {
            ((CallViewModel) DataContext).VisualizeRinging = false;
            ((CallViewModel) DataContext).VisualizeIncoming = false;
            if (ringTimer.Enabled)
                ringTimer.Stop();
        }
        
        private void OnMute(object sender, RoutedEventArgs e)
        {
            _linphoneService.ToggleMute();
            BtnMute.Content = _linphoneService.IsCallMuted() ? "UnMute" : "Mute";
        }

        private void OnEndCall(object sender, RoutedEventArgs e)
        {
            SetTimeout(delegate
            {
                _linphoneService.TerminateCall(_currentCall.NativeCallPtr);
            }, 2);
        }

        private void OnAutoAnswerTimer(object sender, ElapsedEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ElapsedEventArgs>(OnAutoAnswerTimer), sender, new object[] {e});
                return;
            }

            var model = DataContext as CallViewModel;

            if (model != null && model.AutoAnswer > 0)
            {
                model.AutoAnswer--;

                if (model.AutoAnswer == 0)
                {
                    autoAnswerTimer.Stop();
                    _linphoneService.AcceptCall(_currentCall.NativeCallPtr);
                }
            }
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
            var model = DataContext as CallViewModel;
            if (model != null)
            {
                model.DisplayName = call.DisplayName;
                model.RemoteNumber = call.From.Username;
                IncomingPanel.Visibility = Visibility.Visible;
                InCallPanel.Visibility = Visibility.Collapsed;
#if DEBUG
                if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.AUTO_ANSWER, false))
                {
                    if (autoAnswerTimer != null)
                    {
                        var autoAnswerDuration =
                            ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                                Configuration.ConfEntry.AUTO_ANSWER_AFTER, 2);
                        if (autoAnswerDuration > 60)
                            autoAnswerDuration = 60;
                        else if (autoAnswerDuration < 0)
                            autoAnswerDuration = 0;

                        model.AutoAnswer = autoAnswerDuration;
                        if (autoAnswerDuration > 0)
                            autoAnswerTimer.Start();
                    }
                }
#endif
            }
            Show();
        }

        private void AcceptCall(object sender, RoutedEventArgs e)
        {
            var model = DataContext as CallViewModel;
            if (model != null)
            {
#if DEBUG
                if (autoAnswerTimer.Enabled)
                {
                    model.AutoAnswer = 0;
                    autoAnswerTimer.Stop();
                }
#endif
                StopAnimation();
                model.CallState = VATRPCallState.Connected;
                IncomingPanel.Visibility = Visibility.Collapsed;
                InCallPanel.Visibility = Visibility.Visible;
                BtnMute.Content = "Mute";
            }

            SetTimeout(delegate
            {
                if (_currentCall != null)
                {
                    try
                    {
                        _linphoneService.AcceptCall(_currentCall.NativeCallPtr);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("AcceptCall", ex);
                    }
                }
            }, 5);
        }

        private void DeclineCall(object sender, RoutedEventArgs e)
        {
            var model = DataContext as CallViewModel;
            if (model != null)
            {
#if DEBUG
                if (autoAnswerTimer.Enabled)
                {
                    model.AutoAnswer = 0;
                    autoAnswerTimer.Stop();
                }
#endif         
                SetTimeout(delegate
                {
                    try
                    {
                        _linphoneService.DeclineCall(_currentCall.NativeCallPtr);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("DeclineCall", ex);
                    }
                },5);
            }
        }

        private void OnSwitchKeypad(object sender, RoutedEventArgs e)
        {
            if (KeypadCtrl != null)
            {
                if (KeypadCtrl.Visibility == Visibility.Visible)
                    KeypadCtrl.Hide();
                else
                    KeypadCtrl.Show();
            }
        }

        #region Call Statistics Info
        private void ToggleInfoWindow(object sender, RoutedEventArgs e)
        {
            if (CallInfoCtrl != null)
            {
                if (CallInfoCtrl.IsVisible)
                    CallInfoCtrl.Hide();
                else
                    CallInfoCtrl.Show();
            }
        }

        public void SubscribeCallStatistics()
        {
            if (subscribedForStats)
                return;
            subscribedForStats = true;
            ServiceManager.Instance.LinphoneService.CallStatisticsChangedEvent += OnCallStatisticsChanged;
        }

        public void UnsubscribeCallStaistics()
        {
            if (!subscribedForStats)
                return;
            subscribedForStats = false;
            ServiceManager.Instance.LinphoneService.CallStatisticsChangedEvent -= OnCallStatisticsChanged;
        }

        private void OnCallStatisticsChanged(VATRPCall call, LinphoneCallStats stats)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke((Action)(() => this.OnCallStatisticsChanged(call, stats)));
                return;
            }

            CallInfoCtrl.UpdateCallInfo(call);
        }

        #endregion
        void SetTimeout(Action callback, int miliseconds)
        {
            System.Timers.Timer timeout = new System.Timers.Timer();
            timeout.Interval = miliseconds;
            timeout.AutoReset = false;
            timeout.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                callback();
            };
            timeout.Start();
        }
    }
}
