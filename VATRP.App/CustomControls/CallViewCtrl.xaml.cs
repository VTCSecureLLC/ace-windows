using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using com.vtcsecure.ace.windows.Views;
using log4net;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for CallViewCtrl.xaml
    /// </summary>
    public partial class CallViewCtrl
    {
        public UnifiedSettings.UnifiedSettingsCtrl SettingsControl;
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(CallViewCtrl));
        private CallViewModel _viewModel;
        private MainControllerViewModel _parentViewModel;
        private CallViewModel _backgroundCallViewModel;
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
            set
            {
                _backgroundCallViewModel = value;
            }
        }
        #endregion

        #region Events
        public delegate void SwitchCallbarButton(bool switch_on);
        public event SwitchCallbarButton VideoOnToggled;
        public event SwitchCallbarButton MuteOnToggled;
        public event SwitchCallbarButton SpeakerOnToggled;
        public event SwitchCallbarButton NumpadToggled;
        public event SwitchCallbarButton RttToggled;
        public event SwitchCallbarButton CallInfoToggled;
        public event EventHandler<KeyPadEventArgs> KeypadClicked;
        public event EventHandler SwitchHoldCallsRequested;
        #endregion

        public CallViewCtrl()
        {
            InitializeComponent();
            DataContext = _viewModel;
            ctrlOverlay.CommandOverlayWidth = 660;
            ctrlOverlay.CommandOverlayHeight = 550;

            ctrlOverlay.NumpadOverlayWidth = 229;
            ctrlOverlay.NumpadOverlayHeight = 305;

            ctrlOverlay.CallInfoOverlayWidth = 660;
            ctrlOverlay.CallInfoOverlayHeight = 200;

            ctrlOverlay.NewCallAcceptOverlayWidth = 370;
            ctrlOverlay.NewCallAcceptOverlayHeight = 160;

            ctrlOverlay.CallsSwitchOverlayWidth = 190;
            ctrlOverlay.CallsSwitchOverlayHeight = 200;
        }

        public CallViewCtrl(MainControllerViewModel parentVM):this()
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

        internal void MuteCall(bool isMuted)
        {
            if (_viewModel.ActiveCall != null )
                _viewModel.MuteCall(isMuted);
            if (SettingsControl != null)
            {
                SettingsControl.RespondToMenuUpdate(Enums.ACEMenuSettings.MuteMicrophoneMenu);
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

        private void OnSwitchVideo(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null) 
                _viewModel.SwitchSelfVideo();
            SaveStates();
        }

        private void AcceptCall(object sender, RoutedEventArgs e)
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
        }

        private void OnToggleRTT(object sender, RoutedEventArgs e)
        {
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
                            KeypadClicked(this, new KeyPadEventArgs((DialpadKey)key));
                        else
                        {
                            Debug.WriteLine("Failed to get keypad: " + btnKey.Tag);
                            KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_Key0));
                        }
                    }
                }
            }
        }

        private void HoldAndAcceptCall(object sender, RoutedEventArgs e)
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
            _viewModel.IsRttOn = _viewModel.SavedIsRttOn;
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

        public void UpdateMuteSettingsIfOpen()
        {
            if (App.CurrentAccount != null)
            {
                this.BtnMuteOn.IsChecked = App.CurrentAccount.MuteMicrophone;
            }
        }

    }
    
}
