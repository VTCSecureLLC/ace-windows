using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using VATRP.App.ViewModel;
using VATRP.Core.Interfaces;

namespace VATRP.App.CustomControls
{
    /// <summary>
    /// Interaction logic for DialPad.xaml
    /// </summary>
    public partial class DialPadScreen : UserControl
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DialPadScreen));
        private readonly IConfigurationService configurationService;
        private bool isRegistered;
        private bool isSeven = false;
        internal string conf = "";
        internal string confText = "";
        internal string resText = "";
        private DialpadViewModel mViewModel;
        private readonly MainWindow mainWindow;
        public DialPadScreen():
            this(null, null)
        {
            
        }

        public DialPadScreen(MainWindow wnd, DockPanel scrPanel)
        {
            InitializeComponent();
            this.mainWindow = wnd;
            DataContext = ViewModel;
        }

       public DialpadViewModel ViewModel
        {
            get { return mViewModel ?? (mViewModel = new DialpadViewModel()); }
        }

        #region KeyPad

        private void buttonKeyPad_Click(object sender, RoutedEventArgs e)
        {
            int oldNumberLendth = ViewModel.RemotePartyNumber.Length;

            if (Equals(e.OriginalSource, buttonKeyPad0))
            {
                ViewModel.RemotePartyNumber += "0";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad1))
            {
                ViewModel.RemotePartyNumber += "1";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad2))
            {
                ViewModel.RemotePartyNumber += "2";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad3))
            {
                ViewModel.RemotePartyNumber += "3";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad4))
            {
                ViewModel.RemotePartyNumber += "4";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad5))
            {
                ViewModel.RemotePartyNumber += "5";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad6))
            {
                ViewModel.RemotePartyNumber += "6";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad7))
            {
                ViewModel.RemotePartyNumber += "7";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad8))
            {
                ViewModel.RemotePartyNumber += "8";
            }
            else if (Equals(e.OriginalSource, buttonKeyPad9))
            {
                ViewModel.RemotePartyNumber += "9";
            }
            else if (Equals(e.OriginalSource, buttonKeyPadStar))
            {
                ViewModel.RemotePartyNumber += "*";
            }
            else if (Equals(e.OriginalSource, buttonKeyPadSharp))
            {
                ViewModel.RemotePartyNumber += "#";
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
            //if (e.Key == Key.Enter) call to number
                
            
           
        }


    }
        
   
}
