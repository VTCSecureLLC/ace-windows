using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
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

        #endregion

        public DialPadScreen()
        {
            InitializeComponent();
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
                key = DialpadKey.DialpadKey_Key0;
                _viewModel.RemotePartyNumber += "0";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad1))
            {
                key = DialpadKey.DialpadKey_Key1;
                _viewModel.RemotePartyNumber += "1";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad2))
            {
                key = DialpadKey.DialpadKey_Key2;
                _viewModel.RemotePartyNumber += "2";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad3))
            {
                key = DialpadKey.DialpadKey_Key3;
                _viewModel.RemotePartyNumber += "3";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad4))
            {
                key = DialpadKey.DialpadKey_Key4;
                _viewModel.RemotePartyNumber += "4";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad5))
            {
                key = DialpadKey.DialpadKey_Key5;
                _viewModel.RemotePartyNumber += "5";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad6))
            {
                key = DialpadKey.DialpadKey_Key6;
                _viewModel.RemotePartyNumber += "6";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad7))
            {
                key = DialpadKey.DialpadKey_Key7;
                _viewModel.RemotePartyNumber += "7";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad8))
            {
                key = DialpadKey.DialpadKey_Key8;
                _viewModel.RemotePartyNumber += "8";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad9))
            {
                key = DialpadKey.DialpadKey_Key9;
                _viewModel.RemotePartyNumber += "9";
            }
            else if (Equals(e.OriginalSource, buttonKeyPadStar))
            {
                key = DialpadKey.DialpadKey_KeyStar;
                _viewModel.RemotePartyNumber += "*";
            }
            else if (Equals(e.OriginalSource, buttonKeyPadSharp))
            {
                key = DialpadKey.DialpadKey_KeyPound;
                _viewModel.RemotePartyNumber += "#";
            }

            if (key != DialpadKey.DialpadKey_KeyNone)
            {
                if (KeypadPressed != null)
                {
                    var args = new KeyPadEventArgs(key);

                    KeypadPressed(this, args);
                }
            }

        }

        #endregion
        
       
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

       private void OnUnloaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void numberTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    MediaActionHandler.MakeVideoCall(_viewModel.RemotePartyNumber);
            }
            catch (Exception ex)
            {
                
            }
        }
        
        private void VideoCallClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.RemotePartyNumber))
            {
                MessageBox.Show("Destination address is empty", "ACE", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MediaActionHandler.MakeVideoCall(_viewModel.RemotePartyNumber);

        }

        private void OnBackSpaceClicked(object sender, MouseButtonEventArgs e)
        {
            if (!String.IsNullOrEmpty(_viewModel.RemotePartyNumber))
            {
                _viewModel.RemotePartyNumber = _viewModel.RemotePartyNumber.Substring(0,
                    _viewModel.RemotePartyNumber.Length - 1);
            }
        }
    }
        
   
}
