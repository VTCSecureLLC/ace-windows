using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VATRP.App.Services;

namespace VATRP.App.CustomControls
{
    /// <summary>
    /// Interaction logic for ProviderLoginScreen.xaml
    /// </summary>
    public partial class ProviderLoginScreen : UserControl
    {

        #region Memebers
        private string _login;
        private string _passCode;
        private bool _rememberPasswd;
        private bool _autoLogin;
        private readonly MainWindow _mainWnd;
        #endregion

        #region Properties
        public bool AutoLogin
        {
            get { return _autoLogin; }
            set { _autoLogin = value; }
        }
        public bool RememberPassword
        {
            get { return _rememberPasswd; }
            set { _rememberPasswd = value; }
        }
        public string PassCode
        {
            get { return _passCode; }
            set { _passCode = value; }
        }
        public string Login
        {
            get { return _login; }
            set { _login = value; }
        }
        #endregion
        public ProviderLoginScreen(MainWindow theMain)
        {
            _mainWnd = theMain;
            InitializeComponent();
        }

        private void OnForgotpassword(object sender, RequestNavigateEventArgs e)
        {
            
        }

        private void OnRegister(object sender, RequestNavigateEventArgs e)
        {
            
        }

        private void LoginCmd_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginBox.Text;
            if (string.IsNullOrEmpty(username))
                return;
            string passwd = PasswdBox.Password;
            if (string.IsNullOrEmpty(passwd))
                return;

            if (!ServiceManager.Instance.RequestLinphoneCredentials(Login, passwd))
                MessageBox.Show("failed to call request");
        }
    }
}
