using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using log4net;
using VATRP.Core.Model;
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
        private IAccountService _accountService;
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

        #region Overrides
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

        public override IAccountService AccountService
        {
            get { return _accountService ?? (_accountService = new AccountService(this)); }
        }

        public override System.Windows.Threading.Dispatcher Dispatcher
        {
            get { return App.Current.Dispatcher; }
        }
 #endregion

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
            retVal &= AccountService.Start();
            return retVal;
        }

        public bool UpdateLinphoneConfig()
        {
            if (App.CurrentAccount == null)
                return false;

            LinphoneSipService.LinphoneConfig.ProxyHost = string.IsNullOrEmpty(App.CurrentAccount.ProxyHostname) ?
                Configuration.LINPHONE_SIP_SERVER : App.CurrentAccount.ProxyHostname;
            LinphoneSipService.LinphoneConfig.ProxyPort = App.CurrentAccount.ProxyPort;
            LinphoneSipService.LinphoneConfig.UserAgent = this.ConfigurationService.Get(Configuration.ConfSection.LINPHONE, Configuration.ConfEntry.LINPHONE_USERAGENT,
                    Configuration.LINPHONE_USERAGENT);
            LinphoneSipService.LinphoneConfig.Username = App.CurrentAccount.RegistrationUser;
            LinphoneSipService.LinphoneConfig.DisplayName = App.CurrentAccount.DisplayName;
            LinphoneSipService.LinphoneConfig.Password = App.CurrentAccount.RegistrationPassword;

            return true;
        }

        internal void Stop()
        {
            ConfigurationService.Stop();
            LinphoneSipService.Unregister();
            LinphoneSipService.Stop();
            AccountService.Stop();
        }

        internal bool RequestLinphoneCredentials(string username, string passwd)
        {
            bool retValue = true;
            var requestLink = ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.REQUEST_LINK, Configuration.DEFAULT_REQUEST);
            var request = (HttpWebRequest)WebRequest.Create(requestLink);

            var postData = string.Format("{{ \"user\" : {{ \"email\" : \"{0}\", \"password\" : \"{1}\" }} }}", username, passwd);
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            try
            {
                var response = (HttpWebResponse) request.GetResponse();

                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    var responseString = new StreamReader(responseStream).ReadToEnd();
                    ParseHttpResponse(responseString);
                }
                else
                {
                    retValue = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return retValue;
        }

        private void ParseHttpResponse(string response)
        {
            // parse response stream, 
            var jss = new JavaScriptSerializer();
            var dict = jss.Deserialize<Dictionary<string, string>>(response);

            if (dict.ContainsKey("pbx_extension"))
            {
                App.CurrentAccount.RegistrationUser = dict["pbx_extension"];
            }
            if (dict.ContainsKey("auth_token"))
            {
                App.CurrentAccount.RegistrationPassword = dict["auth_token"];
            }

            if (UpdateLinphoneConfig())
            {
                if (LinphoneSipService.Start(true))
                    LinphoneSipService.Register();
            }
        }

        private void CredentialsReceived(object sender, DownloadStringCompletedEventArgs e)
        {
            
        }

        internal VATRPAccount LoadActiveAccount()
        {
            var accountUID = ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.ACCOUNT_IN_USE, "");
            if (string.IsNullOrEmpty(accountUID))
                return null;

            return AccountService.FindAccount(accountUID);
        }
    }
}