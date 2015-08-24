using System;
using System.IO;
using System.Media;
using System.Net;
using System.Windows;
using log4net;
using VATRP.Core.Services;
using VATRP.Core.Interfaces;

namespace VATRP.App.Services
{
    internal class ServiceManager : ServiceManagerBase
    {
        private string _applicationDataPath;
        private static ServiceManager _singleton = null; // only one instance
        private static ILog _log = LogManager.GetLogger(typeof(ServiceManager));
        private IConfigurationService _configurationService;
        private IContactService _contactService;
        private IHistoryService _historyService;
        private ISoundService _soundService;
        private LinphoneService _linphoneSipService;
        private WebClient _webClient;
        private bool _initialized;
        
        public static ServiceManager Instance
        {
            get { return _singleton ?? (_singleton = new ServiceManager()); }
        }

        public string ApplicationDataPath
        {
            get
            {
                if (_applicationDataPath == null)
                {
                    String applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    this._applicationDataPath = Path.Combine(applicationData, "VATRP");
                    Directory.CreateDirectory(this._applicationDataPath);
                }
                return this._applicationDataPath;
            }
        }

        public override string BuildStoragePath(string folder)
        {
            return Path.Combine(this.ApplicationDataPath, folder);
        }

        public override IConfigurationService ConfigurationService
        {
            get { return _configurationService ?? (_configurationService = new XmlConfigurationService(this, true)); }
        }

        public override IContactService ContactService
        {
            get { return _contactService ?? (_contactService = new ContactService(this)); }
        }

        public override IHistoryService HistoryService
        {
            get { return _historyService ?? (_historyService = new HistoryService(this)); }
        }

        public override ISoundService SoundService
        {
            get { return _soundService ?? (_soundService = new SoundService(this)); }
        }

        public LinphoneService LinphoneSipService
        {
            get { return _linphoneSipService ?? (_linphoneSipService = new LinphoneService(this)); }
        }
        public ServiceManager()
        {
            
        }

        public bool Initialize()
        {
            _initialized = true;
            _webClient = new WebClient();
            _webClient.DownloadStringCompleted += CredentialsReceived;

            return true;
        }

        internal bool Start()
        {
            bool retVal = true;
            retVal &= ConfigurationService.Start();
            UpdateLinphoneConfig();
            LinphoneSipService.Start(true);
            return retVal;
        }

        public void UpdateLinphoneConfig()
        {
            LinphoneSipService.LinphoneConfig.ProxyHost = "192.168.24.25";
            LinphoneSipService.LinphoneConfig.ProxyPort = 5060;
            LinphoneSipService.LinphoneConfig.UserAgent = "VATRP";
            LinphoneSipService.LinphoneConfig.Username = "master";
            LinphoneSipService.LinphoneConfig.DisplayName = "John Doe";
            LinphoneSipService.LinphoneConfig.Password = "123456";
        }

        internal void Stop()
        {
            ConfigurationService.Stop();
            LinphoneSipService.Stop();
        }

        internal bool RequestLinphoneCredentials(string username, string passwd)
        {
            if (_webClient == null)
                return false;

            string url = "https://crm.videoremoteassistance.com";
            var myCache =  new CredentialCache();
            myCache.Add(new Uri(url), "Basic", new NetworkCredential(username, passwd));

            _webClient.Credentials = myCache;

            try
            {
                _webClient.DownloadStringAsync(new Uri(url), null);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private void CredentialsReceived(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }

            if (e.Error != null)
            {
                return;
            }

            var credentialsParams = e.Result;

        }
    }
}