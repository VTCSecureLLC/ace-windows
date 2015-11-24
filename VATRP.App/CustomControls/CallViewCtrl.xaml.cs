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
    public partial class CallViewCtrl : System.Windows.Controls.UserControl
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(CallViewCtrl));
        private CallViewModel _viewModel;
        #endregion
        
        #region Properties
        public KeyPadCtrl KeypadCtrl { get; set; }
        
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
            ctrlOverlay.CommandOverlayWidth = 550;
            ctrlOverlay.CommandOverlayHeight = 550;

            ctrlOverlay.NumpadOverlayWidth = 229;
            ctrlOverlay.NumpadOverlayHeight = 305;

            ctrlOverlay.CallInfoOverlayWidth = 550;
            ctrlOverlay.CallInfoOverlayHeight = 200;
        }

        public CallViewCtrl(CallViewModel viewModel) :
            this()
        {
            SetCallViewModel(viewModel);
        }

        public void SetCallViewModel(CallViewModel viewModel)
        {
            DataContext = viewModel;
            _viewModel = viewModel;

            BtnMuteOn.IsChecked = false;
            BtnVideoOn.IsChecked = false;
            BtnSpeaker.IsChecked = false;
            BtnNumpad.IsChecked = false;
            BtnRTT.IsChecked = false;
            BtnInfo.IsChecked = false;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        internal void EndCall()
        {
            _viewModel.TerminateCall();
        }

        internal void MuteCall()
        {
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
            if (_viewModel != null)
                _viewModel.AcceptCall();
        }

        private void DeclineCall(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.DeclineCall();

            
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
    }
    
}
