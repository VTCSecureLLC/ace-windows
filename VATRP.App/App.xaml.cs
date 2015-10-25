using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using log4net;
using VATRP.App.Services;
using VATRP.Core.Model;
using HockeyApp;

namespace VATRP.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        #region Members

        private static readonly log4net.ILog _log = LogManager.GetLogger(typeof (App));
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

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //main configuration of HockeySDK
            HockeyClient.Current.Configure(HOCKEYAPP_ID);
                //.UseCustomResourceManager(HockeyApp.ResourceManager) //register your own resourcemanager to override HockeySDK i18n strings
                //.RegisterCustomUnhandledExceptionLogic((eArgs) => { /* do something here */ }) // define a callback that is called after unhandled exception
                //.RegisterCustomUnobserveredTaskExceptionLogic((eArgs) => { /* do something here */ }) // define a callback that is called after unobserved task exception
                //.RegisterCustomDispatcherUnhandledExceptionLogic((args) => { }) // define a callback that is called after dispatcher unhandled exception
                //.SetApiDomain("https://your.hockeyapp.server")
                //.SetContactInfo("John Smith", "email@example.com");

            //optional should only used in debug builds. register an event-handler to get exceptions in HockeySDK code that are "swallowed" (like problems writing crashlogs etc.)
#if DEBUG
            ((HockeyClient) HockeyClient.Current).OnHockeySDKInternalException += (sender, args) =>
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            };
#endif
            try
            {

                //send crashes to the HockeyApp server
               await HockeyClient.Current.SendCrashesAsync();
            }
            catch (Exception eArgs)
            {
                if (_log != null)
                    _log.Error("HockeyApp SendCrashesAsync exception: " + eArgs.ToString());
            }

            //check for updates on the HockeyApp server
            await HockeyClient.Current.CheckForUpdatesAsync(true, () =>
            {
                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                }
                return true;
            });
        }

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            _log.Info("====================================================");
            _log.Info(String.Format("============== Starting VATRP v{0} =============",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version));
            _log.Info("====================================================");
            //try
            //{
                CurrentAccount = null;
                AppDomain.CurrentDomain.SetData("DataDirectory",
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                this.StartupUri = new Uri("MainWindow.xaml", UriKind.RelativeOrAbsolute);

                var culture = new CultureInfo("en-US");
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

                if (!ServiceManager.Instance.Initialize())
                {
                    MessageBox.Show("Failed to initialize service manager");
                    this.Shutdown();
                }
            //}
            //catch (Exception error)
            //{
            //    MessageBox.Show("App Global error:" + error.Message);
            //}
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (_log != null)
                _log.Error("Not handled exception: " + e.Exception.ToString() + "\n" + e.Exception.StackTrace);
        }
    }
}
