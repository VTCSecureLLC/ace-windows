using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using log4net;
using VATRP.App.Services;
using VATRP.Core.Model;

namespace VATRP.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Members
        private static readonly ILog _log = LogManager.GetLogger(typeof(App));
        private static bool _allowDestroyWindows = false;
        #endregion

        #region Properties
        public static bool AllowDestroyWindows
        {
            get { return _allowDestroyWindows; }
            set { _allowDestroyWindows = value; }
        }
        public static Core.Model.VATRPAccount CurrentAccount { get; set; }
        public static bool CanMakeVideoCall { get; set; }

        internal static VATRPCallEvent ActiveCallHistoryEvent { get; set; }
        #endregion

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _log.Info("====================================================");
            _log.Info(String.Format("====== Starting VATRP v{0} ====",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version));
            _log.Info("====================================================");
            try
            {
                CurrentAccount = null;
                AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                this.StartupUri = new Uri("MainWindow.xaml", UriKind.RelativeOrAbsolute);

                var culture = new CultureInfo("en-US");
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

                if (!ServiceManager.Instance.Initialize())
                {
                    MessageBox.Show("Failed to initialize service manager");
                    this.Shutdown();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("App Global error:" + error.Message);
            }

        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _log.Debug("Not handled exception: " + e.Exception.ToString() + "\n" + e.Exception.StackTrace);
        }
    }
}
