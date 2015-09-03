using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using VATRP.App.Model;
using VATRP.App.Services;
using VATRP.App.ViewModel;
using VATRP.Core.Interfaces;

namespace VATRP.App.CustomControls
{
    /// <summary>
    /// Interaction logic for DialPad.xaml
    /// </summary>
    public partial class DialPadScreen : UserControl
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DialPadScreen));
        private DialpadViewModel mViewModel;
        private readonly MainWindow mainWindow;
        public event EventHandler<KeyPadEventArgs> KeypadPressed;

        #endregion
        public DialPadScreen():
            this(null)
        {
            
        }

        public DialPadScreen(MainWindow wnd)
        {
            DataContext = ViewModel;
            InitializeComponent();
            this.mainWindow = wnd;
        }

       public DialpadViewModel ViewModel
        {
            get { return mViewModel ?? (mViewModel = new DialpadViewModel()); }
        }

        #region KeyPad

        private void buttonKeyPad_Click(object sender, RoutedEventArgs e)
        {
            int oldNumberLendth = ViewModel.RemotePartyNumber.Length;
            var key = DialpadKey.DialpadKey_KeyNone;

            if (Equals(e.OriginalSource, buttonKeyPad0))
            {
                key = DialpadKey.DialpadKey_Key0;
                ViewModel.RemotePartyNumber += "0";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad1))
            {
                key = DialpadKey.DialpadKey_Key1;
                ViewModel.RemotePartyNumber += "1";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad2))
            {
                key = DialpadKey.DialpadKey_Key2;
                ViewModel.RemotePartyNumber += "2";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad3))
            {
                key = DialpadKey.DialpadKey_Key3;
                ViewModel.RemotePartyNumber += "3";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad4))
            {
                key = DialpadKey.DialpadKey_Key4;
                ViewModel.RemotePartyNumber += "4";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad5))
            {
                key = DialpadKey.DialpadKey_Key5;
                ViewModel.RemotePartyNumber += "5";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad6))
            {
                key = DialpadKey.DialpadKey_Key6;
                ViewModel.RemotePartyNumber += "6";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad7))
            {
                key = DialpadKey.DialpadKey_Key7;
                ViewModel.RemotePartyNumber += "7";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad8))
            {
                key = DialpadKey.DialpadKey_Key8;
                ViewModel.RemotePartyNumber += "8";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad9))
            {
                key = DialpadKey.DialpadKey_Key9;
                ViewModel.RemotePartyNumber += "9";
            }
            else if (Equals(e.OriginalSource, buttonKeyPadStar))
            {
                key = DialpadKey.DialpadKey_KeyStar;
                ViewModel.RemotePartyNumber += "*";
            }
            else if (Equals(e.OriginalSource, buttonKeyPadSharp))
            {
                key = DialpadKey.DialpadKey_KeyPound;
                ViewModel.RemotePartyNumber += "#";
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
            LOG.Debug("*************** Dialpad page OnLoaded ********* ");
            this.ViewModel.RemotePartyNumber = string.Empty;
        }

       private void OnUnloaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void NumberTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void NumberTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void numberTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    MediaActionHandler.MakeVideoCall(numberTextBox.Text);
            }
            catch (Exception ex)
            {
                
            }
        }
        
        private void AudioCallClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(numberTextBox.Text))
            {
                MessageBox.Show("Destination address is empty", "VATRP", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MediaActionHandler.MakeAudioCall(numberTextBox.Text);
        }

        private void VideoCallClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(numberTextBox.Text))
            {
                MessageBox.Show("Destination address is empty", "VATRP", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MediaActionHandler.MakeVideoCall(numberTextBox.Text);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }
    }
        
   
}
