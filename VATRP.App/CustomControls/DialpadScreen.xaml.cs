using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for DialPad.xaml
    /// </summary>
    public partial class DialPadScreen 
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DialPadScreen));
        private DialpadViewModel _viewModel;
        public event EventHandler<KeyPadEventArgs> KeypadPressed;
        private bool plusButtonHold;
        private System.Windows.Forms.Timer timerHold;
        #endregion

        public DialPadScreen()
        {
            InitializeComponent();
            timerHold = new System.Windows.Forms.Timer()
            {
                Interval = 300
            };

            timerHold.Tick += TimerHoldOnTick;
        }

        public void SetViewModel(DialpadViewModel viewModel)
        {
            DataContext = viewModel;
            _viewModel = viewModel;
        }

        #region KeyPad

        private void buttonKeyPad_Click(object sender, RoutedEventArgs e)
        {
            int oldNumberLendth = _viewModel.RemotePartyNumber.Length;
            var key = DialpadKey.DialpadKey_KeyNone;

            if (Equals(e.OriginalSource, buttonKeyPad0))
            {
                if (plusButtonHold)
                {
                    plusButtonHold = false;
                    return;
                }

                key = DialpadKey.DialpadKey_Key0;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad1))
            {
                key = DialpadKey.DialpadKey_Key1;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad2))
            {
                key = DialpadKey.DialpadKey_Key2;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad3))
            {
                key = DialpadKey.DialpadKey_Key3;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad4))
            {
                key = DialpadKey.DialpadKey_Key4;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad5))
            {
                key = DialpadKey.DialpadKey_Key5;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad6))
            {
                key = DialpadKey.DialpadKey_Key6;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad7))
            {
                key = DialpadKey.DialpadKey_Key7;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad8))
            {
                key = DialpadKey.DialpadKey_Key8;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad9))
            {
                key = DialpadKey.DialpadKey_Key9;
            }
            else if (Equals(e.OriginalSource, buttonKeyPadStar))
            {
                key = DialpadKey.DialpadKey_KeyStar;
            }
            else if (Equals(e.OriginalSource, buttonKeyPadSharp))
            {
                key = DialpadKey.DialpadKey_KeyPound;
            }

            if (key != DialpadKey.DialpadKey_KeyNone)
            {
                _viewModel.RemotePartyNumber += Convert.ToChar(key);
                if (KeypadPressed != null)
                {
                    var args = new KeyPadEventArgs(key);
                    KeypadPressed(this, args);
                }
            }

        }

        #endregion
        
        private void VideoCallClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.RemotePartyNumber))
            {
                MessageBox.Show("Destination address is empty", "ACE", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MediaActionHandler.MakeVideoCall(_viewModel.RemotePartyNumber);
            _viewModel.RemotePartyNumber = "";
        }

        private void OnBackSpaceClicked(object sender, MouseButtonEventArgs e)
        {
            if (!String.IsNullOrEmpty(_viewModel.RemotePartyNumber))
            {
                _viewModel.RemotePartyNumber = _viewModel.RemotePartyNumber.Substring(0,
                    _viewModel.RemotePartyNumber.Length - 1);
            }
        }

        private void OnPlusPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (timerHold != null)
                timerHold.Enabled = false;
        }

        private void TimerHoldOnTick(object sender, EventArgs e)
        {
            timerHold.Stop();
            _viewModel.RemotePartyNumber += "+";
            plusButtonHold = true;
        }

        private void OnPlusPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (timerHold != null)
                timerHold.Enabled = true;
        }

        private void OnDialpadPreviewKeyup(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;
            try
            {
                var remote = _viewModel.RemotePartyNumber.Trim();
                if (remote.NotBlank())
                {
                    if (App.CurrentAccount != null)
                    {
                        remote = string.Format("sip:{0}@{1}", remote, App.CurrentAccount.ProxyHostname);
                        if (MediaActionHandler.MakeVideoCall(remote))
                            _viewModel.RemotePartyNumber = "";
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("OnDialString", ex);
            }
        }

        private void OnDialpadPreviewKeydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                e.Handled = true; // prevent further processing
        }
    }
}
