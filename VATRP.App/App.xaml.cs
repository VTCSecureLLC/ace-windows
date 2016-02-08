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
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using HockeyApp;
using System.Threading;
using com.vtcsecure.ace.windows.Views;

namespace com.vtcsecure.ace.windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        #region Members

        private static readonly log4net.ILog _log = LogManager.GetLogger(typeof (App));
        private static bool _allowDestroyWindows = false;
        private Mutex mutex;
        #endregion

        #region Properties
        public static bool AllowDestroyWindows
        {
            get { return _allowDestroyWindows; }
            set { _allowDestroyWindows = value; }
        }
        public static VATRPAccount CurrentAccount { get; set; }
        public static bool CanMakeVideoCall { get; set; }

        internal static bool AppClosing { get; set; }
        #endregion

        public App()
        {
            try
            {
                Mutex.OpenExisting("Global\\84D29A79-09A3-4CBF-A12A-B15CEF971672");
                MessageBox.Show("Instance already running");
                Environment.Exit(0);
            }
            catch
            {
                mutex=new Mutex ( true, "Global\\84D29A79-09A3-4CBF-A12A-B15CEF971672");
            }
      
        }
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

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _log.Info("====================================================");
            _log.Info(String.Format("============== Starting VATRP v{0} =============",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version));
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            try
            {
                var appDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                if (string.IsNullOrEmpty(appDirectory))
                {
                    MessageBox.Show("Current directory is null", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown(1);
                    return;
                }

                if (currentDirectory != appDirectory )
                {
                    try
                    {
                        System.IO.Directory.SetCurrentDirectory(appDirectory);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to change application directory" + Environment.NewLine + ex.Message, "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown(1);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get application directory" + Environment.NewLine + ex.Message, "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }

            string linphoneLibraryVersion = VATRP.LinphoneWrapper.LinphoneAPI.linphone_core_get_version_asString();
            _log.Info(String.Format("======= LinphoneLib Version v{0} =======",
                linphoneLibraryVersion));

            _log.Info("====================================================");

            CurrentAccount = null;
            AppDomain.CurrentDomain.SetData("DataDirectory",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            var culture = new CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            if (!ServiceManager.Instance.Initialize())
            {
                MessageBox.Show("Failed to initialize service manager");
                this.Shutdown();
            }

            ServiceManager.Instance.Start();
            var mainWnd = new MainWindow();
            this.MainWindow = mainWnd;

            if (ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.SHOW_LEGAL_RELEASE, true))
            {
                LegalReleaseWindow lrWnd = new LegalReleaseWindow();
                var dlgResult = lrWnd.ShowDialog();
                if (dlgResult == null || (bool)!dlgResult)
                {
                    ServiceManager.Instance.Stop();
                    this.Shutdown();
                    return;
                }
                else
                {
                    ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                       Configuration.ConfEntry.SHOW_LEGAL_RELEASE, false);
                    ServiceManager.Instance.ConfigurationService.SaveConfig();
                }
            }

            mainWnd.InitializeMainWindow();
            mainWnd.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            mutex.Dispose();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (_log != null)
                _log.Error("Not handled exception: " + e.Exception.ToString() + "\n" + e.Exception.StackTrace);
        }
    }
}
