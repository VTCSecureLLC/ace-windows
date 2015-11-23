using System;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using System.Windows.Threading;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class CallViewModel : ViewModelBase
    {
        private bool _visualizeRing;
        private bool _visualizeIncoming;
        private int _ringCount;
        private bool _showInfo;
        private VATRPCallState _callState;
        private string _displayName;
        private string _remoteNumber;
        private int _duration;
        private int _autoAnswer;
        private ILinphoneService _linphoneService;
        private bool _hasVideo;
        private double _displayNameSize;
        private double _remotePartyTextSize;
        private double _infoTextSize;
        private VATRPCall _currentCall = null;
        private readonly System.Timers.Timer ringTimer;
        private readonly System.Timers.Timer autoAnswerTimer;
        private bool subscribedForStats;
        private System.Timers.Timer timerCall;
        private CallInfoViewModel _callInfoViewModel;
        private bool _showIncomingCallPanel;
        private bool _showOutgoingCallPanel;
        private SolidColorBrush _ringCounterBrush;
        private bool _isVideoOn;
        private bool _isMuteOn;
        private bool _isSpeakerOn;
        private bool _isNumpadOn;
        private bool _isRttOn;
        private int _videoWidth;
        private int _videoHeight;

        public CallViewModel()
        {
            _visualizeRing = false;
            _visualizeIncoming = false;
            _callState = VATRPCallState.None;
            _hasVideo = true;
            _displayNameSize = 30;
            _remotePartyTextSize = 25;
            _infoTextSize = 20;

            timerCall = new System.Timers.Timer
            {
                Interval = 1000,
                AutoReset = true
            };
            timerCall.Elapsed += OnUpdatecallTimer;

            ringTimer = new System.Timers.Timer
            {
                Interval = 1800,
                AutoReset = true
            };
            ringTimer.Elapsed += OnUpdateRingCounter;

#if DEBUG
            autoAnswerTimer = new System.Timers.Timer
            {
                Interval = 1000,
                AutoReset = true
            };
            autoAnswerTimer.Elapsed += OnAutoAnswerTimer;
#endif
            _callInfoViewModel = new CallInfoViewModel();
        }

        public CallViewModel(ILinphoneService linphoneSvc, VATRPCall call):this()
        {
            _linphoneService = linphoneSvc;
            _currentCall = call;
       }

        #region Properties

        public bool VisualizeIncoming
        {
            get { return _visualizeIncoming; }
            set
            {
                _visualizeIncoming = value;
                OnPropertyChanged("VisualizeIncoming");
            }
        }

        public bool VisualizeRinging
        {
            get { return _visualizeRing; }
            set
            {
                _visualizeRing = value;
                OnPropertyChanged("VisualizeRinging");
            }
        }

        public SolidColorBrush RingCounterBrush
        {
            get { return _ringCounterBrush; }
            set
            {
                _ringCounterBrush = value;
                OnPropertyChanged("RingCounterBrush");
            }
        }

        public int RingCount
        {
            get { return _ringCount; }
            set
            {
                _ringCount = value;
                OnPropertyChanged("RingCount");
            }
        }

        public bool ShowInfo
        {
            get
            {
                return _showInfo;
            }
            set
            {
                if (_showInfo != value)
                {
                    _showInfo = value;
                    OnPropertyChanged("ShowInfo");
                }
            }
        }

        public VATRPCall ActiveCall
        {
            get
            {
                return _currentCall;
            }
        }

        public VATRPCallState CallState
        {
            get
            {
                return _callState;
            }
            set
            {
                _callState = value;
                OnPropertyChanged("CallState");
                switch (_callState)
                {
                    case VATRPCallState.StreamsRunning:
                        ShowInfo = true;
                        break;
                    default:
                        ShowInfo = false;
                        break;
                }
            }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged("DisplayName");
                OnPropertyChanged("CallerInfo");
            }
        }

        public string RemoteNumber
        {
            get { return _remoteNumber; }
            set
            {
                _remoteNumber = value;
                OnPropertyChanged("RemoteNumber");
                OnPropertyChanged("CallerInfo");
            }
        }

        public string CallerInfo
        {
            get
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return RemoteNumber;
                return DisplayName;
            }
        }
        public int Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged("Duration");
                }
            }
        }

        public int AutoAnswer
        {
            get { return _autoAnswer; }
            set
            {
                _autoAnswer = value; 
                OnPropertyChanged("AutoAnswer");
            }
        }

        public bool HasVideo
        {
            get { return _hasVideo; }
            set
            {
                _hasVideo = value; 
                OnPropertyChanged("HasVideo");
            }
        }

        public bool ShowIncomingCallPanel
        {
            get
            {
                return _showIncomingCallPanel;
            }
            set
            {
                _showIncomingCallPanel = value;
                OnPropertyChanged("ShowIncomingCallPanel");
                OnPropertyChanged("ShowCallParams");
            }
        }

        public bool ShowCallParams
        {
            get { return ShowIncomingCallPanel || ShowOutgoingEndCall; }
        }
        public bool ShowOutgoingEndCall
        {
            get
            {
                return !ShowInfo && _showOutgoingCallPanel;
            }
            set
            {
                _showOutgoingCallPanel = value ;
                OnPropertyChanged("ShowOutgoingEndCall");
                OnPropertyChanged("ShowCallParams");
            }
        }

        public bool IsVideoOn
        {
            get { return _isVideoOn; }
            set
            {
                _isVideoOn = value; 
                OnPropertyChanged("IsVieoOn");
            }
        }

        public bool IsMuteOn
        {
            get { return _isMuteOn; }
            set
            {
                _isMuteOn = value;
                OnPropertyChanged("IsMuteOn");
            }
        }

        public bool IsSpeakerOn
        {
            get { return _isSpeakerOn; }
            set
            {
                _isSpeakerOn = value;
                OnPropertyChanged("IsSpeakerOn");
            }
        }

        public bool IsNumpadOn
        {
            get { return _isNumpadOn; }
            set
            {
                _isNumpadOn = value;
                OnPropertyChanged("IsNumpadOn");
            }
        }

        public bool IsRttOn
        {
            get { return _isRttOn; }
            set
            {
                _isRttOn = value;
                OnPropertyChanged("IsRttOn");
            }
        }

        public double DisplayNameSize
        {
            get { return _displayNameSize; }
            set
            {
                _displayNameSize = value; 
                OnPropertyChanged("DisplayNameSize");
            }
        }

        public double RemotePartyTextSize
        {
            get { return _remotePartyTextSize; }
            set
            {
                _remotePartyTextSize = value;
                OnPropertyChanged("RemotePartyTextSize");
            }
        }

        public double InfoTextSize
        {
            get { return _infoTextSize; }
            set
            {
                _infoTextSize = value;
                OnPropertyChanged("InfoTextSize");
            }
        }

        public int VideoWidth
        {
            get { return _videoWidth; }
            set
            {
                _videoWidth = value;
                OnPropertyChanged("VideoWidth");
            }
        }

        public int VideoHeight
        {
            get { return _videoHeight; }
            set
            {
                _videoHeight = value;
                OnPropertyChanged("VideoHeight");
            }
        }

        public CallInfoViewModel CallInfoModel
        {
            get
            {
                return _callInfoViewModel;
            }
        }
        #endregion

        #region Methods

        private void OnUpdatecallTimer(object sender, ElapsedEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher != null)
            {
                if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
                {
                    ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new EventHandler<ElapsedEventArgs>(OnUpdatecallTimer), sender, new object[] {e});
                    return;
                }
                Duration++;
                timerCall.Start();
            }
        }

        private void OnUpdateRingCounter(object sender, ElapsedEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher != null)
            {
                if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
                {
                    ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new EventHandler<ElapsedEventArgs>(OnUpdateRingCounter), sender, new object[] {e});
                    return;
                }
                RingCount += 1;
                ringTimer.Start();
            }
        }

        private void OnAutoAnswerTimer(object sender, ElapsedEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher != null)
            {
                if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
                {
                    ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new EventHandler<ElapsedEventArgs>(OnAutoAnswerTimer), sender, new object[] {e});
                    return;
                }

                if (AutoAnswer > 0)
                {
                    AutoAnswer--;

                    if (AutoAnswer == 0)
                    {
                        autoAnswerTimer.Stop();
                        //Hide();
                        _linphoneService.AcceptCall(_currentCall.NativeCallPtr,
                            ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                                Configuration.ConfEntry.USE_RTT, true));
                    }
                }
            }
        }


        private void StopAnimation()
        {
            VisualizeRinging = false;
            VisualizeIncoming = false;
            if (ringTimer.Enabled)
                ringTimer.Stop();
        }

        internal void TerminateCall()
        {
            if (_currentCall != null) 
                _linphoneService.TerminateCall(_currentCall.NativeCallPtr);
        }

        internal void MuteCall()
        {
            _linphoneService.ToggleMute();
            IsMuteOn = _linphoneService.IsCallMuted();
        }

        private void ReceiveCall(VATRPCall call)
        {
            DisplayName = call.DisplayName;
            RemoteNumber = call.From.Username;
            ShowIncomingCallPanel = true;
            ShowOutgoingEndCall = false;
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

                    AutoAnswer = autoAnswerDuration;
                    if (autoAnswerDuration > 0)
                        autoAnswerTimer.Start();
                }
            }
