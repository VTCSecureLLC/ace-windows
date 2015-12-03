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
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(CallViewCtrl));
        private CallViewModel _viewModel;
        private MainControllerViewModel _parentViewModel;
        #endregion
        
        #region Properties
        public KeyPadCtrl KeypadCtrl { get; set; }

        public MainControllerViewModel ParentViewModel
        {
            get { return _parentViewModel; }
            set { _parentViewModel = value; }
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
        #endregion

        public CallViewCtrl()
        {
            InitializeComponent();
            DataContext = _viewModel;
            ctrlOverlay.CommandOverlayWidth = 550;
            ctrlOverlay.CommandOverlayHeight = 550;

            ctrlOverlay.NumpadOverlayWidth = 229;
            ctrlOverlay.NumpadOverlayHeight = 305;

            ctrlOverlay.CallInfoOverlayWidth = 550;
            ctrlOverlay.CallInfoOverlayHeight = 200;
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

        internal void EndCall()
        {
            if (_parentViewModel != null) 
                _parentViewModel.TerminateCall(_viewModel);
        }

        internal void MuteCall()
        {
            if (_viewModel.ActiveCall != null )
                _viewModel.MuteCall();
        }

        private void OnEndCall(object sender, RoutedEventArgs e)
        {
            EndCall();
        }

        private void OnSwitchVideo(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null) 
                _viewModel.SwitchSelfVideo();
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

        internal void ToggleKeypadView()
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
        private void OnToggleInfo(object sender, RoutedEventArgs e)
        {
            if (CallInfoToggled != null)
                CallInfoToggled(BtnInfo.IsChecked ?? false);
            if (_viewModel != null)
                _viewModel.ToggleCallStatisticsInfo(BtnInfo.IsChecked ?? false);
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
        }

        private void OnToggleSpeaker(object sender, RoutedEventArgs e)
        {
            if (SpeakerOnToggled != null)
                SpeakerOnToggled(BtnSpeaker.IsChecked ?? false);
        }

        private void OnToggleRTT(object sender, RoutedEventArgs e)
        {
            if (RttToggled != null)
                RttToggled(BtnRTT.IsChecked ?? false);
        }

        private void OnToggleKeypad(object sender, RoutedEventArgs e)
        {
            if (NumpadToggled != null)
                NumpadToggled(BtnNumpad.IsChecked ?? false);
            ctrlOverlay.ShowNumpadWindow(BtnNumpad.IsChecked ?? false);
            //ToggleKeypadView();
        }

        private void OnMute(object sender, RoutedEventArgs e)
        {
            if (MuteOnToggled != null)
                MuteOnToggled(BtnMuteOn.IsChecked ?? false);
            MuteCall();
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
            _viewModel.SaveStates();
            if (_parentViewModel != null)
                _parentViewModel.AcceptCall(_viewModel);
        }

        private void AcceptAndEndCall(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveStates();
            if (_parentViewModel != null)
                _parentViewModel.EndAndAcceptCall(_viewModel);
        }
        
        internal void UpdateControls()
        {
            if (_viewModel != null)
            {
                _viewModel.LoadStates();
                _viewModel.IsMuteOn = false;
                BtnMuteOn.IsChecked = _viewModel.IsMuteOn;
                BtnVideoOn.IsChecked = _viewModel.IsVideoOn;
                BtnSpeaker.IsChecked = _viewModel.IsSpeakerOn;
                BtnNumpad.IsChecked = _viewModel.IsNumpadOn;
                BtnRTT.IsChecked = _viewModel.IsRttOn;
                BtnInfo.IsChecked = _viewModel.IsCallInfoOn;

                _viewModel.ToggleCallStatisticsInfo(BtnInfo.IsChecked ?? false);

            }

            if (RttToggled != null)
                RttToggled(BtnRTT.IsChecked ?? false);
            ctrlOverlay.ShowNumpadWindow(BtnNumpad.IsChecked ?? false);
        }
    }
    
}
