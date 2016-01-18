using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using com.vtcsecure.ace.windows.Views;
using log4net;
using VATRP.Core.Model;
using Win32Api = com.vtcsecure.ace.windows.Services.Win32NativeAPI;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for CallViewCtrl.xaml
    /// </summary>
    public partial class CallViewCtrl
    {
        public UnifiedSettings.UnifiedSettingsCtrl SettingsControl;

        #region Members

        private static readonly ILog LOG = LogManager.GetLogger(typeof (CallViewCtrl));
        private CallViewModel _viewModel;
        private MainControllerViewModel _parentViewModel;
        private CallViewModel _backgroundCallViewModel;
        private DispatcherTimer _mouseInactivityTimer;
        private bool _controlsHiddenByTimer = false;
        private bool _showControlsOnTimeout = false;
        private bool restoreVisibilityStates = false;
        private System.Drawing.Point _lastMousePosition;
        private bool _inactivityTimerStopped;
        private bool _isFullScreenOn;
        #endregion

        #region Properties

        public KeyPadCtrl KeypadCtrl { get; set; }

        public MainControllerViewModel ParentViewModel
        {
            get { return _parentViewModel; }
            set { _parentViewModel = value; }
        }

        public CallViewModel BackgroundCallViewModel
        {
            get { return _backgroundCallViewModel; }
            set { _backgroundCallViewModel = value; }
        }

        #endregion

        #region Events

        public delegate void SwitchCallbarButton(bool switch_on);

        public event SwitchCallbarButton VideoOnToggled;
        public event SwitchCallbarButton FullScreenOnToggled;
        public event SwitchCallbarButton MuteOnToggled;
        public event SwitchCallbarButton SpeakerOnToggled;
        public event SwitchCallbarButton NumpadToggled;
        public event SwitchCallbarButton RttToggled;
        public event SwitchCallbarButton CallInfoToggled;
        public event EventHandler<KeyPadEventArgs> KeypadClicked;
        public event EventHandler SwitchHoldCallsRequested;
        private bool _mouseInControlArea = false;

        #endregion

        public CallViewCtrl()
        {
            InitializeComponent();
            DataContext = _viewModel;
            ctrlOverlay.CommandOverlayWidth = 660;
            ctrlOverlay.CommandOverlayHeight = 160;

            ctrlOverlay.NumpadOverlayWidth = 229;
            ctrlOverlay.NumpadOverlayHeight = 305;

            ctrlOverlay.CallInfoOverlayWidth = 660;
            ctrlOverlay.CallInfoOverlayHeight = 200;

            ctrlOverlay.NewCallAcceptOverlayWidth = 370;
            ctrlOverlay.NewCallAcceptOverlayHeight = 160;

            ctrlOverlay.CallsSwitchOverlayWidth = 190;
            ctrlOverlay.CallsSwitchOverlayHeight = 200;

            _mouseInactivityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3),
            };
            _mouseInactivityTimer.Tick += OnMouseInactivityTimer;
        }

        public CallViewCtrl(MainControllerViewModel parentVM) : this()
        {
            _parentViewModel = parentVM;
        }

        public void SetCallViewModel(CallViewModel viewModel)
        {
            if (_viewModel == viewModel)
                return;
            DataContext = viewModel;
            _viewModel = viewModel;

            UpdateControls();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        internal void EndCall(bool bRunning)
        {
            if (_parentViewModel != null)
            {
                _parentViewModel.TerminateCall(bRunning ? _viewModel : _backgroundCallViewModel);
            }
        }

        internal void MuteSpeaker(bool isMuted)
        {
            if (_viewModel.ActiveCall != null)
                _viewModel.MuteSpeaker(isMuted);
            if (SettingsControl != null)
            {
                SettingsControl.RespondToMenuUpdate(Enums.ACEMenuSettingsUpdateType.MuteSpeakerMenu);
            }

        }

        internal void MuteCall(bool isMuted)
        {
            if (_viewModel.ActiveCall != null)
                _viewModel.MuteCall(isMuted);
            if (SettingsControl != null)
            {
                SettingsControl.RespondToMenuUpdate(Enums.ACEMenuSettingsUpdateType.MuteMicrophoneMenu);
            }

        }

        private void OnEndCall(object sender, RoutedEventArgs e)
        {
            EndCall(true);
        }

        private void OnEndPaused(object sender, RoutedEventArgs e)
        {
            EndCall(false);
        }

        public void AcceptCall(object sender, RoutedEventArgs e)
        {
            if (_parentViewModel != null)
                _parentViewModel.AcceptCall(_viewModel);
        }

        private void DeclineCall(object sender, RoutedEventArgs e)
        {
            if (_parentViewModel != null)
                _parentViewModel.DeclineCall(_viewModel);
        }

        #region Call Statistics Info

        private void OnToggleInfo(object sender, RoutedEventArgs e)
        {
            if (CallInfoToggled != null)
                CallInfoToggled(BtnInfo.IsChecked ?? false);
            if (_viewModel != null)
                _viewModel.ToggleCallStatisticsInfo(BtnInfo.IsChecked ?? false);
            SaveStates();
        }

        #endregion


        internal void AddVideoControl()
        {
            try
            {
                ServiceManager.Instance.LinphoneService.SetVideoCallWindowHandle(ctrlVideo.GetVideoControlPtr);
                ctrlVideo.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("Main OnCallStateChanged", ex);
            }
        }

        private void OnToggleFullScreen(object sender, RoutedEventArgs e)
        {
            if (FullScreenOnToggled != null)
                FullScreenOnToggled(BtnFullScreen.IsChecked ?? false);
            SaveStates();
        }

        private void OnToggleVideo(object sender, RoutedEventArgs e)
        {
            if (VideoOnToggled != null)
                VideoOnToggled(BtnVideoOn.IsChecked ?? false);
            if (_viewModel != null)
                _viewModel.ToggleVideo(!BtnVideoOn.IsChecked ?? false);
            SaveStates();
        }

        private void OnToggleSpeaker(object sender, RoutedEventArgs e)
        {
            if (SpeakerOnToggled != null)
                SpeakerOnToggled(BtnSpeaker.IsChecked ?? false);
            SaveStates();
            MuteSpeaker(BtnSpeaker.IsChecked ?? false);
        }

        private void OnToggleRTT(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsRTTEnabled)
            {
                _inactivityTimerStopped = true;
                _mouseInactivityTimer.Stop();
                MessageBox.Show("RTT has been disabled for this call", "ACE", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                RestartInactivityDetectionTimer();
                BtnRTT.IsChecked = false;
                return;
            }

            if (RttToggled != null)
                RttToggled(BtnRTT.IsChecked ?? false);
            SaveStates();
        }

        private void OnToggleKeypad(object sender, RoutedEventArgs e)
        {
            if (NumpadToggled != null)
                NumpadToggled(BtnNumpad.IsChecked ?? false);
            ctrlOverlay.ShowNumpadWindow(BtnNumpad.IsChecked ?? false);
            SaveStates();
        }

        private void OnMute(object sender, RoutedEventArgs e)
        {
            if (MuteOnToggled != null)
                MuteOnToggled(BtnMuteOn.IsChecked ?? false);
            SaveStates();
            MuteCall(BtnMuteOn.IsChecked ?? false);
        }

        private void buttonKeyPad(object sender, RoutedEventArgs e)
        {
            if (KeypadClicked != null)
            {
                var btnKey = e.OriginalSource as Button;
                if (btnKey != null)
                {
                    if (Equals(e.OriginalSource, buttonKeyPadStar))
                    {
                        KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_KeyStar));
                    }
                    else if (Equals(e.OriginalSource, buttonKeyPadPound))
                    {
                        KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_KeyPound));
                    }
                    else
                    {
                        char key;
                        if (char.TryParse(btnKey.Tag.ToString(), out key))
                            KeypadClicked(this, new KeyPadEventArgs((DialpadKey) key));
                        else
                        {
                            KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_Key0));
                        }
                    }
                }
            }
        }

        public void HoldAndAcceptCall(object sender, RoutedEventArgs e)
        {
            if (_parentViewModel != null)
                _parentViewModel.AcceptCall(_viewModel);
        }

        private void AcceptAndEndCall(object sender, RoutedEventArgs e)
        {
            SaveStates();
            if (_parentViewModel != null)
                _parentViewModel.EndAndAcceptCall(_viewModel);
        }

        private void SaveStates()
        {
            _viewModel.SavedIsVideoOn = BtnVideoOn.IsChecked ?? false;
            _viewModel.SavedIsMuteOn = BtnMuteOn.IsChecked ?? false;
            _viewModel.SavedIsSpeakerOn = BtnSpeaker.IsChecked ?? false;
            _viewModel.SavedIsNumpadOn = BtnNumpad.IsChecked ?? false;
            _viewModel.SavedIsRttOn = BtnRTT.IsChecked ?? false;
            _viewModel.SavedIsInfoOn = BtnInfo.IsChecked ?? false;
            _viewModel.SavedIsCallHoldOn = BtnHold.IsChecked ?? false;
        }

        private void LoadStates()
        {
            _viewModel.IsVideoOn = _viewModel.SavedIsVideoOn;
            _viewModel.IsMuteOn = _viewModel.SavedIsMuteOn;
            _viewModel.IsSpeakerOn = _viewModel.SavedIsSpeakerOn;
            _viewModel.IsNumpadOn = _viewModel.SavedIsNumpadOn;
            _viewModel.IsRttOn = _viewModel.SavedIsRttOn && _viewModel.IsRTTEnabled;
            _viewModel.IsCallInfoOn = _viewModel.SavedIsInfoOn;
            _viewModel.IsCallOnHold = _viewModel.SavedIsCallHoldOn;
        }

        internal void UpdateControls()
        {
            if (_viewModel != null)
            {
                LoadStates();
                // do not force this to false. make sure that the call is muted if this setting is 
//                _viewModel.IsMuteOn = false;
                BtnMuteOn.IsChecked = _viewModel.IsMuteOn;
                BtnVideoOn.IsChecked = _viewModel.IsVideoOn;
                BtnSpeaker.IsChecked = _viewModel.IsSpeakerOn;
                BtnNumpad.IsChecked = _viewModel.IsNumpadOn;
                BtnRTT.IsChecked = _viewModel.IsRttOn;
                BtnInfo.IsChecked = _viewModel.IsCallInfoOn;
                BtnHold.IsChecked = _viewModel.IsCallOnHold;
                _viewModel.ToggleCallStatisticsInfo(BtnInfo.IsChecked ?? false);
            }

            if (RttToggled != null)
                RttToggled(BtnRTT.IsChecked ?? false);
            ctrlOverlay.ShowNumpadWindow(BtnNumpad.IsChecked ?? false);

            bool rttEnabled = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, true);
            EnableRTTButton(rttEnabled);
            UpdateVideoSettingsIfOpen();
        }



        private void OnDeclineCall(object sender, RoutedEventArgs e)
        {
            if (_parentViewModel != null)
                _parentViewModel.DeclineCall(_viewModel);
        }

        private void SwitchCall(object sender, RoutedEventArgs e)
        {
            if (_parentViewModel != null)
            {
                if (!_parentViewModel.SwitchCall(_backgroundCallViewModel, _viewModel))
                {
                    if (SwitchHoldCallsRequested != null)
                        SwitchHoldCallsRequested(this, EventArgs.Empty);
                }
            }
        }

        private void OnToggleHold(object sender, RoutedEventArgs e)
        {
            if (_parentViewModel != null)
            {
                if (_viewModel.CallState == VATRPCallState.RemotePaused)
                {
                    BtnHold.IsChecked = false;
                    SaveStates();
                }
                else
                {
                    SaveStates();
                    if (BtnHold.IsChecked ?? false)
                    {
                        _viewModel.PauseRequest = true;
                        _parentViewModel.PauseCall(_viewModel);
                    }
                    else
                    {
                        _viewModel.PauseRequest = false;
                        _viewModel.ResumeRequest = true;
                        _parentViewModel.ResumeCall(_viewModel);
                    }
                }
            }
        }

        public bool IsRTTViewShown()
        {
            return this.BtnRTT.IsChecked ?? false;
        }

        public void UpdateRTTToggle(bool enable)
        {
            if ((BtnRTT.IsChecked ?? false) != enable)
            {
                BtnRTT.IsChecked = enable;
            }
        }

        public void EnableRTTButton(bool enable)
        {
            BtnRTT.IsEnabled = enable;

            if ((BtnRTT.IsChecked ?? false))
            {
                BtnRTT.IsChecked = false;
                if (RttToggled != null)
                    RttToggled(false);
            }
        }

        public void UpdateMuteSettingsIfOpen()
        {
            if (App.CurrentAccount != null)
            {
                this.BtnMuteOn.IsChecked = App.CurrentAccount.MuteMicrophone;
                this.BtnSpeaker.IsChecked = App.CurrentAccount.MuteSpeaker;
            }
        }

        public void UpdateVideoSettingsIfOpen()
        {
            if (_viewModel != null && _viewModel.ActiveCall != null)
            {
                var isVideoEnabled = ServiceManager.Instance.LinphoneService.IsVideoEnabled(_viewModel.ActiveCall);
                this.BtnVideoOn.IsEnabled = isVideoEnabled;
                this.BtnVideoOn.IsChecked = !isVideoEnabled ||
                                            !ServiceManager.Instance.LinphoneService.IsCameraEnabled(
                                                _viewModel.ActiveCall.NativeCallPtr);
            }
            else
            {
                this.BtnVideoOn.IsChecked = true;
                this.BtnVideoOn.IsEnabled = false;
            }
        }

        private void RestoreControlsVisibility()
        {
            if (!restoreVisibilityStates)
                return;

            //Debug.WriteLine("ShowControl: " + opt);
            // Show controls with their last visibility state
            if (_viewModel != null)
            {
                if (_viewModel.CallInfoLastTimeVisibility == Visibility.Visible)
                {
                    ctrlOverlay.ShowCallInfoWindow(true);
                }

                if (_viewModel.CommandbarLastTimeVisibility == Visibility.Visible)
                {
                    ctrlOverlay.ShowCommandBar(true);
                }

                if (_viewModel.CallSwitchLastTimeVisibility == Visibility.Visible)
                {
                    ctrlOverlay.ShowCallsSwitchWindow(true);
                }

                if (_viewModel.NumpadLastTimeVisibility == Visibility.Visible)
                {
                    ctrlOverlay.ShowNumpadWindow(true);
                }
            }
            restoreVisibilityStates = false;
        }

        private void HideOverlayControls()
        {
            if (_viewModel == null || restoreVisibilityStates)
                return;

            var wndObject = ctrlOverlay.CallInfoWindow;
            if (wndObject != null)
            {
                _viewModel.CallInfoLastTimeVisibility = wndObject.ShowWindow
                    ? Visibility.Visible
                    : Visibility.Hidden;
                if (wndObject.ShowWindow)
                {
                    _viewModel.CallInfoLastTimeVisibility = Visibility.Visible;
                    ctrlOverlay.ShowCallInfoWindow(false);
                }
            }

            wndObject = ctrlOverlay.CommandBarWindow;
            if (wndObject != null)
            {
                _viewModel.CommandbarLastTimeVisibility = wndObject.ShowWindow
                    ? Visibility.Visible
                    : Visibility.Hidden;
                if (wndObject.ShowWindow)
                {
                    _viewModel.CommandbarLastTimeVisibility = Visibility.Visible;
                    ctrlOverlay.ShowCommandBar(false);
                }
            }

            wndObject = ctrlOverlay.CallsSwitchWindow;
            if (wndObject != null)
            {
                _viewModel.CallSwitchLastTimeVisibility = wndObject.ShowWindow
                    ? Visibility.Visible
                    : Visibility.Hidden;
                if (wndObject.ShowWindow)
                {
                    _viewModel.CallSwitchLastTimeVisibility = Visibility.Visible;
                    ctrlOverlay.ShowCallsSwitchWindow(false);
                }
            }

            wndObject = ctrlOverlay.NumpadWindow;
            if (wndObject != null)
            {
                _viewModel.NumpadLastTimeVisibility = wndObject.ShowWindow
                    ? Visibility.Visible
                    : Visibility.Hidden;
                if (wndObject.ShowWindow)
                {
                    _viewModel.NumpadLastTimeVisibility = Visibility.Visible;
                    ctrlOverlay.ShowNumpadWindow(false);
                }
            }

            restoreVisibilityStates = true;
        }

        private void CtrlVideo_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel == null || !_viewModel.AllowHideContorls)
                return;

            Win32Api.POINT mousePositionInControl;
            Win32Api.GetCursorPos(out mousePositionInControl);

            //Debug.WriteLine("Current:{0} {1} Last: x:{2} y:{3}", mousePositionInControl.X, mousePositionInControl.Y,
            //    _lastMousePosition.X, _lastMousePosition.Y);
            if (_lastMousePosition.X != mousePositionInControl.X ||
                _lastMousePosition.Y != mousePositionInControl.Y)
            {
                _mouseInControlArea = false;
                RestoreControlsVisibility();
                RestartInactivityDetectionTimer();
            }
            _lastMousePosition = mousePositionInControl;
        }
        
        private void CtrlVideo_OnMouseEnter(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine("OnMouseEnter: Restore Visibility - " + restoreVisibilityStates);
            if (_viewModel == null || !_viewModel.AllowHideContorls)
                return;

            Win32Api.POINT mousePositionInControl;
            Win32Api.GetCursorPos(out mousePositionInControl);

            if (_lastMousePosition.X == mousePositionInControl.X &&
                _lastMousePosition.Y == mousePositionInControl.Y)
            {
                //Debug.WriteLine("Unchanged coordinates. Should be skipped. Control area: " + _mouseInControlArea);
                if (restoreVisibilityStates)
                {
                    if (_mouseInactivityTimer.IsEnabled)
                    {
                        _inactivityTimerStopped = true;
                        _mouseInactivityTimer.Stop();
                    }
                }
                return;
            }

            _lastMousePosition = mousePositionInControl;
            if (!_mouseInControlArea)
                RestoreControlsVisibility();
            RestartInactivityDetectionTimer();
        }

        private void CtrlVideo_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_viewModel == null || !_viewModel.AllowHideContorls)
                return;

            Point mousePosition = Mouse.GetPosition(this);

            //Debug.WriteLine("MouseLeave: X = {0}, Y={1}", mousePositionInControl.X, mousePositionInControl.Y); 
            if (mousePosition.X > 0 && mousePosition.Y > 0)
            {
                //Debug.WriteLine("we are in control area, ");
                _mouseInControlArea = true;
            }

            Win32Api.POINT mousePositionInControl;
            Win32Api.GetCursorPos(out mousePositionInControl);
            _lastMousePosition = mousePositionInControl;
            RestartInactivityDetectionTimer();
        }


        private void OnMouseInactivityTimer(object sender, EventArgs e)
        {
            _mouseInactivityTimer.Stop();

            if (_inactivityTimerStopped)
            {
                Debug.WriteLine("Inactivity timer stopped. Do not process ");
                return;
            }

            if (!restoreVisibilityStates)
            {
                Win32Api.POINT mousePositionInControl;
                Win32Api.GetCursorPos(out mousePositionInControl);
                _lastMousePosition = mousePositionInControl;
                HideOverlayControls();
            }
            else
            {
                RestoreControlsVisibility();
            }
        }

        public void RestartInactivityDetectionTimer()
        {
//            Debug.WriteLine("Restart detection timer");
            if (_mouseInactivityTimer != null)
            {
                if (_mouseInactivityTimer.IsEnabled)
                {
                    _inactivityTimerStopped = true;
                    _mouseInactivityTimer.Stop();
                }

                _mouseInactivityTimer.Start();
                _inactivityTimerStopped = false;
            }
        }
        
        public void CheckRttButton()
        {
            _viewModel.IsRttOn = true;
            BtnRTT.IsChecked = _viewModel.IsRttOn;
            _viewModel.SavedIsRttOn = BtnRTT.IsChecked ?? false;
        }
    }
}