#endif
        }

        private void OnCallClosed()
        {
            ShowIncomingCallPanel = false;
            UnsubscribeCallStaistics();
            
            if (timerCall.Enabled)
                timerCall.Stop();
#if DEBUG
            if (autoAnswerTimer.Enabled)
                autoAnswerTimer.Stop();
#endif

            if (App.ActiveCallHistoryEvent != null)
            {
                if (Duration == 0)
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
            _currentCall = null;
            ServiceManager.Instance.ActiveCallPtr = IntPtr.Zero;
        }

        #endregion

        #region Events
        internal void OnCallStateChanged(VATRPCall call)
        {
            bool stopAnimation = false;
            _currentCall = call;
            ServiceManager.Instance.ActiveCallPtr = call.NativeCallPtr;
            CallState = call.CallState;

            switch (call.CallState)
            {
                case VATRPCallState.Trying:
                    DisplayName = _currentCall.To.DisplayName;
                    RemoteNumber = _currentCall.To.Username;
                    Duration = 0;
                    AutoAnswer = 0;
                    ShowIncomingCallPanel = false;
                    ShowOutgoingEndCall = true;
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
                        CallState = VATRPCallState.InProgress;
                        Duration = 0;
                        AutoAnswer = 0;
                        DisplayName = _currentCall.From.DisplayName;
                        RemoteNumber = _currentCall.From.Username;
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
                        if (timerCall != null)
                        {
                            if (!timerCall.Enabled)
                                timerCall.Start();
                        }
                        
                        VisualizeIncoming = true;
                        if (!VisualizeRinging)
                        {
                            RingCounterBrush = new SolidColorBrush(Colors.White);
                            VisualizeRinging = true;
                            RingCount = 1;
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
                    ShowIncomingCallPanel = false;
                    ShowOutgoingEndCall = true;
                    Duration = 0;
                    AutoAnswer = 0;
                    VisualizeIncoming = false;
                    if (timerCall != null)
                    {
                        if (!timerCall.Enabled)
                            timerCall.Start();
                    }
                    if (!VisualizeRinging)
                    {
                        RingCounterBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0x1C, 0x1C));
                        VisualizeRinging = true;
                        RingCount = 1;
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
                    break;
                case VATRPCallState.Connected:
                    {
                        if (timerCall != null && timerCall.Enabled)
                            timerCall.Stop();
                        Duration = 0;
                        stopAnimation = true;
                        if (timerCall != null)
                        {
                            timerCall.Start();
                        }
                        ShowIncomingCallPanel = false;
                        _currentCall.CallEstablishTime = DateTime.Now;
                        IsMuteOn = _linphoneService.IsCallMuted();
                    }
                    break;
                case VATRPCallState.StreamsRunning:
                    SubscribeCallStatistics();
                    break;
                case VATRPCallState.Closed:
                    stopAnimation = true;
                    OnCallClosed();
                    break;
                case VATRPCallState.Error:
                    stopAnimation = true;
                    CallState = VATRPCallState.Error;
                    OnCallClosed();
                    break;
            }

            if (stopAnimation)
            {
                StopAnimation();
            }
        }



        #endregion

        internal void AcceptCall()
        {
#if DEBUG
            if (autoAnswerTimer.Enabled)
            {
                AutoAnswer = 0;
                autoAnswerTimer.Stop();
            }
#endif
            StopAnimation();

            //Hide();

            CallState = VATRPCallState.Connected;
            ShowIncomingCallPanel = false;
            IsMuteOn = false;

            SetTimeout(delegate
            {
                if (_currentCall != null)
                {
                    try
                    {
                        _linphoneService.AcceptCall(_currentCall.NativeCallPtr,
                            ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                                Configuration.ConfEntry.USE_RTT, true));
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("AcceptCall", ex);
                    }
                }
            }, 5);
        }

        internal void DeclineCall()
        {
#if DEBUG
                if (autoAnswerTimer.Enabled)
                {
                    AutoAnswer = 0;
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
                }, 5);
        }

        public void SubscribeCallStatistics()
        {
            if (subscribedForStats)
                return;
            subscribedForStats = true;
            ServiceManager.Instance.LinphoneService.CallStatisticsChangedEvent += _callInfoViewModel.OnCallStatisticsChanged;
        }

        public void UnsubscribeCallStaistics()
        {
            if (!subscribedForStats)
                return;
            subscribedForStats = false;
            ServiceManager.Instance.LinphoneService.CallStatisticsChangedEvent -= _callInfoViewModel.OnCallStatisticsChanged;
        }

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

        internal void SwitchSelfVideo()
        {
            _linphoneService.SwitchSelfVideo();
        }
    }
}